using Unity.Netcode;

namespace CodeRebirthLib.Utils;
public class HUDDisplayTip(string header, string body, HUDDisplayTip.AlertType type = HUDDisplayTip.AlertType.Hint) : INetworkSerializable
{
    public enum AlertType
    {
        Hint,
        Warning,
    }

    private AlertType _alertType = type;

    private string _header = header, _body = body;

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