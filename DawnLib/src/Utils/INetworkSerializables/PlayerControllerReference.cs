using System;
using GameNetcodeStuff;
using Unity.Netcode;

namespace CodeRebirthLib.Utils;
public class PlayerControllerReference : INetworkSerializable
{
    private int _playerID;

    public bool IsLocalClient => StartOfRound.Instance.allPlayerScripts[_playerID].IsLocalPlayer();
    public bool IsAlive => !StartOfRound.Instance.allPlayerScripts[_playerID].isPlayerDead && StartOfRound.Instance.allPlayerScripts[_playerID].isPlayerControlled;
    public bool IsValid => _playerID != -1 && StartOfRound.Instance.allPlayerScripts.Length > _playerID;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _playerID);
    }

    public static implicit operator PlayerControllerB(PlayerControllerReference reference)
    {
        if (reference == null) return null;
        if (reference._playerID == -1) return null;
        return StartOfRound.Instance.allPlayerScripts[reference._playerID];
    }

    public static implicit operator PlayerControllerReference(PlayerControllerB player)
    {
        return new PlayerControllerReference
        {
            _playerID = Array.IndexOf(StartOfRound.Instance.allPlayerScripts, player), // (int)player.playerClientId,
        };
    }

    public static implicit operator bool(PlayerControllerReference reference)
    {
        return reference.IsValid;
    }

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