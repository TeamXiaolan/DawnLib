using System;
using Dawn.Interfaces;
using DunGen;

namespace Dawn;

public static class DungeonArchetypeExtensions
{
    extension(DungeonArchetype archetype)
    {
        public DawnArchetypeInfo DawnInfo
        {
            get => archetype.GetDawnInfoCore();
            set => archetype.SetDawnInfoCore(value);
        }

        [Obsolete("Use DungeonArchetype.DawnInfo instead")]
        public DawnArchetypeInfo GetDawnInfo()
        {
            return archetype.GetDawnInfoCore();
        }

        [Obsolete("Use DungeonArchetype.DawnInfo instead")]
        public void SetDawnInfo(DawnArchetypeInfo archetypeInfo)
        {
            archetype.SetDawnInfoCore(archetypeInfo);
        }

        private DawnArchetypeInfo GetDawnInfoCore()
        {
            object newObject = archetype;
            return ((IDunGenArchetypeDawnObject)newObject).DawnInfo;
        }

        private void SetDawnInfoCore(DawnArchetypeInfo archetypeInfo)
        {
            object newObject = archetype;
            ((IDunGenArchetypeDawnObject)newObject).DawnInfo = archetypeInfo;
        }
    }
}
