using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class SeekTarget : SteeringBehaviour
{
    private GetTarget target;
    private Vector3 targetPosition;

    private float fleeDuration = 0;
    private bool stopFleeing = false;
    private bool rocketIncoming = false;
    private Vector3 rocketPosition;

    private new void Start()
    {
        target = GetComponent<GetTarget>();
    }

    public override Vector3 UpdateBehaviour(SteeringAgent steeringAgent)
    {
        Mathf.Clamp(transform.position.z, 0, 0);
        if (fleeDuration > 0)
        {
            fleeDuration -= Time.deltaTime;
        }
        rocketIncoming = false;
        var attacks = GameData.Instance.attacks;
        foreach (var attack in attacks)
        {
            if (attack.IsEnemy && attack.Type == Attack.AttackType.Rocket)
            {
                if ((transform.position - attack.currentPosition).magnitude < 25.0f)
                {
                    rocketIncoming = true;
                    rocketPosition = attack.currentPosition;
                    break;
                }
            }
        }
        if (target.closestEnemy != null)
        {
            targetPosition = target.closestEnemy.transform.position;
            targetPosition.z = 0.0f;

            if (steeringAgent.Health <= 0.25f && stopFleeing == false)
            {
                fleeDuration = 2f;
                stopFleeing = true;
            }

            if (fleeDuration > 0)
            {
                Flee();
            }
            else if (rocketIncoming)
            {
                FleeRocket();
            }
            else
            {
                Seek();
            }
        }
        else
        {
            desiredVelocity = Vector3.zero;
        }
        steeringVelocity = desiredVelocity - steeringAgent.CurrentVelocity;
        return steeringVelocity;
    }

    void Seek()
    {
        desiredVelocity = Vector3.Normalize(targetPosition - transform.position) * SteeringAgent.MaxCurrentSpeed;
    }

    void Flee()
    {
        desiredVelocity = Vector3.Normalize(transform.position - targetPosition) * SteeringAgent.MaxCurrentSpeed;
    }

    void FleeRocket()
    {
        desiredVelocity = Vector3.Normalize(transform.position - rocketPosition) * SteeringAgent.MaxCurrentSpeed;
    }
}

