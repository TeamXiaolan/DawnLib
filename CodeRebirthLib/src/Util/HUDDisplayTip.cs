using Unity.Netcode;

namespace CodeRebirthLib.Util;
// todo: add other things that are used in a hud display tip.
public class HUDDisplayTip : INetworkSerializable
{
    private string _header, _body;

    public string Header => _header;
    public string Body => _body;
    
    public HUDDisplayTip(string header, string body)
    {
        _header = header;
        _body = body;
    }


    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _header);
        serializer.SerializeValue(ref _body);
    }
}