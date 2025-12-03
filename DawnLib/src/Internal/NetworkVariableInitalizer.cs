using Dawn.Utils;
using Unity.Netcode;
using Unity.Netcode.Components;

namespace Dawn.Internal;
static class NetworkVariableInitalizer
{
    internal static void Init()
    {
        NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedByMemcpy<bool>();
        NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedIEquatable<bool>();

        NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedByMemcpy<float>();
        NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedIEquatable<float>();

        NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedByMemcpy<double>();
        NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedIEquatable<double>();

        NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedByMemcpy<int>();
        NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedIEquatable<int>();

        NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedINetworkSerializable<NetworkTransform.NetworkTransformState>();
        NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedValueEquals<NetworkTransform.NetworkTransformState>();

        NetworkVariableSerializationTypes.InitializeSerializer_ManagedINetworkSerializable<PlayerControllerReference>();
        NetworkVariableSerializationTypes.InitializeEqualityChecker_ManagedIEquatable<PlayerControllerReference>();
    }
}