using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;
public class BuildingController 
{
    public BuildingDataModel BuildingData;
    public BuildingView BuildingView;
    public BuildingConfig BuildingConfig { get; private set; }

    #region INIT FUNCTION
    public virtual void Init(BuildingInitData buildingInitData, BuildingView buildingView)
    {
        BuildingData = new BuildingDataModel();
        BuildingData.Init(buildingInitData);
        LoadConfig(BuildingData.CodeName);
        this.BuildingView = buildingView;
        BuildingData.InitBuildingType(BuildingConfig.Type);
        BuildingData.InitShelfOutputSlot();
    }
    private void LoadConfig(string codeName)
    {
        BuildingConfig = ConfigManager.Instance.GetBuildingConfig(codeName);
        
    }
    #endregion
    #region ITEM FUNCTION

    public Dictionary<string, List<ItemSlotDataModel>> GetItemSlots(ItemSlotType type)
    {
       return BuildingData.GetItemSlots(type);
    }
    public List<ItemSlotDataModel> GetListOccupiedSlotItems(ItemSlotType type, string codeName)
    {
       return BuildingData.GetListOccupiedSlotItems(type, codeName);
    }
    public void AddItemToSlot(ItemSlotType type, ItemController item, out ItemSlotDataModel slotAdditem, bool canCallView = true)
    {

        slotAdditem = null;
        // Call API
        ItemSlotDataModel freeSlotAddItem = BuildingData.AddItemToSlot(type, item);
        if (freeSlotAddItem == null) return;
        slotAdditem = freeSlotAddItem;
        // Call View
        if (canCallView)
            BuildingView.GetItemToInput(item.itemView, freeSlotAddItem.Slotpoint);
    }
    public void RemoveItemAwaySlot(ItemSlotType type, string Id,out ItemController itemRemoveOut)
    {
        itemRemoveOut = null;
        //Call API
        ItemController itemRemove =  BuildingData.RemoveItemAwaySlot(type, Id);
        if(itemRemove == null) return;
        itemRemoveOut = itemRemove;
        //Call View

    }
    #endregion
    #region PROCESS FUNCTION

    public virtual void Processing()
    {
        switch(BuildingData.Type)
        {          
            case BuildingType.Farm:
                HandleFarmProcessing();
                break;
            case BuildingType.FarmAutoProcess:
                HandleFarmAutoProcessing();
                break;
        }
    }
    public virtual void HandleCashierDeskProcessing()
    {
        
    }
    public virtual void HandleFarmProcessing()
    {
        if (!CheckAvaliableSlotItemForProcessing()) return;
        if (!CheckCanProcessItem(ItemSlotType.Input, out var slotAvaliableProcess))
            return;
        {
            //Remove input item and add output Item
            foreach (var slotItemList in slotAvaliableProcess)
            {
                foreach (var slotItem in slotItemList.Value)
                {
                    var item = slotItem.OccupierItem;
                    if (item != null)
                    {
                        RemoveItemAwaySlot(ItemSlotType.Input, slotItem.OccupierItem.itemData.Id, out ItemController ItemRemove);
                        if (ItemRemove != null)
                        {
                            BuildingView.MoveItemToModelProcess(item);
                        }

                    }
                    var outputItemConfigs = GetItemSlots(ItemSlotType.Output).Keys;
                   
                    if (outputItemConfigs != null)
                    {
                        foreach (var outputItemConfig in outputItemConfigs)
                        {
                            if (CheckFreeSlotToAddItem(ItemSlotType.Output, outputItemConfig))
                            {
                                //Create item
                                var itemView = SpawnerManager.CreateItemView(outputItemConfig, Vector3.zero, BuildingView.transform);
                                if (itemView != null)
                                {
                                    AddItemToSlot(ItemSlotType.Output, itemView.ItemController, out ItemSlotDataModel slotAddItem);

                                    //Call View
                                    if (slotAddItem != null)
                                        BuildingView.AddItemToOutput(itemView, slotAddItem);

                                }


                            }
                        }
                        

                    }

                }
            }
        }
    }
    public virtual void HandleFarmAutoProcessing()
    {
        var outputItemConfigs = GetItemSlots(ItemSlotType.Output).Keys;
        if (outputItemConfigs == null) return;
        foreach (var itemConfig in outputItemConfigs)
        {
            
            if (CheckFreeSlotToAddItem(ItemSlotType.Output, itemConfig))
            {
                //Create item
                var itemView = SpawnerManager.CreateItemView(itemConfig, Vector3.zero, BuildingView.transform);
                if (itemView != null)
                {
                    AddItemToSlot(ItemSlotType.Output, itemView.ItemController, out ItemSlotDataModel slotAddItem);
                    if (slotAddItem != null)
                    {
                        BuildingView.AddItemToOutput( itemView, slotAddItem);
                    }
                }
            }
         
        }
    }
    #endregion
    #region UNIT API FUNCTION
    public virtual QueueSlotDataModel AddUnitToQueueSlot(ActionType type, UnitController unit)
    {
        //Call API
        QueueSlotDataModel queueSlotFree= BuildingData.AddUnitToQueueSlot(type, unit);
        if(queueSlotFree == null) return null;
        return queueSlotFree;
        //Call View

    }
    public virtual UnitController RemoveUnitAwayQueueSlot(ActionType type, string Id)
    {
        return BuildingData.RemoveUnitAwayQueueSlot(type, Id);
    }
    #endregion
    #region CHECK ACTION FUNCTION
    public bool CheckCanJoinQueueSlot(ActionType actionType) => BuildingData.CheckCanJoinQueueSlot(actionType);
    #endregion

