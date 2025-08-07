using System;
using System.Collections.Generic;

namespace CodeRebirthLib.ContentManagement.Enemies;
[Obsolete("Use LethalContent.Enemies instead")]
public static class VanillaEnemies
{
    public static IReadOnlyList<EnemyType> AllEnemyTypes => LethalContent.Enemies.All;

    public static EnemyType Flowerman => LethalContent.Enemies.Flowerman;
    public static EnemyType Centipede => LethalContent.Enemies.Centipede;
    public static EnemyType MouthDog => LethalContent.Enemies.MouthDog;
    public static EnemyType Crawler => LethalContent.Enemies.Crawler;
    public static EnemyType HoarderBug => LethalContent.Enemies.HoarderBug;
    public static EnemyType SandSpider => LethalContent.Enemies.SandSpider;
    public static EnemyType Blob => LethalContent.Enemies.Blob;
    public static EnemyType ForestGiant => LethalContent.Enemies.ForestGiant;
    public static EnemyType DressGirl => LethalContent.Enemies.DressGirl;
    public static EnemyType SpringMan => LethalContent.Enemies.SpringMan;
    public static EnemyType SandWorm => LethalContent.Enemies.SandWorm;
    public static EnemyType Jester => LethalContent.Enemies.Jester;
    public static EnemyType Puffer => LethalContent.Enemies.Puffer;
    public static EnemyType Doublewing => LethalContent.Enemies.Doublewing;
    public static EnemyType DocileLocustBees => LethalContent.Enemies.DocileLocustBees;
    public static EnemyType RedLocustBees => LethalContent.Enemies.RedLocustBees;
    public static EnemyType BaboonHawk => LethalContent.Enemies.BaboonHawk;
    public static EnemyType Nutcracker => LethalContent.Enemies.Nutcracker;
    public static EnemyType MaskedPlayerEnemy => LethalContent.Enemies.MaskedPlayerEnemy;
    public static EnemyType RadMech => LethalContent.Enemies.RadMech;
    public static EnemyType Butler => LethalContent.Enemies.Butler;
    public static EnemyType ButlerBees => LethalContent.Enemies.ButlerBees;
    public static EnemyType FlowerSnake => LethalContent.Enemies.FlowerSnake;
    public static EnemyType BushWolf => LethalContent.Enemies.BushWolf;
    public static EnemyType ClaySurgeon => LethalContent.Enemies.ClaySurgeon;
    public static EnemyType CaveDweller => LethalContent.Enemies.CaveDweller;
    public static EnemyType GiantKiwi => LethalContent.Enemies.GiantKiwi;
}