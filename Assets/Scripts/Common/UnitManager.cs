using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class UnitManager : MonoBehaviour
{
    [SerializeField]
    private List<Transform> existedUnit;
    [SerializeField] List<Transform> ExitPoints;
    public static UnitManager Instance { get; private set; }
    private bool hasInit;

    public static List<UnitView> unitViews = new List<UnitView>();
  
    public void Init()
    {
        if (Instance == null)
            Instance = this;
        hasInit = true;
    }
    public void AddUnit(UnitView unitView)
    {
        if (unitView == null) return;
        if (!unitViews.Contains(unitView))
        {
            unitViews.Add(unitView);        
            UnitConfig unitConfig = unitView.unitController.unitConfig;
            if (unitConfig.FollowRoutine)
            {
                unitView.unitController.StartCommand();
            }
        }
    }
    public void RemoveUnit(UnitView unitView)
    {
        if (unitViews.Contains(unitView))
        {
            unitViews.Remove(unitView);
            unitView.Disposed();
            SpawnerManager.DespawnUnit(unitView);
        }
    }
    private void Update()
    {
        Tick();
        
    }
    public void Tick()
    {
        if (!hasInit) return;
        List<UnitView> unitsToRemove = new List<UnitView>();
        foreach (var unit in unitViews)
        {
            if (unit.unitController.unitData.IsCanExited)
            {
                unitsToRemove.Add(unit);
            }
            else
            unit.Tick(Time.deltaTime);
        }
        foreach (var unitToRemove in unitsToRemove)
        {
            RemoveUnit(unitToRemove);
        }
    }

    public List<Transform> GetExistedUnit()
    {
        return existedUnit;
    }
    public Transform GetExitPoint()
    {
        int randomIndex = UnityEngine.Random.Range(0, ExitPoints.Count);
        return ExitPoints[randomIndex];
    }
    public void Disposed()
    {

    }
}
