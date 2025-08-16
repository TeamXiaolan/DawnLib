using UnityEngine;

namespace CodeRebirthLib.CRMod;
public class NamespacedKeyNameAttribute(string key) : PropertyAttribute
{
    public string Key = key;
}