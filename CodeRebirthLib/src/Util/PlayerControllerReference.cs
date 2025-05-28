using System;
using GameNetcodeStuff;
using Unity.Netcode;

namespace CodeRebirthLib.Util;
public class PlayerControllerReference : INetworkSerializable
{
    private int _playerID;

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
        return obj is PlayerControllerReference other && other._playerID == _playerID;
    }

    public override int GetHashCode()
    {
        return _playerID;
    }
}