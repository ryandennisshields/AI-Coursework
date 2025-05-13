using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class GameData : MonoBehaviour
{
    public static GameData Instance { get; private set; }

    public Map Map { get; private set; }

	public MapRenderer MapRenderer { get; private set; }

    public readonly List<GameObject> enemies = new List<GameObject>();
	public readonly List<GameObject> allies = new List<GameObject>();
	public readonly List<Attack> attacks = new List<Attack>();

	public int AllyRocketsAvailable => AttackManager.AllyRocketsAvailable;
	public int EnemyRocketsAvailable => AttackManager.EnemyRocketsAvailable;

	public float GetUnitHealth(GameObject unit)
	{
		return UnitManager.GetUnitHealth(unit);
	}

	public void CreateAllyUnits(Dictionary<int, Type> mapPositionToUnitType)
	{
		UnitManager.CreateAllyUints(mapPositionToUnitType);
	}




	#region Private interface

	private UnitManager UnitManager { get; set; }
	private AttackManager AttackManager { get; set; }

	// Start is called before the first frame update
	private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Another GameData instance has been created! This is a Singleton game object there can only be one instance!");
            DestroyImmediate(this);
            return;
        }

        Map = new Map();

		UnitManager = gameObject.AddComponent<UnitManager>();
		AttackManager = gameObject.AddComponent<AttackManager>();
        AttackManager.Initialise(UnitManager);
		MapRenderer = gameObject.AddComponent<MapRenderer>();
	}

	private void Start()
	{
		MapRenderer.Initialise(Map.GetMapData(), Map.MapWidth, Map.MapHeight);
	}

	private void Update()
	{
		AttackManager.Tick();
		UnitManager.Tick();
	}

	public bool CreateAttack(Attack.AttackType attack, SteeringAgent agent)
	{
		return AttackManager.Create(attack, agent);
	}
	#endregion
}
