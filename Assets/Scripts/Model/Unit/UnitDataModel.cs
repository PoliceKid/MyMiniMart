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

    public int RoutineCommandIndex = -1;
    //State
    public UnitState CurrentState { get; private set; } = UnitState.None;

    #endregion
    #region ACTION     
    public event System.Action<BuildingController> OnTargetBuildingSet;
    public event System.Action<BuildingController,CommandData,string> OnCommandComplete;// Last join building, command active, Id

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
        CommandBuilding listSupportCommand = GetListSupportCommandbuilding(commandData.CodeName);
        if(listSupportCommand!= null)
        {
            listSupportCommand.CommpleteAllSupportCommand(false);
        }
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
    public void SetSupportedActiveCommanData(CommandData commandData)
    {
        if (commandData == null) return; 
        CommandData activeSupportCommand = ActiveSupportCommand();
        if(activeSupportCommand != null)
        {
            activeSupportCommand.CompleteCommand(true);
        }
        GetListSupportCommandbuilding(MainRoutineCommandData.ActiveCommandData.CodeName).SetActiveCommanData(commandData);
    }
    public CommandData ActiveSupportCommand() => GetListSupportCommandbuilding(MainRoutineCommandData.ActiveCommandData.CodeName).ActiveCommandData;
    public bool IsCompleteRoutine => MainRoutineCommandData.IsCommpleteAllSupportCommand; // meaning complete without loop infinite
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
    //public CommandData GetNextRoutineCommand()
    //{
    //    List<CommandData> listRoutineCommand = MainRoutineCommandData.Commands;
    //    if (listRoutineCommand.Count == 0) return null;
    //    listRoutineCommand = listRoutineCommand.Where(x => GetTargetBuilding(x) != null).ToList();
    //    listRoutineCommand = listRoutineCommand
    //        .OrderByDescending(y => {
    //            var targetBuilding = GetTargetBuilding(GetNextSupportCommand(y.CodeName));
    //            return targetBuilding != null ?
    //                targetBuilding.BuildingData.GetTotalCountItem(ItemSlotType.Output) :0; // 
    //        })
    //        .OrderBy(x => {
    //            var targetBuilding = GetTargetBuilding(x);
    //            return targetBuilding != null ?
    //                targetBuilding.BuildingData.GetTotalCountItem(ItemSlotType.Input) :0; // Hoặc giá trị mặc định khác tùy thuộc vào logic của bạn
    //        }).ToList();
    //    return listRoutineCommand.Count>0 ? listRoutineCommand.First(): null;
    //}
    //public CommandData GetNextRoutineCommand(int index)
    //{
    //    List<CommandData> listRoutineCommand = MainRoutineCommandData.Commands;
    //    if (listRoutineCommand.Count == 0) return null;
    //    index = Math.Clamp(index, 0, listRoutineCommand.Count-1);
    //    return listRoutineCommand[index];
    //}
    //public CommandData GetNextSupportCommand(string commandCodeName)
    //{
    //    if (!CommandBuildings.ContainsKey(commandCodeName)) return null;
    //    List<CommandData> listCommandSupport = GetListSupportCommandbuilding(commandCodeName).Commands;
    //    if (listCommandSupport != null)
    //    {   
    //        // Lọc ra list command khác null
    //        listCommandSupport = listCommandSupport.Where(x =>GetTargetBuilding(x) != null).ToList();
    //        // sort support building với output item count nhiều nhất để đi lấy
    //        listCommandSupport = listCommandSupport.OrderByDescending(x => {
    //            var targetBuilding = GetTargetBuilding(x);
    //            return targetBuilding != null ?
    //                targetBuilding.BuildingData.GetTotalCountItem(ItemSlotType.Output) :0; // Hoặc giá trị mặc định khác tùy thuộc vào logic của bạn
    //        }).
    //            ThenBy(y => GetTargetBuilding(y).BuildingData.GetUnitJoinQueue(ActionType.Output)).ToList();
    //        return listCommandSupport.Count>0 ? listCommandSupport.First(): null;
    //    }
    //    return null;
    //}
    public CommandBuilding GetListSupportCommandbuilding(string commandCodeName)
    {
        if (CommandBuildings.TryGetValue(commandCodeName, out CommandBuilding listCommandSupport))
        {
            return listCommandSupport;
        }
        return null;
    }
    // NEXT FUNCTION
    //public CommandData NextRoutineCommand(int index =-1)
    //{
    //    if (ActiveRoutineCommandData != null)
    //        ActiveRoutineCommandData.CompleteCommand(true);
    //    // get next Routine command
    //    CommandData nextRoutineCommand = null;
    //    if (index != -1)
    //    {
    //        nextRoutineCommand = GetNextRoutineCommand(index);
    //    }
    //    else
    //    {
    //        nextRoutineCommand = GetNextRoutineCommand();
    //    }
    //    if (nextRoutineCommand == null) return null;
    //    // Set active routine command  = next routine command
    //    SetActiveRoutineCommandData(nextRoutineCommand);

    //    // return active command. 
    //    return nextRoutineCommand;
    //}
    //public CommandData NextSupportCommand( CommandData routineActiveCommand)
    //{
    //    // If Unit max slot item carry hoặc 
    //    if(routineActiveCommand == null) return null;
    //    if (ActiveRoutineCommandData != null)
    //        ActiveRoutineCommandData.CompleteCommand(false);
        
    //    CommandData supportCommandPriority = GetNextSupportCommand(routineActiveCommand.CodeName);
    //    if (supportCommandPriority != null)
    //    {
    //        SetSupportedActiveCommanData(supportCommandPriority);
    //        return supportCommandPriority;
    //    }
    //    return null;
    //}
    // CHECK FUNTION
    public bool IsCompleteMainRountineCommand()
    {
        return MainRoutineCommandData.IsComplete;
    }
    public void CompleteSupportCommand(CommandData routineActiveCommand)
    {
        GetListSupportCommandbuilding(routineActiveCommand.CodeName).CompeleteSupportCommand(true);
    }
    public bool IsCompleteSupportCommand(CommandData routineActiveCommand)
    {
        if (routineActiveCommand == null) return false;
        var listSupportCommandbuilding = GetListSupportCommandbuilding(routineActiveCommand.CodeName);
        if (listSupportCommandbuilding == null) return false;
        bool isCompleteSupportCommand =  listSupportCommandbuilding.IsCommpleteAllSupportCommand;
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

