using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ShevlerController : UnitController
{
    // Key action , value shelfbuilding. 
    public static Dictionary<string, List<CommandData>> ShelfBuildings = new Dictionary<string, List<CommandData>>();
    public static List<CommandData> Commands = new List<CommandData>();
    // Ex shelf_Wheat_Input, Value: { Plant_Wheat1, Plant_Wheat2}


    public static void AddCommandShelfBuilding(string commandKey, CommandData commandBuilding)
    {
        if (ShelfBuildings.ContainsKey(commandKey))
        {
            if(!ShelfBuildings[commandKey].Contains(commandBuilding))
            ShelfBuildings[commandKey].Add(commandBuilding);
        }
        else
        {
            ShelfBuildings.Add(commandKey, new List<CommandData> {commandBuilding });
            CommandData commandData = new CommandData(commandKey);
            if (!Commands.Contains(commandData))
                Commands.Add(commandData);
        }
    }

    public override void NextCommand(CommandData activeCommand = null)
    {
        CommandData nextCommand = null;
        if (GetActiveRoutineCommand() == null)
        {
            nextCommand = GetNextShelfBuilding();
        }
        else
        {
            if( GetActiveRoutineCommand().IsComplete)
            {
                nextCommand = GetNextShelfBuilding();
            }
            else
            {
          
                nextCommand = GetCommandDataSupportShelfBuilding(GetActiveRoutineCommand().CodeName);
            }
           
        }
        if(nextCommand != null)
        {
            StartActiveCommand(nextCommand);
        }
    }

    public CommandData GetNextShelfBuilding()
    {
        // First get shelf building with minimum Item.
        // Second Maybe priority shelf building that have item customer needed.

        BuildingView shelfBuilding = BuildingManager.Instance.GetShelfBuildingPriorityShelver();

        if(shelfBuilding != null)
        {
            foreach (var item in Commands)
            {
                if(GameHelper.GetStringSplitSpaceRemoveLast(item.CodeName) == shelfBuilding.CodeName)
                {
                    //SetActiveRoutineCommand(item);
                    GetActiveRoutineCommand().CompleteCommand(false);
                    return item;
                }
            }
        }
        return null;
    }
    public CommandData GetCommandDataSupportShelfBuilding(string commandCodeName)
    {
        if (ShelfBuildings.ContainsKey(commandCodeName))
        {
            CommandData supportedCommand = GetSupportCommandBuildingPriority(commandCodeName);
            return supportedCommand;
        }
        return null;
    }
    public List<BuildingController> GetListSupportCommandbuilding(string commandCodeName)
    {
        List<BuildingController> buildingControllers = new List<BuildingController>();
        if (ShelfBuildings.ContainsKey(commandCodeName))
        {
            foreach (var item in ShelfBuildings[commandCodeName])
            {
                BuildingController building = BuildingManager.Instance.GetTargetBuilding(item);
                buildingControllers.Add(building);
            }
            buildingControllers = buildingControllers.OrderByDescending(x => x.BuildingData.GetTotalCountItem(ItemSlotType.Output)).ToList();
            
        }
        return buildingControllers;
    }
    public CommandData GetSupportCommandBuildingPriority(string commandCodeName)
    {
        if (ShelfBuildings.ContainsKey(commandCodeName))
        {
            BuildingController shelfBuilding = GetListSupportCommandbuilding(commandCodeName).FirstOrDefault();
            if(shelfBuilding != null)
            {
                foreach (var item in ShelfBuildings[commandCodeName])
                {
                    if (GameHelper.GetStringSplitSpaceRemoveLast(item.CodeName) == shelfBuilding.BuildingData.CodeName)
                    {
                        return item;
                    }
                }
            }
          
        }
        return null;
        
    }
}
