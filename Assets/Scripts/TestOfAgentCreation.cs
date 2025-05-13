using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach this MonoBehaviour to an active GameObject that exists in the scene to see the demonstration
/// of diffent classes being assigned to ally agents randomly. You can use this to construct your own
/// types of ally agents according to what you desire
/// </summary>
public class TestOfAgentCreation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		// Used only to randomly index for test purposes
		var types = new Type[]
{
			typeof(AllyAgent2),
			typeof(AllyAgent3),
			typeof(AllyAgent4),
			typeof(AllyAgent5),
};

		var unitLocations = GameData.Instance.Map.GetInitialPlayerLocations();
		Dictionary<int, Type> mapIndexToUnitType = new Dictionary<int, Type>();
		foreach (var location in unitLocations)
		{
			// Just randomly assign agent classes. You would probably do something else based on map index locations
			// or based on some logic for how many you want
			mapIndexToUnitType.Add(location, types[UnityEngine.Random.Range(0, types.Length)]);
		}
		GameData.Instance.CreateAllyUnits(mapIndexToUnitType);
	}
}

public class AllyAgent2 : SteeringAgent
{
	protected override void InitialiseFromAwake()
	{
		gameObject.AddComponent<SeekToMouse>();
	}
}

public class AllyAgent3 : SteeringAgent
{
	protected override void InitialiseFromAwake()
	{
		gameObject.AddComponent<SeekToMouse>();
	}
}


public class AllyAgent4 : SteeringAgent
{
	protected override void InitialiseFromAwake()
	{
		gameObject.AddComponent<SeekToMouse>();
	}
}


public class AllyAgent5 : AllyAgent4
{
	protected override void InitialiseFromAwake()
	{
		gameObject.AddComponent<SeekToMouse>();
	}
}