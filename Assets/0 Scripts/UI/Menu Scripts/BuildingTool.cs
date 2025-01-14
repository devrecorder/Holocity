﻿using HoloToolkit.Unity.Buttons;
using HoloToolkit.Unity.InputModule;
using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using Infrastructure.Grid.Entities;

public class BuildingTool : MonoBehaviour {

    public class PreviewBuilding
    {
        public WorldGridTile tile;
        public GameObject ghostBuilding;

        public PreviewBuilding(WorldGridTile Tile, GameObject GhostBuilding)
        {
            tile = Tile;
            ghostBuilding = GhostBuilding;
        }
    }

    public BuildingMenu buildingMenu;
    public GameObject menuPrefab;
    
    private int buildingIndex;
    private int gridID = 0;
    private Type _tileEnt;
    private GameObject ghostEnt;
    private List<PreviewBuilding> ghostTiles = new List<PreviewBuilding>();

    private Color onColour = new Color(0.0f,1.0f,0.0f,0.4f);
    private Color offColour = new Color(1.0f, 0.0f, 0.0f, 0.4f);

    private WorldGridTile prevTile;

    private GameObject model;
    private uint cost;

    Vector3 pos = new Vector3(0, 0.01f, 0);

    public bool active = false;
    private bool validPlace = false;


    public void StartTool(Type tileEnt, int buildingIndex, int gridID)
    {
        _tileEnt = tileEnt;
        BuildingMap map = BuildingLibrary.GetBuildingsForCategory(Infrastructure.Grid.Entities.Buildings.BuildingCategory.All).Where(x => x.BuildingType.IsEquivalentTo(tileEnt)).First();
        model = map.Model;
        cost = map.Cost;

        if (_tileEnt != null) active = true;
        
        this.gridID = gridID;
        this.buildingIndex = buildingIndex;

        OpenMenu();
        ghostEnt = CreateGhostBuilding();
    }

    private void StopTool()
    {
        _tileEnt = null;
        menuPrefab.SetActive(false);
        gridID = 0;

        for(int i = ghostTiles.Count - 1; i >= 0; i--)
        {
            Destroy(ghostTiles[i].ghostBuilding);
          
        }

        ghostEnt.SetActive(false);
        ghostTiles.Clear();
        active = false;
    }

    private void Start()
    {
        menuPrefab = Instantiate(menuPrefab);
        menuPrefab.SetActive(false);

        GetButtons();
    }

    private void Update()
    {
        if (active) CheckTarget();
    }

    void GetButtons()
    {
        foreach (Transform child in menuPrefab.transform)
        {
            foreach (Transform gc in child)
            {
                if (gc.GetComponent<CompoundButton>())
                {
                    if (gc.name.ToLower() == "place") gc.GetComponent<CompoundButton>().OnButtonClicked += PlaceBuildings;
                    else if (gc.name.ToLower() == "undo") gc.GetComponent<CompoundButton>().OnButtonClicked += UndoBuilding;
                    else gc.GetComponent<CompoundButton>().OnButtonClicked += CancelBuildings;
                }
            }
        }
    }
    
    void OpenMenu()
    {
        Vector3 pos = Game.CurrentSession.City.GetGrid(gridID).Position;
        pos.y += 0.1f;

        menuPrefab.transform.position = pos;
        menuPrefab.SetActive(true);
    }

    GameObject CreateGhostBuilding()
    {
        GameObject ghost = Instantiate(model);
        MeshRenderer mat = ghost.GetComponent<MeshRenderer>();
        if(mat == null) mat = ghost.GetComponentInChildren<MeshRenderer>();
        mat.material.color = onColour;
        ghost.layer = LayerMask.NameToLayer("Default");
        ghost.SetActive(false);
        return ghost;
    }

    void SetGhostPosition(GameObject ghost, Transform parent)
    {
        ghost.transform.parent = parent;
        ghost.transform.localPosition = pos;

        ghost.transform.localRotation = Quaternion.identity;
        ghost.transform.localScale = model.transform.localScale;
        ghost.SetActive(true);
    }

    void SetGhostColour(bool on)
    {
        MeshRenderer mat = ghostEnt.GetComponent<MeshRenderer>();
        if(mat == null) mat = ghostEnt.GetComponentInChildren<MeshRenderer>();
        if (on) mat.material.color = onColour;
        else mat.material.color = offColour;
    }

    void CheckTarget()
    {
        if (GazeManager.Instance.HitObject && GazeManager.Instance.HitObject.GetComponent<FocusHighlighter>())
        {
            WorldGridTile tile = GazeManager.Instance.HitObject.transform.parent.GetComponent<WorldGridTile>();

            CheckPlacement(tile);

            if (tile == prevTile) return;
            else prevTile = tile;

            SetGhostPosition(ghostEnt, tile.transform);
            
        }
        else
        {
            ghostEnt.SetActive(false);
            validPlace = false;
        }
    }

    private void CheckPlacement(WorldGridTile tile)
    {



        validPlace = buildingMenu.CheckCanPlace(tile, buildingIndex);
        validPlace = CheckCost();

        if (validPlace)
        {
            foreach (PreviewBuilding ghostTile in ghostTiles)
            {
                if (ghostTile.tile == tile)
                {
                    validPlace = false;
                    break;
                }
            }
        }

        SetGhostColour(validPlace);
    }

    private bool CheckCost()
    {
        return (Game.CurrentSession.Settings.Funds > cost * (ghostTiles.Count + 1));
    }

    public void TilePressed()
    {
        if (validPlace)
        {
            GameObject ghost = CreateGhostBuilding();
            SetGhostPosition(ghost, prevTile.transform);

            ghostTiles.Add(new PreviewBuilding(prevTile, ghost));

            if (prevTile.Model) prevTile.Model.SetActive(false);
        }
        else
        {
            //play error sound
        }
    }

    void PlaceBuildings(GameObject go)
    {
        foreach(PreviewBuilding previewBuilding in ghostTiles)
        {
            buildingMenu.PlaceBuilding(previewBuilding.tile, buildingIndex, Activator.CreateInstance(_tileEnt) as TileEntity);
        }
        AudioManager.Instance.PlaySound("build");

        StopTool();
    }
    void UndoBuilding(GameObject go)
    {
        if (ghostTiles.Count == 0) return;

        PreviewBuilding previewBuilding = ghostTiles[ghostTiles.Count - 1];
        ghostTiles.Remove(previewBuilding);

        if (previewBuilding.tile.Model) previewBuilding.tile.Model.SetActive(true);
        Destroy(previewBuilding.ghostBuilding);
    }
    void CancelBuildings(GameObject go)
    {
        StopTool();
    }
}
