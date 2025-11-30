using System;
using UnityEngine;

namespace Dusk;

[AttributeUsage(AttributeTargets.Field)]
public class DontDrawIfEmpty() : PropertyAttribute
{
}