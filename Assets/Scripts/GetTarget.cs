using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GetTarget : MonoBehaviour
{
    public List<EnemyAgent> enemies;
    public GameObject closestEnemy;
    public float closestEnemyDistance;

    void Start()
    {
        closestEnemyDistance = Mathf.Infinity;
        FindClosestEnemy();
    }

    void Update()
    {
        enemies = FindObjectsOfType<EnemyAgent>().ToList();

        if (closestEnemy == null)
        {
            FindClosestEnemy();
        }
        else
        {
            float distance = Vector3.Distance(transform.position, closestEnemy.transform.position);

            if (distance < closestEnemyDistance)
            {
                closestEnemyDistance = distance;
            }
            else
            {
                FindClosestEnemy();
            }
        }
    }

    void FindClosestEnemy()
    {
        closestEnemyDistance = Mathf.Infinity;

        if (enemies != null)
        {
            foreach (EnemyAgent enemy in enemies)
            {
                if (enemy != null)
                {
                    float distance = Vector3.Distance(transform.position, enemy.transform.position);

                    if (distance < closestEnemyDistance)
                    {
                        closestEnemyDistance = distance;
                        closestEnemy = enemy.gameObject;
                    }
                }
            }
        }
    }
}

