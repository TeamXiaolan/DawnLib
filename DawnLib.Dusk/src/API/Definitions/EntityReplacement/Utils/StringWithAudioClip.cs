using System;
using System.Collections.Generic;
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

[Serializable]
public class StringWithAudioClipArray // TODO move this elsewhere
{
    [field: SerializeField]
    public string FieldName { get; private set; }

    [field: SerializeField]
    public AudioClip[] ReplacementAudioClipArray { get; private set; }
}

[Serializable]
public class StringWithAudioClipList // TODO move this elsewhere
{
    [field: SerializeField]
    public string FieldName { get; private set; }

    [field: SerializeField]
    public List<AudioClip> ReplacementAudioClipList { get; private set; }
}