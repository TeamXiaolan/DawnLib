using Dawn;
using UnityEngine;

namespace Dusk;
public class NoContentDefinitionInBundle(AssetBundle bundle) : BundleException(bundle, $"Main bundle did not contain a {nameof(ContentContainer)}!");