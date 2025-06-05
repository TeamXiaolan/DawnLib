using CodeRebirthLib.ContentManagement;
using UnityEngine;

namespace CodeRebirthLib.Exceptions;
public class NoContentDefinitionInBundle(AssetBundle bundle) : BundleException(bundle, $"Main bundle did not contain a {nameof(ContentContainer)}!");