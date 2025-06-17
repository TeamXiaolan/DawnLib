using System;
using GameNetcodeStuff;
using Unity.Netcode;

namespace CodeRebirthLib.Util;
public class PlayerControllerReference : INetworkSerializable
{
    private int _playerID;
    private PlayerControllerReference() { }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _playerID);
    }

    public static implicit operator PlayerControllerB(PlayerControllerReference reference) => StartOfRound.Instance.allPlayerScripts[reference._playerID];
    public static implicit operator PlayerControllerReference(PlayerControllerB player) => new()
    {
        _playerID = Array.IndexOf(StartOfRound.Instance.allPlayerScripts, player), // (int)player.playerClientId,
    };

    public override bool Equals(object? obj)
    {
        if (obj is PlayerControllerReference otherReference) return otherReference._playerID == _playerID;
        if (obj is PlayerControllerB player) return Array.IndexOf(StartOfRound.Instance.allPlayerScripts, player) == _playerID;
        return false;
    }

    public override int GetHashCode()
    {
        return _playerID;
    }
}