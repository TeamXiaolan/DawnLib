using Dawn.Dusk;
using UnityEngine;

namespace Dawn;
public class NoContentDefinitionInBundle(AssetBundle bundle) : BundleException(bundle, $"Main bundle did not contain a {nameof(ContentContainer)}!");