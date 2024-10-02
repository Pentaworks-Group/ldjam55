using UnityEngine;

namespace Assets.Scripts.Scenes.MainMenu
{
    public class LoadingGameBehaviour : MonoBehaviour
    {
        void Awake()
        {
            if (Base.Core.Game.IsLoaded)
            {
                gameObject.SetActive(false);
                //Destroy(gameObject);
            }
            else
            {
                Base.Core.Game.GameLoadedEvent.AddListener(() => gameObject.SetActive(false));
                //Base.Core.Game.GameLoadedEvent.AddListener(() => Destroy(gameObject));
            }
        }
    }
}
