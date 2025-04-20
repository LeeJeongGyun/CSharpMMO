START ../../PacketGenerator/bin/PacketGenerator.exe ../../PacketGenerator/bin

XCOPY .\GenPackets.cs ..\..\DummyClient\Packet\ /Y
XCOPY .\GenPackets.cs ..\..\Server\Packet\ /Y
XCOPY .\GenPackets.cs ..\..\UnityClient\Assets\Scripts\Packet\ /Y

XCOPY .\ClientPacketManager.cs ..\..\DummyClient\Packet\ /Y
XCOPY .\ClientPacketManager.cs ..\..\UnityClient\Assets\Scripts\Packet\ /Y
XCOPY .\ServerPacketManager.cs ..\..\Server\Packet\ /Y

IF ERRORLEVEL 1 (
	echo "error"
	PAUSE
)