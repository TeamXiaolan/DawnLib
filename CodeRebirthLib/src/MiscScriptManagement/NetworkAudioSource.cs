using System;
using Unity.Netcode;
using UnityEngine;

namespace CodeRebirthLib.MiscScriptManagement;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class NetworkAudioSource : NetworkBehaviour
{
    class PlayPacket : INetworkSerializable
    {
        public bool HasPoolClipID;
        public int ClipID;

        public bool HasPitch;
        public float Pitch;

        public bool HasVolume;
        public float Volume;

        public bool IsOneShot;

        public ulong CallerID;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref CallerID);
            serializer.SerializeValue(ref IsOneShot);

            serializer.SerializeValue(ref HasPoolClipID);
            if (HasPoolClipID)
            {
                serializer.SerializeValue(ref ClipID);
            }

            serializer.SerializeValue(ref HasPitch);
            if (HasPitch)
            {
                serializer.SerializeValue(ref Pitch);
            }

            serializer.SerializeValue(ref HasVolume);
            if (HasVolume)
            {
                serializer.SerializeValue(ref Volume);
            }
        }
    }

    [SerializeField]
    bool _syncClipFromPool, _syncPitch, _syncVolume;

    [SerializeField]
    bool _requiresOwnership = true;

    [SerializeField]
    AudioClip[] _poolToSync;

    AudioSource _source;

    void Awake()
    {
        _source = GetComponent<AudioSource>();
    }

    void CreateNetworkEvent(AudioClip clip, bool isOneShot)
    {
        PlayPacket packet = new PlayPacket()
        {
            CallerID = NetworkManager.LocalClientId,
            IsOneShot = isOneShot
        };

        if (_syncClipFromPool)
        {
            packet.HasPoolClipID = true;
            packet.ClipID = Array.IndexOf(_poolToSync, clip);
        }
        else
        {
            if (isOneShot)
            {
                CodeRebirthLibPlugin.Logger.LogError($"NetworkAudioSource failure on {gameObject.name}. PlayOneShot requires SyncClipFromPool = true.");
                return;
            }
        }

        if (_syncPitch)
        {
            packet.HasPitch = true;
            packet.Pitch = _source.pitch;
        }

        if (_syncVolume)
        {
            packet.HasVolume = true;
            packet.Volume = _source.volume;
        }

        SendPlayPacketServerRPC(packet);
        ActOnPlayPacket(packet);
    }

    public void Play()
    {
        CreateNetworkEvent(_source.clip, false);
    }

    public void PlayOneShot(AudioClip clip)
    {
        CreateNetworkEvent(clip, true);
    }

    [ServerRpc(RequireOwnership = false)]
    void SendPlayPacketServerRPC(PlayPacket packet, ServerRpcParams serverRpcParams = default)
    {
        ReceivePlayPacketClientRPC(packet, serverRpcParams.Receive.SenderClientId == OwnerClientId);
    }

    [ClientRpc]
    void ReceivePlayPacketClientRPC(PlayPacket packet, bool isFromOwner)
    {
        if (packet.CallerID == NetworkManager.LocalClientId) return;

        if (isFromOwner && _requiresOwnership)
        {
            CodeRebirthLibPlugin.Logger.LogWarning("Received Play Packet from non-owner. Dropping");
            return;
        }
        ActOnPlayPacket(packet);
    }

    void ActOnPlayPacket(PlayPacket packet)
    {
        if (packet.HasPitch) _source.pitch = packet.Pitch;
        if (packet.HasVolume) _source.volume = packet.Volume;

        AudioClip clip = null;
        if (packet.HasPoolClipID)
        {
            clip = _poolToSync[packet.ClipID];
        }

        if (packet.IsOneShot)
        {
            _source.PlayOneShot(clip);
        }
        else
        {
            _source.clip = clip;
            _source.Play();
        }
    }
}