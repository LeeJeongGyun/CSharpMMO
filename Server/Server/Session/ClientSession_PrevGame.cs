namespace Server;

using System.Runtime.Serialization;
using CloudStructures.Structures;
using Microsoft.EntityFrameworkCore;
using Protocol;
using Server.Content;
using Server.Content.Object;
using Server.Data;
using Server.DB;
using Server.Utils;
using ServerCore;
using SharedRedisData.Redis;

public partial class ClientSession : PacketSession
{
    public int AccountDbId { get; private set; }
    public List<LobbyPlayerInfo> LobbyPlayerInfos { get; private set; } = new List<LobbyPlayerInfo>();

    public async Task HandleLogin(C2S_Login loginPacket)
    {
        // DummyTest용 Client는 Redis 검사 진행하지 않는다.
        bool dummyClient = loginPacket.UniqueId.Contains("Dummy");
        bool result = await CheckValidUserToken(loginPacket.AccountDbId, loginPacket.UserToken);
        if (result == false && dummyClient == false)
            return;

        // Login 상태가 아니면 뭔가 이상하다..
        if (PlayerServerState != PlayerServerState.PlayerStateLogin)
            return;

        LobbyPlayerInfos.Clear();

        S2C_Login loginPacketRes = new S2C_Login();
        using (AppDbContext db = new AppDbContext())
        {
            AccountDb? account = db.Accounts
                .Include(account => account.Players)
                .Where(account => account.AccountName == loginPacket.UniqueId).FirstOrDefault();

            if (account == null)
            {
                account = new AccountDb() { AccountName = loginPacket.UniqueId };
                db.Accounts.Add(account);
                bool success = db.SaveChangesEx();
            }
            else
            {
                foreach (PlayerDb playerDb in account.Players)
                {
                    LobbyPlayerInfo lobbyPlayerInfo = new LobbyPlayerInfo()
                    {
                        PlayerDbId = playerDb.PlayerDbId,
                        Name = playerDb.PlayerName,
                        StatInfo = new StatInfo()
                        {
                            Level = playerDb.StatInfo.Level,
                            Hp = playerDb.StatInfo.Hp,
                            MaxHp = playerDb.StatInfo.MaxHp,
                            Attack = playerDb.StatInfo.Attack,
                            Speed = playerDb.StatInfo.Speed,
                            TotalExp = playerDb.StatInfo.TotalExp
                        }
                    };

                    // 메모리 저장
                    LobbyPlayerInfos.Add(lobbyPlayerInfo);

                    // 패킷 저장
                    loginPacketRes.PlayerInfos.Add(lobbyPlayerInfo);
                }
            }

            AccountDbId = account.AccountDbId;
        }

        loginPacketRes.LoginOk = 1;
        Send(loginPacketRes);
        PlayerServerState = PlayerServerState.PlayerStateLobby;
    }

    public void HandleEnterRoom(C2S_EnterRoom enterRoomPacket)
    {
        // Game 상태가 아니면 무시
        if (PlayerServerState != PlayerServerState.PlayerStateLobby)
            return;

        LobbyPlayerInfo? playerInfo = LobbyPlayerInfos.Find(p => p.Name == enterRoomPacket.Name);
        if (playerInfo == null)
            return;

        // 1. Player 생성
        Player player = Server.Content.ObjectManager.Instance.AddObject<Player>();
        {
            player.PlayerDbId = playerInfo.PlayerDbId;
            player.State = ObjectState.Idle;
            player.Dir = MoveDir.Down;
            player.PosInfo.PosX = 0;
            player.PosInfo.PosY = 0;
            player.Name = playerInfo.Name;
            player.StatInfo.MergeFrom(playerInfo.StatInfo);
            player.Session = this;

            {
                // Item 로딩
                S2C_ItemList itemListPacket = new S2C_ItemList();
                using (AppDbContext db = new AppDbContext())
                {
                    var items = db.Items.Where(item => item.OwnerDbId == player.PlayerDbId).ToList();
                    foreach (var itemDb in items)
                    {
                        Item? item = Item.MakeItem(itemDb);
                        if (item != null)
                        {
                            player.Inven.Add(item);

                            ItemInfo itemInfo = new ItemInfo();
                            itemInfo.MergeFrom(item.Info);
                            itemListPacket.ItemInfos.Add(itemInfo);
                        }
                    }

                    Send(itemListPacket);
                }
            }
        }

        ObjectId = player.ObjectId;
        PlayerServerState = PlayerServerState.PlayerStateGame;

        // 2. Room에 Player 입장
        GameLogic.Instance.Push(() =>
        {
            GameRoom? gameRoom = GameLogic.Instance.FindRoom(1);
            gameRoom?.Push(gameRoom.EnterRoom, player);
        });
    }

    public void HandleCreatePlayer(C2S_CreatePlayer createPlayerPacket)
    {
        // Lobby 상태가 아니면 무시
        if (PlayerServerState != PlayerServerState.PlayerStateLobby)
            return;

        using (AppDbContext db = new AppDbContext())
        {
            PlayerDb? player = db.Players.Where(p => p.PlayerName == createPlayerPacket.Name).FirstOrDefault();
            if (player != null)
            {
                Console.WriteLine($"중복된 이름: {createPlayerPacket.Name}");
                return;
            }

            DataManager.Stats.TryGetValue(1, out StatInfo? statInfo);
            if (statInfo == null)
                return;

            PlayerDb newPlayer = new PlayerDb()
            {
                PlayerName = createPlayerPacket.Name,
                AccountDbId = AccountDbId,
                StatInfo = new StatInfo()
                {
                    Level = statInfo.Level,
                    Hp = statInfo.Hp,
                    MaxHp = statInfo.MaxHp,
                    Attack = statInfo.Attack,
                    Speed = statInfo.Speed,
                    TotalExp = 0
                }
            };

            db.Players.Add(newPlayer);
            db.SaveChangesEx();

            LobbyPlayerInfo playerInfo = new LobbyPlayerInfo() { StatInfo = new StatInfo() };
            playerInfo.PlayerDbId = newPlayer.PlayerDbId;
            playerInfo.Name = newPlayer.PlayerName;
            playerInfo.StatInfo.MergeFrom(newPlayer.StatInfo);

            LobbyPlayerInfos.Add(playerInfo);

            S2C_CreatePlayer createPlayerRes = new S2C_CreatePlayer();
            createPlayerRes.PlayerInfo = playerInfo;
            Send(createPlayerRes);
        }
    }

    private async Task<bool> CheckValidUserToken(int accountDbId, int userToken)
    {
        var redis = new RedisDictionary<int, int>(RedisInfo.Connection, "UserToken", TimeSpan.FromSeconds(10));
        var redisUserToken = await redis.GetAndDeleteAsync(accountDbId);
        if (redisUserToken.HasValue == false)
            return false;

        return redisUserToken.Value == userToken;
    }
}
