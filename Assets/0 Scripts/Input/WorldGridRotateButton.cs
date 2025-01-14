﻿using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.XR.WSA;

public class WorldGridRotateButton : MonoBehaviour, INavigationHandler
{
    public WorldGrid GridParent;

    float RotationSpeed = 2.0f;

    public void OnNavigationCanceled(NavigationEventData eventData)
    {

        InputManager.Instance.PopModalInputHandler();

        if (!GridParent.transform.GetComponent<WorldAnchor>()) GridParent.transform.gameObject.AddComponent<WorldAnchor>();
        eventData.Use();
    }

    public void OnNavigationCompleted(NavigationEventData eventData)
    {
        InputManager.Instance.PopModalInputHandler();
        if (!GridParent.transform.GetComponent<WorldAnchor>()) GridParent.transform.gameObject.AddComponent<WorldAnchor>();
        eventData.Use();
    }

    public void OnNavigationStarted(NavigationEventData eventData)
    {
        InputManager.Instance.PushModalInputHandler(gameObject);

        if(GridParent.transform.GetComponent<WorldAnchor>()) DestroyImmediate(GridParent.transform.GetComponent<WorldAnchor>());

        eventData.Use();
    }

    public void OnNavigationUpdated(NavigationEventData eventData)
    {
        InputManager.Instance.PushModalInputHandler(gameObject);
        
        GridParent.transform.Rotate(new Vector3(0, eventData.NormalizedOffset.x * -RotationSpeed, 0));

        eventData.Use();
    }
}
