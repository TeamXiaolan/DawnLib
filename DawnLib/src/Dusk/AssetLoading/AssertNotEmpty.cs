using System;
using UnityEngine;

namespace Dawn.Dusk;

[AttributeUsage(AttributeTargets.Field)]
public class AssertNotEmpty() : PropertyAttribute
{
}