using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillUnit : TriggerActionBase
{
    public override void Proceed(Unit unit)
    {
        UnitsController.KillUnit(unit);
        Destroy(unit.gameObject);
    }
}
