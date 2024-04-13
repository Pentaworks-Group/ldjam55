using Assets.Scripts.Base;

using UnityEngine;
using UnityEngine.UI;

public class OptionsMenuBehaviour : MonoBehaviour
{
    public Slider EffectsVolumeSlider;
    public Slider AmbienceVolumeSlider;
    public Slider BackgroundVolumeSlider;

    private void Awake()
    {
    }

    private void Start()
    {
        UpdateValues();
    }

    private void FixedUpdate()
    {
        UpdateValues();
    }

    public void OnEffectsVolumeSliderChanged()
    {
        if (Core.Game?.Options != default)
        {
            Core.Game.Options.EffectsVolume = EffectsVolumeSlider.value;
        }
    }

    public void OnAmbienceVolumeSliderChanged()
    {
        if (Core.Game?.Options != default)
        {
            Core.Game.Options.AmbienceVolume = AmbienceVolumeSlider.value;
        }
    }

    public void OnBackgroundVolumeSliderChanged()
    {
        if (Core.Game?.Options != default)
        {
            Core.Game.Options.BackgroundVolume = BackgroundVolumeSlider.value;
        }
    }

    public void OnRestoreDefaultsClick()
    {
        Core.Game.PlayButtonSound();

        Core.Game.RestoreDefaultOptions();

        UpdateValues();
    }

    public void SaveOptions()
    {
        Core.Game.PlayButtonSound();

        Core.Game.SaveOptions();
    }

    private void UpdateValues()
    {
        if (Core.Game.Options != default)
        {
            if (this.EffectsVolumeSlider.value != Core.Game.Options.EffectsVolume)
            {
                this.EffectsVolumeSlider.value = Core.Game.Options.EffectsVolume;
            }

            if (this.AmbienceVolumeSlider.value != Core.Game.Options.AmbienceVolume)
            {
                this.AmbienceVolumeSlider.value = Core.Game.Options.AmbienceVolume;
            }

            if (this.BackgroundVolumeSlider.value != Core.Game.Options.BackgroundVolume)
            {
                this.BackgroundVolumeSlider.value = Core.Game.Options.BackgroundVolume;
            }
        }
    }
}
