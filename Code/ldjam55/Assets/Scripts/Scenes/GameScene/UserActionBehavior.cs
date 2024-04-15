using Assets.Scripts.Core.Model;
using System;
using TMPro;
using UnityEngine;

public class UserActionBehavior : MonoBehaviour
{
    [SerializeField]
    private TMP_Text Name; 
    [SerializeField]
    private TMP_Text Remaining;


    private UserAction userAction;
    private Action<UserAction> selectAction;

    public void SelectThisAction()
    {
        selectAction.Invoke(userAction);
    }

    public void Init(UserAction userAction, Action<UserAction> selectAction)
    {
        this.userAction = userAction;
        this.selectAction = selectAction;
    }

    public void UpdateUI()
    {
        Name.text = userAction.Name;
        Remaining.text = userAction.UsesRemaining.ToString();
    }
}
