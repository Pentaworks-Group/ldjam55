using UnityEngine;

namespace Assets.Scripts.Scenes.GameScene
{
    public class GameSceneMenuBehaviour : MonoBehaviour
    {
        [SerializeField]
        private GameDisplayBehaviour gameDisplayBehaviour;
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
        }
    }
}