using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
public class UnitDataModel 
{
    #region DATA INFOR
    public string Id { get; private set; }
    public string CodeName { get; private set; }
    public UnitType UnitType { get; private set; }
    public float Speed { get; private set; }
    public string HatCodeName { get; private set; }
    #endregion
    #region DATA
    public Vector3? TargetDestination { get; protected set; } = null;
    public bool HasExisted { get; protected set; }
    // Building Action
    public BuildingController TargetBuilding { get; protected set; }
    public BuildingController LastJoinBuilding { get; protected set; }
    // ActionSLot
    public QueueSlotDataModel TargetActionSlot { get; protected set; }
    // Item slot
    public List<ItemSlotDataModel> ListSlotItemCarry { get; protected set; } = new List<ItemSlotDataModel>();

    // Rountine Command
    public CommandBuilding MainRoutineCommandData { get; protected set; }
    // key là command dạng string, value là các command đi kèm. Shelfer, farmer, Chef Ex: [Shelf_Tomato] [Plant_Tomato1,Plant_Tomato2]
    // Another EX for customer: [Shelf_Egg],[shelf_Wheat],[Cashier_Desk]
    public Dictionary<string, CommandBuilding> CommandBuildings = new Dictionary<string, CommandBuilding>();
    public CommandData MainActiveCommand { get; protected set; }

    public bool IsLoopInfinite { get; protected set; }
    public bool IsCanExited { get; protected set; }

    public int RoutineCommandIndex = -1;
    //State
    public UnitState CurrentState { get; private set; } = UnitState.None;

    #endregion
    #region ACTION     
    public event System.Action<BuildingController> OnTargetBuildingSet;
    public event System.Action<BuildingController,CommandData,string> OnCommandComplete;// Last join building, command active, Id
    public Action<UnitView> OnCompleteRoutine; 
    #endregion
    public UnitDataModel(string codeName, UnitType unitType, float speed)
    {
        Guid newGuid = Guid.NewGuid();
        Id = newGuid.ToString();
        CodeName = codeName;
        UnitType = unitType;
        this.Speed = speed;
    }
    public void SetSpeed(float speed)
    {
        Speed = speed;
    }
    public void SetHatCodeName(string hatCodeName)
    {
        HatCodeName = hatCodeName;
    }
    #region COMMAND ROUTINE 
    // INIT GET SET
    public void SetRoutineCommandChainData(CommandBuilding routineCommandChainData)
    {
        this.MainRoutineCommandData = routineCommandChainData;
    }
    public CommandData ActiveRoutineCommandData => MainRoutineCommandData.ActiveCommandData;
    public void SetActiveRoutineCommandData(CommandData commandData)
    {
        if (commandData == null) return;
        MainRoutineCommandData.SetActiveCommanData(commandData);
    }
    public void SetMainActiveCommand(CommandData commandData)
    {
        MainActiveCommand = commandData;
    }
    public void SetLoopInfinite(bool isLoopInfinite)
    {
        IsLoopInfinite = isLoopInfinite;
    }
    public void SetExited(bool isCanExited)
    {
        IsCanExited = isCanExited;
    }
    public void SetSupportedActiveCommanData(CommandData commandData)
    {
        if (commandData == null) return; 
        GetListSupportCommandbuilding(MainRoutineCommandData.ActiveCommandData.CodeName).SetActiveCommanData(commandData);
    }
    public CommandData ActiveSupportCommand() => GetListSupportCommandbuilding(MainRoutineCommandData.ActiveCommandData.CodeName).ActiveCommandData;
    public bool IsCompleteRoutine => MainRoutineCommandData.IsCommpleteAllCommand; // meaning complete without loop infinite
    public ActionType GetActiveActionType()
    {
        return GameHelper.ConvertStringToEnum(MainActiveCommand.ActionType);
    }

