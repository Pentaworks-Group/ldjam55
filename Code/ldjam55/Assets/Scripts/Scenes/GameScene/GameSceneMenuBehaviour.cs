using UnityEngine;

namespace Assets.Scripts.Scenes.GameScene
{
    public class GameSceneMenuBehaviour : MonoBehaviour
    {
        [SerializeField]
        private GameDisplayBehaviour gameDisplayBehaviour;

        [SerializeField]
        private GameSceneBehaviour gameSceneBehaviour;

        private bool isRunning = false;

        private void Awake()
        {
            if (Base.Core.Game.State == default)
            {
                Base.Core.Game.Start();
            }
        }


        private void Start()
        {
            gameDisplayBehaviour.GenerateGameField();
            gameSceneBehaviour.StartGame();
            isRunning = true;
        }


        private void Update()
        {
            if (isRunning)
            {
                gameDisplayBehaviour.UpdateCreep();
            }
        }

        public void ClearCreep()
        {
            gameDisplayBehaviour.ClearCreep();
        }


        public void ToggleCreeperActivity()
        {
            isRunning = !isRunning;
            gameSceneBehaviour.ToggleCreeperActivity();
            if (isRunning )
            {
                Time.timeScale = 1f;
            } else
            {
                Time.timeScale = 0f;
            }
        }
    }
}