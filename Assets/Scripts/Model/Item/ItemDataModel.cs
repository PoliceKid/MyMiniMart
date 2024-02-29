using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ItemDataModel : IOccupier
{
    public string Id;
    public string CodeName;

    public ItemDataModel( string codeName)
    {
        Guid newGuid = Guid.NewGuid();
        Id = newGuid.ToString();
        CodeName = codeName;
    }
}
