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

    public void UpdateUI(GameEndCondition conditon, string customMessage)
    {
        if (conditon.IsWin)
        {
            Title.text = "You have won";
            nextLevelButton.SetActive(true);
        } else
        {
            Title.text = "You have lost";
        }
        Message.text = "Because: " + conditon.Name + customMessage;

        Time.timeScale = 0.0f;
    }
}
