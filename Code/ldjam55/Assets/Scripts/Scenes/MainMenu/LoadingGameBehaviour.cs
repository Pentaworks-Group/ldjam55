using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Scenes.MainMenu
{
    public class LoadingGameBehaviour : MonoBehaviour
    {
        void Awake()
        {
            Assets.Scripts.Base.Core.Game.GameLoadedEvent.AddListener(() => Destroy(gameObject));
        }
    }
}
