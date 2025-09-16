using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dusk;

[Serializable]
public class SkinnedMeshReplacement : RendererReplacement
{
    [field: SerializeField]
    public SkinnedMeshRenderer? ReplacementRenderer { get; private set; }
}

[Serializable]
public class MeshReplacement : RendererReplacement
{
    [field: SerializeField]
    public Mesh? ReplacementMesh { get; private set; }
}

[Serializable]
public class RendererReplacement
{
    [field: SerializeField]
    public string PathToRenderer { get; private set; }
    [field: SerializeField]
    public List<MaterialWithIndex> ReplacementMaterials { get; private set; } = new();
}