using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddUnit : TriggerActionBase
{
    public override void Proceed(Unit unit)
    {
        UnitsController.AddUnit(transform.position);
        Destroy(gameObject);
    }
}
