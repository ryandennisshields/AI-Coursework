using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFleeRocketBehaviour : SteeringBehaviour
{
	public override Vector3 UpdateBehaviour(SteeringAgent steeringAgent)
	{
		var enemySteeringAgent = steeringAgent as EnemyAgent;

		Vector3 targetPosition = enemySteeringAgent.rocketPosition;
		desiredVelocity = Vector3.Normalize(transform.position - targetPosition) * SteeringAgent.MaxCurrentSpeed;
		steeringVelocity = desiredVelocity - steeringAgent.CurrentVelocity;
		return steeringVelocity;
	}
}
