using UnityEngine;
using UnityEngine.Events;

namespace Dawn.Utils;
[RequireComponent(typeof(ScanNodeProperties))]
public class ExtraScanEvents : MonoBehaviour
{
    [SerializeField]
    internal UnityEvent _onScan;
}