
using UnityEngine;

public class ItemBag
{
    private readonly ItemDatabase db;
    public ItemBag(ItemDatabase database){ db = database; }
    public int Next() => db.RandomId();
}
