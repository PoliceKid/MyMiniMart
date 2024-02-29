using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[System.Serializable]
public class ItemSlotDataModel
{
    public string Id { get;private set; }
    public ItemController OccupierItem { get; protected set; }
    public bool IsOccupied => OccupierItem != null;

    public Transform Slotpoint { get => slotpoint; set => slotpoint = value; }
    [SerializeField]
    private Transform slotpoint;

    public ItemSlotDataModel()
    {
        Guid newGuid = Guid.NewGuid();
        Id = newGuid.ToString();
    }
    public ItemSlotDataModel CloneSlotDataModel()
    {
        ItemSlotDataModel newSlotDataModel = new ItemSlotDataModel()
        {
            Id = this.Id,
        };
        return newSlotDataModel;
    }
    public bool IsLocked { get; set; }

    public void SetOccupier(ItemController item)
    {
        OccupierItem = item;
    }
    public virtual void ResetOccupier()
    {
        SetOccupier(null);
    }

    public virtual void ToogleLock(bool isLock)
    {
        IsLocked = isLock;
    }
}

