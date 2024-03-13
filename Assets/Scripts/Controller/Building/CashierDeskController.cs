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
        if (!SlotCustomer.IsOccupied && type == ActionType.Input)
        {
            BuildingData.AddUnitToQueueSlot(type, unit, SlotCustomer);
            if(BuildingData.GetUnitWithId(unit.unitData.Id) != null)
            {
                CustomerController.GotoCustomerSlot((CustomerController)unit, SlotCustomer);
            }
           
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
        return CheckSlotCustomerHasUnit(Id) ;
    
    }
    public virtual bool CheckSlotCustomerHasUnit(string Id)
    {
        if (!SlotCustomer.IsOccupied) return false;
        return SlotCustomer.OccupierUnit.unitData.Id == Id;
    }
    public override void HandleCashierDeskProcessing()
    {
        // Add condition have unit standing ong cashier slot
        if (!BuildingData.CheckContaintUnitWithActionType(ActionType.Process)) return;
        if (!CheckAvaliableSlotItemForProcessing()) return;
        var InputSlotItems = GetItemSlots(ItemSlotType.Input);
        foreach (var InputSlotItem in InputSlotItems)
        {
            var slotItemOccupied = BuildingData.GetListOccupiedSlotItems(InputSlotItem.Value);
            foreach (var slotItem in slotItemOccupied)
            {
                var item = slotItem.OccupierItem;
                if (item != null)
                {
                    RemoveItemAwaySlot(ItemSlotType.Input, slotItem.OccupierItem.itemData.Id, out ItemController ItemRemove);
                    if (ItemRemove != null)
                    {
                        BuildingView.MoveItemToModelProcess(item);
                    }

                }
                var outputItemConfigs = GetItemSlots(ItemSlotType.Output).Keys;
                if (outputItemConfigs != null)
                {
                    foreach (var outputItem in outputItemConfigs)
                    {
                        if (CheckFreeSlotToAddItem(ItemSlotType.Output, outputItem))
                        {
                            //Create item
                            var itemView = SpawnerManager.CreateItemView(outputItem, Vector3.zero, BuildingView.transform);
                            if (itemView != null)
                            {
                                AddItemToSlot(ItemSlotType.Output, itemView.ItemController, out ItemSlotDataModel slotAddItem);

                                //Call View
                                if (slotAddItem != null)
                                    BuildingView.AddItemToOutput( itemView, slotAddItem);

                            }


                        }
                    }
                

                }
            }
        }
    }
    

}
