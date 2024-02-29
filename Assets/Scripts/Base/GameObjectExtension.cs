using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtension 
{
    public static GameObject Spawn(this GameObject prefab, Vector3 worldPosition, Transform parent, bool prefabScale = false)
    {
        return PoolManager.Instance.Spawn(prefab, worldPosition, parent, prefabScale: prefabScale);
    }
    public static GameObject SpawnLocal(this GameObject prefab, Vector3 localPosition, Transform parent, bool prefabScale = false)
    {
        var go = PoolManager.Instance.Spawn(prefab, Vector3.zero, parent, prefabScale: prefabScale);
        go.transform.localPosition = localPosition;
        return go;
    }
    public static GameObject SpawnDelay(this GameObject prefab, Vector3 worldPosition, float duration, Transform parent)
    {
        return PoolManager.Instance.SpawnDelay(prefab, worldPosition, duration, parent);
    }
    public static RectTransform UISpawn(this GameObject prefab, Vector2 uiPosition, Transform parent)
    {
        var obj = PoolManager.Instance.Spawn(prefab, Vector3.zero, parent);
        var item = obj.GetComponent<RectTransform>();
        if (item)
        {
            item.anchoredPosition = uiPosition;
        }
        return item;
    }
    public static void Despawn(this GameObject obj, bool surpassWarning = false)
    {
        PoolManager.Instance.Kill(obj, surpassWarning);
    }
    public static void DespawnDelay(this GameObject obj, float duration, bool surpassWarning = false)
    {
        PoolManager.Instance.KillDelay(obj, duration, surpassWarning);
    }
    //public static AsyncOperationHandle<GameObject> GetPrefab(string codeName, string path = "Assets/Prefabs")
    //{
    //    path = string.Format("{0}/{1}.prefab", path, codeName);
    //    Debug.Log("Path Get Unit: " + path);
    //    return Addressables.LoadAssetAsync<GameObject>(path);
    //}
    //public static AsyncOperationHandle<GameObject> GetFxPrefab(string codeName, string path = "Assets/Prefabs/FX")
    //{
    //    path = $"{path}/{codeName}_FX.prefab";
    //    return Addressables.LoadAssetAsync<GameObject>(path);
    //}
}
