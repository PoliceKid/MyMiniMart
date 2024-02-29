using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[System.Serializable]
public class QueueSlotDataModel 
{
    public string Id { get; private set; }
    public string ActionName;
    public UnitController OccupierUnit { get; protected set; }
    public bool IsOccupied => OccupierUnit != null;

    public Transform Slotpoint { get => slotpoint; set => slotpoint = value; }
    [SerializeField]
    private Transform slotpoint;

    public QueueSlotDataModel()
    {
        Guid newGuid = Guid.NewGuid();
        Id = newGuid.ToString();
    }
    public QueueSlotDataModel CloneSlotDataModel()
    {
        QueueSlotDataModel newSlotDataModel = new QueueSlotDataModel()
        {
            Id = this.Id,
        };
        return newSlotDataModel;
    }
    public bool IsLocked { get; set; }

    public void SetOccupier(UnitController unit)
    {
        OccupierUnit = unit;
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
