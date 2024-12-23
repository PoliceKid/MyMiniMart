﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class UnitController : IOccupier
{
    public UnitView unitView;
    public UnitDataModel unitData { get; private set; }
    public UnitConfig unitConfig { get; private set; }

    #region Delegate
    public System.Func<bool> CheckDestinationReached;
    public System.Func<CommandData, BuildingController> GetTargetBuilding;
    #endregion
    #region INIT FUNCTION
    public virtual void Init(string codeName,UnitView unitView)
    {        
        this.unitView = unitView;
        LoadConfig(codeName);

        unitData = new UnitDataModel(codeName, (UnitType)unitConfig.Type, unitConfig.BaseMoveSpeed);
        string hatCodename = SpawnerManager.instance.GetRandomHatString();
        SetHatCodeName(hatCodename);

        GetTargetBuilding = BuildingManager.Instance.GetTargetBuilding;
        InitRoutineCommand(unitConfig);

    }
    public void SetHatCodeName(string hatCodeName)
    {
        if (hatCodeName == null) return;
        unitData.SetHatCodeName(hatCodeName);
    }
    public void SetSpeed(float speed)
    {
        unitData.SetSpeed(speed);
        unitView.UnitMovement.SetSpeed(speed);
    }
    private void LoadConfig(string codeName)
    {
        unitConfig = ConfigManager.Instance.GetUnitConfig(codeName);
    }
    private void InitRoutineCommand(UnitConfig unitConfig)
    {
        if (unitConfig == null) return;
        CommandBuilding MainRoutineCommand = new CommandBuilding();
        SetRoutineCommandChain(MainRoutineCommand);
        foreach (var command in unitConfig.Routine)
        {
            unitData.AddCommandBuilding(command.CodeName);
            var listSupportCommandBuilding= BuildingManager.Instance.GetListSupportCommandBuilding(command.CodeName);
            if(listSupportCommandBuilding != null)
            {
                foreach (var item in listSupportCommandBuilding)
                {
                    unitData.AddCommandBuilding(command.CodeName, item);
                }
            } 
        }        
        unitData.SetLoopInfinite(unitConfig.IsLoopInfinite);
        CommandData InitCommand = GetNextRoutineCommand();
        if (InitCommand != null)
        {
            SetActiveRoutineCommand(InitCommand);
            //unitData.SetMainActiveCommand(InitCommand);
        }

    }
    #endregion
    #region ITEM FUNCTION
    public void CreateSlotItemCarry(Transform point)
    {
        ItemSlotDataModel slotCarry = new ItemSlotDataModel()
        {
            Slotpoint = point
        };
        unitData.AddItemSlotCarry(slotCarry);
    }
    public bool CheckCanAddItemToSlotCarry()
    {
        return unitData.CheckCanAddItemToSlotCarry();
    }
    public virtual int GetItemFromBuilding(string ItemCodeName,BuildingController buildingController, int quantity =0)
    {
        int itemCount = 0;
        // first Get item in routine for getting
        if (!CheckCanAddItemToSlotCarry()) return 0;
        List<ItemSlotDataModel> listOccupiedSlotItems =  buildingController.GetListOccupiedSlotItems(ItemSlotType.Output, ItemCodeName);
        if (listOccupiedSlotItems == null) return 0;
        foreach (var item in listOccupiedSlotItems)
        {
            if (unitData.GetFreeSlotCarry() == null) return 0;
            if(quantity > 0)
            {
                if (itemCount >= quantity) return itemCount;
            }           
            buildingController.RemoveItemAwaySlot(ItemSlotType.Output, item.OccupierItem.itemData.Id, out ItemController itemRemove);
            if(itemRemove != null)
            {              
                AddItemToSlotCarry(itemRemove,0);        
                itemCount++;                         
            }
          
        }
        return itemCount;
    }
   
    public virtual void GetAllItemFromBuilding( BuildingController buildingController, int quantity =0)
    {
        int itemCount = 0;
        foreach (var item in buildingController.GetItemSlots(ItemSlotType.Output).Keys)
        {
            if (quantity > 0)
            {
              if (itemCount > quantity) return;
            }
            itemCount += GetItemFromBuilding(item, buildingController, quantity);
        }
         
    }

    public virtual bool AddItemToBuilding(BuildingController buildingController)
    {
        if (!buildingController.CheckUnitCanProcessItem(unitData.Id)) return false;
        List<ItemSlotDataModel> GetlistSlotItemCarryOccupied = unitData.GetlistSlotItemCarryOccupied;
        if (GetlistSlotItemCarryOccupied.Count == 0) return false;

        foreach (var slotItem in GetlistSlotItemCarryOccupied)
        {
            ItemController item = slotItem.OccupierItem;
            if (!buildingController.CheckFreeSlotToAddItem(ItemSlotType.Input, item.itemData.CodeName)) continue;

            buildingController.AddItemToSlot(ItemSlotType.Input, item,out ItemSlotDataModel slotAddItem, false);
            if(slotAddItem != null)
            {
                unitView.AddItem(item.itemView, slotAddItem.Slotpoint, buildingController.BuildingView.transform);
                slotItem.ResetOccupier();
            }
                   
        }
        return false;
    }

    public virtual void AddItemToSlotCarry(ItemController item, float delayTime)
    {
        ItemSlotDataModel freeSlotForItem = unitData.AddItemToSlotCarry(item);
        if (freeSlotForItem != null)
            TimerHelper.instance.StartTimer(delayTime, () =>
             {
                 unitView.GetItem(item.itemView, freeSlotForItem.Slotpoint);
             }
             );
            
    }
    public virtual void RemoveItemFromSlotCarry(ItemController item)
    {
        unitData.RemoveItemAwaySlotCarry(item);
    }
    #endregion
    #region ACTION FUNCTION

    public void SetRoutineCommandChain(CommandBuilding commandChainData)
    {
        unitData.SetRoutineCommandChainData(commandChainData);
    }
    public void StartActiveCommand(CommandData activeCommand)
    {
        if (activeCommand == null) return;
        BuildingController building = GetTargetBuilding(activeCommand);
        if (building == null) return;

        ActionType enumValue =GameHelper.ConvertStringToEnum(activeCommand.ActionType);
        SetTargetBuilding(enumValue, building);
    }
    //Next Command
    public virtual void StartCommand()
    {
        CommandData nextCommand = GetNextCommand();
        if (nextCommand != null)
        {
            NextCommand(nextCommand);
        }
        else
        {
            unitData.SetState(UnitState.Commanding);
        }
    }
    public virtual void NextCommand(CommandData nextCommand = null)
    {
        if (unitData.IsCompleteRoutine ) return;
        if (nextCommand != null)
        {
            unitData.SetMainActiveCommand(nextCommand);
            StartActiveCommand(nextCommand);         
        }        
    }
    public virtual CommandData GetNextCommand()
    {
        CommandData nextCommand = null;
        CommandData activeCommandTemp = GetActiveRoutineCommand();
        // If Compelete list support command -> move to active routine command
        // If Compelete Routine command -> Next routine command -> Next support command
        if (IsCompleteCommand(activeCommandTemp))
        {
            //Complete Active routine command and all support Command before next Routine command 
            GetActiveRoutineCommand().CompleteCommand(false);
            unitData.CompleteAllSupportCommand(activeCommandTemp);

            CommandData nextRoutineCommand = NextRoutineCommand();
            if(nextRoutineCommand != null)
            nextCommand = NextSupportCommand(nextRoutineCommand);
            // Active Routine command complete == true when unit complete list support routine and move to active routine command
        }
        else
        {
            if (unitData.IsCompleteSupportCommand(activeCommandTemp) || unitData.IsFullItemCarry)
            {
                nextCommand = activeCommandTemp;
            }
            else
            {              
                nextCommand = NextSupportCommand(activeCommandTemp);                       
            }
        }
        if(nextCommand != null)
        {
            BuildingController targetBuilding = GetTargetBuilding(nextCommand);
            if (!targetBuilding.CheckCanJoinQueueSlot(GameHelper.ConvertStringToEnum(nextCommand.ActionType))) return null;
        }
        return nextCommand;
    }
    public virtual CommandData GetActiveRoutineCommand()
    {
        return unitData.ActiveRoutineCommandData;
    }
    public virtual CommandData GetMainActiveCommand()
    {
        return unitData.MainActiveCommand;
    }
    public void SetActiveRoutineCommand(CommandData commandData)
    {
        unitData.SetActiveRoutineCommandData(commandData);
    }
    public virtual CommandData GetNextRoutineCommand(int index = -1)
    {
        List<CommandData> listRoutineCommand = unitData.MainRoutineCommandData.Commands;
        if (listRoutineCommand.Count == 0) return null;
        if (index != -1)
        {
           
            index = Math.Clamp(index, 0, listRoutineCommand.Count - 1);
            return listRoutineCommand[index];
        }
        else
        {
            listRoutineCommand = listRoutineCommand.Where(x => GetTargetBuilding(x) != null).ToList();
            listRoutineCommand = listRoutineCommand
                .OrderByDescending(y => {
                    var targetBuilding = GetTargetBuilding(GetNextSupportCommand(y.CodeName));
                    return targetBuilding != null ?
                        targetBuilding.BuildingData.GetTotalCountItem(ItemSlotType.Output) : 0; // support building have output product nhiều nhất
                })
                .OrderBy(x => {
                    var targetBuilding = GetTargetBuilding(x);
                    return targetBuilding != null ?
                        targetBuilding.BuildingData.GetTotalCountItem(ItemSlotType.Input) : 0; // Hoặc giá trị mặc định khác tùy thuộc vào logic của bạn
                }).ToList();
            return listRoutineCommand.Count > 0 ? listRoutineCommand.First() : null;
        }
        
    }
    public virtual CommandData GetNextSupportCommand(string commandCodeName)
    {
        if (!unitData.CommandBuildings.ContainsKey(commandCodeName)) return null;
        List<CommandData> listCommandSupport = unitData.GetListSupportCommandbuilding(commandCodeName).Commands;
        if (listCommandSupport != null)
        {
            // Lọc ra list command khác null
            listCommandSupport = listCommandSupport.Where(x => GetTargetBuilding(x) != null && GetTargetBuilding(x).BuildingData.GetTotalCountItem(ItemSlotType.Output) >0).ToList();
            // sort support building với output item count nhiều nhất để đi lấy
            listCommandSupport = listCommandSupport.OrderByDescending(x => {
                var targetBuilding = GetTargetBuilding(x);
                return targetBuilding != null ?
                    targetBuilding.BuildingData.GetTotalCountItem(ItemSlotType.Output) : 0; // Hoặc giá trị mặc định khác tùy thuộc vào logic của bạn
            }).
                ThenBy(y => GetTargetBuilding(y).BuildingData.GetTotalUnitJoinQueue(ActionType.Output)).ThenByDescending(z => !z.IsComplete).ToList();
            return listCommandSupport.Count > 0 ? listCommandSupport.First() : null;
        }
        return null;
    }
    public virtual CommandData NextSupportCommand(CommandData routineActiveCommand)
    {
        if (routineActiveCommand == null) return null;
        
        CommandData supportCommandPriority = GetNextSupportCommand(routineActiveCommand.CodeName);
        if (supportCommandPriority != null)
        {
            unitData.CompleteSupportCommand(true);
            unitData.SetSupportedActiveCommanData(supportCommandPriority);
            return supportCommandPriority;
        }
        return null;
    }
    public virtual CommandData NextRoutineCommand(int index =-1)
    {
        // get next Routine command       
        CommandData nextRoutineCommand = GetNextRoutineCommand(index);
        if (nextRoutineCommand == null) return null;
        // Set active routine command  = next routine command
        unitData.SetActiveRoutineCommandData(nextRoutineCommand);

        // return active command. 
        return nextRoutineCommand;
    }
    public virtual bool IsCompleteCommand(CommandData commandData)
    {
        return commandData.IsComplete;
    }
    public virtual bool SetTargetBuilding( ActionType type, BuildingController buildingController)
    {
        //Call API
        if (buildingController == null) return false;
        if (!buildingController.CheckCanJoinQueueSlot(type)) return false;
        
        unitData.SetTargetBuilding(buildingController);
        QueueSlotDataModel queueSlotFree = buildingController.AddUnitToQueueSlot(type, this);
        //Call View
        unitView.SetTargetMovePosition(queueSlotFree.Slotpoint.position, GetAnimationType());
        StartGotoDestination();
        return true;
    }
    public virtual void CompleteActiveCommandAction(ActionType type, BuildingController lastJoinBuilding)
    {
        // Call API
        if (lastJoinBuilding != null)
        {
            lastJoinBuilding.RemoveUnitAwayQueueSlot(type, unitData.Id);
            
        }
    }
    public void Tick(float deltaTime)
    {
        switch (unitData.CurrentState)
        {
            case UnitState.None:
                break;
            case UnitState.Destinationing:                
                HandleDestinationing();                
                break;
            case UnitState.Commanding:
                HandleCommanding();
                break;
            case UnitState.Queueing:
                break;
            case UnitState.Waiting:
                HandleWaiting();
                break;
            case UnitState.Actioning:
                // Test only come to next action
                HandleActioning();
                break;
            default:
                break;
        }
    }
    public void StartGotoDestination()
    {       
        unitData.SetState(UnitState.Destinationing);
    }
    public virtual void HandleDestinationing()
    {
        if (CheckDestinationReached())
        {
            unitData.SetState(UnitState.Waiting);
            TimerHelper.instance.StartTimer(0.3f, () =>
            {
               
                unitData.SetState(UnitState.Actioning);
            });
          
        }
    }
    public virtual void HandleWaiting()
    {
        unitView.ChangeState("Idle_Happy_Carry");
        unitData.SetState(UnitState.None);
    }
    public virtual void HandleCommanding()
    {
        if (!CheckDestinationReached()) return;
        CommandData nextCommand = GetNextCommand();
        if(nextCommand != null)
        {
            if(nextCommand != GetMainActiveCommand())
            {
                NextCommand(nextCommand);
            }
        }
    }

    public virtual void HandleActioning()
    {
        if (!CheckDestinationReached()) return;
        CommandData activeCommand = GetMainActiveCommand();
 
        BuildingController targetBuilding = GetTargetBuilding(activeCommand);
        if (targetBuilding == null) return;
        if (!targetBuilding.CheckUnitCanProcessItem(unitData.Id))
        {
            unitData.SetState(UnitState.Waiting);
            return;
        }
        ProcessWithBuilding(activeCommand);
        if (IsCompleteCommand(activeCommand))
        {

            if (unitData.IsCompleteRoutine)
            {
                if (unitData.IsCompleteRoutine)
                {
                    FinishRoutine(UnitManager.Instance.GetExitPoint().position);
                    return;
                }                   
            }
            unitData.SetState(UnitState.Commanding);
        }
        else
        {           
            unitData.SetState(UnitState.Waiting);
        }
    }
  
    public virtual ActionType GetActiveCommandType()
    {
        return unitData.GetActiveActionType();
    }
    public virtual AnimationType GetAnimationType()
    {
         return unitData.IsEmptyItemCarry ? AnimationType.Run : AnimationType.Run_Carry;
    }
    public virtual void ProcessWithBuilding(CommandData activeCommand)
    {
        ActionType type = GameHelper.ConvertStringToEnum(activeCommand.ActionType);
        BuildingController targetBuilding = GetTargetBuilding(activeCommand);
        switch (type)
        {
            case ActionType.Input:
                AddItemToBuilding(targetBuilding);
                break;
            case ActionType.Output:
                HandleGetItemFromBuilding(activeCommand,targetBuilding);
                break;
            case ActionType.Thief:
                break;
            case ActionType.Process:
                break;
            default:
                break;
        }
        // After actioning complete command
        if (CheckCanCompleteCommand(activeCommand))
        {
            activeCommand.CompleteCommand(true);
            if (unitData.TargetBuilding != null)
            {
                CompleteActiveCommandAction(GetActiveCommandType(), unitData.TargetBuilding);
            }
        }
    }
    public virtual bool CheckCanCompleteCommand(CommandData targetCommand)
    {
        return true;
    }
    public virtual void HandleGetItemFromBuilding(CommandData targetCommand ,BuildingController buildingController)
    {
        GetAllItemFromBuilding(buildingController);
    }
    public virtual void FinishRoutine(Vector3 exitPoint)
    {
        unitData.OnCompleteRoutine?.Invoke(unitView);
    }
    public void Disposed()
    {

    }
    #endregion
}
public enum UnitType {
    Player =0,
    Worker =1,
    Customer =2
}