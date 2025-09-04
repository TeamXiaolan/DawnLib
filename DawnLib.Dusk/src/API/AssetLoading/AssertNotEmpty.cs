using System;
using UnityEngine;

namespace Dusk;

[AttributeUsage(AttributeTargets.Field)]
public class AssertNotEmpty() : PropertyAttribute
{
}