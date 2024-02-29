using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class CommandData 
{
    public string CodeName { get; protected set; }
    public string ActionType { get; protected set; }
    public bool IsComplete { get; protected set; }
    public int Priority { get; protected set; }
    public CommandData(string codeName)
    {
        CodeName = codeName;
        Priority = 0;
        string[] parts = codeName.Split("_");
        if (parts.Length > 1)
        {
            ActionType = parts.Last();
        }
        
    }
    public void CompleteCommand(bool complete)
    {
        IsComplete = complete;
    }

}
public class CommandBuilding
{
   
    // List các command key.
    public List<CommandData> Commands { get; private set; } = new List<CommandData>();
    public int CommandCount => Commands.Count;
    public bool IsComplete { get; protected set; }

    public CommandData ActiveCommandData { get; protected set; }

    public void SetActiveCommanData(CommandData commandData)
    {
        ActiveCommandData = commandData;
    }
    public void AddCommand(CommandData command)
    {
        if (!Commands.Contains(command))
        {
            Commands.Add(command);
        }
    }
    public void CompeleteSupportCommand(bool isComplete)
    {
        IsComplete = isComplete;
    }
    public bool IsCommpleteAllSupportCommand => Commands.All(x => x.IsComplete);
    public void CommpleteAllSupportCommand(bool isComplete)
    {
        foreach (var item in Commands)
        {
            item.CompleteCommand(isComplete);
        }
    }
}

