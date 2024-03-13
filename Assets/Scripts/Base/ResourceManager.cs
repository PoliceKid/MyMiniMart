using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }
    //Data
    public static Dictionary<string, Resource> Resources;
    //Action
    public Action<string,float> OnUpdateResource = delegate { };

    //UI Manager
    [SerializeField] List<ResourceUI> resourcesUIs = new List<ResourceUI>();

    public void Init()
    {
        if(Instance == null)
        {
            Instance = this;
        }      

        Resources = new Dictionary<string, Resource>();
        Resources.Add(ResourceType.Cash.ToString(), new Resource(ResourceType.Cash.ToString(),0));
        Resources.Add(ResourceType.Coin.ToString(), new Resource(ResourceType.Coin.ToString(),0));

        foreach (var resourceUI in resourcesUIs)
        {
            resourceUI.Init();
        }
    }
    [ContextMenu("Test Update Resource")]
    public void TestUpdateRs()
    {
        UpdateResource(ResourceType.Cash, 10);
    }
    public void UpdateResource(ResourceType resourceType, float value)
    {
        string key = resourceType.ToString();
        if (!Resources.ContainsKey(key)) return;
        Resources[key].Value += value;

        Resources[key].Value = Mathf.Clamp(Resources[key].Value, 0, float.MaxValue);
        OnUpdateResource?.Invoke(key,Resources[key].Value);
        UpdateUImanager(key, Resources[key].Value);
    }


    public void UpdateUImanager(string resourceType, float value)
    {
        ResourceUI resourcesUI = resourcesUIs.FirstOrDefault(x => x.type.ToString() == resourceType);

        resourcesUI?.UpdateValue(value);
    }
}
public enum ResourceType
{
    Cash =0,
    Coin =1
}
[System.Serializable]
public class Resource
{
    public string CodeName;
    public float Value;

    public Resource(string codeName, float value)
    {
        CodeName = codeName;
        Value = value;
    }
}