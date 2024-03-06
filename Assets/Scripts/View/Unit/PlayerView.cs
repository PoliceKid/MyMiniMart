using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerView : UnitView
{

    private bool hasInit;
    private float capsuleHeight = 0.05f;
    private float capsuleRadius = 0.01f;
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
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("trigger unit");
        BuildingView targetBuildingView = other.GetComponent<BuildingView>();
        if (targetBuildingView != null)
        {
            targetBuildingView?.OnPlayerInteract(1.2f);
            SlotContainer nearestActionPoint = targetBuildingView.FindNearestActionPoint(transform.position);
            if(nearestActionPoint != null)
            {
                ActionType type = GameHelper.ConvertStringToEnum(nearestActionPoint.CodeName);
                switch (type)
                {
                    case ActionType.Input:
                        AddItemToBuilding(targetBuildingView);
                        break;
                    case ActionType.Output:
                        GetItemFromBuilding(targetBuildingView);
                        break;
                    case ActionType.Thief:
                        break;
                    case ActionType.Process:
                        break;
                    default:
                        break;
                }
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        BuildingView targetBuildingView = other.GetComponent<BuildingView>();
        targetBuildingView?.OnPlayerInteract(1);
    }
}
