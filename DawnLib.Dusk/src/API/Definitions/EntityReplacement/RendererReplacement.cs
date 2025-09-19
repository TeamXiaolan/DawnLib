using System.Collections.Generic;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(fileName = "New Skinned Mesh Replacement", menuName = $"Entity Replacements/Renderer Replacements/Skinned Mesh Replacement")]
public class SkinnedMeshReplacement : RendererReplacement
{
    [field: SerializeField]
    public SkinnedMeshRenderer? ReplacementRenderer { get; private set; }
}

[CreateAssetMenu(fileName = "New Mesh Replacement", menuName = $"Entity Replacements/Renderer Replacements/Mesh Replacement")]
public class MeshReplacement : RendererReplacement
{
    [field: SerializeField]
    public Mesh? ReplacementMesh { get; private set; }
}

public class RendererReplacement : ScriptableObject
{
    [field: SerializeField]
    public string PathToRenderer { get; private set; }
    [field: SerializeField]
    public List<MaterialWithIndex> ReplacementMaterials { get; private set; } = new();
}