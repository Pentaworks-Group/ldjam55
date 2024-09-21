using UnityEngine;

namespace Assets.Scripts.Scenes.MainMenu
{
    public class LoadingGameBehaviour : MonoBehaviour
    {
        void Awake()
        {
            if (Base.Core.Game.isLoaded)
            {
                Destroy(gameObject);
            } else
            {
                Base.Core.Game.GameLoadedEvent.AddListener(() => Destroy(gameObject));
            }
        }
    }
}
