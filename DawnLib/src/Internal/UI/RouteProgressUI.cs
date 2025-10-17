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
    [field: SerializeField]
    private float _lerpSmoothing = 14f;

    [field: Header("Nameplate UI")]
    [field: SerializeField]
    private PlayerNameplateUI _nameplatePrefab;

    [field: SerializeField]
    private Transform _nameplateParent;

    [field: SerializeField]
    private List<BundleStateColour> _colours;
    
    private Dictionary<PlayerControllerB, PlayerNameplateUI> _nameplates = new();
    private float _targetProgress;

    public void Refresh(Dictionary<PlayerControllerB, DawnMoonNetworker.BundleState> states)
    {
        // first update progress bar
        int completedPlayers = states.Count(it => it.Value == DawnMoonNetworker.BundleState.Done);
        _targetProgress = (float)completedPlayers / states.Count;

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
        _progressSlider.value = Mathf.Lerp(_progressSlider.value, _targetProgress, _lerpSmoothing * Time.deltaTime);
    }

    public void Setup(string moonName)
    {
        _routingToText.text = $"Routing to: {moonName}";
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