    // ADD FUNCTION
    public void AddCommandBuilding(string commandKey, CommandData commandBuilding = null)
    {
        if (!CommandBuildings.ContainsKey(commandKey))
        {
            CommandBuildings.Add(commandKey, new CommandBuilding());
            //Add to main routine command
            CommandData commandData = new CommandData(commandKey);
            MainRoutineCommandData.AddCommand(commandData);
        }
        if (commandBuilding != null)
            CommandBuildings[commandKey].AddCommand(commandBuilding);
    }
    public void AddCommandBuilding(string commandData, List<CommandData> commandBuildings)
    {
        foreach (var item in commandBuildings)
        {
            AddCommandBuilding(commandData, item);
        }
    }
    public CommandBuilding GetListSupportCommandbuilding(string commandCodeName)
    {
        if (CommandBuildings.TryGetValue(commandCodeName, out CommandBuilding listCommandSupport))
        {
            return listCommandSupport;
        }
        return null;
    }
    public void CompleteAllSupportCommand(CommandData commandData)
    {
        if (commandData == null) return;
        CommandBuilding listSupportCommand = GetListSupportCommandbuilding(commandData.CodeName);
        if (listSupportCommand != null)
        {
            listSupportCommand.CommpleteAllCommand(false);
        }
    }
    // CHECK FUNTION
    public bool IsCompleteMainRountineCommand()
    {
        return MainRoutineCommandData.IsComplete;
    }
    public void CompleteSupportCommand(bool isComplete)
    {
        CommandData activeSupportCommand = ActiveSupportCommand();
        if (activeSupportCommand != null)
        {
            activeSupportCommand.CompleteCommand(isComplete);
        }
    }
    public bool IsCompleteSupportCommand(CommandData routineActiveCommand)
    {
        if (routineActiveCommand == null) return false;
        var listSupportCommandbuilding = GetListSupportCommandbuilding(routineActiveCommand.CodeName);
        if (listSupportCommandbuilding == null) return false;
        bool isCompleteSupportCommand =  listSupportCommandbuilding.IsCommpleteAllCommand;
        return isCompleteSupportCommand;
    }
   
    #endregion

    #region ACTION API

    public void SetTargetMovement(Vector3 position)
    {
        TargetDestination = position;
    }
    public void SetTargetBuilding(BuildingController buildingController)
    {
        if(TargetBuilding != null)
        LastJoinBuilding = TargetBuilding;
        TargetBuilding = buildingController;

        OnTargetBuildingSet?.Invoke(buildingController);
    }
    public void SetTargetActionSlot(QueueSlotDataModel actionSlotData)
    {   
        TargetActionSlot = actionSlotData;       
    }
    public void NotifyCompleteCommand()
    {
        if(LastJoinBuilding != null)
        OnCommandComplete?.Invoke(LastJoinBuilding, ActiveRoutineCommandData, Id);
    }
    #endregion
    public bool IsPlayer => UnitType == UnitType.Player;
    #region SLOT ITEM CARRY API
    public void AddItemSlotCarry(ItemSlotDataModel slotCarry)
    {
        if (slotCarry == null) return;
        if (!ListSlotItemCarry.Contains(slotCarry))
        {
            ListSlotItemCarry.Add(slotCarry);
        }
    }
    public ItemSlotDataModel AddItemToSlotCarry(ItemController item)
    {
        ItemSlotDataModel freeSlot =GetFreeSlotCarry();
        if (freeSlot == null) return null;
        freeSlot.SetOccupier(item);
        return freeSlot;
    }
    public ItemSlotDataModel GetFreeSlotCarry()
    {
        return ListSlotItemCarry.FirstOrDefault(x => !x.IsOccupied);
    }
    public List<ItemSlotDataModel> GetlistSlotItemCarryOccupied => ListSlotItemCarry.Where(x => x.IsOccupied).ToList();

    public ItemSlotDataModel GetSlotCarryWithId(string Id)
    {
        return GetlistSlotItemCarryOccupied.FirstOrDefault(x => x.OccupierItem.itemData.Id == Id);
    }
    public int GetItemCount(string codeName)
    {
       return GetlistSlotItemCarryOccupied.Count(x => x.OccupierItem.itemData.CodeName == codeName);
    }
    #endregion
    #region CHECKING FUNCTION
    public bool CheckCanAddItemToSlotCarry()
    {
        return ListSlotItemCarry.Any(x => !x.IsOccupied);
    }
    public bool IsEmptyItemCarry=> ListSlotItemCarry.All(x => !x.IsOccupied);

    public bool IsFullItemCarry => ListSlotItemCarry.All(x => x.IsOccupied);
    #endregion
    #region STATE FUNCTION
    public void SetState(UnitState unitState, BuildingController newBuilding = null)
    {
        CurrentState = unitState;
        if (newBuilding != null)
            LastJoinBuilding = newBuilding;
    }
    public bool IsState(UnitState unitState)
    {
        return CurrentState == unitState;
    }
    #endregion

   
}
public enum UnitState
{
    None,
    Destinationing,
    Commanding,
    Queueing,
    Waiting,
    Actioning,
}

