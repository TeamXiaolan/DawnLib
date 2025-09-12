using System;
using UnityEngine;

namespace Dusk;

[Serializable]
public class StringWithAudioClip // TODO move this elsewhere
{
    [field: SerializeField]
    public string FieldName { get; private set; }

    [field: SerializeField]
    public AudioClip ReplacementAudioClip { get; private set; }
}