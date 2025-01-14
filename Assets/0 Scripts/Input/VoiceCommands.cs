﻿using System;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using UnityEngine.SceneManagement;


public class VoiceCommands: MonoBehaviour, ISpeechHandler
{
    public bool IsNavigating { get; private set; }
    public GesturesManager gesturesManager;

    [Header("Voice Commands")]
    public const string move = "move building";
    public const string rotate = "rotate building";
    public const string menu = "main menu";
    public const string money = "give me money";
    public const string close = "close menu";
    //Used for Audio
    public Action<bool> VoiceCommand = delegate { };


    public void SwitchMode(bool On)
    {
        IsNavigating = On;
        if (gesturesManager)
            gesturesManager.IsNavigating = IsNavigating;
    }

    void ISpeechHandler.OnSpeechKeywordRecognized(SpeechEventData eventData)
    {
        // Return if gesture is in progress
        if (InputManager.Instance.CheckModalInputStack())
        {
            VoiceCommand(false);
            return;
        }


        string command = eventData.RecognizedText.ToLower();

        switch (command)
        {
            case move:
                IsNavigating = false;
                break;
            case rotate:
                IsNavigating = true;
                break;
            case menu:
                UIManager.Instance.SwitchState(UIManager.MenuState.MainMenu);
                break;
            case close:
                UIManager.Instance.SwitchState(UIManager.MenuState.Off);
                break;
            case money:
                Game.CurrentSession.AddFunds(10000);
                break;
            default:
                break;
        }

        if (gesturesManager && gesturesManager.IsNavigating != IsNavigating)
        {
            gesturesManager.IsNavigating = IsNavigating;
            VoiceCommand(true);
        }
        else
        {
            VoiceCommand(false);
        }
    }
}
