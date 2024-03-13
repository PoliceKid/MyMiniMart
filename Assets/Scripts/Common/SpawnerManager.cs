using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SpawnerManager : MonoBehaviour
{
    public static SpawnerManager instance { get; private set; }
    [SerializeField] private List<Transform> SpawnPoints;
    [SerializeField] private Transform PlayerSpawnPoint;

    public static List<string> ListHatItems = new List<string>();
    public void Init()
    {
        if(instance == null)
        instance = this;
        ListHatItems = GameHelper.LoadAllNameOfItemInFolder("HatModels");
    }
    private void Update()
    {
        //Test add item to building
        if (Input.GetKeyDown(KeyCode.Escape))
        {
           
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {         
            CreateUnitView("stickman_Customer02", GetSpawnPoint().position, transform);
            CreateUnitView("stickman_Customer03", GetSpawnPoint().position, transform);
            CreateUnitView("stickman_Customer04", GetSpawnPoint().position, transform);
            CreateUnitView("stickman_Customer05", GetSpawnPoint().position, transform);
            CreateUnitView("stickman_Customer06", GetSpawnPoint().position, transform);
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
    public void SpawnPlayer()
    {
        UnitView player = CreateUnitView("Player", PlayerSpawnPoint.position, transform);
        CameraManager.Instance.FollowTarget(player.gameObject);
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
        string unitRsName = codeName.Contains("Player") ? "PlayerView" : "UnitView";
        var unitResource = Resources.Load($"CommonPrefabs/{unitRsName}") as GameObject;
        if (unitResource != null)
        {

            if (ConfigManager.Instance.GetUnitConfig(codeName) == null) return null;
            GameObject itemGo = unitResource.SpawnLocal(Vector3.zero, parent, prefabScale: true);
            itemGo.transform.localPosition = pos;

            UnitConfig unitConfig = ConfigManager.Instance.GetUnitConfig(codeName);

            if (unitConfig == null) return null;
            UnitView unitView = unitConfig.Type ==0? itemGo.GetComponent<PlayerView>(): itemGo.GetComponent<UnitView>();
            UnitController unitController = GetUnitControllerByType( (UnitType)unitConfig.Type );
            //codeName.Contains("Shelf") ? new ShevlerController() :
            unitController.Init(codeName,unitView);
            unitView.Init(unitController,pos);

            if(!codeName.Contains("Player"))
            UnitManager.Instance.AddUnit(unitView);

            if (unitView != null) return unitView;
        }
        return null;
    }
    public static UnitView CreatePlayerView(string codeName, Vector3 pos, Transform parent)
    {
        if (string.IsNullOrEmpty(codeName)) return null;
        var unitResource = Resources.Load($"CommonPrefabs/PlayerView") as GameObject;
        if (unitResource != null)
        {

            if (ConfigManager.Instance.GetUnitConfig(codeName) == null) return null;
            GameObject itemGo = unitResource.SpawnLocal(Vector3.zero, parent, prefabScale: true);
            itemGo.transform.localPosition = pos;

            UnitConfig unitConfig = ConfigManager.Instance.GetUnitConfig(codeName);

            if (unitConfig == null) return null;
            UnitView unitView = itemGo.GetComponent<PlayerView>();
            UnitController unitController = GetUnitControllerByType((UnitType)unitConfig.Type);
            //codeName.Contains("Shelf") ? new ShevlerController() :
            unitController.Init(codeName, unitView);
            unitView.Init(unitController, pos);
            UnitManager.Instance.AddUnit(unitView);
            if (unitView != null) return unitView;
        }
        return null;
    }
    private static UnitController GetUnitControllerByType(UnitType type)
    {
        switch (type)
        {
            case UnitType.Player:
                return new PlayerController();
            case UnitType.Worker:
                return new UnitController();
            case UnitType.Customer:
                return new CustomerController();
            default:
                return new UnitController();
        }
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
    public string GetRandomHatString()
    {
        return GameHelper.GetRandomInList(ListHatItems);
    }

    public static GameObject CreateHatModel(string codeName, Vector3 pos, Transform parent)
    {
        if (string.IsNullOrEmpty(codeName)) return null;
        var modelResource = Resources.Load($"HatModels/{codeName}") as GameObject;
        if (modelResource != null)
        {
            GameObject itemGo = modelResource.SpawnLocal(Vector3.zero, parent, prefabScale: true);
            if (itemGo != null) return itemGo;
        }
        return null;
    }
    public static void DespawnUnit(UnitView unit)
    {
        if (unit == null) return;
        unit.gameObject.Despawn();
    }
    public static void DespawnItem(ItemView item)
    {
        if (item == null) return;
        item.gameObject.Despawn();
    }
    public Transform GetSpawnPoint()
    {
        int randomIndex = Random.Range(0, SpawnPoints.Count);
        return SpawnPoints[randomIndex];
    }

}
