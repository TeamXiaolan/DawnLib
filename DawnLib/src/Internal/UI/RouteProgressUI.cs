using System;
using System.Collections.Generic;
using System.Linq;
using Dawn.Utils;
using GameNetcodeStuff;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dawn.Internal;
public class RouteProgressUI : Singleton<RouteProgressUI>
{
    [Serializable]
    public class BundleStateColour
    {
        public DawnMoonNetworker.BundleState State;
        public Color Color;
    }
    
    [Header("Progress Bar")]
    [field: SerializeField]
    private TMP_Text _routingToText;
    [field: SerializeField]
    private Slider _progressSlider;

    [field: Header("Nameplate UI")]
    [field: SerializeField]
    private PlayerNameplateUI _nameplatePrefab;

    [field: SerializeField]
    private Transform _nameplateParent;

    [field: SerializeField]
    private List<BundleStateColour> _colours;
    
    private Dictionary<PlayerControllerB, PlayerNameplateUI> _nameplates = new();
    private float _targetProgress;
    private List<Image> imagesToBeRedOrBlue = new();
    
    void Start()
    {
        foreach (Image image in _progressSlider.GetComponentsInChildren<Image>())
        {
            if (image.gameObject.name != "Fill" && image.gameObject.name != "ShipIcon")
                continue;

            imagesToBeRedOrBlue.Add(image);
        }
        gameObject.SetActive(false);
    }

    public void Refresh(Dictionary<PlayerControllerB, DawnMoonNetworker.BundleState> states)
    {
        // first update progress bar
        float totalProgress = states.Count;
        float currentProgress = 0;
        bool anyErrors = false;
        foreach (DawnMoonNetworker.BundleState state in states.Values)
        {
            if (state == DawnMoonNetworker.BundleState.Error)
            {
                anyErrors = true;
            }

            if (state == DawnMoonNetworker.BundleState.Queued)
            {
                currentProgress += 0.25f;
            }
            else if (state == DawnMoonNetworker.BundleState.Loading)
            {
                currentProgress += 0.50f;
            }
            else if (state == DawnMoonNetworker.BundleState.Loading)
            {
                currentProgress += 0.75f;
            }
            else if (state == DawnMoonNetworker.BundleState.Done)
            {
                currentProgress += 1f;
            }
        }

        if (anyErrors)
        {
            foreach (Image image in imagesToBeRedOrBlue)
            {
                image.color = Color.red;
            }
        }

        _targetProgress = currentProgress / totalProgress;

        // remove any disconnected players (no longer in states dictionary)
        foreach (PlayerControllerB player in _nameplates.Keys)
        {
            if (states.ContainsKey(player))
                continue;

            Destroy(_nameplates[player].gameObject);
            _nameplates.Remove(player);
        }

        // update (or create) new references
        foreach (PlayerControllerB player in states.Keys)
        {
            if (!_nameplates.TryGetValue(player, out PlayerNameplateUI ui))
            {
                ui = CreateUI(player);
            }

            ui.TextColor = GetBundleStateColour(states[player]);
        }
    }

    private void Update()
    {
        _progressSlider.value = Mathf.Lerp(_progressSlider.value, _targetProgress, Time.deltaTime);
    }

    public void Setup(string moonName)
    {
        _routingToText.text = $"  {moonName}";
        _nameplateParent.KillAllChildren();
        _nameplates.Clear();
        _progressSlider.value = 0;
        _targetProgress = 0;
    }

    private PlayerNameplateUI CreateUI(PlayerControllerB player)
    {
        PlayerNameplateUI created = Instantiate(_nameplatePrefab, _nameplateParent);
        created.Setup(player);
        _nameplates[player] = created;
        return created;
    }

    private Color GetBundleStateColour(DawnMoonNetworker.BundleState state)
    {
        BundleStateColour? colour = _colours.FirstOrDefault(it => it.State == state);
        if (colour == null)
        {
            return Color.magenta;
        }

        return colour.Color;
    }
}