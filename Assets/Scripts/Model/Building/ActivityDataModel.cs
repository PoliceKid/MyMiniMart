using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivityDataModel 
{
    public string CodeName;
    public float Duration;

    public List<ItemSlotDataModel> ItemSlots = new List<ItemSlotDataModel>();



    //public Dictionary<string, SlotDataModel> ItemTestSlots { get; private set; } = new Dictionary<string, SlotDataModel>();
    //public Dictionary<string, SlotDataModel>ActionSlots { get; private set; } = new Dictionary<string, SlotDataModel>();
    //public void Init(BuildingInitActivityData buildingInitActivityData, ActivityConfig activityConfig)
    //{
    //    CodeName = buildingInitActivityData.CodeName;
    //    foreach (var item in buildingInitActivityData.AcitonPoint)
    //    {
    //        // init Aciton slot
    //        var produceSlotData = new SlotDataModel()
    //        {
    //            Slotpoint = item.Point,
    //        };
    //        produceSlotData.ToogleLock(item.IsLocked);
    //        AddActivity(CodeName, produceSlotData);
    //    }
    //    LoadConfig(activityConfig);
    //}
    //private void AddActivity(string codeName, SlotDataModel slotDataModel)
    //{
    //    if (codeName.Contains("Mat"))
    //    {
    //        if (!ItemTestSlots.ContainsKey(CodeName))
    //            ItemTestSlots.Add(CodeName, slotDataModel);
    //    }
    //    else if (codeName.Contains("Action"))
    //    {
    //        if (!ActionSlots.ContainsKey(CodeName))
    //            ActionSlots.Add(CodeName, slotDataModel);
    //    }
    //}
    //public void LoadConfig(ActivityConfig activityConfig)
    //{
    //    Duration = activityConfig.Duration;

    //}
    //public Transform GetPosition => Position;
   
}
public interface IOccupier
{

}

