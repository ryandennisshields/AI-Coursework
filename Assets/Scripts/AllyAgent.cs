using System;
using Unity.VisualScripting;
using UnityEngine;

public class AllyAgent : SteeringAgent
{
    protected override void InitialiseFromAwake()
    {
        gameObject.AddComponent<GetTarget>();
        gameObject.AddComponent<SeekTarget>();
        gameObject.AddComponent<Attacking>();
    }

    protected override void CooperativeArbitration()
    {
        base.CooperativeArbitration();

        AttackWith(GetComponent<Attacking>().attackType);

        if (Input.GetMouseButtonDown(1))
        {
            SteeringVelocity = Vector3.zero;
            CurrentVelocity = Vector3.zero;
            var seekTarget = GetComponent<SeekTarget>();
            seekTarget.enabled = !seekTarget.enabled;
        }
    }
}
