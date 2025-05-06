namespace AccountServer.Controllers;

using AccountServer.DB;
using CloudStructures.Structures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedRedisData.Redis;
using StackExchange.Redis;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private AppDbContext _dbContext;

    public AccountController(AppDbContext dbContext) => _dbContext = dbContext;

    [HttpPost]
    [Route("create")]
    public CreateAccountRes CreateAccount([FromBody] CreateAccountReq accountReqPacket)
    {
        CreateAccountRes createAccountRes = new CreateAccountRes();

        AccountDb? account = _dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.AccountName == accountReqPacket.AccountName)
            .FirstOrDefault();

        if (account == null)
        {
            account = new AccountDb();
            account.AccountName = accountReqPacket.AccountName;
            account.Password = accountReqPacket.Password;
            _dbContext.Accounts.Add(account);
            _dbContext.SaveChanges(); // 동시에 같은 AccountName이 들어올 경우 여기서 Crash, 지금은 같지 않다고 가정

            createAccountRes.Result = true;
        }
        else
        {
            createAccountRes.Result = false;
        }

        return createAccountRes;
    }

    [HttpPost]
    [Route("login")]
    public async Task<LoginAccountRes> LoginAccount([FromBody] LoginAccountReq loginReqPacket)
    {
        LoginAccountRes loginAccountRes = new LoginAccountRes();

        AccountDb? account = _dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.AccountName == loginReqPacket.AccountName && a.Password == loginReqPacket.Password)
            .FirstOrDefault();

        if (account == null)
        {
            loginAccountRes.Result = false;
        }
        else
        {
            loginAccountRes.Result = true;

            // User Login 랜덤 토큰 생성
            int userToken = Random.Shared.Next(int.MinValue, int.MaxValue);

            // User Token Redis 저장
            TimeSpan expiredTime = TimeSpan.FromSeconds(10);
            var redisUserToken = new RedisDictionary<int, int>(RedisInfo.Connection, "UserToken", expiredTime);
            await redisUserToken.SetAsync(account.AccountDbId, userToken);

            loginAccountRes.AccountDbId = account.AccountDbId;
            loginAccountRes.UserToken = userToken;

            // Redis에서 서버 정보 확인 후 클라이언트에게 리턴
            var redisServerInfo = new RedisDictionary<string, RedisServerInfo>(RedisInfo.Connection, "ServerInfos", null);
            var serverInfos = await redisServerInfo.GetAllAsync();
            if (serverInfos != null)
            {
                foreach (RedisServerInfo info in serverInfos.Values)
                {
                    loginAccountRes.ServerInfos.Add(new ServerInfo()
                    {
                        Name = info.Name,
                        Ip = info.Ip,
                        Port = info.Port,
                        ServerLoad = info.ServerLoad
                    });
                }
            }
        }

        return loginAccountRes;
    }
}
