using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;

public enum PacketID
{
    C2S_PlayerInfoReq = 1,
	S2C_PlayerInfoRes = 2,
	C2S_Chat = 3,
	S2C_Chat = 4,
	
}

public interface IPacket
{
    public ushort PacketId { get; }

    public void Deserialize(ArraySegment<byte> buffer);

    public ArraySegment<byte> Serialize();
}

public class C2S_PlayerInfoReq : IPacket
{
    public int playerId;
	public byte time;
	public string name;
	public List<Skill> skills = new List<Skill>();
	public class Skill
	{
	    public int id;
		public string name;
	
	    public void Deserialize(ArraySegment<byte> buffer, ref ushort count)
	    {
	        ReadOnlySpan<byte> span = buffer;
	        id = BitConverter.ToInt32(span.Slice(count));
			count += sizeof(int);
			ushort nameLen = BitConverter.ToUInt16(span.Slice(count));
			count += sizeof(ushort);
			name = Encoding.Unicode.GetString(span.Slice(count, nameLen));
			count += nameLen;
	    }
	
	    public bool Serialize(Span<byte> span, ref ushort count)
	    {
	        bool success = true;
	        success &= BitConverter.TryWriteBytes(span.Slice(count), id);
			count += sizeof(int);
			int nameLen = Encoding.Unicode.GetBytes(name.AsSpan(), span.Slice(count + sizeof(ushort)));
			success &= BitConverter.TryWriteBytes(span.Slice(count), (ushort)nameLen);
			count += sizeof(ushort);
			count += (ushort)nameLen;
	        return success;
	    }
	}

    public ushort PacketId => (ushort)PacketID.C2S_PlayerInfoReq;

    public void Deserialize(ArraySegment<byte> buffer)
    {
        ReadOnlySpan<byte> span = buffer;
        ushort count = 4;

        playerId = BitConverter.ToInt32(span.Slice(count));
		count += sizeof(int);
		time = buffer[count];
		count += sizeof(byte);
		ushort nameLen = BitConverter.ToUInt16(span.Slice(count));
		count += sizeof(ushort);
		name = Encoding.Unicode.GetString(span.Slice(count, nameLen));
		count += nameLen;
		int skillLen = BitConverter.ToUInt16(span.Slice(count));
		count += sizeof(ushort);
		
		skills.Clear();
		for (int i = 0; i < skillLen; ++i)
		{
		    Skill skill = new Skill();
		    skill.Deserialize(buffer, ref count);
		    skills.Add(skill);
		}
    }

    public ArraySegment<byte> Serialize()
    {
        Span<byte> span = SendBufferHelper.Open(4096);
        bool success = true;
        ushort count = 2;
        success &= BitConverter.TryWriteBytes(span.Slice(count), (ushort)PacketID.C2S_PlayerInfoReq);
        count += sizeof(ushort);

        success &= BitConverter.TryWriteBytes(span.Slice(count), playerId);
		count += sizeof(int);
		span[count] = time;
		count += sizeof(byte);
		int nameLen = Encoding.Unicode.GetBytes(name.AsSpan(), span.Slice(count + sizeof(ushort)));
		success &= BitConverter.TryWriteBytes(span.Slice(count), (ushort)nameLen);
		count += sizeof(ushort);
		count += (ushort)nameLen;
		BitConverter.TryWriteBytes(span.Slice(count), (ushort)skills.Count);
		count += sizeof(ushort);
		foreach (Skill skill in skills)
		    success &= skill.Serialize(span, ref count);

        success &= BitConverter.TryWriteBytes(span, (ushort)count);
        if (success == false)
            throw new InvalidOperationException("Serialization Error");

        return SendBufferHelper.Close(count);
    }
}

public class S2C_PlayerInfoRes : IPacket
{
    public bool result;
	public int playerId;
	public byte time;
	public string name;

    public ushort PacketId => (ushort)PacketID.S2C_PlayerInfoRes;