    #region CHECKING SLOT ITEM FUNCTION
    public bool CheckFreeSlotToAddItem(ItemSlotType type)
    {
        return BuildingData.CheckFreeSlotToAddItem(type);
    }
    public virtual bool CheckUnitCanProcessItem(string Id)
    {
        return true;
    }
    public bool CheckFreeSlotToAddItem(ItemSlotType type,string codeName)
    {
       return BuildingData.CheckFreeSlotToAddItem(type, codeName);
    }
    public bool CheckContaintItemSlot(ItemSlotType type)
    {
       return BuildingData.CheckContaintItemSlot(type);
    }
    public bool CheckCanProcessItem(ItemSlotType type, out Dictionary<string, List<ItemSlotDataModel>> slotAvaliableForProcess)
    {
        // ki?m tra và tr? v? dict các item v?i nguyên li?u phù h?p ví d?: 1 egg + 1 wheat => 1 cookie
        var itemSlotDics = GetItemSlots(type);
        if (itemSlotDics == null || itemSlotDics.Count == 0)
        {
            slotAvaliableForProcess = null;
            return false;
        }
        bool canProcess = false;
      
        Dictionary<string, List<ItemSlotDataModel>> slotAvaliableForProcessTemp = new Dictionary<string, List<ItemSlotDataModel>>();
        foreach (var listiItemSlots in itemSlotDics)
        {
            var slotItemOccupied = BuildingData.GetListOccupiedSlotItems(listiItemSlots.Value);
            int countAmoutProcess =0;
            int amountProcessCpnfig = BuildingConfig.GetItemConfig(type.ToString(), listiItemSlots.Key).AmoutProcess;
            foreach (var item in slotItemOccupied)
            {
            
                if (countAmoutProcess >= amountProcessCpnfig)
                {
                    break;
                }
                
                if (!slotAvaliableForProcessTemp.ContainsKey(listiItemSlots.Key))
                {
                    slotAvaliableForProcessTemp.Add(listiItemSlots.Key, new List<ItemSlotDataModel> { item });
                }
                else
                {
                    slotAvaliableForProcessTemp[listiItemSlots.Key].Add(item);
                }
                countAmoutProcess++;
            }
            if (countAmoutProcess >= amountProcessCpnfig)
            {
                canProcess = true;             
            }
            else
            {        
                slotAvaliableForProcess = null;
                canProcess = false;
                return canProcess;
            }
        }
        slotAvaliableForProcess = slotAvaliableForProcessTemp;
        return canProcess;
    }
    public bool CheckAvaliableSlotItemForProcessing()
    {
        if (CheckBuildingType(BuildingType.FarmAutoProcess)) return CheckFreeSlotToAddItem(ItemSlotType.Output);
        return CheckContaintItemSlot(ItemSlotType.Input) && CheckFreeSlotToAddItem(ItemSlotType.Output);
    }

    #endregion
    #region OTHER CHECK
    public bool CheckBuildingExistTypeItem(ItemSlotType type, string codeName)
    {
        return BuildingData.CheckBuildingExistTypeItem(type, codeName);
    }
    public bool CheckBuildingType(BuildingType type)
    {
        return BuildingData.Type == type;
    }
    #endregion
}
public enum ItemSlotType
{
    Input,
    Output
}
public struct BuildingInitItemData
{
    public string CodeName;
    public List<PointData> AcitonPoint;
    public Transform Point;
}
public struct PointData
{
    public string CodeName;
    public Transform Point;
    public bool IsLocked;

}
[System.Serializable]
public struct BuildingInitData
{
    public string CodeName;
    public BuildingConfig BuildingConfig;
    public List<BuildingInitItemData> InputItemSlotDatas;
    public List<BuildingInitItemData> OutputItemSlotDatas;
    public List<BuildingInitItemData> ActionSlots;
}

