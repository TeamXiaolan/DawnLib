
using System;
using System.Threading.Tasks;

namespace Dawn;

sealed class RoundLoadingStepEntry(NamespacedKey key, Func<IRoundLoadingContext, Task> callback, NamespacedKey[] hardDependencies, NamespacedKey[] softDependencies)
{
    public NamespacedKey NamespacedKey { get; } = key;
    public Func<IRoundLoadingContext, Task> Callback { get; } = callback;
    public NamespacedKey[] HardDependencies { get; } = hardDependencies;
    public NamespacedKey[] SoftDependencies { get; } = softDependencies;
}