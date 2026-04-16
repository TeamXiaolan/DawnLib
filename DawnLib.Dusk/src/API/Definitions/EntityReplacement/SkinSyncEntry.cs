using Unity.Netcode;

namespace Dusk;

public struct SkinSyncEntry(NetworkObjectReference grabbableRef, ulong networkID) : INetworkSerializable
{
    public NetworkObjectReference GrabbableRef = grabbableRef;
    public ulong NetworkID = networkID;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref GrabbableRef);
        serializer.SerializeValue(ref NetworkID);
    }
}