﻿using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class TileHighlighter : MonoBehaviour {
    

    public bool highlightTiles = true;
    public GameObject indicator;

    private bool validPlace = true;
    [HideInInspector]
    public bool ValidPlace { get { return validPlace;} set { validPlace = value; ChangeColour(); } }
    
    
    public GameObject currentTarget;
    private Quaternion rot = Quaternion.Euler(-90, 0, 0);

    private MeshRenderer meshRenderer;

    private void Start()
    {
        indicator = Instantiate(indicator);
        meshRenderer = indicator.GetComponentInChildren<MeshRenderer>();
        UIManager.Instance.StateChanged += ToggleHighlighter;
    }

    private void Update()
    {
        HighlightTile();
    }
    
    void ChangeColour()
    {
        Material[] mats = meshRenderer.materials;
        Color colour = Color.green;

        if (!validPlace) colour = Color.red;
        
        foreach(Material mat in mats)
        {
            mat.color = colour;
        }

    }

    void HighlightTile()
    {

        if (highlightTiles)
        {
            if (GazeManager.Instance.HitObject && (GazeManager.Instance.HitObject.GetComponentInParent<WorldGridTile>() || GazeManager.Instance.HitObject.transform.parent.GetComponentInParent<WorldGridTile>()))
            {
                if (GazeManager.Instance.HitObject == currentTarget) return;

                currentTarget = GazeManager.Instance.HitObject;
                indicator.SetActive(true);
            }
            else
            {
                indicator.SetActive(false);
            }
        }
        else
        {
           // currentTarget = UIManager.Instance.targetTile.gameObject.transform.GetChild(0).gameObject;/// gameObject;
           indicator.SetActive(true);
        }

       
        if(indicator.activeSelf && currentTarget)
        {
            indicator.transform.parent = currentTarget.transform.parent;
            indicator.transform.localPosition = Vector3.zero;
            indicator.transform.localRotation = rot;
            indicator.transform.localScale = currentTarget.transform.localScale;
            indicator.transform.parent = null;
        }
    }


    void ToggleHighlighter(int state)
    {
       // indicator.SetActive(check);
        highlightTiles = (state == 0) ? true : false;
    }
}