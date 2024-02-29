using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CashierDeskController : BuildingController
{
    public QueueSlotDataModel SlotCustomer { get; private set; }
    public UnitController ActiveUnitProcess { get; private set; } = null;

    public override void Init(BuildingInitData buildingInitData, BuildingView buildingView)
    {
        base.Init(buildingInitData, buildingView);
        SlotCustomer = BuildingData.QueueSlots[ActionType.Input.ToString()].First();
    }
    public override QueueSlotDataModel AddUnitToQueueSlot(ActionType type, UnitController unit)
    {
        QueueSlotDataModel queueSlotFree = base.AddUnitToQueueSlot(type, unit);
        if (!SlotCustomer.IsOccupied)
        {
            SlotCustomer.SetOccupier(unit);
        }
        return queueSlotFree;
    }
    public override bool CheckUnitCanProcessItem(string Id)
    {
        if (!SlotCustomer.IsOccupied) return false;
        return SlotCustomer.OccupierUnit.unitData.Id == Id;
    }
    public override void HandleCashierDeskProcessing()
    {
       // Add condition have unit standing ong cashier slot



    }
    public override void Processing()
    {
        base.Processing();
    }
}
