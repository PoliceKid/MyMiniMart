using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class ConfigManager
{
    public static ConfigManager Instance { get; protected set; }

    public Dictionary<string, BuildingConfig> BuildingConfigs = new Dictionary<string, BuildingConfig>();
    public Dictionary<string, UnitConfig> UnitConfigs = new Dictionary<string, UnitConfig>();
    public static void Init()
    {
        Instance = new ConfigManager();
        Instance.Load();
    }
    private void Load()
    {
        var buildingConfigResources = Resources.Load("Config/BuildingConfig") as TextAsset;
        string buildingConfigContent = buildingConfigResources.text;
        BuildingConfigs = GenericConfigTypeHelpers<BuildingConfig>.ReadDictionaryConfig(buildingConfigContent);

        var unitConfigResources = Resources.Load("Config/UnitConfig") as TextAsset;
        string unitConfigContent = unitConfigResources.text;
        UnitConfigs = GenericConfigTypeHelpers<UnitConfig>.ReadDictionaryConfig(unitConfigContent);

    }
    #region GET CONFIG FUNCTION

    public BuildingConfig GetBuildingConfig(string codeName)
    {
        if(BuildingConfigs.TryGetValue(codeName, out var buildingConfig))
        {
            return buildingConfig;
        }
        return null;
    }
    public UnitConfig GetUnitConfig(string codeName)
    {
        if (UnitConfigs.TryGetValue(codeName, out var unitConfig))
        {
            return unitConfig;
        }
        return null;
    }
    #endregion
    public static class GenericConfigTypeHelpers<T> where T : BaseConfig
    {
        /// <summary>
        /// Convenient function to read dictionaries of [codename - config] pairs.
        /// </summary>
        public static Dictionary<string, T> ReadDictionaryConfig(string content)
        {
            Dictionary<string, T> result = JsonMapper.ToObject<Dictionary<string, T>>(content);
            foreach (KeyValuePair<string, T> p in result)
            {
                p.Value.SetCodeName(p.Key);
            }
            return result;
        }
    }
   
}
[System.Serializable]
public class BaseConfig
{
    public string CodeName { get; protected set; }
    public void SetCodeName(string value)
    {
        CodeName = value;
    }
}
[System.Serializable]
public class BuildingConfig : BaseConfig
{
    public int Type;
    public float Duration;

    public Dictionary<string, List<ItemConfig>> ItemPool { get; set; } 

    public Dictionary<string, ActivityConfig> Activities = new Dictionary<string, ActivityConfig>();

    public ActivityConfig GetActivityConfig(string codeName)
    {
        if(Activities.TryGetValue(codeName, out ActivityConfig activityConfig))
        {
            return activityConfig;
        }
        return null;
    }
    public List<ItemConfig> GetListItemConfig(string ItemSlotTypecodeName)
    {
        //example: Input or output
        if (ItemPool.TryGetValue(ItemSlotTypecodeName, out var itemConfigs))
        {
            return itemConfigs;
        }
        return null;
    }
    public ItemConfig GetItemConfig(string ItemSlotTypecodeName, string codeName)
    {
        // Example: egg, veg_tomato
        var listItemConfig= GetListItemConfig(ItemSlotTypecodeName);
        var itemConfig = listItemConfig.FirstOrDefault(x => x.CodeName == codeName);
        return itemConfig;
    }
}
public class ActivityConfig:BaseConfig
{
    public string ModelItemCodeName;
    public float Duration;
}
[System.Serializable]
public class ItemConfig: BaseConfig
{
    public int MaxSlot;
    public int AmoutProcess = 1;
}
[System.Serializable]
public class UnitConfig : BaseConfig
{
    public int Type;
    public bool FollowRoutine;
    public bool IsLoopInfinite;
    public float BaseMoveSpeed;
    public List<RoutineConfig> Routine= new List<RoutineConfig>();
    public RoutineConfig GetRoutineConfig(string codeName)
    {
        return Routine.FirstOrDefault(x => x.CodeName == codeName);
    }
}

[System.Serializable]
public class RoutineConfig:BaseConfig {
    public int Quantity;
    public string ItemCodeName;
}
