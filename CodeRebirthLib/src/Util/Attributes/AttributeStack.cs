using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeRebirthLib.Util.Attributes;
[Serializable]
public class AttributeStack<T>
{
    [SerializeField] T _baseValue;
    public T BaseValue => _baseValue;

    List<Func<T, T>> _stack = new();

    bool _isDirty = true;
    T _lastCalculated;

    public AttributeStack() { }
    public AttributeStack(T baseValue)
    {
        _baseValue = baseValue;
    }

    public Func<T, T> Add(Func<T, T> transformer) {
        _stack.Add(transformer);
        _isDirty = true;
        return transformer;
    }

    public void Remove(Func<T, T> transformer) {
        _stack.Remove(transformer);
    }

    public void MarkDirty() {
        _isDirty = true;
    }

    public T Calculate(bool forceRecalculate = false) {
        if(!_isDirty && !forceRecalculate) return _lastCalculated;

        T value = _baseValue;
        foreach(Func<T, T> transformer in _stack) {
            value = transformer.Invoke(value);
        }

        _lastCalculated = value;
        _isDirty = false;
        return value;
    }
}