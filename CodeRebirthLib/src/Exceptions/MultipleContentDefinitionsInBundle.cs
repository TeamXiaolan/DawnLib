using CodeRebirthLib.ContentManagement;
using UnityEngine;

namespace CodeRebirthLib.Exceptions;
public class MultipleContentDefinitionsInBundle(AssetBundle bundle) : BundleException(bundle, $"Main bundle contained too many {nameof(ContentContainer)}s!");