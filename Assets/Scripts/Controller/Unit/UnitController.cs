using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class UnitController : IOccupier
{
    protected UnitView unitView;
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
        GetTargetBuilding = BuildingManager.Instance.GetTargetBuilding;
        InitRoutineCommand(unitConfig);

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
    private bool CheckCanAddItemToSlotCarry()
    {
        return unitData.CheckCanAddItemToSlotCarry();
    }
    public int GetItemFromBuilding(string ItemCodeName,BuildingController buildingController, int quantity =0)
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
                AddItemToSlotCarry(itemRemove);        
                itemCount++;                         
            }
          
        }
        return itemCount;
    }
   
    public void GetItemFromBuilding( BuildingController buildingController, int quantity =0)
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
            buildingController.AddItemToSlot(ItemSlotType.Input, item,out ItemSlotDataModel slotAddItem);
            if(slotAddItem != null)
            {
                unitView.AddItem(item.itemView, slotAddItem.Slotpoint, buildingController.BuildingView.transform);
                slotItem.ResetOccupier();
            }
                   
        }
        return false;
    }

    public void AddItemToSlotCarry(ItemController item)
    {
        ItemSlotDataModel freeSlotForItem = unitData.AddItemToSlotCarry(item);
        if (freeSlotForItem != null)
            unitView.GetItem(item.itemView, freeSlotForItem.Slotpoint);
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
        if (unitData.IsCompleteRoutine && !unitConfig.IsLoopInfinite) return;
        if (GetActiveRoutineCommand() == null) return;
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
        if (activeCommandTemp.IsComplete)
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
                //GetActiveRoutineCommand().CompleteCommand(true);
                nextCommand = activeCommandTemp;
                //Complete Active routine command
               
            }
            else
            {
                // If not complete Support command
                nextCommand = NextSupportCommand(activeCommandTemp);                       
            }
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
    //public virtual CommandData GetNextRoutineCommand(int index)
    //{
    //    List<CommandData> listRoutineCommand = unitData.MainRoutineCommandData.Commands;
    //    if (listRoutineCommand.Count == 0) return null;
    //    index = Math.Clamp(index, 0, listRoutineCommand.Count - 1);
    //    return listRoutineCommand[index];
    //}
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
        CommandData nextRoutineCommand = null;
        nextRoutineCommand = GetNextRoutineCommand(index);
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
    public void SetTargetBuilding( ActionType type, BuildingController buildingController)
    {
        //Call API
        if (buildingController == null) return;
        if (!buildingController.CheckCanJoinQueueSlot(type)) return;
        
        unitData.SetTargetBuilding(buildingController);
        QueueSlotDataModel queueSlotFree = buildingController.AddUnitToQueueSlot(type, this);
        //Call View
        unitView.SetTargetMovePosition(queueSlotFree.Slotpoint.position, GetAnimationType());
        StartGotoDestination();
    }
    public void CompleteActiveCommandAction(ActionType type, BuildingController lastJoinBuilding)
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
            unitView.ChangeState("Idle_Happy_Carry");
            unitData.SetState(UnitState.Actioning);
        }
    }
    public virtual void HandleCommanding()
    {
        if (!CheckDestinationReached()) return;
        CommandData nextCommand = GetNextCommand();
        if(nextCommand != null)
        {
            if(nextCommand != GetMainActiveCommand())
            {
                
                if (unitData.CodeName.Contains("Customer03"))
                {
                    Debug.Log(nextCommand.CodeName);
                }
                
                NextCommand(nextCommand);
            }
        
        }
    }
    public virtual void HandleActioning()
    {
        CommandData activeCommand = GetMainActiveCommand();
        BuildingController targetBuilding = GetTargetBuilding(activeCommand);
        if (targetBuilding == null) return;
        if (!targetBuilding.CheckUnitCanProcessItem(this.unitData.Id)) return;
        AIProcessWithBuilding(activeCommand);
        // Do something here for AI behaviour. Ex If building full item, waiting to add item for building, then enable next routine command.
        // TODO
        unitData.SetState(UnitState.Commanding);
    }
    public virtual ActionType GetActiveCommandType()
    {
        return unitData.GetActiveActionType();
    }
    public virtual AnimationType GetAnimationType()
    {
         return unitData.IsEmptyItemCarry ? AnimationType.Run : AnimationType.Run_Carry;
    }
    public void AIProcessWithBuilding(CommandData activeCommand)
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
        if (activeCommand != null)
        {
            activeCommand.CompleteCommand(true);
        }
        if (IsCompleteCommand(activeCommand))
        {
            if (unitData.TargetBuilding != null)
            {
                CompleteActiveCommandAction(GetActiveCommandType(), unitData.TargetBuilding);
            }
        }
      

    }
    public virtual void HandleGetItemFromBuilding(CommandData targetCommand ,BuildingController buildingController)
    {
        GetItemFromBuilding(buildingController);
    }
    public virtual void FinishRoutine(Vector3 exitPoint)
    {
    }
    #endregion
}
public enum UnitType {
    Player =0,
    Worker =1,
    Customer =2
}