using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerController :UnitController
{
    private int currentIndex;
    public override void Init(string codeName, UnitView unitView)
    {
        base.Init(codeName, unitView);
        currentIndex = 0;
        CommandData InitCommand = GetNextRoutineCommand(currentIndex);
        if (InitCommand != null)
        {
            SetActiveRoutineCommand(InitCommand);
        }
        RegisterEvent();
    }
    public void RegisterEvent()
    {
        foreach (var item in unitData.MainRoutineCommandData.Commands)
        {
            BuildingController targetBuilding = GetTargetBuilding(item);
            if(targetBuilding != null)
            {
                targetBuilding.BuildingData.OnUpdateItem += HandleBuildingUpdateData;
            }
        }
    }
    public static void RegisterEventWithCashierDesk(CustomerController customerController ,CashierDeskController cashierDeskController)
    {
        cashierDeskController.OnUpdateUnitFromSlotCustomer += customerController.HandleCashierDeskUpdateSlotCustomer;
    }
    public static void UnRegisterEventWithCashierDesk(CustomerController customerController, CashierDeskController cashierDeskController)
    {
        cashierDeskController.OnUpdateUnitFromSlotCustomer -= customerController.HandleCashierDeskUpdateSlotCustomer;
    }
    private void HandleCashierDeskUpdateSlotCustomer(string Id)
    {
        
    }

    private void HandleBuildingUpdateData(string buildingCodeName)
    {
        if (unitData.IsState(UnitState.Actioning)) return;
        if (IsCompleteCommand(GetActiveRoutineCommand())) return;
        if (GameHelper.GetStringSplitSpaceRemoveLast(unitData.ActiveRoutineCommandData.CodeName) == buildingCodeName)
        {
            unitData.SetState(UnitState.Actioning);
        }
    }
    public override CommandData GetNextCommand()
    {
        CommandData activeCommandTemp = GetActiveRoutineCommand();
        if (activeCommandTemp == null) return null;
        CommandData nextCommand = activeCommandTemp;
        if (IsCompleteCommand(activeCommandTemp))
        {
            currentIndex++;
            currentIndex = Mathf.Clamp(currentIndex, 0, unitData.CommandBuildings.Count);
            activeCommandTemp.CompleteCommand(true);
            nextCommand = NextRoutineCommand(currentIndex);
            return nextCommand;
        }
        return nextCommand;
    }
    public override void HandleActioning()
    {       
        CommandData activeCommand = GetActiveRoutineCommand();
        if (activeCommand == null) return;    
        BuildingController targetBuilding = GetTargetBuilding(activeCommand);
        if (targetBuilding == null) return;
        if (!targetBuilding.CheckUnitCanProcessItem(unitData.Id))
        {
            unitData.SetState(UnitState.Waiting);
            return;
        }
        AIProcessWithBuilding(activeCommand);
        if (IsCompleteCommand(activeCommand))
        {
            activeCommand.CompleteCommand(true);
            if (unitData.IsCompleteRoutine)
            {
                FinishRoutine(UnitManager.Instance.GetExitPoint().position);
                return;
            }
            unitData.SetState(UnitState.Commanding);
        }
        else
        {
            unitData.SetState(UnitState.Waiting);
        }
    }
    public override void HandleDestinationing()
    {
        if (CheckDestinationReached())
        {
            unitView.ChangeState("Idle_Happy_Carry");
            unitData.SetState(UnitState.Actioning);
        }
    }
    public override void HandleCommanding()
    {
        if (unitData.IsCompleteRoutine)
        {
            unitData.SetState(UnitState.None);
        }
        base.HandleCommanding();
    }
    public override void HandleGetItemFromBuilding(CommandData targetCommand, BuildingController buildingController)
    {
        RoutineConfig currentRoutineConfig = unitConfig.GetRoutineConfig(targetCommand.CodeName);
        if (currentRoutineConfig == null) return;
        GetItemFromBuilding(currentRoutineConfig.ItemCodeName,buildingController, currentRoutineConfig.Quantity);
    }
    public override bool IsCompleteCommand(CommandData commandData)
    {
        if (IsCompleteCashierDeskCommand(commandData)) return true;
        BuildingController targetBuilding = GetTargetBuilding(commandData);

        if (targetBuilding == null) return false;
        var ItemOutputs = targetBuilding.GetItemSlots(ItemSlotType.Output);// L?y ???c ra list item config Input, 
        RoutineConfig currentRoutineConfig = unitConfig.GetRoutineConfig(commandData.CodeName);
        if (currentRoutineConfig == null) return false;

        if (ItemOutputs != null)
        {
            foreach (var item in ItemOutputs.Keys)
            {
                int itemCount = unitData.GetItemCount(item);
                if (itemCount >= currentRoutineConfig.Quantity)
                {
                    return true;
                }

            }
        }
        return false;
    }
    public static void ActioningWithCashierDesk(CustomerController customerController ,QueueSlotDataModel queueSlotFree)
    {
        if (queueSlotFree == null) return;
        customerController.unitView.SetTargetMovePosition(queueSlotFree.Slotpoint.position, customerController.GetAnimationType());
        customerController.StartGotoDestination();
    }
    public virtual bool IsCompleteCashierDeskCommand(CommandData commandData)
    {
        if (!commandData.CodeName.Contains("Cashier")) return false;
        BuildingController targetBuilding = GetTargetBuilding(commandData);
        if (targetBuilding == null) return false;
        return targetBuilding.CheckUnitCanProcessItem(this.unitData.Id);
    }
    public override void FinishRoutine(Vector3 exitPoint)
    {
        unitView.SetTargetMovePosition(exitPoint, GetAnimationType());
        StartGotoDestination();
    }
}
