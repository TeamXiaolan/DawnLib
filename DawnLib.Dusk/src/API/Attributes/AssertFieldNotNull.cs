using System;
using UnityEngine;

namespace Dusk;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class AssertNotNull() : PropertyAttribute
{
}