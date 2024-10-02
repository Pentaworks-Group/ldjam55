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
        private GameObject LoadingScreen;

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
            LoadingScreen.SetActive(true);
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

    }
}
