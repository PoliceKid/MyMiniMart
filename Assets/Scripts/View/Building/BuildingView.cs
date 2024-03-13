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
    public Transform DummySpawnPos;
    [SerializeField]
    public Transform InteractRange;

    [SerializeField]
    public List<SlotContainer> InputItemSlots = new List<SlotContainer>();
    [SerializeField]
    public List<SlotContainer> OutputItemSlots = new List<SlotContainer>();
    [SerializeField]
    public List<SlotContainer> ActionSlots = new List<SlotContainer>();

    private List<PointTriggerAction> ActionTriggerPoints = new List<PointTriggerAction>();
    #endregion

    protected GameObject model;
    private float currentTimeProcess =0;
    private bool hasInit;
    public BuildingController building { get; private set; }

    public virtual void Init(BuildingController building)
    {
        this.building = building;
        hasInit = true;
        if (building.BuildingConfig == null) return;
        if (IsLocked) return;           
        InitSpawnEffect();
    }
    private void InitSpawnEffect()
    {
        transform.localScale = Vector3.one * 0.2f;
        transform.DOScale(Vector3.one, 1f).SetEase(Ease.InOutElastic);
    }
    public virtual void InitPlaceHolder()
    {
        CodeName = gameObject.name;

        foreach (SlotContainer slotContainer in InputItemSlots)
        {
            slotContainer.Init();
            PointTriggerAction inputPointTrigger = new PointTriggerAction(ItemSlotType.Input.ToString(), slotContainer.ContainerPoint);
            ActionTriggerPoints.Add(inputPointTrigger);
            foreach (Transform item in slotContainer.SlotsContainer)
            {
                item.transform.parent = this.transform;
            }
           
        }
        foreach (SlotContainer slotContainer in OutputItemSlots)
        {
            slotContainer.Init();
            PointTriggerAction outputPointTrigger = new PointTriggerAction(ItemSlotType.Output.ToString(), slotContainer.ContainerPoint);
            ActionTriggerPoints.Add(outputPointTrigger);
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
    private bool CanProcess()
    {
        return !IsLocked && hasInit && building.BuildingConfig != null && building.CheckAvaliableSlotItemForProcessing();
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
    public void MoveItemToModelProcess( ItemController item)
    {
        Sequence sequence = DOTween.Sequence();
        sequence?.Append(item.itemView.transform.DOMove(ModelProcess.transform.position, 1).OnComplete(() =>
        {
            item.itemView.gameObject.Despawn();

        }));
    }
    public void RemoveItemAwayfromBuilding(ItemView itemView)
    {
        if (itemView == null) return;
        itemView.transform.parent = null;
    }
    public void AddItemToOutput(ItemView itemView, ItemSlotDataModel freeSlot)
    {
        if (freeSlot == null) return;
       // itemView.transform.parent = transform;
        if (ModelProcess != null)
        {
            itemView.transform.position = ModelProcess.transform.position;
        }
        Sequence sequence = DOTween.Sequence();
        sequence?.Append(itemView.transform.DOMove(freeSlot.Slotpoint.position, 1));
    }
    public void GetItemToInput(ItemView itemView, Transform slotPoint)
    {
        if (itemView == null) return;
        itemView.transform.parent = transform;
        Transform target = itemView.transform;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(target.DOLocalMove(slotPoint.localPosition, 0.5f));
    }

    public PointTriggerAction FindNearestActionPointOndistance(Vector3 playerPosition, string ActionType = null)
    {
        PointTriggerAction nearestActionPoint = ActionTriggerPoints.Where(x => x.Point != null && x.CodeName != ActionType)
            .FirstOrDefault(ap => Vector3.Distance(playerPosition, ap.Point.position) <=7f);
        return nearestActionPoint;
    }
    public void OnPlayerInteract(float scaleValue)
    {
        InteractRange?.DOScale(Vector3.one * scaleValue, 0.2f);
    }
}
[System.Serializable]
public class SlotContainer
{
    [SerializeField]
    public string CodeName;
 
    public List<Transform> SlotsContainer;
    public Transform ContainerPoint { get; private set; }
    public void Init()
    {
        if (SlotsContainer.Count == 0) return;
        ContainerPoint = SlotsContainer[0];
    }
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
[SerializeField]
public class PointTriggerAction
{
 

    public string CodeName { get; private set; }
    public Transform Point { get; private set; }

    public PointTriggerAction(string codeName, Transform point)
    {
        CodeName = codeName;
        Point = point;
    }
}

