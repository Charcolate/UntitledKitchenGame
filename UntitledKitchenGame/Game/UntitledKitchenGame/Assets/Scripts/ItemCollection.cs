using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemCollection.Asset", menuName = "Data/Item Collection")]
public class ItemCollection : ScriptableObject
{
    public List<FoodItem> Items;

    //I think I want boolean values of the type to add more customization but rn it works
    public type onlySelect = new type();
    //maybe we could have the variables change as well depending on what kind of object it is?
    public enum type
    {
        Unlisted,
        Meat,
        Grain,
        Veggie,
        Fish
    }
    //how can I get this item to automatically collect the items from the files.
    //[ContextMenu("Select Item")]
    ////could feed it a colleciton of an item data base but right now let us randomize it.
    //public FoodItem SelectRandomItem()
    //{
    //    //not looking for an index looking for the rarity.
    //    int randomItem = Random.Range(0, 101);
    //    Debug.Log(randomItem);
    //    foreach (FoodItem item in Items)
    //    {
    //        //seem to be having an issue with what item is selected.
    //        //seems to grab the small items first? question about how it operates is required for the veggies as it seemed to miss onions and lettuce.
    //        if (randomItem <= item.rarity)
    //        {
    //            if (item.type.ToString() == onlySelect.ToString())
    //            {
    //                //Debug.Log(item.name);
    //                return item;

    //            }else if (onlySelect == type.Unlisted)
    //            {
    //                //Debug.Log(item.name);
    //                return item;
    //            }
    //        }
    //    }
    //    return null;
    //}
}
