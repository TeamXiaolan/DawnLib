using System;
using Unity.Netcode;
using UnityEngine;

namespace Dawn.Utils;
[Serializable]
public class HUDDisplayTip : INetworkSerializable
{
    public HUDDisplayTip(string header, string body, AlertType type)
    {
        (_header, _body, _alertType) = (header, body, type);
    }

    public HUDDisplayTip() : this(string.Empty, string.Empty, AlertType.Hint) { }

    public enum AlertType
    {
        Hint,
        Warning,
    }

    [SerializeField]
    private AlertType _alertType;

    [SerializeField, TextArea(2, 5)]
    private string _header, _body;

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