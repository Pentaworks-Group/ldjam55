using Assets.Scripts.Core.Model;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UserActionBehavior : MonoBehaviour
{
    [SerializeField]
    private TMP_Text Name;
    [SerializeField]
    private TMP_Text HoverText;
    [SerializeField]
    private TMP_Text Remaining;
    [SerializeField]
    private Image overlayImage;
    [SerializeField]
    private Image actionIcon;


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
        this.HoverText.text = userAction.Name;

        Remaining.text = userAction.UsesRemaining.ToString();

        if (userAction.IconName != null)
        {
            var sprite = GameFrame.Base.Resources.Manager.Sprites.Get(userAction?.IconName);
            actionIcon.sprite = sprite;
            actionIcon.gameObject.SetActive(true);
            Name.gameObject.SetActive(false);
        }

        if (getSelectedAction.Invoke() == userAction)
        {
            overlayImage.gameObject.SetActive(true);
        }
        else
        {
            overlayImage.gameObject.SetActive(false);
        }
    }
}
