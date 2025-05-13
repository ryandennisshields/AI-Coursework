using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attacking : MonoBehaviour
{
    public Attack.AttackType attackType = Attack.AttackType.Melee;
    private bool rocketFired = false;

    void Update()
    {
        var distanceToTarget = GetComponent<GetTarget>().closestEnemyDistance;
        var attacks = GameData.Instance.attacks;
        foreach (var attack in attacks)
        {
            if (!attack.IsEnemy && attack.Type == Attack.AttackType.Rocket)
            {
                rocketFired = true;
                break;
            }
            else
            {
                rocketFired = false;
                break;
            }
        }

        if (GameData.Instance.AllyRocketsAvailable > 0 && distanceToTarget < 28.0f && rocketFired == false)
        {
            attackType = Attack.AttackType.Rocket;
            rocketFired = true;
        }
        else
        {
            if (distanceToTarget <= 2f)
            {
                attackType = Attack.AttackType.Melee;
            }
            else if (distanceToTarget <= 15f && distanceToTarget >= 2f)
            {
                attackType = Attack.AttackType.Gun;
            }
            else
            {
                attackType = Attack.AttackType.Melee;
            }
        }

    }
}
