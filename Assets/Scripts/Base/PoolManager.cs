using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    private static PoolManager instance;
    public static PoolManager Instance => instance;
    private Dictionary<GameObject, Pool> currentPools = new Dictionary<GameObject, Pool>();

    private List<DeSpamDelay> deSpamDelays = new List<DeSpamDelay>();
    private List<SpamDelay> spamDelays = new List<SpamDelay>();
    public void Init()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }
    }
    public GameObject Spawn(GameObject prefab, Vector3 position, Transform parent = null, bool isActive = true, bool prefabScale = false)
    {
        if (instance == null)
            instance = this;
        if (!currentPools.ContainsKey(prefab))
        {
            AddNewPool(new Pool(prefab, 0));
        }
        var go = currentPools[prefab].Spawn(position, parent: parent, isActive: isActive);
        if (prefabScale)
        {
            go.transform.localScale = prefab.transform.localScale;
        }
        return go;
    }
    public GameObject SpawnDelay(GameObject prefab, Vector3 position, float duration, Transform parent = null)
    {
        var obj = Spawn(prefab, position, parent, isActive: false);
        spamDelays.Add(new SpamDelay()
        {
            obj = obj,
            duration = duration,
            currentDuration = 0
        });
        return obj;
    }
    private void AddNewPool(Pool pool)
    {
        currentPools[pool.Prefab] = pool;

        GameObject poolObject = new GameObject();
        poolObject.name = pool.Prefab.name;
        //Debug.LogWarning("AddNewPool: " + (poolObject != null) + " / " + (instance != null));
        poolObject.transform.SetParent(instance.transform);
        pool.Transform = poolObject.transform;

        pool.InitPool();
    }
    public void Kill(GameObject obj, bool surpassWarning = false)
    {
        if (obj == null)
        {
            Debug.Log("<color=red>Warning: Despawning a null object.</color");
            return;
        }

        if (!obj.activeInHierarchy) // obj is already disabled or killed
        {
            return;
        }

        foreach (KeyValuePair<GameObject, Pool> pool in currentPools)
        {
            if (pool.Value.IsResponsibleForObject(obj))
            {
                pool.Value.Kill(obj);
                return;
            }
        }
        Destroy(obj);
    }

    private void Update()
    {
        if (deSpamDelays.Count > 0)
        {
            foreach (var item in deSpamDelays)
            {
                item.currentDuration += Time.deltaTime;
                if (item.duration <= item.currentDuration)
                {
                    Kill(item.obj, item.surpassWarning);

                }
            }
            deSpamDelays.RemoveAll(x => x.currentDuration > x.duration);
        }
        if (spamDelays.Count > 0)
        {
            foreach (var item in spamDelays)
            {
                item.currentDuration += Time.deltaTime;
                if (item.duration <= item.currentDuration)
                {
                    item.obj.SetActive(true);
                }
            }
            spamDelays.RemoveAll(x => x.currentDuration > x.duration);
        }
    }
    public void KillDelay(GameObject _obj, float time, bool _surpassWarning = false)
    {
        deSpamDelays.Add(new DeSpamDelay()
        {
            obj = _obj,
            duration = time,
            currentDuration = 0,
            surpassWarning = _surpassWarning

        });
    }

    public class SpamDelay
    {
        public GameObject obj;
        public float duration;
        public float currentDuration;
    }
    public class DeSpamDelay
    {
        public GameObject obj;
        public float duration;
        public float currentDuration;
        public bool surpassWarning;
    }
}
public class Pool
{
    [SerializeField]
    private GameObject prefab;
    public GameObject Prefab => prefab;

    [SerializeField]
    private int initialPoolsize = 10;
    private Stack<GameObject> pooledInstances;
    private List<GameObject> activeInstances;
    public Transform Transform
    {
        get; set;
    }
    public Pool(GameObject prefab, int initialPoolsize)
    {
        this.prefab = prefab;
        this.initialPoolsize = initialPoolsize;
    }
    public void InitPool()
    {
        pooledInstances = new Stack<GameObject>();
        activeInstances = new List<GameObject>();

        for (int i = 0; i < initialPoolsize; i++)
        {
            GameObject instance = GameObject.Instantiate(prefab);
            instance.transform.SetParent(Transform);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localScale = Vector3.one;
            instance.transform.localEulerAngles = Vector3.zero;

            InvokeEvent(instance, PoolEvent.Despawn);

            instance.SetActive(false);

            pooledInstances.Push(instance);
        }
    }
    int count = 0;
    public GameObject Spawn(Vector3 position,
        Quaternion rotation = default,
        Vector3 scale = default,
        Transform parent = null,
        bool useLocalPosition = false,
        bool useLocalRotation = false,
        bool isActive = true)
    {
        if (pooledInstances.Count <= 0) // Every game object has been spawned!
        {
            GameObject freshObject = Object.Instantiate(prefab);
            freshObject.name += count++;
            InvokeEvent(freshObject, PoolEvent.Create);
            pooledInstances.Push(freshObject);
        }

        GameObject obj = pooledInstances.Pop();

        obj.transform.SetParent(parent);
        if (useLocalPosition)
            obj.transform.localPosition = position;
        else
            obj.transform.position = position;
        if (rotation.eulerAngles == Quaternion.identity.eulerAngles)
        {
            rotation = prefab.transform.rotation;
        }
        if (useLocalRotation)
            obj.transform.localRotation = rotation;
        else
            obj.transform.rotation = rotation;
        if (scale == default)
            scale = Vector3.one;
        obj.transform.localScale = scale;
        SetActiveSafe(obj, isActive);

        activeInstances.Add(obj);
        InvokeEvent(obj, PoolEvent.Spawn);

        return obj;
    }
    /// <summary>
    /// Deactivate an object and add it back to the pool, given that it's
    /// in alive objects array.
    /// </summary>
    /// <param name="obj"></param>
    public void Kill(GameObject obj)
    {
        int index = activeInstances.FindIndex(o => obj == o);
        if (index == -1)
        {
            Object.Destroy(obj);
            return;
        }
        InvokeEvent(obj, PoolEvent.Despawn);

        obj.transform.SetParent(Transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;
        obj.transform.localEulerAngles = Vector3.zero;

        pooledInstances.Push(obj);
        activeInstances.RemoveAt(index);

        SetActiveSafe(obj, false);
    }
    private void SetActiveSafe(GameObject obj, bool value)
    {
        if (obj.activeSelf != value)
        {
            obj.SetActive(value);
        }
    }
    public bool IsResponsibleForObject(GameObject obj)
    {
        int index = activeInstances.FindIndex(o => obj == o);
        if (index == -1)
        {
            return false;
        }
        return true;
    }
    private enum PoolEvent { Spawn, Despawn, Create }
    private void InvokeEvent(GameObject instance, PoolEvent ev)
    {
        var poolScripts = instance.GetComponentsInChildren<IPoolObject>();

        if (ev == PoolEvent.Spawn)
        {
            foreach (IPoolObject poolScript in poolScripts)
            {
                poolScript.OnSpawn();
            }
        }
        else if (ev == PoolEvent.Despawn)
        {
            foreach (IPoolObject poolScript in poolScripts)
            {
                poolScript.OnDespawn();
            }
        }
        else if (ev == PoolEvent.Create)
        {
            foreach (IPoolObject poolScript in poolScripts)
            {
                poolScript.OnCreated();
            }
        }
    }
}
public interface IPool<T>
{
    void Prewarm(int amount);
    T Request();
    void Return(T obj);
}
public interface IPoolObject
{
    void OnSpawn();
    void OnDespawn();
    void OnCreated();
}
