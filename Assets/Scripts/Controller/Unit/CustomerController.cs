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

    private void HandleBuildingUpdateData(string buildingCodeName)
    {
        if (IsCompleteCommand(GetActiveRoutineCommand())) return;
        if (GameHelper.GetStringSplitSpaceRemoveLast(unitData.ActiveRoutineCommandData.CodeName) == buildingCodeName)
        {            
            HandleActioning();
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
            nextCommand = NextRoutineCommand(currentIndex);
            return nextCommand;
        }
        return nextCommand;
    }
    public override void HandleActioning()
    {
        CommandData activeCommandTemp = GetActiveRoutineCommand();
        AIProcessWithBuilding(GetMainActiveCommand());
        // Do something here for AI behaviour. Ex If building full item, waiting to add item for building, then enable next routine command.
        // TODO
        if (IsCompleteCommand(activeCommandTemp))
        {
            unitData.SetState(UnitState.Commanding);
        }
        else
        {
            unitData.SetState(UnitState.Waiting);
        }
    }
    public override void HandleGetItemFromBuilding(CommandData targetCommand, BuildingController buildingController)
    {
        RoutineConfig currentRoutineConfig = unitConfig.GetRoutineConfig(targetCommand.CodeName);
        if (currentRoutineConfig == null) return;
        GetItemFromBuilding(currentRoutineConfig.ItemCodeName,buildingController, currentRoutineConfig.Quantity);
    }
    public override bool IsCompleteCommand(CommandData commandData)
    {
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

}
