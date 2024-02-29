using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;
using System.Linq;
public static class GameHelper 
{
    // Kh?i t?o t?p h?p slot v?i size v?i high, 1 callback tr? v? item m?i khi t?o ra.
    public static List<GameObject> InitListItemsSlotPos(Transform centerPos,Transform parent,Vector2 size,int high, float distance = 1f, Action<GameObject> callback = null)
    {
        List<GameObject> list = new List<GameObject>();
        Vector3 conrnerPosition = new Vector3(centerPos.position.x - size.x / 2 * distance, centerPos.position.y, centerPos.position.z);
        for (int h = 0; h < high; h++)
        {
            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    GameObject itemSlot = new GameObject();

                    itemSlot.name = $"{i}:{j}";
                    itemSlot.transform.position = new Vector3(conrnerPosition.x + (i * distance), conrnerPosition.y + (h * distance), conrnerPosition.z + (j * distance));
                    itemSlot.transform.parent = parent;
                    list.Add(itemSlot);

                    if(callback != null)
                    {
                        callback?.Invoke(itemSlot);
                    }
                    
                }
            }
        }
        return list;

    }
    public static float GetPathRemainingDistance(this NavMeshAgent agent)
    {
        if (agent.pathPending ||
            agent.pathStatus == NavMeshPathStatus.PathInvalid ||
            agent.path.corners.Length == 0)
        {
            return -1f;
        }

        float distance = 0.0f;
        for (int i = 0; i < agent.path.corners.Length - 1; ++i)
        {
            distance += Vector3.Distance(agent.path.corners[i], agent.path.corners[i + 1]);
        }

        return distance;
    }
    public static string GetStringSplitSpaceRemoveLast(string codeName)
    {
        string actionCodeName = codeName;
        string[] parts = actionCodeName.Split("_");
        string roomCodeName = string.Join("_", parts.Take(parts.Length - 1));
        return roomCodeName;
    }
    public static ActionType ConvertStringToEnum(string input)
    {
        return (ActionType)Enum.Parse(typeof(ActionType), input);
    }
}
