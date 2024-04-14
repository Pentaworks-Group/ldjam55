using UnityEngine;

namespace Assets.Scripts.Scenes.GameScene
{
    public class GameFieldTestingMenuBehaviour : MonoBehaviour
    {
        [SerializeField]
        private GameDisplayBehaviour gameDisplayBehaviour;

        [SerializeField]
        private CreepBehaviour gameSceneBehaviour;

        private bool isRunning = false;

        private void Awake()
        {
            
            if (Base.Core.Game.State == default)
            {
                var gameState = Base.Core.Game.GetInitGameState();
                gameState.CurrentScene = Constants.SceneNames.GameFieldTest;
                Base.Core.Game.Start(gameState);
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