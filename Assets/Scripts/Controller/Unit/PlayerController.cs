using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : UnitController
{
    public const string PLAYER_HAT_STRING = "hat_Rabbit";

    public override void Init(string codeName, UnitView unitView)
    {
        base.Init(codeName, unitView);
        SetHatCodeName(PLAYER_HAT_STRING);
    }
    public override int GetItemFromBuilding(string ItemCodeName, BuildingController buildingController, int quantity = 0)
    {
        if (ItemCodeName == "cash")
        {
            float timeCount = 0;
            List<ItemSlotDataModel> listCash = buildingController.GetListOccupiedSlotItems(ItemSlotType.Output, ItemCodeName);
            foreach (var item in listCash)
            {
                timeCount += 0.1f;
                buildingController.RemoveItemAwaySlot(ItemSlotType.Output, item.OccupierItem.itemData.Id, out ItemController itemRemove);
                if (itemRemove != null)
                {
                    GetResourse(itemRemove, timeCount);
                }
            }
            return 0;
        }
        return base.GetItemFromBuilding(ItemCodeName, buildingController, quantity);
    }

    public void GetResourse(ItemController item, float delayTime)
    {     
        TimerHelper.instance.StartTimer(delayTime, () =>
        {
            Action OnGetCash = () =>
            {
                ResourceManager.Instance.UpdateResource(ResourceType.Cash, 1);
                SpawnerManager.DespawnItem(item.itemView);
            };
            unitView.GetItem(item.itemView, OnGetCash);
        }
        );
    }
   
}
