using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CodeRebirthLib.ModCompats;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeRebirthLib.MiscScriptManagement;
public class ScanNodeAdditionalData
{
    private static readonly Dictionary<RectTransform, ScanNodeAdditionalData> _additionalData = new();
    private RectTransform _rectTransform;

    private List<Image> _imagesAttached = new();
    private List<TextMeshProUGUI> _textsAttached = new();

    public IReadOnlyList<Image> ImagesAttached => _imagesAttached.AsReadOnly();
    public IReadOnlyList<TextMeshProUGUI> TextsAttached => _textsAttached.AsReadOnly();

    private ScanNodeAdditionalData(RectTransform rectTransform)
    {
        _rectTransform = rectTransform;
        _additionalData[rectTransform] = this;
        _imagesAttached = _rectTransform.GetComponentsInChildren<Image>().ToList();
        Transform transformOfImportance = _rectTransform.GetChild(1);
        _textsAttached = transformOfImportance.GetComponentsInChildren<TextMeshProUGUI>().ToList();
    }

    private void OnDestroy()
    {
        _additionalData.Remove(_rectTransform);
    }

    public bool TryGetScanNode([NotNullWhen(true)] out ScanNodeProperties? scanNodeProperties)
    {
        scanNodeProperties = null;
        if (GoodItemScanCompatibility.Enabled && GoodItemScanCompatibility.TryGetScanNode(_rectTransform, out scanNodeProperties))
        {
            return true;
        }

        if (HUDManager.Instance.scanNodes.TryGetValue(_rectTransform, out scanNodeProperties))
        {
            return true;
        }
        return false;
    }

    public static ScanNodeAdditionalData CreateOrGet(RectTransform rectTransform)
    {
        if (_additionalData.TryGetValue(rectTransform, out ScanNodeAdditionalData data))
        {
            return data;
        }

        data = new ScanNodeAdditionalData(rectTransform);
        return data;
    }
}