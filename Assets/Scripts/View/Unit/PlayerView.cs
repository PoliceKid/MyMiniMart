using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class PlayerView : UnitView
{

    private bool hasInit;
    [SerializeField] LayerMask BuildingModelLayerMask;
    public override void Init(UnitController unitController, Vector3 startingPosition)
    {
        base.Init(unitController, startingPosition);
        hasInit = true;
    }
    // Update is called once per frame
    void Update()
    {
        if (!hasInit) return;
        Vector3 movementDir = UnitMovement.GetMovementDir();
        canMove = CheckColliderToMove(movementDir);
        if (movementDir != Vector3.zero)
        {
            if (canMove)
            {
                ChangeState(unitController.GetAnimationType().ToString());
                UnitMovement.HandleMovement(movementDir);
            }
          
        }
        else
        {
            ChangeState(AnimationType.Idle_Happy.ToString());
        }            
    }
    private bool canMove;
    private float distanceToMove = 2f;
    private bool CheckColliderToMove(Vector3 movementDir)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, movementDir, out hit, distanceToMove))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Model"))
            {               
                return false;
            }
        }
        return true;
    }
    private BuildingView targetBuildingAction;
    private void OnTriggerEnter(Collider other)
    {      
        BuildingView targetBuildingView = other.GetComponent<BuildingView>();
        if (targetBuildingView != null)
        {
            
            targetBuildingView?.OnPlayerInteract(1.2f);
            targetBuildingAction = targetBuildingView;
            //TimerHelper.instance.StartTimer(0.3f, () =>
            //{
                
            //});
          
        }
    }
    private string lastActionType;
    private Vector3 lastPlayerPosition;
    private void OnTriggerStay(Collider other)
    {
        if(targetBuildingAction != null)
        {            
            HandleAction();
        }

    }
    
    private void HandleAction()
    {
        Vector3 currentPlayerPosition = transform.position;

        // Ki?m tra n?u v? trí hi?n t?i c?a ng??i ch?i khác v?i v? trí tr??c ?ó
        if (currentPlayerPosition != lastPlayerPosition)
        {
            lastPlayerPosition = currentPlayerPosition;
            PointTriggerAction nearestActionPoint = targetBuildingAction.FindNearestActionPointOndistance(transform.position, lastActionType);
            if (nearestActionPoint != null)
            {
                ActionType type = GameHelper.ConvertStringToEnum(nearestActionPoint.CodeName);
                switch (type)
                {
                    case ActionType.Input:
                        AddItemToBuilding(targetBuildingAction);                        
                        break;
                    case ActionType.Output:
                        GetItemFromBuilding(targetBuildingAction);                        
                        break;
                    case ActionType.Thief:
                        break;
                    case ActionType.Process:
                        break;
                    default:
                        break;
                }
                lastActionType = type.ToString();

            }

        }
    }

    private void OnTriggerExit(Collider other)
    {       
        targetBuildingAction?.OnPlayerInteract(1);
        targetBuildingAction = null;
        lastActionType = "";
    }
 
}
