using System.Collections.Generic;
using UnityEngine;

public abstract class SteeringAgent : MonoBehaviour
{
	/// <summary>Size of collision radius of agent</summary>
	public const float CollisionRadius = 0.45f;

	/// <summary>Multiplier of speed of agent in water</summary>
	public const float WaterSpeedFactor = 0.25f;

	/// <summary>Multiplier of speed of agent in mud</summary>
	public const float MudSpeedFactor = 0.5f;

	/// <summary>Default time to execute cooperative arbitration. Less than oe equal to 0 means run every frame</summary>
	public const float DefaultUpdateTimeInSecondsForAI = 0.0f;

	/// <summary>Default time to before next attack can be done</summary>
	public const float DefaultTimeToNextAttack = 0.75f;

	/// <summary>The maximum speed the agent can have</summary>
	public const float MaxCurrentSpeed = 6.0f;

	/// <summary>The maximum steering speed that will be applied to the overall steering velocity affter cooperative arbitration</summary>
	public const float MaxSteeringSpeed = 20.0f;

	/// <summary>Adjusts the frequency time in seconds of when the AI will  updates its logic</summary>
	public float MaxUpdateTimeInSecondsForAI { get; protected set; } = DefaultUpdateTimeInSecondsForAI;

	/// <summary>Returns the current velocity (speed and direction) of the Agent</summary>
	public Vector3 CurrentVelocity	{ get; protected set; } = Vector3.zero;

	/// <summary>Returns the steering velocity of the Agent</summary>
	public Vector3 SteeringVelocity { get; protected set; } = Vector3.zero;

	/// <summary>Stores the transform.position fron the previous game frame</summary>
	public Vector3 PreviousPosition { get; private set; } = Vector3.zero;

	/// <summary>Stores the health pf the agent</summary>
	public float TimeToNextAttack { get; private set; } = 0.0f;


	/// <summary>Stores the health of the agent</summary>
	public float Health { get; private set; } = 1.0f;

	public bool IsAttackInProgress { get; private set; } = false;

	protected abstract void InitialiseFromAwake();

	protected virtual void InitialiseFromStart()
	{
	}

	public bool CanAttack(Attack.AttackType attack)
	{
		if(TimeToNextAttack > 0.0f)
		{
			return false;
		}

		if(attack == Attack.AttackType.Rocket)
		{
			bool isEnemy = this is EnemyAgent;
			if(isEnemy && GameData.Instance.EnemyRocketsAvailable <= 0)
			{
				return false;
			}
			else if(!isEnemy && GameData.Instance.AllyRocketsAvailable <= 0)
			{
				return false;
			}
		}

		return true;
	}

	public bool AttackWith(Attack.AttackType attack)
	{
		IsAttackInProgress = true;
		if (GameData.Instance.CreateAttack(attack, this))
		{
			TimeToNextAttack = DefaultTimeToNextAttack;
			IsAttackInProgress = false;
			return true;
		}
		IsAttackInProgress = false;

		return false;
	}

	/// <summary>
	/// Limits the vector to the specified magnitude
	/// </summary>
	/// <param name="vector">Vector to limit</param>
	/// <param name="magnitude">Amount to limit to</param>
	/// <returns>New Vector that has been limited</returns>
	static public Vector3 LimitVector(Vector3 vector, float magnitude)
	{
		// This limits the velocity to max speed. sqrMagnitude is used rather than magnitude as in magnitude a square root must be computed which is a slow operation.
		// By using sqrMagnitude and comparing with maxSpeed squared we can get around using the expensive square root operation.
		if (vector.sqrMagnitude > magnitude * magnitude)
		{
			vector.Normalize();
			vector *= magnitude;
		}
		return vector;
	}

	public bool ShowDebug
	{
		get
		{
			GetComponents<SteeringBehaviour>(steeringBehvaiours);
			foreach (SteeringBehaviour currentBehaviour in steeringBehvaiours)
			{
				if (currentBehaviour.ShowDebugLines)
				{
					return true;
				}
			}

			return false;
		}

		set
		{
			GetComponents<SteeringBehaviour>(steeringBehvaiours);
			foreach (SteeringBehaviour currentBehaviour in steeringBehvaiours)
			{
				currentBehaviour.ShowDebugLines = value;
			}
		}
	}


	#region Private interface

	/// <summary>Stores the transform.position fron the end of update</summary>
	private Vector3 currentPosition;

	/// <summary>Stores a list of all steering behaviours that are on a SteeringAgent GameObject, regardless if they are enabled or not</summary>
	private List<SteeringBehaviour> steeringBehvaiours = new List<SteeringBehaviour>();

	/// <summary>Tracks how many seconds have elapsed since last CooperativeArbitration function has run</summary>
	private float updateTimeInSecondsForAI;

	private Map map;

	private void Awake()
	{
		var gameData = GameData.Instance;
		map = gameData.Map;
		currentPosition = transform.position;

		InitialiseFromAwake();
	}

	private void Start()
	{
		InitialiseFromStart();
	}

