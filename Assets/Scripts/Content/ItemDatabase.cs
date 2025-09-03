
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemDefinition> items;
    public ItemDefinition GetById(int id) => items.Find(i => i.id == id);
    public int RandomId() => items[Random.Range(0, items.Count)].id;
}
