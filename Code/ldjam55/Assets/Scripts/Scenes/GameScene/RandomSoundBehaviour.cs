using Assets.Scripts.Base;
using GameFrame.Core.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSoundBehaviour : MonoBehaviour
{

    //Random Ambient Sounds
    private float nextSoundEffectTime = 0;

    // Update is called once per frame
    void Update()
    {
        PlayRandomEffectSound();        
    }

    private void PlayRandomEffectSound()
    {
        if (Core.Game.State.TimeElapsed > nextSoundEffectTime && nextSoundEffectTime != 0)
        {
            GameFrame.Base.Audio.Effects.Play(Core.Game.EffectsClipList.GetRandomEntry());
            double randomNumber = UnityEngine.Random.value;

            nextSoundEffectTime = (float)(randomNumber * 20.0 + 5.0 + Core.Game.State.TimeElapsed);
        }
        else if (nextSoundEffectTime == 0)
        {
            double randomNumber = UnityEngine.Random.value;

            nextSoundEffectTime = (float)(randomNumber * 20.0 + 5.0 + Core.Game.State.TimeElapsed);
        }
    }
}
