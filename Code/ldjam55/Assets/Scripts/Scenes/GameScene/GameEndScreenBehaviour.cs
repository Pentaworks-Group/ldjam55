using Assets.Scripts.Base;
using Assets.Scripts.Core.Model;
using GameFrame.Core.Extensions;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameEndScreenBehaviour : MonoBehaviour
{
    [SerializeField]
    private TMP_Text Title;
    [SerializeField]
    private TMP_Text Message;


    [SerializeField]
    private GameObject nextLevelButton;

    [SerializeField]
    private GameObject restartLevelButton;


    private Dictionary<string, string> standardEndConditionDescriptions;

    GameEndScreenBehaviour() { 
        standardEndConditionDescriptions = new Dictionary<string, string>()
        {
            { "InstantWin", "Test" },
            { "InstantLoose", "Test" },
            { "WinTouch", "You reached the target field." },
            { "LooseTouch", "The enemy reached the target field." }
        };
    }

    public void UpdateUI(GameEndCondition condition, string customMessage)
    {
        GameFrame.Base.Audio.Effects.Play("Win");

        if (condition.IsWin)
        {
            Title.text = "You have won";
            nextLevelButton.SetActive(true);
            restartLevelButton.SetActive(false);
        } else
        {
            Title.text = "You have lost";
            nextLevelButton.SetActive(false);
            restartLevelButton.SetActive(true);
        }

        if (standardEndConditionDescriptions.TryGetValue(condition.Name, out var message))
        {
            Message.text = "Because: " + message;
        }
        else
        {
            Message.text = "Because: " + customMessage;
        }

        Time.timeScale = 0.0f;

//        gameObject.SetActive(true);
    }
}
