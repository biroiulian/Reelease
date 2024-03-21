using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Item - any object that is used for the gameplay. ex: Consumable, Placeable(plants, trees etc)
/// </summary>
public class ResourceDictionary : MonoBehaviour
{
    public ItemResource[] Items;
    public ChallengeResource[] Challenges;
    public Dictionary<string, ItemResource> ItemDictionary;
    public Dictionary<int, Sprite> DurationSpriteDictionary;

    private void Awake()
    {
        ItemDictionary = new Dictionary<string, ItemResource>();
        foreach (var item in Items)
        {
            ItemDictionary.Add(item.itemType.ToString(), item);
        }

        // Quick validation that we did not repeat ourselves.
        Debug.Assert(ItemDictionary.Count == Items.Length);

        //
        DurationSpriteDictionary = new Dictionary<int, Sprite>();
        foreach (var ch in Challenges)
        {
            DurationSpriteDictionary.Add(ch.duration, ch.sprite);
        }
        // Quick validation that we did not repeat ourselves.
        Debug.Assert(DurationSpriteDictionary.Count == Challenges.Length);
    }

    public ItemResource GetItemResource(string itemType)
    {
        return ItemDictionary.GetValueOrDefault(itemType);
    }

    internal Sprite GetIconFor(ChallengeItem challenge)
    {
        return DurationSpriteDictionary.GetValueOrDefault(challenge.duration);
    }
}

public enum ItemType
{
    BirchTreeModel1,
    BirchTreeModel2,
    BirchTreeModel3,
    BirchTreeModel4,
    CommonTreeModel1,
    CommonTreeModel2,
    CommonTreeModel3,
    CommonTreeModel4,
    PineTreeModel1,
    PineTreeModel2,
    PineTreeModel3,
    PineTreeModel4,
    WillowTreeModel1,
    WillowTreeModel2,
    WillowTreeModel3,
    WillowTreeModel4,
    Bush,
    FruitsBush,
    WheatPlant,
    PlantModel1,
    PlantModel2,
    PlantModel3,
    RockModel1,
    RockModel2,
    RockModel3,
    Fox,
    Horse,
    Sheep,
    Pug,
    Llama,
    StreakRestore,
    WeatherCleaner,
    CheatDay,
    // To be added
}

public enum IconType
{
    Tree,
    Bush,
    Plant,
    Rock,
    Animal,
    PlaceHolder
}

[Serializable]
public struct ItemResource
{
    public ItemGeneralType itemGeneralType;
    public ItemType itemType;
    public Sprite sprite;
    public int price;
    public GameObject placeablePrefab;
}

public enum ItemGeneralType
{
    Placeable,
    Consumable
}

[Serializable]
public struct ChallengeResource
{
    public int duration;
    public Sprite sprite;
}
