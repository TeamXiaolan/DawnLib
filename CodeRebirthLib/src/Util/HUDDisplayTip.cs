using Unity.Netcode;

namespace CodeRebirthLib.Util;
// todo: add other things that are used in a hud display tip.
public class HUDDisplayTip : INetworkSerializable
{
    public enum AlertType
    {
        Hint,
        Warning,
    }

    private AlertType _alertType;

    private string _header, _body;

    public HUDDisplayTip(string header, string body, AlertType type = AlertType.Hint)
    {
        _header = header;
        _body = body;
        _alertType = type;
    }

    public string Header => _header;
    public string Body => _body;
    public AlertType Type => _alertType;


    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _header);
        serializer.SerializeValue(ref _body);
        serializer.SerializeValue(ref _alertType);
    }
}