using System;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using loaforcsSoundAPI;
using loaforcsSoundAPI.SoundPacks.Data.Conditions;

namespace Dawn.Internal;
static class SoundAPICompat
{
    public static bool Enabled => Chainloader.PluginInfos.ContainsKey(SoundAPI.PLUGIN_GUID);

    class DawnTaggableCondition(Func<ITaggable?> generator) : Condition
    {
        public string Value { get; private set; }

        private NamespacedKey? _key;
        
        public override bool Evaluate(IContext context)
        {
            _key ??= NamespacedKey.ForceParse(Value);
            
            ITaggable? taggable = generator();
            return taggable != null && taggable.HasTag(_key);
        }
    }
    
    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    internal static void Init()
    {
        SoundAPI.RegisterCondition("DawnLib:moon:has_tag", () => new DawnTaggableCondition(() =>
        {
            if (!StartOfRound.Instance) return null;
            return StartOfRound.Instance.currentLevel.GetDawnInfo();
        }));
        
        SoundAPI.RegisterCondition("DawnLib:dungeon:has_tag", () => new DawnTaggableCondition(() =>
        {
            if (!RoundManager.Instance) return null;
            if (!RoundManager.Instance.dungeonGenerator) return null;
            return RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow.GetDawnInfo();
        }));
    }
}