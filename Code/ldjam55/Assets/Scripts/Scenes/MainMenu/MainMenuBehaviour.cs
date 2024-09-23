using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Assets.Scripts.Constants;

using UnityEngine;

namespace Assets.Scripts.Scenes.MainMenu
{
    public class MainMenuBehaviour : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void Quit();

        [SerializeField]
        private GameObject Tutorial;
        [SerializeField]
        private GameObject Menu;

        private void Start()
        {
            var backgroundAudioClips = new List<AudioClip>()
            {
                GameFrame.Base.Resources.Manager.Audio.Get("Music_1")
            };

            GameFrame.Base.Audio.Background.ReplaceClips(backgroundAudioClips);
        }

        public void PlayGame()
        {
            Base.Core.Game.PlayButtonSound();
            Base.Core.Game.Start();

        }

        public void ShowOptions()
        {
            ShowScene(SceneNames.Options);
        }

        public void ShowCredits()
        {
            ShowScene(SceneNames.Credits, false);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
            Quit();
#elif UNITY_STANDALONE
            Application.Quit();            
#endif
        }

        private void ShowScene(String sceneName, Boolean playButtonSound = true)
        {
            if (playButtonSound)
            {
                Base.Core.Game.PlayButtonSound();
            }

            Base.Core.Game.ChangeScene(sceneName);
        }

        public void ShowTutorial()
        {
            Tutorial.SetActive(true);
            Menu.SetActive(false);
        }

        public void HideTutorial()
        {
            Base.Core.Game.PlayButtonSound();
            Tutorial.SetActive(false);
            Menu.SetActive(true);
        }
    }
}
