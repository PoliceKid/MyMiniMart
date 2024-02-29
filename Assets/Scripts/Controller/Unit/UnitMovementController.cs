using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class UnitMovementController : MonoBehaviour
{
    public NavMeshAgent agent;
    public bool IsAgentActive => agent.isActiveAndEnabled;
    public bool ReachedDestination
    {
        get
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
            //float remainingDistance = agent.GetPathRemainingDistance();
            //if ((remainingDistance >= 0
            //    && remainingDistance <= agent.stoppingDistance
            //    //&& agent.velocity.sqrMagnitude == 0f
            //    ))
            //{
            //    queueColliderReached = false;
            //    return true;
            //
            return false;
        }
    }
    bool queueColliderReached = false;
    public void Init()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.avoidancePriority = Random.Range(25, 100);
    }
    Vector3 PosLast = Vector3.zero;
    public void MoveTo(Vector3 movePosition)
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
    public bool isMoving => agent.velocity.sqrMagnitude > 0.01;
}
