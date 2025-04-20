namespace PacketGenerator;

internal class PacketFormat
{
    // {0} 패킷 매니저 이름
    // {1} 패킷 등록
    public static string managerFormat =
"""
using System;
using System.Collections.Generic;
using Google.Protobuf;
using Protocol;
using ServerCore;

internal class {0}
{{
    #region Singleton

    private static {0} _instance = null!;

    public static {0} Instance
    {{
        get
        {{
            if (_instance == null)
                _instance = new {0}();

            return _instance;
        }}
    }}

    #endregion Singleton

    private Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>> _makePacketMap = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>>();
    private Dictionary<ushort, Action<PacketSession, IMessage>> _packetHandlerMap = new Dictionary<ushort, Action<PacketSession, IMessage>>();

    public {0}() => Register();

    // Unity에서만 사용
    public Action<ushort, IMessage> CustomHandler {{ get; set; }}

    public void MatchPacketHandler(PacketSession session, ArraySegment<byte> buffer)
    {{
        ushort packetId = BitConverter.ToUInt16(buffer.Array!, buffer.Offset + 2);

        Action<PacketSession, ArraySegment<byte>, ushort>? action;
        if (_makePacketMap.TryGetValue(packetId, out action))
            action.Invoke(session, buffer, packetId);
    }}

    public void Register()
    {{
        {1}
    }}

    // Unity에서만 사용
    public Action<PacketSession, IMessage> GetPacketHandler(ushort packetId)
    {{
        Action<PacketSession, IMessage>? packetHandler;
        _packetHandlerMap.TryGetValue(packetId, out packetHandler);
        return packetHandler;
    }}

    private void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer, ushort packetId) where T : IMessage, new()
    {{
        IMessage packet = new T();
        packet.MergeFrom(buffer.Slice(4));

        if (CustomHandler != null)
            CustomHandler.Invoke(packetId, packet);
        else
        {{
            Action<PacketSession, IMessage>? packetHandler;
            if (_packetHandlerMap.TryGetValue(packetId, out packetHandler))
                packetHandler.Invoke(session, packet);
        }}
    }}
}}
""";

    // {0} 패킷 이름
    // {1} 수정된 패킷 이름
    public static string managerRegisterFormat =
"""
_makePacketMap.Add((ushort)PacketID.{0}, MakePacket<{1}>);
_packetHandlerMap.Add((ushort)PacketID.{0}, PacketHandler.{1}Handler);
""";
}