    public void Deserialize(ArraySegment<byte> buffer)
    {
        ReadOnlySpan<byte> span = buffer;
        ushort count = 4;

        result = BitConverter.ToBoolean(span.Slice(count));
		count += sizeof(bool);
		playerId = BitConverter.ToInt32(span.Slice(count));
		count += sizeof(int);
		time = buffer[count];
		count += sizeof(byte);
		ushort nameLen = BitConverter.ToUInt16(span.Slice(count));
		count += sizeof(ushort);
		name = Encoding.Unicode.GetString(span.Slice(count, nameLen));
		count += nameLen;
    }

    public ArraySegment<byte> Serialize()
    {
        Span<byte> span = SendBufferHelper.Open(4096);
        bool success = true;
        ushort count = 2;
        success &= BitConverter.TryWriteBytes(span.Slice(count), (ushort)PacketID.S2C_PlayerInfoRes);
        count += sizeof(ushort);

        success &= BitConverter.TryWriteBytes(span.Slice(count), result);
		count += sizeof(bool);
		success &= BitConverter.TryWriteBytes(span.Slice(count), playerId);
		count += sizeof(int);
		span[count] = time;
		count += sizeof(byte);
		int nameLen = Encoding.Unicode.GetBytes(name.AsSpan(), span.Slice(count + sizeof(ushort)));
		success &= BitConverter.TryWriteBytes(span.Slice(count), (ushort)nameLen);
		count += sizeof(ushort);
		count += (ushort)nameLen;

        success &= BitConverter.TryWriteBytes(span, (ushort)count);
        if (success == false)
            throw new InvalidOperationException("Serialization Error");

        return SendBufferHelper.Close(count);
    }
}

public class C2S_Chat : IPacket
{
    public string chat;

    public ushort PacketId => (ushort)PacketID.C2S_Chat;

    public void Deserialize(ArraySegment<byte> buffer)
    {
        ReadOnlySpan<byte> span = buffer;
        ushort count = 4;

        ushort chatLen = BitConverter.ToUInt16(span.Slice(count));
		count += sizeof(ushort);
		chat = Encoding.Unicode.GetString(span.Slice(count, chatLen));
		count += chatLen;
    }

    public ArraySegment<byte> Serialize()
    {
        Span<byte> span = SendBufferHelper.Open(4096);
        bool success = true;
        ushort count = 2;
        success &= BitConverter.TryWriteBytes(span.Slice(count), (ushort)PacketID.C2S_Chat);
        count += sizeof(ushort);

        int chatLen = Encoding.Unicode.GetBytes(chat.AsSpan(), span.Slice(count + sizeof(ushort)));
		success &= BitConverter.TryWriteBytes(span.Slice(count), (ushort)chatLen);
		count += sizeof(ushort);
		count += (ushort)chatLen;

        success &= BitConverter.TryWriteBytes(span, (ushort)count);
        if (success == false)
            throw new InvalidOperationException("Serialization Error");

        return SendBufferHelper.Close(count);
    }
}

public class S2C_Chat : IPacket
{
    public int playerId;
	public string chat;

    public ushort PacketId => (ushort)PacketID.S2C_Chat;

    public void Deserialize(ArraySegment<byte> buffer)
    {
        ReadOnlySpan<byte> span = buffer;
        ushort count = 4;

        playerId = BitConverter.ToInt32(span.Slice(count));
		count += sizeof(int);
		ushort chatLen = BitConverter.ToUInt16(span.Slice(count));
		count += sizeof(ushort);
		chat = Encoding.Unicode.GetString(span.Slice(count, chatLen));
		count += chatLen;
    }

    public ArraySegment<byte> Serialize()
    {
        Span<byte> span = SendBufferHelper.Open(4096);
        bool success = true;
        ushort count = 2;
        success &= BitConverter.TryWriteBytes(span.Slice(count), (ushort)PacketID.S2C_Chat);
        count += sizeof(ushort);

        success &= BitConverter.TryWriteBytes(span.Slice(count), playerId);
		count += sizeof(int);
		int chatLen = Encoding.Unicode.GetBytes(chat.AsSpan(), span.Slice(count + sizeof(ushort)));
		success &= BitConverter.TryWriteBytes(span.Slice(count), (ushort)chatLen);
		count += sizeof(ushort);
		count += (ushort)chatLen;

        success &= BitConverter.TryWriteBytes(span, (ushort)count);
        if (success == false)
            throw new InvalidOperationException("Serialization Error");

        return SendBufferHelper.Close(count);
    }
}

