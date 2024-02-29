using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
public class ItemView : MonoBehaviour
{
    [SerializeField]
    public string Codename;
    public Transform visualContainer;
    private GameObject model;
    public ItemController ItemController;
    public void Init(ItemController ItemController)
    {
        this.ItemController = ItemController;
        Codename = ItemController.itemData.CodeName;
        model = CreateVisualModel(GetMainitemCodeName(ItemController.itemData.CodeName));
    }
    private GameObject CreateVisualModel(string codeName)
    {
        foreach (Transform item in visualContainer)
        {
            item.gameObject.Despawn();
        }
        var resource = Resources.Load($"ItemModels/{codeName}") as GameObject;
        if(resource != null)
        {
            var go = resource.SpawnLocal(Vector3.zero, visualContainer,prefabScale:true);
            return go;
        }
        return null;
    }
    public string GetMainitemCodeName(string codeName)
    {
        if (string.IsNullOrEmpty(codeName)) return "";
        string pattern = @"^(.*?)(?:\d{2}_Produce)?$";
        Match match = Regex.Match(codeName, pattern);
        string result = match.Groups[1].Value;
        return result;
    }
}
