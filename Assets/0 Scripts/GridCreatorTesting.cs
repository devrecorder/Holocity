﻿using HoloToolkit.Unity.InputModule;
using UnityEngine;

public class GridCreatorTesting : MonoBehaviour, IInputClickHandler
{
    private Camera _cam;
    public bool CanPlace = true;

    void Start()
    {
        _cam = Camera.main;
    }

    public void EnablePlacing()
    {
        CanPlace = true;
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        if (!CanPlace) return;

        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
        RaycastHit hitinfo;
        if(Physics.Raycast(ray, out hitinfo, 50f))
        {
            //We have hit a spacial mapper surface, whats the angle difference from an up normal?
            if(Vector3.Angle(hitinfo.normal, Vector3.up) < 10)
            {
                Game.CurrentSession.City.CreateGrid(10, 10, hitinfo.point);
                Destroy(this);
            }
        }
    }
}

