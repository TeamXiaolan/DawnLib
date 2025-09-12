using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dawn;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace Dusk.Internal;

static class EntityReplacementRegistrationPatch
{
    internal static void Init()
    {
        using (new DetourContext(priority: int.MaxValue))
        {
            On.EnemyAI.Start += ReplaceEnemyEntity;
        }
    }

    private static void ReplaceEnemyEntity(On.EnemyAI.orig_Start orig, EnemyAI self)
    {
        List<DuskEnemyReplacementDefinition> enemyReplacements = DuskModContent.EntityReplacements.Values.Where(enemyReplacement => enemyReplacement is DuskEnemyReplacementDefinition enemyReplacementDefinition).Cast<DuskEnemyReplacementDefinition>().ToList();
        List<DuskEnemyReplacementDefinition> validEnemyReplacements = enemyReplacements.Where(enemyReplacement => enemyReplacement.Key == self.enemyType.GetDawnInfo().Key).ToList();
        if (validEnemyReplacements.Count <= 0)
        {
            orig(self);
            return;
        }

        DuskEnemyReplacementDefinition pickedEnemyReplacement = validEnemyReplacements[0]; // TODO
        List<(string, FieldInfo)> potentialAudioClipReplacements = self.enemyType.GetDawnInfo().FieldNameFieldInfoDict.Where(kvp => kvp.Value.FieldType == typeof(AudioClip)).Select(kvp => (kvp.Key, kvp.Value)).ToList();
        foreach (StringWithAudioClip fieldNameWithAudioClip in pickedEnemyReplacement.AudioClipToReplaceWithFieldNames)
        {
            foreach ((string fieldName, FieldInfo fieldInfo) potentialAudioClipReplacement in potentialAudioClipReplacements)
            {
                if (potentialAudioClipReplacement.fieldName != fieldNameWithAudioClip.FieldName)
                    continue;

                ReplaceAudioClip(self, fieldNameWithAudioClip.ReplacementAudioClip, potentialAudioClipReplacement.fieldInfo);
            }
        }
        orig(self);
    }

    internal static void ReplaceAudioClip(object @object, AudioClip clip, FieldInfo fieldWithAudioClip)
    {
        fieldWithAudioClip.SetValue(@object, clip);
    }
}