using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeRebirthLib.MiscScriptManagement;
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

        RectTransform? rectTransformOfImportance = null;
        if (!HUDManager.Instance.scanNodes.ContainsValue(scanNodeProperties))
            return;

        foreach (var (key, value) in HUDManager.Instance.scanNodes)
        {
            if (value == scanNodeProperties)
            {
                rectTransformOfImportance = key;
                // Plugin.ExtendedLogging($"Found scan node's gameobject: {key}");
            }
        }
        if (rectTransformOfImportance == null)
            return;

        foreach (Image image in rectTransformOfImportance.GetComponentsInChildren<Image>())
        {
            image.color = new Color(borderColor.r, borderColor.g, borderColor.b, image.color.a);
        }

        Transform transformOfImportance = rectTransformOfImportance.GetChild(1);
        foreach (TextMeshProUGUI text in transformOfImportance.GetComponentsInChildren<TextMeshProUGUI>())
        {
            text.color = new Color(textColor.r, textColor.g, textColor.b, text.color.a);
        }
    }
}