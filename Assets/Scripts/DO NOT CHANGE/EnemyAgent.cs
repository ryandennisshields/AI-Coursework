using UnityEngine;

public class EnemyAgent : SteeringAgent
{
	protected EnemyFleeBehaviour fleeBehaviour;
	protected EnemyFleeRocketBehaviour fleeRocketBehaviour;
	protected EnemySeekBehaviour seekBehaviour;

	protected bool rocketIncoming = false;

	public GameObject nearestAlly;
	public Vector3 rocketPosition;
	public Vector3 startPosition;

	protected override void InitialiseFromAwake()
	{
		fleeBehaviour = gameObject.AddComponent<EnemyFleeBehaviour>();
		fleeRocketBehaviour = gameObject.AddComponent<EnemyFleeRocketBehaviour>();
		seekBehaviour = gameObject.AddComponent<EnemySeekBehaviour>();

		fleeBehaviour.enabled = false;
		fleeRocketBehaviour.enabled = false;

		startPosition = transform.position;
	}

	protected override void CooperativeArbitration()
	{
		nearestAlly = SteeringBehaviour.GetNearestUnit(transform.position, GameData.Instance.allies);

		bool wasRocketIncoming = rocketIncoming;
		rocketIncoming = false;
		var attacks = GameData.Instance.attacks;
		foreach (var attack in attacks)
		{
			if (!attack.IsEnemy && attack.Type == Attack.AttackType.Rocket)
			{
				if ((transform.position - attack.currentPosition).magnitude < 25.0f)
				{
					rocketIncoming = true;
					rocketPosition = attack.currentPosition;
					break;
				}
			}
		}

		if (wasRocketIncoming != rocketIncoming)
		{
			if(rocketIncoming && Random.value <= 0.5f)
			{
				fleeBehaviour.enabled = false;
				seekBehaviour.enabled = false;
				fleeRocketBehaviour.enabled = true;
			}

			if(rocketIncoming == false && fleeRocketBehaviour.enabled)
			{
				fleeBehaviour.enabled = false;
				seekBehaviour.enabled = true;
				fleeRocketBehaviour.enabled = false;
			}
		}

		if(Health <= 0.25f)
		{
			fleeBehaviour.enabled = true;
		}

		base.CooperativeArbitration();

		
		if(TimeToNextAttack <= 0 && nearestAlly != null)
		{
			var distanceToAlly = (nearestAlly.transform.position - transform.position).magnitude;

			if (GameData.Instance.EnemyRocketsAvailable > 0 &&
				distanceToAlly < 28.0f && 
				Random.value <= 0.01f)
			{
				AttackWith(Attack.AttackType.Rocket);
			}
			else
			{
				if(distanceToAlly <= 15.0f)
				{
					AttackWith(Attack.AttackType.Gun);
				}
			}
		}
	}

	protected override void UpdateDirection()
	{
		base.UpdateDirection();

		if(nearestAlly == null)
		{
			return;
		}

		var difference = nearestAlly.transform.position - transform.position;
		if (nearestAlly != null && (difference).magnitude < 30.0f)
		{
			transform.up = Vector3.Normalize(new Vector3(difference.x, difference.y, 0.0f));
		}
	}
}
