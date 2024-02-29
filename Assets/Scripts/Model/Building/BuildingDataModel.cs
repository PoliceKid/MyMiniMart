using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
public class BuildingDataModel 
{
    public string CodeName { get; private set; }
    public BuildingType Type { get; private set; }
    public Dictionary<string, List<ItemSlotDataModel>> InputSlotItemPools { get; private set; } = new Dictionary<string, List<ItemSlotDataModel>>();
    public Dictionary<string, List<ItemSlotDataModel>> OutputSlotItemPools { get; private set; } = new Dictionary<string, List<ItemSlotDataModel>>();
    public Dictionary<string, List<QueueSlotDataModel>> QueueSlots { get; private set; } = new Dictionary<string, List<QueueSlotDataModel>>();

    public List<ItemController> itemMaps = new List<ItemController>();
    public List<UnitController> unitMaps = new List<UnitController>();

    public Action<string> OnUpdateItem = delegate { };
    #region INIT FUNCTION
    public void Init(BuildingInitData buildingInitData)
    {
        CodeName = buildingInitData.CodeName;
        InitSlotItemPools(ItemSlotType.Input, buildingInitData.InputItemSlotDatas);
        InitSlotItemPools(ItemSlotType.Output, buildingInitData.OutputItemSlotDatas);
        InitActionSlot(buildingInitData.ActionSlots);
        
    }
    public void InitBuildingType(int typeIndex)
    {
        Type = (BuildingType)typeIndex;
    }
    private void InitSlotItemPools(ItemSlotType type, List<BuildingInitItemData> buildingInitItemDatas)
    {
        var slotItems = GetItemSlots(type);
        if (slotItems == null) return;
        if (buildingInitItemDatas == null || buildingInitItemDatas.Count == 0) return;
        foreach (var item in buildingInitItemDatas)
        {
            slotItems.Add(item.CodeName, new List<ItemSlotDataModel>());
            //Add Item if building is Shelf (K? ??ng item)   
        }
        foreach (var buildingInitItemData in buildingInitItemDatas)
        {
            foreach (var item in buildingInitItemData.AcitonPoint)
            {
                ItemSlotDataModel slotDataModel = new ItemSlotDataModel()
                {
                    Slotpoint = item.Point
                };
                slotItems[buildingInitItemData.CodeName].Add(slotDataModel);
                //Add Item if building is Shelf (K? ??ng item)
            }
        }
    }
    public void InitShelfOutputSlot()
    {
        if (Type != BuildingType.Shelf) return;
        if (OutputSlotItemPools.Count == 0)
        {
            foreach (var slotContainer in InputSlotItemPools)
            {
                OutputSlotItemPools.Add(slotContainer.Key, new List<ItemSlotDataModel> { });
                foreach (var item in slotContainer.Value)
                {
                    OutputSlotItemPools[slotContainer.Key].Add(item);
                }
            }
        }
    }
    private void InitActionSlot( List<BuildingInitItemData> buildingInitItemDatas)
    {
       
        foreach (var buildingInitItemData in buildingInitItemDatas)
        {
            QueueSlots.Add(buildingInitItemData.CodeName, new List<QueueSlotDataModel>());
            foreach (var item in buildingInitItemData.AcitonPoint)
            {
                QueueSlotDataModel queueSlotDataModel = new QueueSlotDataModel()
                {
                    Slotpoint = item.Point
                };
                QueueSlots[buildingInitItemData.CodeName].Add(queueSlotDataModel);
                //Add Item if building is Shelf (K? ??ng item)
            }
        }
    }
    #endregion
    #region ACTION API
    public QueueSlotDataModel GetQueueSlotFree(ActionType actionType)
    {
        if (!QueueSlots.ContainsKey(actionType.ToString())) return null;
        var listQueueSlot = QueueSlots[actionType.ToString()];
        return listQueueSlot.FirstOrDefault(x => !x.IsOccupied);
    }
    public int GetUnitJoinQueue(ActionType actionType)
    {
        if (!QueueSlots.ContainsKey(actionType.ToString())) return 0;
        var listQueueSlot = QueueSlots[actionType.ToString()];
        return listQueueSlot.Count(x => x.IsOccupied);
    }
    public bool CheckCanJoinQueueSlot(ActionType actionType)
    {
        if (!QueueSlots.ContainsKey(actionType.ToString())) return false;
        var listQueueSlot = QueueSlots[actionType.ToString()];
        return listQueueSlot.Any(x => !x.IsOccupied);
    }
    public QueueSlotDataModel AddUnitToQueueSlot(ActionType type, UnitController unit)
    {
        QueueSlotDataModel queueSlotFree = GetQueueSlotFree(type);
        if (queueSlotFree == null) return null;
        queueSlotFree.SetOccupier(unit);
        AddUnitToMap(unit);
        return queueSlotFree;
    }
    public UnitController RemoveUnitAwayQueueSlot(ActionType type, string Id)
    {
        UnitController unit = GetUnitWithId(Id);
        if (unit != null)
        {
            if (QueueSlots.ContainsKey(type.ToString()))
            {
                var removeSlotItem = QueueSlots[type.ToString()].FirstOrDefault(x => x.OccupierUnit == unit);
                if (removeSlotItem != null)
                {
                    removeSlotItem.ResetOccupier();
                    RemoveUnitAwayMaps(Id);
                    return unit;

                }
            }
        }
        return null;
    }
    private void AddUnitToMap(UnitController unit)
    {
        if (!unitMaps.Contains(unit))
        {
            unitMaps.Add(unit);
        }
    }
    public void RemoveUnitAwayMaps(string Id)
    {
        UnitController unit = GetUnitWithId(Id);
        if (unitMaps.Contains(unit))
        {
            unitMaps.Remove(unit);
        }
    }
    public UnitController GetUnitWithId(string Id)
    {
        foreach (var unit in unitMaps)
        {
            if (unit.unitData.Id == Id)
            {
                return unit;
            }
        }
        return null;
    }
    #endregion
    #region API FUNCTION
    public Dictionary<string, List<ItemSlotDataModel>> GetItemSlots(ItemSlotType type)
    {
        switch (type)
        {
            case ItemSlotType.Input:
                return InputSlotItemPools;
            case ItemSlotType.Output:
                return OutputSlotItemPools;
            default:
                return null;
        }
    }
    public int GetTotalCountItem(ItemSlotType type)
    {
        int count = 0;
        var itemSlotDics = GetItemSlots(type);
        if (itemSlotDics == null || itemSlotDics.Count == 0)
        {
            return 0;
        }
        foreach (var item in itemSlotDics)
        {
            var x = GetListOccupiedSlotItems(item.Value);
            count += x.Count;
        }
        return count;
    }
    public ItemSlotDataModel GetFreeSlotItems(List<ItemSlotDataModel> slotDataModels)
    {
        return slotDataModels.FirstOrDefault(x => !x.IsOccupied);
    }
    public List<string> GetListItemCanPay()
    {
        return new List<string> { "egg", "bread", "veg_Tomato", "veg_Wheat", "flourBag" };
    }
    public ItemController GetItemWithId(string Id)
    {
        foreach (var item in itemMaps)
        {
            if (item.itemData.Id == Id)
            {
                return item;
            }
        }
        return null;
    }
    public ItemSlotDataModel GetOccupiedSlotItems(List<ItemSlotDataModel> slotDataModels)
    {
        return slotDataModels.FirstOrDefault(x => x.IsOccupied);
    }
    public List<ItemSlotDataModel> GetListOccupiedSlotItems(List<ItemSlotDataModel> slotDataModels)
    {
        return slotDataModels.Where(x => x.IsOccupied).ToList();
    }
    public List<ItemSlotDataModel> GetListOccupiedSlotItems(ItemSlotType type, string codeName)
    {
        var itemSlotDics = GetItemSlots(type);
        if (itemSlotDics == null || itemSlotDics.Count == 0)
        {
            return null;
        }
        if (!itemSlotDics.ContainsKey(codeName)) return null;
        return itemSlotDics[codeName].Where(x => x.IsOccupied).ToList();
    }
    public Dictionary<string, List<ItemSlotDataModel>> GetAvailableSlotsForProcess(ItemSlotType type)
    {
        var itemSlotDics = GetItemSlots(type);

        if (itemSlotDics == null || itemSlotDics.Count == 0)
        {
            return null;
        }

        Dictionary<string, List<ItemSlotDataModel>> slotAvaliableForProcessTemp = new Dictionary<string, List<ItemSlotDataModel>>();

        foreach (var listiItemSlots in itemSlotDics)
        {
            var slotItemOccupied = GetListOccupiedSlotItems(listiItemSlots.Value);

            foreach (var item in slotItemOccupied)
            {
                if (!slotAvaliableForProcessTemp.ContainsKey(listiItemSlots.Key))
                {
                    slotAvaliableForProcessTemp.Add(listiItemSlots.Key, new List<ItemSlotDataModel> { item });
                }
                else
                {
                    slotAvaliableForProcessTemp[listiItemSlots.Key].Add(item);
                }
            }
        }

        return slotAvaliableForProcessTemp;
    }
    public ItemSlotDataModel AddItemToSlot(ItemSlotType type, ItemController item)
    {
        var itemSlots = GetItemSlots(type);
        if (itemSlots == null) return null;
        string codeName = itemSlots.ContainsKey("anything") ? "anything" : item.itemData.CodeName;
        //string codeName = item.itemData.CodeName;

        if (itemSlots.ContainsKey(codeName))
        {
            if (!CheckFreeSlotToAddItem(type, codeName)) return null;
            ItemSlotDataModel freeSlot = GetFreeSlotItems(itemSlots[codeName]);
            if (freeSlot == null) return null;        
            freeSlot.SetOccupier(item);
            OnUpdateItem?.Invoke(this.CodeName);
            AddSlotItemMap(item);
            return freeSlot;

        }
        return null;
    }
    public ItemController RemoveItemAwaySlot(ItemSlotType type, string Id)
    {
     
        var itemSlots = GetItemSlots(type);
        if (itemSlots == null)
        {
            return null;
        }
        ItemController item = GetItemWithId(Id);
        if (item != null)
        {
            string codeName = itemSlots.ContainsKey("anything") ? "anything" : item.itemData.CodeName;
            //string codeName = item.itemData.CodeName;
            if (itemSlots.ContainsKey(codeName))
            {
                var removeSlotItem = itemSlots[codeName].FirstOrDefault(x => x.OccupierItem == item);
                if (removeSlotItem != null)
                {
                    removeSlotItem.ResetOccupier();
                    RemoveItemAwayItemMaps(Id);

                    return item;
                    
                }
            }
        }
        return null;
    }
    private void AddSlotItemMap(ItemController item)
    {
        if (!itemMaps.Contains(item))
        {
            itemMaps.Add(item);
        }
    }
    public void RemoveItemAwayItemMaps(string Id)
    {
        ItemController item = GetItemWithId(Id);
        if (itemMaps.Contains(item))
        {
            itemMaps.Remove(item);
        }
    }
    #endregion
    #region CHECKING FUNCTION
    public bool CheckFreeSlotToAddItem(ItemSlotType type, string codeName)
    {
        var itemSlots = GetItemSlots(type);
        if (itemSlots == null || itemSlots.Count == 0) return false;
        if (itemSlots.ContainsKey("anything") && GetListItemCanPay().Contains(codeName)) return true;
        if (!itemSlots.ContainsKey(codeName)) return false;
        return itemSlots[codeName].Any(x => !x.IsOccupied);
    }
    public bool CheckFreeSlotToAddItem(ItemSlotType type)
    {
        var itemSlots = GetItemSlots(type);
        if (itemSlots == null || itemSlots.Count == 0) return false;
        foreach (var item in itemSlots)
        {
            if (item.Value.Any(x => !x.IsOccupied) == true)
            {
                return true;
            }
        }
        return false;
    }
    public bool CheckContaintItemSlot(ItemSlotType type)
    {
        var itemSlots = GetItemSlots(type);
        if (itemSlots == null || itemSlots.Count == 0)
        {
            return false;
        }
        foreach (var item in itemSlots)
        {
            ItemSlotDataModel occupiedSlot = GetOccupiedSlotItems(item.Value);
            if (occupiedSlot != null)
            {
                return true;
            }
        }
        return false;
    }
    public bool CheckBuildingExistTypeItem(ItemSlotType type, string codeName)
    {
        var itemSlots = GetItemSlots(type);
        if (itemSlots == null || itemSlots.Count == 0)
        {
            return false;
        }
        return itemSlots.ContainsKey(codeName);
    }
    #endregion

}
public enum ActionType
{
    Input = 0,
    Output = 1,
    Thief = 2,
    Process =3
}