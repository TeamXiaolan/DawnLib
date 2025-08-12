using CodeRebirthLib.CRMod;
using UnityEngine;

namespace CodeRebirthLib;
public class NoContentDefinitionInBundle(AssetBundle bundle) : BundleException(bundle, $"Main bundle did not contain a {nameof(ContentContainer)}!");