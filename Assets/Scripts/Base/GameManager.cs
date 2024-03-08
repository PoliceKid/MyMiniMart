using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; protected set; }
    #region MANAGER PROPERTY
    private BuildingManager buildingManager;
    private SpawnerManager spawnerManager;
    private PoolManager poolManager;
    private UnitManager unitManager;
    private CameraManager cameraManager;
    private TimerHelper timerManager;
    #endregion
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }      
        ConfigManager.Init();
    }
    private void InitManager()
    {
        poolManager ??= FindObjectOfType<PoolManager>();
        if (poolManager != null)
            poolManager.Init();
        spawnerManager ??= FindObjectOfType<SpawnerManager>();
        if (spawnerManager != null)
            spawnerManager.Init();
        buildingManager ??= FindObjectOfType<BuildingManager>();
        if (buildingManager != null)
            buildingManager.Init();
        unitManager ??= FindObjectOfType<UnitManager>();
        if (unitManager != null)
            unitManager.Init();
        cameraManager ??= FindObjectOfType<CameraManager>();
        if (cameraManager != null)
            cameraManager.Init();
        timerManager ??= FindObjectOfType<TimerHelper>();
        if (timerManager != null)
            timerManager.Init();
    }
    void Start()
    {
        InitManager();
        spawnerManager.SpawnPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
