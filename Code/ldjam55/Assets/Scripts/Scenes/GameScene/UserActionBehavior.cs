using Assets.Scripts.Core.Model;
using Assets.Scripts.Scene.GameScene;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UserActionBehavior : MonoBehaviour
{
    [SerializeField]
    private TMP_Text Name; 
    [SerializeField]
    private TMP_Text Remaining;


    private UserAction userAction;
    private GameFieldTestingClickBehaviour gameFieldTestingClickBehaviour;

    public void SelectThisAction()
    {
        gameFieldTestingClickBehaviour.SelectUserAction(userAction);
    }

    public void Init(UserAction userAction, GameFieldTestingClickBehaviour gameFieldTestingClickBehaviour)
    {
        this.userAction = userAction;
        this.gameFieldTestingClickBehaviour = gameFieldTestingClickBehaviour;
    }

    public void UpdateUI()
    {
        Name.text = userAction.Name;
        Remaining.text = userAction.UsesRemaining.ToString();
    }
}