	/// <summary>
	/// Called once per frame
	/// </summary>
	private void Update()
	{
		Health = GameData.Instance.GetUnitHealth(gameObject);

		transform.position = currentPosition;
		PreviousPosition = transform.position;

		TimeToNextAttack -= Time.deltaTime;
		if(TimeToNextAttack < 0.0f)
		{
			TimeToNextAttack = 0.0f;
		}

		if (MaxUpdateTimeInSecondsForAI <= 0.0f)
		{
			CooperativeArbitration();
		}
		else
		{
			updateTimeInSecondsForAI += Time.deltaTime;
			while (updateTimeInSecondsForAI > MaxUpdateTimeInSecondsForAI)
			{
				updateTimeInSecondsForAI -= MaxUpdateTimeInSecondsForAI;
				CooperativeArbitration();
			}
		}

		UpdatePosition();
		UpdateDirection();

		// Show debug lines in scene view
		foreach (SteeringBehaviour currentBehaviour in steeringBehvaiours)
		{
			currentBehaviour.DebugDraw(this);
		}

		currentPosition = transform.position;
	}

	/// <summary>
	/// This is responsible for how to deal with multiple behaviours and selecting which ones to use.
	/// 
	/// Please see this link for some decent descriptions of below:
	/// https://alastaira.wordpress.com/2013/03/13/methods-for-combining-autonomous-steering-behaviours/
	/// Remember some options for choosing are:
	/// 1 Finite state machines which can be part of the steering behaviours or not (Not the best approach but quick)
	/// 2 Weighted Truncated Sum
	/// 3 Prioritised Weighted Truncated Sum
	/// 4 Prioritised Dithering
	/// 5 Context Behaviours: https://andrewfray.wordpress.com/2013/03/26/context-behaviours-know-how-to-share/
	/// 6 Any other approach you come up with
	/// </summary>
	protected virtual void CooperativeArbitration()
	{
		SteeringVelocity = Vector3.zero;
		
		GetComponents<SteeringBehaviour>(steeringBehvaiours);
		foreach (SteeringBehaviour currentBehaviour in steeringBehvaiours)
		{
			if(currentBehaviour.enabled)
			{
				SteeringVelocity += currentBehaviour.UpdateBehaviour(this);
			}
		}
	}

	/// <summary>
	/// Updates the position of the GAmeObject via Teleportation. In Craig Reynolds architecture this would the Locomotion layer
	/// </summary>
	private void UpdatePosition()
	{
		var terrain = GameData.Instance.Map.GetTerrainAt((int)transform.position.x, (int)transform.position.y);
		var maxSpeed = MaxCurrentSpeed;
		switch (terrain)
		{
			case Map.Terrain.Water:
				maxSpeed *= WaterSpeedFactor;
				break;

			case Map.Terrain.Mud:
				maxSpeed *= MudSpeedFactor;
				break;
		}

		// Limit steering velocity to supplied maximum so it can be used to adjust current velocity. Ensure to subtract this limnited
		// amount from the current value of the steering velocity so that it decreases as over multiple game frames to reach the target
		SteeringVelocity = LimitVector(SteeringVelocity, MaxSteeringSpeed * Time.deltaTime);

		// Set final velocity
		CurrentVelocity += SteeringVelocity;
		CurrentVelocity = LimitVector(CurrentVelocity, maxSpeed);

		// Apply current velocity amount for this frame
		var distanceToApply = CurrentVelocity * Time.deltaTime;
		transform.position += distanceToApply;

		var restrictPosition = transform.position;
		restrictPosition.x = Mathf.Clamp(transform.position.x, 0.0f, (float)(Map.MapWidth) - 0.01f);
		restrictPosition.y = Mathf.Clamp(transform.position.y, 0.0f, (float)(Map.MapHeight) - 0.01f);
		transform.position = restrictPosition;

		// Check for collision with obstacles and prevent movement, but allow sliding collision if possible. Note: Sliding collision only
		// works in this context as obstacles are aligned to othogonals. Proper way to do this would be finding out normals of collision
		// and working out tangent to move along
		if (map.IsNavigatable((int)transform.position.x, (int)transform.position.y) == false)
		{
			transform.position = PreviousPosition;
			transform.position += new Vector3(distanceToApply.x, 0.0f, 0.0f);
			if (map.IsNavigatable((int)transform.position.x, (int)transform.position.y) == false)
			{
				transform.position = PreviousPosition;
				transform.position += new Vector3(0.0f, distanceToApply.y, 0.0f);
				if (map.IsNavigatable((int)transform.position.x, (int)transform.position.y) == false)
				{
					transform.position = PreviousPosition;
				}
			}
		}

		// Prevent cheating by teleportation and log it as an error
		var distanceTravelled = (transform.position - PreviousPosition).magnitude;
		if (distanceTravelled - 0.001f > maxSpeed * Time.deltaTime)
		{
			transform.position = PreviousPosition;
			Debug.LogError("Steering agent position has exceeded max steering speed! Distance allowed this frame: " + MaxSteeringSpeed * Time.deltaTime + ", Actual distance: " + distanceTravelled);
		}
	}

	/// <summary>
	/// Sets the direction of the triangle to the direction it is moving in to give the illusion it is turning. Try taking out the function
	/// call in Update() to see what happens
	/// </summary>
	protected virtual void UpdateDirection()
	{
		// Don't set the direction if no direction
		if (CurrentVelocity.sqrMagnitude > 0.0f)
		{
			transform.up = Vector3.Normalize(new Vector3(CurrentVelocity.x, CurrentVelocity.y, 0.0f));
		}
	}

	#endregion
}
