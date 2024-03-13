using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }
    private bool hasInit;
    public List<BuildingView> buildingViews = new List<BuildingView>();
       
    #region INIT METHOD
    public void Init()
    {
        if(Instance== null)
            Instance = this;
        InitBuildingView();
        hasInit = true;
    }

    private void InitBuildingView()
    {
        foreach (Transform item in transform)
        {
            BuildingView buildingView = item.GetComponent<BuildingView>();
            if (buildingView != null)
            {              
                buildingView.gameObject.SetActive(!buildingView.IsLocked);
                
                InitBuilding(buildingView);
                if (buildingView.building == null) continue;
                if (!buildingView.IsLocked && buildingView.building.BuildingConfig != null)
                {
                    buildingViews.Add(buildingView);
                }                          
            }

        }
    }
    public List<CommandData> GetListSupportCommandBuilding(string commandKey)
    {
        List<CommandData> listSupportCommandBuilding = new List<CommandData>();
        if (commandKey.Contains(ItemSlotType.Output.ToString())) return listSupportCommandBuilding;
        BuildingView buildingView = GetBuildingView(GameHelper.GetStringSplitSpaceRemoveLast(commandKey));
        if(buildingView == null) return null;
       
        var ItemInputConfigs = buildingView.building.GetItemSlots(ItemSlotType.Input);
        if (ItemInputConfigs != null)
        {
            foreach (var itemConfig in ItemInputConfigs.Keys)
            {

                var listBuildingWithItems = GetBuildingWithItem(ItemSlotType.Output, itemConfig);
                if (listBuildingWithItems != null)
                {
                    foreach (var bView in listBuildingWithItems)
                    {
                        if (bView.building.CheckBuildingType(BuildingType.Shelf)) continue;
                        CommandData commandData = new CommandData(bView.CodeName + "_" + ItemSlotType.Output);
                        listSupportCommandBuilding.Add(commandData);
                    }
                }
            }
            return listSupportCommandBuilding;
        }
        return null;
    }
    
    private void InitBuilding(BuildingView buildingView)
    {
        if(buildingView == null|| buildingView.IsLocked) return;
        buildingView.InitPlaceHolder();
        string codeName = buildingView.CodeName;
        var buildingConfig = ConfigManager.Instance.GetBuildingConfig(codeName);
        if (buildingConfig == null)
        {
            Debug.Log("Init Building That bai! - Khong Ton tai config building: " + codeName);
            return;
        }
        BuildingController building = CreateBuildingController(codeName);
        building.Init(new BuildingInitData()
        {
            CodeName = codeName,
            BuildingConfig = buildingConfig,
            InputItemSlotDatas = GetItemSlotData(buildingView.InputItemSlots),
            OutputItemSlotDatas = GetItemSlotData(buildingView.OutputItemSlots),
            ActionSlots = GetItemSlotData(buildingView.ActionSlots)
        }, buildingView);
        buildingView.Init(building);
    }
    private List<BuildingInitItemData> GetItemSlotData(List<SlotContainer> ItemSlotPlaceHolders)
    {
        if (ItemSlotPlaceHolders == null) return null;
        var ItemDatas = new List<BuildingInitItemData>();
        foreach (var item in ItemSlotPlaceHolders)
        {
            var slotContainer = new BuildingInitItemData()
            {
                CodeName = item.CodeName,
                AcitonPoint = GetPointDatas(item.SlotsContainer)
            };
            ItemDatas.Add(slotContainer);
        }
        return ItemDatas;
    }
    private List<PointData> GetPointDatas(List<Transform> slotContainerPlaceHolders)
    {
        if (slotContainerPlaceHolders == null) return null;
        var points = new List<PointData>();
        foreach (var item in slotContainerPlaceHolders)
        {   
            PointData point = new PointData()
            {
                Point = item.transform
            };
            points.Add(point);
        }
        return points;
    }
    #endregion
    #region GET BUILIDING METHOD
    public List<BuildingView> GetBuildingWithItem(ItemSlotType type,string codeName)
    {
        return buildingViews.Where(x => x.building.CheckBuildingExistTypeItem(type,codeName)).ToList();
    }
    #endregion
    #region BUILDING API 

  
    public BuildingView GetBuildingView(string codeName)
    {
        BuildingView buildingView = null;
        buildingView = buildingViews.FirstOrDefault(x => x.CodeName == codeName);
        return buildingView;
    }
    public BuildingController GetTargetBuilding(CommandData action)
    {
        if(action == null) return null;
        string actionCodeName = action.CodeName;
        string[] parts = actionCodeName.Split("_");
        string roomCodeName = string.Join("_", parts.Take(parts.Length - 1));
        BuildingView buildingView = GetBuildingView(roomCodeName);
        if(buildingView == null) return null;
        return buildingView.building;
  
    }
    #endregion

    #region MONOBEHAVIOR METHOD
    private void Update()
    {
        if (!hasInit) return;
        foreach (var item in buildingViews)
        {
            item.Processing();
        }
    }
    #endregion

    #region CREATE METHOD
    private BuildingController CreateBuildingController(string codeName)
    {
        return codeName.Contains("CashierDesk")? new CashierDeskController() : new BuildingController();
    }
    #endregion
}
public enum BuildingType
{
    Cashier =0,
    Shelf =1,
    Farm =2,
    FarmAutoProcess =3
}
