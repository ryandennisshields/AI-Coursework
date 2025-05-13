using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySeekBehaviour : SteeringBehaviour
{
	public override Vector3 UpdateBehaviour(SteeringAgent steeringAgent)
	{
		var enemySteeringAgent = steeringAgent as EnemyAgent;

		Vector3 targetPosition = transform.position;

		var nearestUnit = GetNearestUnit(transform.position, GameData.Instance.allies);
		if (nearestUnit != null && (nearestUnit.transform.position - transform.position).magnitude <= 15.0f)
		{
			targetPosition = nearestUnit.transform.position;
		}

		if((targetPosition - enemySteeringAgent.startPosition).magnitude > 20.0f)
		{
			targetPosition = enemySteeringAgent.startPosition;
		}

		desiredVelocity = Vector3.Normalize(targetPosition - transform.position) * SteeringAgent.MaxCurrentSpeed;
		steeringVelocity = desiredVelocity - steeringAgent.CurrentVelocity;
		return steeringVelocity;
	}
}
