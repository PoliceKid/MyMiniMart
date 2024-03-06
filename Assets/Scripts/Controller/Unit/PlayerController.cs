using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : UnitController
{
    public const string PLAYER_HAT_STRING = "hat_Rabbit";

    public override void Init(string codeName, UnitView unitView)
    {
        base.Init(codeName, unitView);
        SetHatCodeName(PLAYER_HAT_STRING);
    }
}
