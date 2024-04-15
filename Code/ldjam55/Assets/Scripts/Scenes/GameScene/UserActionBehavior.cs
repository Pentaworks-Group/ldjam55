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
    private Image overlayImage;


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
            overlayImage.gameObject.SetActive(true);
        }
        else
        {
            overlayImage.gameObject.SetActive(true);
        }
    }
}
