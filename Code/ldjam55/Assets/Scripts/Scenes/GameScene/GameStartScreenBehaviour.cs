using Assets.Scripts.Base;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameStartScreenBehaviour : MonoBehaviour
{
    [SerializeField]
    private GameObject gameStartScreen;


    [SerializeField]
    private TextMeshProUGUI nameField;

    [SerializeField]
    private TextMeshProUGUI descriptionField;

    [SerializeField]
    private List<GameObject> ObjectsToHide = new();

    public void ShowStartScreen()
    {
        updateUI();

        gameStartScreen.SetActive(true);

        foreach (GameObject gameObject in ObjectsToHide)
        {
            gameObject.SetActive(false);
        }

    }

    private void updateUI()
    {
        nameField.SetText(Core.Game.State.CurrentLevel.Name);
        descriptionField.SetText(Core.Game.State.CurrentLevel.Description);
    }

    public void HideStartScreen()
    {
        gameStartScreen.SetActive(false);

        foreach (GameObject gameObject in ObjectsToHide)
        {
            gameObject.SetActive(true);
        }
    }
}
