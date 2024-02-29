using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[System.Serializable]
public class SlotDataModel 
{
    public string Id { get; private set; }
    public IOccupier Occupier { get; protected set; }
    public bool IsOccupied => Occupier != null;

    public Transform Slotpoint { get => slotpoint; set => slotpoint = value; }
    [SerializeField]
    private Transform slotpoint;

    public SlotDataModel()
    {
        Guid newGuid = Guid.NewGuid();
        Id = newGuid.ToString();
    }
    public bool IsLocked { get; set; }

    public void SetOccupier(IOccupier occupier)
    {
        Occupier = occupier;
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
