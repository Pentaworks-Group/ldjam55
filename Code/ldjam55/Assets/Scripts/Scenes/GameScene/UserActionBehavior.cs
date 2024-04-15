using Assets.Scripts.Core.Model;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserActionBehavior : MonoBehaviour
{
    [SerializeField]
    private TMP_Text Name; 
    [SerializeField]
    private TMP_Text Remaining;
    [SerializeField]
    private Image backgroundImage;


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

        if(getSelectedAction.Invoke() == userAction)
        {
            backgroundImage.color = new Color { r = 255, g = 160, b = 194, a = 255 };
        }
        else
        {
            backgroundImage.color = new Color { r = 255, g = 0, b = 91, a = 255 };
        }
    }
}
