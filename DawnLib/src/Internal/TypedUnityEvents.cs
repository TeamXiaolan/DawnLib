using System;
using Dawn.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Dawn.Internal;

[Serializable] public class StringEvent : UnityEvent<string> { }
[Serializable] public class FloatEvent : UnityEvent<float> { }
[Serializable] public class IntEvent : UnityEvent<int> { }
[Serializable] public class BoolEvent : UnityEvent<bool> { }
[Serializable] public class Vector3Event : UnityEvent<Vector3> { }
[Serializable] public class ColorEvent : UnityEvent<Color> { }
[Serializable] public class AnimationCurveEvent : UnityEvent<AnimationCurve> { }

[Serializable] public class BoundedRangeEvent : UnityEvent<BoundedRange> { }