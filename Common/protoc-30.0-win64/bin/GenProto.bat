protoc.exe -I=./ --csharp_out=./ ./Protocol.proto
IF ERRORLEVEL 1 PAUSE

START ..\..\..\Server\PacketGenerator\bin\PacketGenerator.exe	.\Protocol.proto
IF ERRORLEVEL 1 PAUSE

XCOPY .\Protocol.cs	..\..\..\Server\Server\Packet	/Y
XCOPY .\Protocol.cs	..\..\..\Server\DummyTest\Packet	/Y
XCOPY .\Protocol.cs	..\..\..\Unity_Client\Assets\Scripts\Packet	/Y

XCOPY .\ServerPacketManager.cs	..\..\..\Server\Server\Packet	/Y
XCOPY .\ClientPacketManager.cs	..\..\..\Server\DummyTest\Packet	/Y
XCOPY .\ClientPacketManager.cs	..\..\..\Unity_Client\Assets\Scripts\Packet	/Y
