using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
	public GameObject GetEnemyAt(int index)
	{
		return enemyUnits[index];
	}

	public int GetEnemyCount()
	{
		return enemyUnits.Count;
	}

	public List<GameObject> GetCopyOfEnemiesList()
	{
		return new List<GameObject>(enemyUnits);
	}

	public GameObject GetAllyAt(int index)
	{
		return allyUnits[index];
	}

	public int GetAllyCount()
	{
		return allyUnits.Count;
	}

	public List<GameObject> GetCopyOfAlliesList()
	{
		return new List<GameObject>(allyUnits);
	}

	public float GetUnitHealth(GameObject gameObject)
	{
		if (unitToHealth.ContainsKey(gameObject))
		{
			return unitToHealth[gameObject];
		}
		return -1.0f;
	}

	/// <summary>
	/// Recreates ally units at the map index locations as keys in the dictionary to the class types you want to create.
	/// NOTE: use Map.MapIndex(int x, int y) to convert unit x, y coordinates to a map index
	/// NOTE: this will only work before the first update cycle so its important to call this as soon as possible
	/// </summary>
	/// <param name="mapIndexToUnitType">Map index to the unit type to create. Use typeof(YourClassName) to get the Type</param>
	public void CreateAllyUints(Dictionary<int, Type> mapIndexToUnitType)
	{
		if(hasFristUpdateExcuted)
		{
			Debug.LogError("CreateAllyUints needs to be call before the first update cycle is executed. Ensure this called in a Start function and that the GameObject is in the Heirarchy from the beginning,");
			return;
		}

		var map = GameData.Instance.Map;
		var allyUnitLocations = map.GetInitialPlayerLocations();
		foreach (var allyUnitLocation in allyUnitLocations)
		{
			allyUnits.Add(CreateUnit(map, map.MapIndexToX(allyUnitLocation), map.MapIndexToY(allyUnitLocation), false, mapIndexToUnitType[allyUnitLocation]));
		}
	}

	#region Private interface

	private Dictionary<GameObject, float> unitToHealth = new Dictionary<GameObject, float>();

    private List<GameObject> enemyUnits = new List<GameObject>();
	private List<GameObject> allyUnits = new List<GameObject>();

    private Sprite unitSprite;

    private GameObject enemiesGO;
    private GameObject alliesGO;

	private bool hasFristUpdateExcuted = false;

	private void Awake()
	{
		enemiesGO = new GameObject("Enemies");
		enemiesGO.transform.parent = transform;

		alliesGO = new GameObject("Allies");
		alliesGO.transform.parent = transform;

		unitSprite = Resources.Load<Sprite>("Unit");
	}

	// Start is called before the first frame update
	private void Start()
    {
		var gameData = GetComponent<GameData>();
		var map = gameData.Map;

		var enemyType = typeof(EnemyAgent);
		var enemyUnitLocations = map.GetInitialUnitLocations();
		foreach (var enemyUnitLocation in enemyUnitLocations)
		{
			enemyUnits.Add(CreateUnit(map, map.MapIndexToX(enemyUnitLocation), map.MapIndexToY(enemyUnitLocation), true, enemyType));
		}

		if (allyUnits.Count <= 0)
		{
			var allyType = typeof(AllyAgent);
			var allyUnitLocations = map.GetInitialPlayerLocations();
			foreach (var allyUnitLocation in allyUnitLocations)
			{
				allyUnits.Add(CreateUnit(map, map.MapIndexToX(allyUnitLocation), map.MapIndexToY(allyUnitLocation), false, allyType));
			}
		}

		CopyUnitsToLists();
	}

    public void Tick()
    {
		CopyUnitsToLists();
		hasFristUpdateExcuted = true;
	}

	private void CopyUnitsToLists()
	{
		var gameData = GameData.Instance;
		if(gameData.allies.Count != allyUnits.Count)
		{
			gameData.allies.Clear();
			foreach (var unit in allyUnits)
			{
				gameData.allies.Add(unit);
			}
		}
		if (gameData.enemies.Count != enemyUnits.Count)
		{
			gameData.enemies.Clear();
			foreach (var unit in enemyUnits)
			{
				gameData.enemies.Add(unit);
			}
		}
	}


    private GameObject CreateUnit(Map map, int mapX, int mapY, bool isEnemy, Type type)
    {
		if(!typeof(SteeringAgent).IsAssignableFrom(type))
		{
			throw new TypeAccessException("Type " + type.Name + " does not derive from " + typeof(SteeringAgent).Name);
		}

		var unit = new GameObject(isEnemy ? "Enemy " + enemyUnits.Count : "Ally " + allyUnits.Count);
        unit.transform.parent = isEnemy ? enemiesGO.transform : alliesGO.transform;
		unit.transform.position = new Vector3(mapX, mapY, 0.0f);

		var spriteRenderer = unit.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = unitSprite;
		spriteRenderer.color = isEnemy ? Color.yellow : Color.magenta;

        var collider = unit.AddComponent<CircleCollider2D>();
        collider.radius = SteeringAgent.CollisionRadius;

		unitToHealth.Add(unit, 1.0f);

		// Ensure this is last as the users entry point into these classes will be called when this happens
		// so everything needs setup before this
		unit.AddComponent(type);
		return unit;
    }

	/// <summary>
	/// Never call this! Anyone calling this function manually will deducted marks from their coursework!
	/// Applies damage to the unit and will Destroy the unit if dead
	/// </summary>
	/// <param name="unit">Unit to apply damage to</param>
	/// <param name="damage">Amount of damage to apply</param>
    public void ApplyDamageToUnit(GameObject unit, float damage)
    {
        if(unitToHealth.ContainsKey(unit))
        {
            unitToHealth[unit] -= damage;

            if(unitToHealth[unit] <= 0.0f)
            {
                unitToHealth.Remove(unit);
                unit.SetActive(false);
                Destroy(unit);
			}
        }
    }
	#endregion
}
