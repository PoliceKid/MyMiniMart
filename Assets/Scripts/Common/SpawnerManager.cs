using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SpawnerManager : MonoBehaviour
{
    public static SpawnerManager instance { get; private set; }
    [SerializeField] List<Transform> SpawnPoints;
    public void Init()
    {
        if(instance == null)
        instance = this;
    }
    private void Update()
    {
        //Test add item to building
        if (Input.GetKeyDown(KeyCode.Escape))
        {
           CreateUnitView("Player", Vector3.zero, transform);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {         
            CreateUnitView("stickman_Customer02", GetSpawnPoint().position, transform);
            CreateUnitView("stickman_Customer03", GetSpawnPoint().position, transform);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            foreach (var item in UnitManager.Instance.GetExistedUnit())
            {
                item.gameObject.SetActive(false);
                UnitView unitView = CreateUnitView(item.gameObject.name, item.position, transform);
                
            }
           
        }
    }
    public static ItemView CreateItemView(string codeName, Vector3 pos, Transform parent,bool prefabScale = true)
    {
        if (string.IsNullOrEmpty(codeName)) return null;
        var modelResource = Resources.Load($"CommonPrefabs/ItemView") as GameObject;
        if (modelResource != null)
        {
            GameObject itemGo = modelResource.SpawnLocal(Vector3.zero, parent, prefabScale: prefabScale);
            itemGo.transform.position = pos;
            ItemView itemView = itemGo.GetComponent<ItemView>();
            ItemController itemController = new ItemController();
            itemController.Init(codeName, itemView);
            itemView.Init(itemController);
            if (itemView != null) return itemView;
        }
        return null;
    }

    public static UnitView CreateUnitView(string codeName, Vector3 pos, Transform parent)
    {
        if (string.IsNullOrEmpty(codeName)) return null;
        var unitResource = Resources.Load($"CommonPrefabs/UnitView") as GameObject;
        if (unitResource != null)
        {

            if (ConfigManager.Instance.GetUnitConfig(codeName) == null) return null;
            GameObject itemGo = unitResource.SpawnLocal(Vector3.zero, parent, prefabScale: true);
            itemGo.transform.localPosition = pos;
            UnitView unitView = itemGo.GetComponent<UnitView>();
            UnitController unitController = codeName.Contains("Customer") ? new CustomerController() :new UnitController();
            //codeName.Contains("Shelf") ? new ShevlerController() :
            unitController.Init(codeName,unitView);
            unitView.Init(unitController,pos);

            //Init Action data for command chain data
            UnitConfig unitConfig = unitView.unitController.unitConfig;
            if(unitConfig != null)
            {

               
            }            
            UnitManager.Instance.AddUnit(unitView);
            if (unitView != null) return unitView;
        }
        return null;
    }
    public static GameObject CreateUnitModel(string codeName, Vector3 pos, Transform parent)
    {
        if (string.IsNullOrEmpty(codeName)) return null;
        var modelResource = Resources.Load($"UnitModels/{codeName}") as GameObject;
        if (modelResource != null)
        {
            GameObject itemGo = modelResource.SpawnLocal(Vector3.zero, parent, prefabScale: true);
            if (itemGo != null) return itemGo;
        }
        return null;
    }
    public Transform GetSpawnPoint()
    {
        int randomIndex = Random.Range(0, SpawnPoints.Count);
        return SpawnPoints[randomIndex];
    }
}
