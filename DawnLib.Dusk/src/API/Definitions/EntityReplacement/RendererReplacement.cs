using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dusk;

[Serializable]
public class SkinnedMeshReplacement : RendererReplacement
{
    [SerializeField]
    public SkinnedMeshRenderer? ReplacementRenderer { get; private set; }
}

[Serializable]
public class MeshReplacement : RendererReplacement
{
    [SerializeField]
    public Mesh? ReplacementMesh { get; private set; }
}

[Serializable]
public class RendererReplacement
{
    [SerializeField]
    public string PathToRenderer { get; private set; }
    [SerializeField]
    public List<MaterialWithIndex> ReplacementMaterials { get; private set; } = new();
}