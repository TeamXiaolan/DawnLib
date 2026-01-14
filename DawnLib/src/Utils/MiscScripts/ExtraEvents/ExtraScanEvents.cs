using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Dawn.Utils;
[RequireComponent(typeof(ScanNodeProperties))]
[AddComponentMenu($"{DawnConstants.ExtraEvents}/Extra Scan Events")]
public class ExtraScanEvents : MonoBehaviour
{
    private readonly static NamespacedKey _dataKey = NamespacedKey.From("dawn_lib", "already_scanned");

    [SerializeField]
    private UnityEvent _onScan;

    [Header("First scan"), SerializeField, Tooltip("Optional")]
    private NamespacedKey _saveId;

    [SerializeField]
    private UnityEvent _onFirstScan;

    internal void OnScan()
    {
        _onScan.Invoke();
        if (string.IsNullOrWhiteSpace(_saveId.Namespace) || string.IsNullOrWhiteSpace(_saveId.Key))
            return;

        HashSet<NamespacedKey> alreadyScanned = DawnLib.GetCurrentSave()!.GetOrCreateDefault<HashSet<NamespacedKey>>(_dataKey);
        if (!alreadyScanned.Contains(_saveId))
        {
            _onFirstScan.Invoke();
            alreadyScanned.Add(_saveId);
        }
        // make sure to update the persistent data container
        DawnLib.GetCurrentSave()!.Set(_dataKey, alreadyScanned);
    }
}