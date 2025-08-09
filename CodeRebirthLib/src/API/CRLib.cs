using System;

namespace CodeRebirthLib;
public static class CRLib
{
    public static void DefineItem(NamespacedKey<CRItemInfo> key, Item item, Action<ItemInfoBuilder> callback)
    {
        ItemInfoBuilder builder = new ItemInfoBuilder(key, item);
        callback(builder);
        LethalContent.Items.Register(builder.Build());
    }

    public static void DefineEnemy(NamespacedKey<CREnemyInfo> key, EnemyType enemy, Action<EnemyInfoBuilder> callback)
    {
        EnemyInfoBuilder builder = new EnemyInfoBuilder(key, enemy);
        callback(builder);
        LethalContent.Enemies.Register(builder.Build());
    }

    public static TerminalNodeBuilder DefineTerminalNode(string name)
    {
        return new TerminalNodeBuilder(name);
    }
}