using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Dawn.Utils;
[DefaultExecutionOrder(-999)]
[AddComponentMenu($"{DawnConstants.AudioComponents}/Synced Weighted Outcomes")]
public class WeightedOutcomeScript : NetworkBehaviour
{
    [Serializable]
    public class Outcome
    {
        [Min(0f)]
        public float weight = 1f;

        [Tooltip("Invoked on all clients when this outcome is selected.")]
        public UnityEvent onSelected = new();

        [Tooltip("Invoked on all clients when this outcome is NOT selected (only if Disable Others is true).")]
        public UnityEvent onNotSelected = new();
    }

    [Header("Outcomes")]
    [SerializeField]
    private Outcome[] _outcomes = Array.Empty<Outcome>();

    [Header("Selection")]
    [Tooltip("Pick between Min and Max outcomes (inclusive).")]
    [SerializeField]
    private int _minSelect = 1;

    [SerializeField]
    private int _maxSelect = 1;

    [Tooltip("If false, an outcome can only be picked once per roll.")]
    [SerializeField]
    private bool _allowDuplicates = false;

    [Tooltip("If true, disable non-selected targets and invoke onNotSelected for them.")]
    [SerializeField]
    private bool _disableOthers = true;

    [Header("Roll Timing")]
    [Tooltip("If true, server rolls on OnNetworkSpawn().")]
    [SerializeField]
    private bool _rollOnSpawn = true;

    private NetworkList<int> _chosen;

    private void Awake()
    {
        _chosen = new NetworkList<int>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _chosen.OnListChanged += _ => ApplySelection();

        if (IsServer && _rollOnSpawn)
        {
            RollServer();
        }
        else
        {
            ApplySelection();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (_chosen != null)
        {
            _chosen.OnListChanged -= _ => ApplySelection();
        }

        base.OnNetworkDespawn();
    }

    public void Roll()
    {
        if (IsServer)
        {
            RollServer();
        }
        else
        {
            RequestRollServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestRollServerRpc()
    {
        RollServer();
    }

    private void RollServer()
    {
        if (_outcomes == null || _outcomes.Length == 0)
        {
            _chosen.Clear();
            return;
        }

        int min = Mathf.Clamp(_minSelect, 0, _outcomes.Length);
        int max = Mathf.Clamp(_maxSelect, min, _outcomes.Length);

        int picks = UnityEngine.Random.Range(min, max + 1);

        List<int> pool = new(_outcomes.Length);
        for (int i = 0; i < _outcomes.Length; i++)
        {
            if (_outcomes[i].weight > 0f)
            {
                pool.Add(i);
            }
        }

        _chosen.Clear();

        if (pool.Count == 0 || picks == 0)
        {
            return;
        }

        for (int p = 0; p < picks; p++)
        {
            int selectedIndex = PickWeightedIndex(pool);

            _chosen.Add(selectedIndex);

            if (!_allowDuplicates)
            {
                pool.Remove(selectedIndex);
            }

            if (pool.Count == 0)
            {
                break;
            }
        }

        ApplySelection();
    }

    private int PickWeightedIndex(List<int> indices)
    {
        float total = 0f;
        for (int i = 0; i < indices.Count; i++)
        {
            total += _outcomes[indices[i]].weight;
        }

        float roll = UnityEngine.Random.Range(0f, total);
        float accum = 0f;

        for (int i = 0; i < indices.Count; i++)
        {
            int idx = indices[i];
            accum += _outcomes[idx].weight;
            if (roll <= accum)
            {
                return idx;
            }
        }

        return indices[^1];
    }

    private void ApplySelection()
    {
        if (_outcomes == null)
        {
            return;
        }

        HashSet<int> selected = new();
        for (int i = 0; i < _chosen.Count; i++)
        {
            selected.Add(_chosen[i]);
        }

        for (int i = 0; i < _outcomes.Length; i++)
        {
            bool isSelected = selected.Contains(i);

            if (isSelected)
            {
                _outcomes[i].onSelected?.Invoke();
            }
            else if (_disableOthers)
            {
                _outcomes[i].onNotSelected?.Invoke();
            }
        }
    }
}