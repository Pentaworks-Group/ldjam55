using Assets.Scripts.Base;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameStartScreenBehaviour : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI nameField;

    [SerializeField]
    private TextMeshProUGUI descriptionField;


    public void ShowStartScreen()
    {
        UpdateUI();

        gameObject.SetActive(true);

        Time.timeScale = 0.0f;
    }

    private void UpdateUI()
    {
        nameField.SetText(Core.Game.State.CurrentLevel.Name);
        descriptionField.SetText(Core.Game.State.CurrentLevel.Description);
    }

    public void HideStartScreen()
    {
        gameObject.SetActive(false);

        Time.timeScale = 1.0f;
    }
}
