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
    private Func<UserAction> getSelectedAction;

    public void SelectThisAction()
    {
        selectAction.Invoke(userAction);
    }

    public void Init(UserAction userAction, Action<UserAction> selectAction, Func<UserAction> getSelectedAction)
    {
        this.userAction = userAction;
        this.selectAction = selectAction;
        this.getSelectedAction = getSelectedAction;
    }

    public void UpdateUI()
    {
        Name.text = userAction.Name;
        Remaining.text = userAction.UsesRemaining.ToString();
        if (getSelectedAction.Invoke() == userAction)
        {
            //HiglightTHIngs
        }
    }
}
