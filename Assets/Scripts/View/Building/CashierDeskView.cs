using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CashierDeskView : BuildingView
{
    #region PLACEHOLDER

    #endregion
    public override void Init(BuildingController building)
    {
        base.Init(building);

    }

    public override void InitPlaceHolder()
    {
        base.InitPlaceHolder();
        if (CodeName.Contains("CashierDesk"))
        {
            if (InputItemSlots.Count > 0)
            {
                foreach (var item in InputItemSlots)
                {
                    item.Init(2);
                }
            }
            if (OutputItemSlots.Count > 0)
            {
                foreach (var item in OutputItemSlots)
                {
                    item.Init(8);
                }
            }
        }
        foreach (var item in ActionSlots)
        {
            foreach (var slot in item.SlotsContainer)
            {
                slot.gameObject.SetActive(false);
            }
        }
    }
}
