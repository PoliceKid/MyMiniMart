using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class UnitMovementController : MonoBehaviour, IMovement
{
    public NavMeshAgent agent;
    public bool IsAgentActive => agent.isActiveAndEnabled;
    public bool ReachedDestination()
    {       
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }
        return false;
        
    }
    bool queueColliderReached = false;
    public void Init()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.avoidancePriority = Random.Range(25, 100);
    }
    Vector3 PosLast = Vector3.zero;
    public void HandleMovement(Vector3 movePosition)
    {
        PosLast = movePosition;
        try
        {
            agent.SetDestination(movePosition);
        }
        catch (System.Exception e)
        {
            Debug.Log($"{name}: {e.Message}");
        }

    }
    public void SetPosition(Vector3 pos)
    {
        agent.Warp(pos);
    }
    public void SetSpeed(float value)
    {
        agent.speed = value;
    }

    public void ToggleActive(bool isActive)
    {
        agent.enabled = isActive;
    }
    public NavMeshAgent GetNavMeshAgent()
    {
        return agent;
    }

    public Vector3 GetMovementDir()
    {
        throw new System.NotImplementedException();
    }

    public bool isMoving => agent.velocity.sqrMagnitude > 0.01;
}
