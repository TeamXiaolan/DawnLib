using Dawn;
using UnityEngine;

namespace Dusk;
public class MultipleContentDefinitionsInBundle(AssetBundle bundle) : BundleException(bundle, $"Main bundle contained too many {nameof(ContentContainer)}s!");