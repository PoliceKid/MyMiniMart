using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController :IOccupier
{
    public ItemDataModel itemData;
    public ItemView itemView;
    public ItemConfig ItemConfig { get; private set; }

    public void Init(string codeName, ItemView itemView)
    {
        itemData = new ItemDataModel(codeName);
        this.itemView = itemView;
    }

}
