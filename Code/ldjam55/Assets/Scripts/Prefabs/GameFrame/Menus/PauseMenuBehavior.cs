using System;
using System.Collections.Generic;

using Assets.Scripts.Constants;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.Prefabs.Menus
{
    public class PauseMenuBehavior : MonoBehaviour
    {

        public UnityEvent<Boolean> PauseToggled = new UnityEvent<Boolean>();

        public List<GameObject> ObjectsToHide = new();

        private GameObject menuToggle;
        private GameObject pauseArea;
        private GameObject optionsArea;
        private GameObject savedGameArea;
        private Button backButton;
        private Button continueButton;


        private Text currentOpenMenu;
        private List<Action> openSubMenues = new List<Action>();


        public GameObject Tutorial;


        private void Awake()
        {
            menuToggle = transform.Find("ToggleArea").gameObject;
            pauseArea = transform.Find("ToggleArea/Background/Background/ContentArea/PauseArea").gameObject;
            optionsArea = transform.Find("ToggleArea/Background/Background/ContentArea/OptionsArea").gameObject;
            savedGameArea = transform.Find("ToggleArea/Background/Background/ContentArea/SaveGameArea").gameObject;

            backButton = transform.Find("ToggleArea/Background/Background/Header/Back").GetComponent<Button>();
            continueButton = transform.Find("ToggleArea/Background/Background/Header/Continue").GetComponent<Button>();

            currentOpenMenu = transform.Find("ToggleArea/Background/Background/Header/Openmenu").GetComponent<Text>();
        }

        void Start()
        {
            menuToggle.SetActive(false);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleEsc();
            }
        }

        private void HandleEsc()
        {
            if (openSubMenues.Count > 0)
            {
                var index = openSubMenues.Count - 1;
                openSubMenues[index].Invoke();
                openSubMenues.RemoveAt(index);
            }
            else
            {
                ToggleMenu();
            }
        }

        public void OpenTutorial()
        {
            Tutorial.SetActive(true);
            menuToggle.SetActive(false);

            OpeningSubMenu(CloseTutorial);
        }

        public void CloseTutorial()
        {
            Tutorial.SetActive(false);
            menuToggle.SetActive(true);
            Base.Core.Game.PlayButtonSound();
        }
        public void ToggleMenu()
        {
            if (menuToggle.activeSelf == true)
            {
                if (this.optionsArea.activeSelf || this.savedGameArea.activeSelf)
                {
                    OnBackButtonClicked();
                }
                else
                {
                    Hide();
                    this.PauseToggled.Invoke(false);
                    foreach (GameObject gameObject in ObjectsToHide)
                    {
                        gameObject.SetActive(true);
                    }
                }
            }
            else
            {
                this.PauseToggled.Invoke(true);
                foreach (GameObject gameObject in ObjectsToHide)
                {
                    gameObject.SetActive(false);
                }
                Show();
                Base.Core.Game.PlayButtonSound();

            }
        }

        public void OpeningSubMenu(Action closeWithEsc)
        {
            openSubMenues.Add(closeWithEsc);
        }

        public void ShowSavedGames()
        {
            Base.Core.Game.PlayButtonSound();

            SetVisible(savedGame: true);
        }

        public void Hide()
        {
            menuToggle.SetActive(false);

            Time.timeScale = 1;

            Base.Core.Game.PlayButtonSound();
        }

        public void Show()
        {
            Time.timeScale = 0;

            SetVisible(pauseMenu: true);

            menuToggle.SetActive(true);
        }

        public void OnBackButtonClicked()
        {
            if (this.savedGameArea.activeSelf)
            {
                Base.Core.Game.SaveOptions();
            }

            Base.Core.Game.PlayButtonSound();
            SetVisible(pauseMenu: true);
        }

        public void ShowOptions()
        {
            Base.Core.Game.PlayButtonSound();
            SetVisible(options: true);
        }

        public void Quit()
        {
            Base.Core.Game.PlayButtonSound();
            Time.timeScale = 1;

            Base.Core.Game.Stop();
            Base.Core.Game.ChangeScene(SceneNames.MainMenu);

        }

        private void SetVisible(Boolean pauseMenu = false, Boolean options = false, Boolean savedGame = false)
        {
            if (pauseMenu)
            {
                currentOpenMenu.text = "Pause";
            }
            else if (options)
            {
                currentOpenMenu.text = "Options";
            }
            else if (savedGame)
            {
                currentOpenMenu.text = "Saved games";
            }

            this.pauseArea.SetActive(pauseMenu);
            this.optionsArea.SetActive(options);
            this.savedGameArea.SetActive(savedGame);

            this.continueButton.gameObject.SetActive(pauseMenu);
            this.backButton.gameObject.SetActive(!pauseMenu);
        }
    }
}
