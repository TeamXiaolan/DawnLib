using System;
using UnityEngine;

namespace Dusk;

[AttributeUsage(AttributeTargets.Field)]
public class DontDrawIfEmpty(string? groupId = null, string? header = null) : PropertyAttribute
{
    public string? GroupId { get; } = groupId;
    public string? Header { get; } = header;
}