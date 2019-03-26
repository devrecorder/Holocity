﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.Buttons;

public class TapGesture : MonoBehaviour, IInputClickHandler {
    
    float timeofuse = -999;


  
    public void OnInputClicked(InputClickedEventData eventData)
    {
        if(timeofuse + 0.5f > Time.time) return;

        timeofuse = Time.time;

        eventData.Use();


        if (GazeManager.Instance.HitObject && GazeManager.Instance.HitObject.GetComponent<CompoundButton>())
        {
            AudioManager.Instance.UISound(true);
        }
        else if (GazeManager.Instance.HitObject && GazeManager.Instance.HitObject.GetComponent<FocusHighlighter>())
        {
            WorldGridTile tile = GazeManager.Instance.HitObject.transform.parent.GetComponent<WorldGridTile>();
            UIManager.Instance.MoveToTile(tile);
            AudioManager.Instance.SelectSound(true);
        }
        else if (!GazeManager.Instance.HitObject && UIManager.Instance.menuState != UIManager.MenuState.Off)
        {
            UIManager.Instance.SwitchState(UIManager.MenuState.Off);
            AudioManager.Instance.SelectSound(false);
            //turn off UI if player clicks away.
        }
    }

}
