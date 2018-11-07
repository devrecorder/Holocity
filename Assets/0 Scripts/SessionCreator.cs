﻿using Infrastructure.Grid;
using Infrastructure.Grid.Entities.Buildings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SessionCreator : MonoBehaviour {

    [Header("Grid settings")]
    public int width;
    public int height;

	void Start () {
        Game.SetSession(new Session());
        Debug.Log("New session created and set: " + Game.CurrentSession.Name);
        Game.CurrentSession.City.CreateGrid(width, height);

        Game.CurrentSession.City.GetGrid(0).AddBuildingToTile(1, 1, new House());
	}
	
}
