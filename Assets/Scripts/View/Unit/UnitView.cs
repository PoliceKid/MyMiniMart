using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class UnitView : MonoBehaviour
{
    #region PLACEHOLDER
    [SerializeField] private Transform holdContainer;
    [SerializeField] private Vector2 holdSize;
    [SerializeField] private int holdContainerHigh;
    [SerializeField] Transform VisualContainer;
    #endregion
    #region DATA
    public UnitController unitController { get; private set; }
    public IMovement UnitMovement { get; private set; }
    public UnitAnimationController UnitAnimation{ get; private set; }
    #endregion
    #region INIT FUNCTIONS
    public virtual void Init(UnitController unitController, Vector3 startingPosition)
    {
        this.unitController = unitController;
        RegisterEvents(unitController);
        LoadModel(unitController.unitData.CodeName, unitController.unitData.HatCodeName);

        UnitMovement = GetComponent<IMovement>();
        UnitAnimation = GetComponent<UnitAnimationController>();
        // Init Movement
        UnitMovement.Init();
        UnitMovement.SetPosition(startingPosition);
        UnitMovement.SetSpeed(unitController.unitData.Speed);
        // Init Animation
        UnitAnimation.Init(VisualContainer,"Idle_Happy");
        // Init Item slot carry
        InitListItemsCarryPos();
    }
    public void LoadModel(string codeName,string hatCodeName)
    {
        GameObject unitModelGO = SpawnerManager.CreateUnitModel(codeName,Vector3.zero, VisualContainer);
      
        UnitModelView unitModelView = unitModelGO.GetComponent<UnitModelView>();
        GameObject hatModelGO = SpawnerManager.CreateHatModel(hatCodeName, Vector3.zero, unitModelView.hatSlotTransform);
        if(hatCodeName != null)
        {
           unitModelView?.InitModel(hatModelGO);
        }
    }
    protected void RegisterEvents(UnitController unit)
    {

        //unitData.OnTargetBuildingSet += HandleTargetBuildingSet;
        unit.CheckDestinationReached = () => UnitMovement.ReachedDestination();
    }
    protected void UnRegisterEvents(UnitDataModel unitData)
    {
        //unitData.OnTargetBuildingSet -= HandleTargetBuildingSet;
    }

    #region HANDLE ACTIONS FUNCTION
    private void HandleTargetBuildingSet(BuildingController building)
    {
        SetTargetMovePosition(building.BuildingView.transform.position,AnimationType.Run);
    }
    #endregion

    #endregion
    #region ITEM FUNCTIONS
    public void AddItemToBuilding(BuildingView buildingView)
    {
        unitController.AddItemToBuilding(buildingView.building);
    }
    public void GetItemFromBuilding(string ItemCodeName, BuildingView buildingView)
    {
        unitController.GetItemFromBuilding(ItemCodeName, buildingView.building);
    } 
    public void GetItemFromBuilding(BuildingView buildingView)
    {
        unitController.GetItemFromBuilding( buildingView.building);
    }
    public void GetItem(ItemView itemView,Transform slotPoint)
    {
        if (itemView == null) return;

        itemView.transform.parent = transform;
        Transform target = itemView.transform;
        target.DOLocalMove(slotPoint.localPosition, 1f);
    }
    public void AddItem(ItemView itemView, Transform slotPoint, Transform parent)
    {
        if (itemView == null) return;
        itemView.transform.parent = parent;
        Transform target = itemView.transform;
        target.DOLocalMove(slotPoint.localPosition, 0.5f);
    }
    #endregion
    #region SORT ITEM
    public void InitListItemsCarryPos()
    {
        GameHelper.InitListItemsSlotPos(holdContainer, transform, holdSize, holdContainerHigh, callback: (itemCarry) =>
        {
            unitController.CreateSlotItemCarry(itemCarry.transform);
        });
    }
    #endregion

    #region ACTION
    public void Tick(float deltaTime)
    {
        unitController.Tick(deltaTime);
    }
    public void SetTargetMovePosition(Vector3 position, AnimationType animationType)
    {
        UnitMovement.HandleMovement(position);
        ChangeState(animationType.ToString());
    }
    public void ChangeState(string newState)
    {
        if (string.IsNullOrEmpty(newState)) return;
        UnitAnimation.ChangeState(newState);
    }
    #endregion
    public void Disposed()
    {
        unitController.Disposed();
    }
}
public enum AnimationType
{
    Idle_Happy =0,
    Run_Carry =1,
    Run =2,
    Idle_Happy_Carry =3
}
public interface IMovement
{
    public void Init();
    public void HandleMovement(Vector3 movePosition);
    public void SetSpeed(float value);
    public bool ReachedDestination();
    public void SetPosition(Vector3 pos);
    public Vector3 GetMovementDir();
}