using System.Collections.Generic;
using UnityEngine;

public class AttackManager : MonoBehaviour
{
	public int AllyRocketsAvailable { get; private set; }
	public int EnemyRocketsAvailable { get; private set; }

	#region Private interface

	private static readonly Dictionary<string, Sprite> spriteNameToSprite = new Dictionary<string, Sprite>();

	private int attackNumber = 0;
	private GameObject attacksGO;

	public readonly List<Attack> attacks = new List<Attack>();

	private UnitManager unitManager;


	public void Initialise(UnitManager unitManager)
	{
		this.unitManager = unitManager;
		AllyRocketsAvailable = GameData.Instance.Map.GetInitialHouseLocations().Count;
		EnemyRocketsAvailable = AllyRocketsAvailable;
	}

	// Start is called before the first frame update
	private void Start()
	{
		attacksGO = new GameObject("Attacks");
		attacksGO.transform.parent = transform;

		foreach (Attack.Data attackData in Attack.AttackDatas)
		{
			if(string.IsNullOrEmpty(attackData.spriteName))
			{
				continue;
			}
			spriteNameToSprite.Add(attackData.spriteName, Resources.Load<Sprite>(attackData.spriteName));
		}
	}

	// Called once per frame
	public void Tick()
	{
		var gameData = GameData.Instance;
		var map = gameData.Map;

		var allies = gameData.allies;
		var enemies = gameData.enemies;

		for (int attackIndex = attacks.Count - 1; attackIndex >= 0; -- attackIndex)
		{
			bool attackHit = false;

			var attack = attacks[attackIndex];
			attack.currentPosition += attack.Direction * attack.Speed * Time.deltaTime;
			attack.GameObject.transform.position = attack.currentPosition;

			var x = (int)attack.currentPosition.x;
			var y = (int)attack.currentPosition.y;

			var unitListsToCheck = new List<List<GameObject>>() { attack.IsEnemy ? allies : enemies };
			if (attack.FriendlyFire)
			{
				unitListsToCheck.Add((!attack.IsEnemy) ? allies : enemies);
			}
			
			foreach (var unitList in unitListsToCheck)
			{
				foreach (var unit in unitList)
				{
					if (unit == null || (unit == attack.AttackerGO && attack.Type != Attack.AttackType.Explosion))
					{
						continue;
					}

					var radii = attack.Radius + SteeringAgent.CollisionRadius;
					if ((attack.currentPosition - unit.transform.position).sqrMagnitude <= (radii * radii))
					{
						unitManager.ApplyDamageToUnit(unit, attack.Damage);

						if(attack.OneHit)
						{
							attackHit = true;
							break;
						}
					}
				}

				if (attack.OneHit)
				{
					break;
				}
			}
			
			if(attackHit)
			{
				if(attack.Type == Attack.AttackType.Rocket)
				{
					CreateExplosion(attack);
				}

				Destroy(attack.GameObject);
				attacks.RemoveAt(attackIndex);
				continue;
			}


			// Destroy attack if outside map, hits a tree, or exceeded range
			if (x < 0 || x >= Map.MapWidth || y < 0 || y >= Map.MapHeight ||
				map.IsNavigatable(x, y) == false ||
				(attack.currentPosition - attack.StartPosition).magnitude >= attack.Range)
			{
				attackHit = true;
			}

			if (attackHit)
			{
				if (attack.Type == Attack.AttackType.Rocket && !(x < 0 || x >= Map.MapWidth || y < 0 || y >= Map.MapHeight))
				{
					CreateExplosion(attack);
				}

				Destroy(attack.GameObject);
				attacks.RemoveAt(attackIndex);
				continue;
			}

			attacks[attackIndex] = attack;
		}

		gameData.attacks.Clear();
		foreach (var attack in attacks)
		{
			gameData.attacks.Add(attack);
		}
	}

	/// <summary>
	/// Never call directly! Call through Steering
	/// </summary>
	/// <param name="attackType"></param>
	/// <param name="agent"></param>
	/// <returns></returns>
	public bool Create(Attack.AttackType attackType, SteeringAgent agent)
	{
		if(agent == null || agent.CanAttack(attackType) == false)
		{
			return false;
		}

		if(!agent.IsAttackInProgress)
		{
			Debug.LogError("DO NOT CALL THIS FUNCTION DIRECTLY - use SteeringAgent.AttackWith() instead");
			return false;
		}

		var attackGO = new GameObject("Attack " + attackNumber.ToString() + " from " + agent.name);
		attackGO.transform.parent = attacksGO.transform;
		attackGO.transform.position = agent.transform.position;
		attackGO.transform.up = agent.transform.up;

		if (attackType == Attack.AttackType.Melee)
		{
			attackGO.transform.position += attackGO.transform.up * (SteeringAgent.CollisionRadius * 2.0f);
		}

		var attack = new Attack(attackType, attackGO, agent.gameObject);
		attacks.Add(attack);

		var spriteRenderer = attackGO.AddComponent<SpriteRenderer>();
		spriteRenderer.sprite = spriteNameToSprite[attack.SpriteName];
		spriteRenderer.color = agent.gameObject.GetComponent<SpriteRenderer>().color;
		spriteRenderer.sortingOrder = 1;

		var collider = attackGO.AddComponent<CircleCollider2D>();
		collider.radius = attack.Radius;

		if (attackType == Attack.AttackType.Rocket)
		{
			if(attack.IsEnemy)
			{
				--EnemyRocketsAvailable;
			}
			else
			{
				--AllyRocketsAvailable;
			}
		}

		++attackNumber;
		return true;
	}

	private bool CreateExplosion(Attack rocket)
	{
		if(rocket.AttackerGO == null)
		{
			return false;
		}

		var agent = rocket.AttackerGO.GetComponent<SteeringAgent>();

		var attackGO = new GameObject("Attack " + attackNumber.ToString() + " from " + agent.name);
		attackGO.transform.parent = attacksGO.transform;
		attackGO.transform.position = rocket.currentPosition;
		attackGO.transform.up = rocket.Direction;

		var attack = new Attack(Attack.AttackType.Explosion, attackGO, agent.gameObject);
		attacks.Add(attack);

		var spriteRenderer = attackGO.AddComponent<SpriteRenderer>();
		spriteRenderer.sprite = spriteNameToSprite[attack.SpriteName];
		spriteRenderer.color = agent.gameObject.GetComponent<SpriteRenderer>().color;

		var collider = attackGO.AddComponent<CircleCollider2D>();
		collider.radius = attack.Radius;

		++attackNumber;
		return true;
	}
	#endregion
}