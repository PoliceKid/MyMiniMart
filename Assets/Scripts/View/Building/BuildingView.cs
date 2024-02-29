using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

public class BuildingView : MonoBehaviour
{
    #region PLACEHOLDER
    [SerializeField]
    public string CodeName;
    [SerializeField]
    public bool IsLocked;
    [SerializeField]
    public GameObject ModelProcess;
    [SerializeField]
    public Transform InputWorkerSlot;

    [SerializeField]
    public List<SlotContainer> InputItemSlots = new List<SlotContainer>();
    [SerializeField]
    public List<SlotContainer> OutputItemSlots = new List<SlotContainer>();
    [SerializeField]
    public List<SlotContainer> ActionSlots = new List<SlotContainer>();

    #endregion

    protected GameObject model;
    private float currentTimeProcess =0;
    private bool hasInit;
    Sequence sequence;
    public BuildingController building { get; private set; }

    public virtual void Init(BuildingController building)
    {
        this.building = building;
        hasInit = true;
        if (building.BuildingConfig == null) return;
        if (IsLocked) return;
       
        sequence = DOTween.Sequence();
    }
    
    public virtual void InitPlaceHolder()
    {
        CodeName = gameObject.name;
        foreach (SlotContainer slotContainer in InputItemSlots)
        {
            foreach (Transform item in slotContainer.SlotsContainer)
            {
                item.transform.parent = this.transform;
            }
           
        }
        foreach (SlotContainer slotContainer in OutputItemSlots)
        {
            foreach (Transform item in slotContainer.SlotsContainer)
            {
                item.transform.parent = this.transform;
            }

        }
        foreach (SlotContainer slotContainer in ActionSlots)
        {
            foreach (Transform item in slotContainer.SlotsContainer)
            {
                item.transform.parent = this.transform;
            }

        }
    }
    //private void Update()
    //{
    //    Processing();
    //}
    private bool CanProcess()
    {
        return !IsLocked && hasInit && building.BuildingConfig != null && building.CheckAvaliableSlotForProcessing();
    }
 
    public virtual void Processing()
    {
        if (!CanProcess()) return;
        currentTimeProcess += Time.deltaTime;
        if(currentTimeProcess >= building.BuildingConfig.Duration)
        {
            currentTimeProcess = 0f;          
            building.Processing();
        }
    }
    public void RemoveItem(ItemSlotType type, ItemController item)
    {
        sequence.Append(item.itemView.transform.DOMove(ModelProcess.transform.position, 1).OnComplete(() =>
        {
            item.itemView.gameObject.Despawn();

        }));
    }    
    public void AddItem(ItemSlotType type,ItemView itemView,ItemSlotDataModel freeSlot)
    {
        if (freeSlot == null) return;
        itemView.transform.position = ModelProcess.transform.position;
        sequence.Append(itemView.transform.DOMove(freeSlot.Slotpoint.position, 1).OnComplete(() =>
        {
            // Do something when complete
        }));
    }
   
    public void RemoveItem(ItemView itemView)
    {
        if (itemView == null) return;
        itemView.transform.parent = null;
    }
    public void AddItem(ItemView itemView, Transform slotPoint, Transform parent)
    {
        if (itemView == null) return;
        itemView.transform.parent = parent;
        Transform target = itemView.transform;
        target.DOLocalMove(slotPoint.localPosition, 0.5f);
    }
    public void GetItem(ItemView itemView, Transform slotPoint)
    {
        if (itemView == null) return;
        itemView.transform.parent = transform;
        Transform target = itemView.transform;
        target.DOLocalMove(slotPoint.localPosition, 0.5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("trigger unit");
        UnitView unitView = other.GetComponentInParent<UnitView>();
        if(unitView != null)
        {
            if (!unitView.unitController.unitData.IsPlayer) return;
            unitView.AddItemToBuilding(this);
            unitView.GetItemFromBuilding(this);
        }
    }

}
[System.Serializable]
public class SlotContainer
{
    [SerializeField]
    public string CodeName;
    public List<Transform> SlotsContainer;
    public void Init(int high)
    {
       
        var ItemSlotsContainerTemp = SlotsContainer.ToList();
        foreach (var item in ItemSlotsContainerTemp)
        {
            SlotsContainer.Remove(item);
        }
        foreach (var item in ItemSlotsContainerTemp)
        {
            GameHelper.InitListItemsSlotPos(item, item.parent, Vector2.one, high, callback: (slot) =>
            {
                SlotsContainer.Add(slot.transform);
            });
        }
       
    }
}
