using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UnitManager : MonoBehaviour
{
    [SerializeField]
    private List<Transform> existedUnit;
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
}
