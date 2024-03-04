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
        itemOuputConfigs = BuildingConfig.GetListItemConfig(ItemSlotType.Output.ToString());
        
    }
    List<ItemConfig> itemOuputConfigs;
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
    public void AddItemToSlot(ItemSlotType type, ItemController item, out ItemSlotDataModel slotAdditem)
    {

        slotAdditem = null;
        // Call API
        ItemSlotDataModel freeSlotAddItem = BuildingData.AddItemToSlot(type, item);
        if (freeSlotAddItem == null) return;
        slotAdditem = freeSlotAddItem;
        // Call View
        BuildingView.GetItem(item.itemView, freeSlotAddItem.Slotpoint);
    }
    public void RemoveItemAwaySlot(ItemSlotType type, string Id,out ItemController itemRemoveOut)
    {
        itemRemoveOut = null;
        //Call API
        ItemController itemRemove =  BuildingData.RemoveItemAwaySlot(type, Id);
        if(itemRemove == null) return;
        itemRemoveOut = itemRemove;
        //Call View
        BuildingView.RemoveItem(itemRemove.itemView);
      
    }
    #endregion
    #region ACTION FUNCTION

    public virtual void Processing()
    {
        switch(BuildingData.Type)
        {
            case BuildingType.Cashier:
                HandleFarmProcessing();
                break;
            case BuildingType.Shelf:
                break;
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
        if (!CheckAvaliableSlotForProcessing()) return;
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
                            BuildingView.RemoveItem(ItemSlotType.Input, item);
                        }

                    }
                    ItemConfig outputItemConfigs = itemOuputConfigs.First();
                    if (outputItemConfigs != null)
                    {
                        if (CheckFreeSlotToAddItem(ItemSlotType.Output, outputItemConfigs.CodeName))
                        {
                            //Create item
                            var itemView = SpawnerManager.CreateItemView(outputItemConfigs.CodeName, Vector3.zero, BuildingView.transform);
                            if (itemView != null)
                            {
                                AddItemToSlot(ItemSlotType.Output, itemView.ItemController, out ItemSlotDataModel slotAddItem);

                                //Call View
                                if (slotAddItem != null)
                                    BuildingView.AddItem(ItemSlotType.Output, itemView, slotAddItem);

                            }


                        }

                    }

                }
            }
        }
    }
    public virtual void HandleFarmAutoProcessing()
    {
        if (itemOuputConfigs == null) return;
        foreach (var itemConfig in itemOuputConfigs)
        {
            for (int i = 0; i < itemConfig.AmoutProcess; i++)
            {
                if (CheckFreeSlotToAddItem(ItemSlotType.Output, itemConfig.CodeName))
                {
                    //Create item
                    var itemView = SpawnerManager.CreateItemView(itemConfig.CodeName, Vector3.zero, BuildingView.transform);
                    if (itemView != null)
                    {
                        AddItemToSlot(ItemSlotType.Output, itemView.ItemController, out ItemSlotDataModel slotAddItem);
                        if (slotAddItem != null)
                        {
                            BuildingView.AddItem(ItemSlotType.Output, itemView, slotAddItem);
                        }
                    }
                }
            }
        }
    }
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
    public virtual UnitController GetNextUnitProcess(ActionType type)
    {
        if (BuildingData.GetTotalUnitJoinQueue(ActionType.Input) == 0) return null;
        return BuildingData.GetListUnitJoinQueue(ActionType.Input).First();
    }
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
            foreach (var item in slotItemOccupied)
            {
            
                if (countAmoutProcess >= BuildingConfig.GetItemConfig(type.ToString(), listiItemSlots.Key).AmoutProcess)
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
            if (countAmoutProcess >= BuildingConfig.GetItemConfig(type.ToString(), listiItemSlots.Key).AmoutProcess)
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
    public bool CheckAvaliableSlotForProcessing()
    {
        if (CheckBuildingType(BuildingType.FarmAutoProcess)) return CheckFreeSlotToAddItem(ItemSlotType.Output);
        return CheckContaintItemSlot(ItemSlotType.Input) && CheckFreeSlotToAddItem(ItemSlotType.Output);
    }
    public Dictionary<string, List<ItemSlotDataModel>> GetAvailableSlotsForProcess(ItemSlotType type)
    {
      return BuildingData.GetAvailableSlotsForProcess(type);
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

