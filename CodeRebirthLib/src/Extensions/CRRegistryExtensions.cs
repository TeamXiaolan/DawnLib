using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CodeRebirthLib.AssetManagement;

namespace CodeRebirthLib.Extensions;

public static class CRRegistryExtensions
{
    public static bool TryGetFirstBySomeName<TDefinition>(this IEnumerable<TDefinition> registry, Func<TDefinition, string> transformer, string name, [NotNullWhen(true)] out TDefinition? value, string? failContext = null) where TDefinition : CRContentDefinition
    {
        value = registry.FirstOrDefault(it => transformer(it).Contains(name, StringComparison.OrdinalIgnoreCase));
        if (!value && !string.IsNullOrEmpty(failContext))
        {
            CodeRebirthLibPlugin.ExtendedLogging(failContext);
        }
        return value; // implicit cast to bool
    }
}