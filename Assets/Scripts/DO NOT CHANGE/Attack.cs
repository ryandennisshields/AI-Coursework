using UnityEngine;

public struct Attack
{
	public enum AttackType
	{
		None,
		Melee,
		Gun,
		Rocket,
		Explosion
	}

	public class Data
	{
		public readonly float damage;
		public readonly float radius;
		public readonly float speed;
		public readonly float range;
		public readonly bool friendlyFire;
		public readonly bool oneHit;
		public readonly string spriteName;

		public Data(float damage, float radius, float speed, float range, bool friendlyFire, bool oneHit, string spriteName)
		{
			this.damage = damage;
			this.radius = radius;
			this.speed = speed;
			this.range = range;
			this.friendlyFire = friendlyFire;
			this.oneHit = oneHit;
			this.spriteName = spriteName;
		}
	}

	public static readonly Data[] AttackDatas = new Data[]
	{
		new Data(0.0f, 0.0f, 0.0f, 0.0f, false, false, ""),
		new Data(1.0f, SteeringAgent.CollisionRadius, 0.1f, 0.02f, false, true, "Melee"),
		new Data(0.25f, 0.125f, 18.0f, 15.0f, false, true, "Bullet"),
		new Data(0.1f, 0.25f, 12.0f, 30.0f, true, true, "Rocket"),
		new Data(1.0f, 1.5f, 0.1f, 0.05f, true, false, "Explosion"),
	};



	public float Damage => AttackDatas[(int)Type].damage;
	public float Radius => AttackDatas[(int)Type].radius;
	public float Speed => AttackDatas[(int)Type].speed;
	public float Range => AttackDatas[(int)Type].range;
	public bool FriendlyFire => AttackDatas[(int)Type].friendlyFire;
	public bool OneHit => AttackDatas[(int)Type].oneHit;
	public string SpriteName => AttackDatas[(int)Type].spriteName;

	public readonly AttackType Type;
	public readonly bool IsEnemy;
	public readonly GameObject AttackerGO;
	public readonly GameObject GameObject;
	public readonly Vector3 Direction;
	public readonly Vector3 StartPosition;
	public Vector3 currentPosition;
	public GameObject unitHit;

	public Attack(AttackType attackType, GameObject gameObject, GameObject attackerGO)
	{
		Type = attackType;
		IsEnemy = attackerGO.GetComponent<EnemyAgent>() != null;
		GameObject = gameObject;
		AttackerGO = attackerGO;
		Direction = Vector3.Normalize(gameObject.transform.up);
		StartPosition = gameObject.transform.position;
		currentPosition = gameObject.transform.position;
		unitHit = null;
	}
}
