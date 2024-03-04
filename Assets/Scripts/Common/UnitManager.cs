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
        RegisterEvent();
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
    public void RegisterEvent()
    {
        CashierDeskController.OnUnitFinishRoutine += HandleUnitFinishRoutine;
    }

    private void HandleUnitFinishRoutine(UnitController unit)
    {
        unit.FinishRoutine(GetExitPoint().position);
    }

    private void Update()
    {
        Tick();
        
    }
    public void Tick()
    {
        if (!hasInit) return;
        foreach (var unit in unitViews)
        {
            if (unit.unitController.unitData.IsPlayer) continue;
            //if (!unit.unitController.unitData.IsState(UnitState.Actioning)) continue;
            unit.Tick(Time.deltaTime);
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
}
