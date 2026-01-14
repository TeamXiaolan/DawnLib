using System.Collections.Generic;
using Dawn.Internal;

namespace Dawn;
public static partial class Tags
{
    internal static void AddToList(this HashSet<NamespacedKey> namespacedKeys, IEnumerable<(string @namespace, string key)> tagsWithModNames, DebugLogSource? debugLogSource = null, string? objectName = null)
    {
        foreach ((string modName, string tagName) in tagsWithModNames)
        {
            string normalizedModName = NamespacedKey.NormalizeStringForNamespacedKey(modName, false);
            string normalizedTagName = NamespacedKey.NormalizeStringForNamespacedKey(tagName, false);

            if (normalizedModName == "lethalcompany")
            {
                normalizedModName = "lethal_level_loader";
            }

            if (objectName != null)
            {
                debugLogSource?.Log($"Adding tag {normalizedModName}:{normalizedTagName} to {objectName}");
            }
            namespacedKeys.Add(NamespacedKey.From(normalizedModName, normalizedTagName));
        }
    }
}