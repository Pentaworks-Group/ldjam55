using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Assets.Scripts.Core
{
    public class Game : GameFrame.Core.Game<GameState, PlayerOptions, SavedGamePreview>
    {
        private readonly Dictionary<String, Definitions.GameMode> availableGameModes = new Dictionary<String, Definitions.GameMode>();

        public static Definitions.GameMode SelectedGameMode { get; set; }


        public IList<Definitions.GameMode> AvailableGameModes
        {
            get
            {
                if (this.availableGameModes.Count == 0)
                {
                    LoadGameSettings();
                }

                return this.availableGameModes.Values.ToList();
            }
        }



        public void PlayButtonSound()
        {
            GameFrame.Base.Audio.Effects.Play("Button");
        }

        protected override GameState InitializeGameState()
        {
            if (SelectedGameMode == default)
            {
                if (this.availableGameModes.Count > 0)
                {
                    if (this.availableGameModes.TryGetValue("default", out var defaultGameMode))
                    {
                        SelectedGameMode = defaultGameMode;
                    }
                    else
                    {
                        throw new Exception("No 'default' GameMode defined!");
                    }
                }
                else
                {
                    throw new Exception("Failed to load GameModes!");
                }
            }

            var gameState = new GameState()
            {
                CreatedOn = DateTime.Now,
                //CurrentScene = Constants.SceneNames.Space,
                Mode = ConvertGameMode(SelectedGameMode),
            };

            return gameState;
        }

        protected override PlayerOptions InitialzePlayerOptions()
        {
            return new PlayerOptions()
            {
                EffectsVolume = 0.7f,
                AmbienceVolume = 0.10f,
                BackgroundVolume = 0.05f,
            };
        }

        protected override void OnGameStart()
        {
            LoadGameSettings();

            InitializeAudioClips();

        }

        private void LoadGameSettings()
        {
            //new ResourceLoader<Definitions.Spacecraft>(this.availableSpacecrafts).LoadDefinition("Spacecrafts.json");
            //new ResourceLoader<Definitions.Star>(this.availableStars).LoadDefinition("Stars.json");
            //new GameModesLoader(this.availableGameModes, this.availableStars, this.availableSpacecrafts).LoadDefinition("GameModes.json");
        }

        private void InitializeAudioClips()
        {
            InitializeBackgroundAudio();
        }

        private void InitializeBackgroundAudio()
        {
            var backgroundAudioClips = new List<AudioClip>()
            {

            };

            GameFrame.Base.Audio.Background.Play(backgroundAudioClips);
        }

        private Model.GameMode ConvertGameMode(Definitions.GameMode selectedGameMode)
        {
            var gameMode = new Model.GameMode()
            {
                Name = selectedGameMode.Name,
                Description = selectedGameMode.Description,
                //JunkSpawnInterval = selectedGameMode.JunkSpawnInterval.GetValueOrDefault(-1),
                //JunkSpawnInitialDistance = selectedGameMode.JunkSpawnInitialDistance.GetValueOrDefault(),
                //JunkSpawnPosition = selectedGameMode.JunkSpawnPosition?.Copy(),
                //JunkSpawnForce = selectedGameMode.JunkSpawnForce.Copy(),
                //JunkSpawnTorque = selectedGameMode.JunkSpawnTorque.Copy(),
                //ShipSpawnDistance = selectedGameMode.ShipSpawnDistance.GetValueOrDefault(),
                //RequiredSurvivors = selectedGameMode.RequiredSurvivors,
            };

            //gameMode.Star = ConvertStar(selectedGameMode.Stars.GetRandomEntry());
            //gameMode.Spacecrafts = ConvertSpacecrafts(selectedGameMode.Spacecrafts);
            //gameMode.PlayerSpacecrafts = ConvertSpacecrafts(selectedGameMode.PlayerSpacecrafts);

            return gameMode;
        }



        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void GameStart()
        {
            Base.Core.Game.Startup();
        }
    }
}