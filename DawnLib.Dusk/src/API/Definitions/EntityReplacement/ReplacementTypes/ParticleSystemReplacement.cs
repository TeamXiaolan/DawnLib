using System;
using UnityEngine;

namespace Dusk;

[Serializable]
public class ParticleSystemReplacement
{
    [field: SerializeField]
    public string PathToParticleSystem { get; private set; }
    [field: SerializeField]
    public ParticleSystem NewParticleSystem { get; private set; }
}