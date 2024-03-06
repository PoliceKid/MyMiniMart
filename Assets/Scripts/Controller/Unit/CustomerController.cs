using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerController :UnitController
{
    private int currentIndex;
    CommandData InitCommand;
    public override void Init(string codeName, UnitView unitView)
    {
        base.Init(codeName, unitView);
        currentIndex = 0;
        InitCommand = GetNextRoutineCommand(currentIndex);
        if (InitCommand != null)
        {
            SetActiveRoutineCommand(InitCommand);
        }
    
    }
    public override void StartCommand()
    {
        if(InitCommand != null)
        {
            NextCommand(InitCommand);
        }
    }
    public override bool SetTargetBuilding(ActionType type, BuildingController buildingController)
    {
        //if(unitData.LastJoinBuilding != null)
        //{
        //   unitData.LastJoinBuilding.BuildingData.OnUpdateQueueSlot -= HandleBuildingUpdateQueueSlot;
        //}
        bool setBuildingSuccess =  base.SetTargetBuilding(type, buildingController); 
        if (setBuildingSuccess)
        {
            buildingController.BuildingData.OnUpdateItem += HandleTargetBuildingUpdateData;
            //CommandData nextRoutineCommand = GetNextRoutineCommand(currentIndex + 1);
            // if (!unitData.IsCompleteRoutine)
            // {
            //    BuildingController nextTargetBuilding = GetTargetBuilding(nextRoutineCommand);
            //    if (nextTargetBuilding != null)
            //    {
            //        nextTargetBuilding.BuildingData.OnUpdateQueueSlot += HandleBuildingUpdateQueueSlot;
            //    }
            // }
            //else
            //{
            //    Debug.LogWarning("Unit has completed routine");
            //}
        }
       
        return setBuildingSuccess;
    }
    public override void CompleteActiveCommandAction(ActionType type, BuildingController lastJoinBuilding)
    {
        base.CompleteActiveCommandAction(type, lastJoinBuilding);
        if (lastJoinBuilding != null)
        {
            lastJoinBuilding.BuildingData.OnUpdateItem -= HandleTargetBuildingUpdateData;
            
        }

    }
    //private void HandleBuildingUpdateQueueSlot(string obj)
    //{
    //    if (unitData.IsState(UnitState.Commanding)) return;
    //    if (!IsCompleteCommand(GetActiveRoutineCommand())) return;
    //    HandleCommanding();
    //}
    private void HandleTargetBuildingUpdateData(string buildingCodeName)
    {
        if (unitData.IsState(UnitState.Actioning)) return;
        if (unitData.IsState(UnitState.Destinationing)) return;
        if (IsCompleteCommand(GetActiveRoutineCommand())) return;
        if (GameHelper.GetStringSplitSpaceRemoveLast(unitData.ActiveRoutineCommandData.CodeName) == buildingCodeName)
        {
            unitData.SetState(UnitState.Actioning); 
            //HandleActioning();
        }
    }
    public override CommandData GetNextCommand()
    {
        CommandData activeCommandTemp = GetActiveRoutineCommand();
        if (activeCommandTemp == null) return null;
        CommandData nextCommand = null;

        if (IsCompleteCommand(activeCommandTemp))
        {
            nextCommand = GetNextRoutineCommand(currentIndex+1);
            if (nextCommand == null) return null;
            BuildingController targetBuilding = GetTargetBuilding(nextCommand);
            if (!targetBuilding.CheckCanJoinQueueSlot(GameHelper.ConvertStringToEnum(nextCommand.ActionType))) return null;

            currentIndex++;
            currentIndex = Mathf.Clamp(currentIndex, 0, unitData.CommandBuildings.Count);
            SetActiveRoutineCommand(nextCommand);
            return nextCommand;
        }
        return nextCommand;
    }
    public override void HandleGetItemFromBuilding(CommandData targetCommand, BuildingController buildingController)
    {
        RoutineConfig currentRoutineConfig = unitConfig.GetRoutineConfig(targetCommand.CodeName);
        if (currentRoutineConfig == null) return;
        GetItemFromBuilding(currentRoutineConfig.ItemCodeName,buildingController, currentRoutineConfig.Quantity);
    }
    public override bool CheckCanCompleteCommand(CommandData commandData)
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
