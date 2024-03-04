using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
public class CashierDeskController : BuildingController
{
    public QueueSlotDataModel SlotCustomer { get; private set; }
    public UnitController ActiveUnitProcess { get; private set; } = null;

    public Action<string> OnUpdateUnitFromSlotCustomer = delegate { };
    public static Action<UnitController> OnUnitFinishRoutine = delegate { };
    public override void Init(BuildingInitData buildingInitData, BuildingView buildingView)
    {
        base.Init(buildingInitData, buildingView);
        InitSlotCustomer();
    }
    private void InitSlotCustomer()
    {
        QueueSlotDataModel FirstQueueSlot = BuildingData.QueueSlots[ActionType.Input.ToString()].First();
        SlotCustomer = FirstQueueSlot;
        BuildingData.QueueSlots[ActionType.Input.ToString()].Remove(FirstQueueSlot);
    }
    public override QueueSlotDataModel AddUnitToQueueSlot(ActionType type, UnitController unit)
    {     
        if (!SlotCustomer.IsOccupied)
        {
            BuildingData.AddUnitToQueueSlot(type, unit, SlotCustomer);
            CustomerController.ActioningWithCashierDesk((CustomerController)unit, SlotCustomer);
            return SlotCustomer;
        }
        else
        {
            return base.AddUnitToQueueSlot(type, unit);
        } 
    }
    public override UnitController RemoveUnitAwayQueueSlot(ActionType type, string Id)
    {       
        if (CheckSlotCustomerHasUnit(Id))
        {
            UnitController unitRemoveSlotCustomer = SlotCustomer.OccupierUnit;
            BuildingData.RemoveUnitAwayQueueSlot(type, unitRemoveSlotCustomer, SlotCustomer);
            //OnUnitFinishRoutine?.Invoke(unitRemoveSlotCustomer);
            RerangeUnitInQueue(type);
       
            return unitRemoveSlotCustomer;
        }
        return base.RemoveUnitAwayQueueSlot(type, Id);
     
    }
    public void RerangeUnitInQueue(ActionType type)
    {
        if (BuildingData.GetTotalUnitJoinQueue(ActionType.Input) == 0) return;
        List<UnitController> listUnitInQueue = BuildingData.GetListUnitJoinQueue(ActionType.Input);
    
        List<QueueSlotDataModel> listQueueSlotHasUnit = BuildingData.QueueSlots[ActionType.Input.ToString()];
        foreach (var item in listQueueSlotHasUnit)
        {
            item.ResetOccupier();
        }
        foreach (var unit in listUnitInQueue)
        {
            AddUnitToQueueSlot(type, unit);
        }
    }
    public UnitController GetNextUnitAddToSlotCustomer()
    {
        if (BuildingData.GetTotalUnitJoinQueue(ActionType.Input) == 0) return null;
        return  BuildingData.GetListUnitJoinQueue(ActionType.Input).First();
    }
    public override bool CheckUnitCanProcessItem(string Id)
    {
        return CheckSlotCustomerHasUnit(Id);
    }
    public virtual bool CheckSlotCustomerHasUnit(string Id)
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
