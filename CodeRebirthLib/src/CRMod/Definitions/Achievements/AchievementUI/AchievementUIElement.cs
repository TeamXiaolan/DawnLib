
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace CodeRebirthLib.CRMod;

public class AchievementUIElement : MonoBehaviour
{
    [SerializeField]
    [FormerlySerializedAs("_achievementNameTMP")]
    internal TextMeshProUGUI achievementNameTMP = null!;

    [SerializeField]
    [FormerlySerializedAs("_achievementDescriptionTMP")]
    internal TextMeshProUGUI achievementDescriptionTMP = null!;

    [SerializeField]
    [FormerlySerializedAs("_achievementProgressGO")]
    internal GameObject achievementProgressGO = null!;

    [SerializeField]
    [FormerlySerializedAs("_achievementHiddenButton")]
    internal Button achievementHiddenButton = null!;

    [SerializeField]
    [FormerlySerializedAs("_achievementIcon")]
    internal Image achievementIcon = null!;

    [SerializeField]
    [FormerlySerializedAs("_unfinishedAchievementColor")]
    internal Color unfinishedAchievementColor = new(0.5f, 0.5f, 0.5f);

    [SerializeField]
    [FormerlySerializedAs("_backgroundImage")]
    internal Image backgroundImage = null!;

    [SerializeField]
    [FormerlySerializedAs("_progressBar")]
    internal Image progressBar = null!;

    [SerializeField]
    [FormerlySerializedAs("_eventTrigger")]
    internal EventTrigger? eventTrigger = null;


    internal CRMAchievementDefinition achievementDefinition = null!;

    public void SetupAchievementUI(CRMAchievementDefinition definition)
    {
        achievementDefinition = definition;
        CRAchievementHandler.UpdateUIElement(this, definition);
    }

    public void ResetAchievementProgress()
    {
        achievementDefinition.ResetProgress();
    }
}