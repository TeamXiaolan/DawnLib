using CodeRebirthLib.CRMod;
using UnityEngine;

namespace CodeRebirthLib;
public class MultipleContentDefinitionsInBundle(AssetBundle bundle) : BundleException(bundle, $"Main bundle contained too many {nameof(ContentContainer)}s!");