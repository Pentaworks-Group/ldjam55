using Assets.Scripts.Core.Model;
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
        if (condition.IsWin)
        {
            Title.text = "You have won";
            nextLevelButton.SetActive(true);
        } else
        {
            Title.text = "You have lost";
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
    }
}
