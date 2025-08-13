using CodeRebirthLib.Internal.ModCompats.cs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeRebirthLib.Utils;

public class ForceScanColorOnItem : MonoBehaviour
{
    public GrabbableObject grabbableObject = null!;
    public Color borderColor = Color.green;
    public Color textColor = Color.green;

    [SerializeField]
    private ScanNodeProperties? scanNodeProperties = null;

    public void Start()
    {
        FindGrabbableObjectsScanObject();
    }

    public void FindGrabbableObjectsScanObject()
    {
        if (grabbableObject == null || scanNodeProperties != null)
            return;

        scanNodeProperties = grabbableObject.GetComponentInChildren<ScanNodeProperties>();
    }

    public void LateUpdate()
    {
        if (scanNodeProperties == null)
            return;

        if (GoodItemScanCompat.Enabled && GoodItemScanCompat.TryGetRectTransform(scanNodeProperties, out var rectTransform))
        {
            HandleChangingColor(rectTransform);
        }

        if (!HUDManager.Instance.scanNodes.ContainsValue(scanNodeProperties))
            return;

        foreach (var (key, value) in HUDManager.Instance.scanNodes)
        {
            if (value == scanNodeProperties)
            {
                HandleChangingColor(key);
                break;
                // Plugin.ExtendedLogging($"Found scan node's gameobject: {key}");
            }
        }
    }

    private void HandleChangingColor(RectTransform rectTransformOfImportance)
    {
        var scanNodeAdditionalData = ScanNodeAdditionalData.CreateOrGet(rectTransformOfImportance);
        foreach (Image image in scanNodeAdditionalData.ImagesAttached)
        {
            image.color = new Color(borderColor.r, borderColor.g, borderColor.b, image.color.a);
        }

        foreach (TextMeshProUGUI text in scanNodeAdditionalData.TextsAttached)
        {
            text.color = new Color(textColor.r, textColor.g, textColor.b, text.color.a);
        }
    }
}