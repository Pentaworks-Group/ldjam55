using Assets.Scripts.Core.Definitions.Loaders;
using Assets.Scripts.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Assets.Scripts.Core
{
    public class Game : GameFrame.Core.Game<GameState, PlayerOptions, SavedGamePreview>
    {
        private readonly Dictionary<String, Definitions.GameMode> availableGameModes = new Dictionary<String, Definitions.GameMode>();
        private readonly Dictionary<String, Definitions.GameField> availableGameFields = new Dictionary<String, Definitions.GameField>();

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
                CurrentScene = Constants.SceneNames.Game,
                GameField = ConvertGameField(SelectedGameMode.GameField),
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
            new ResourceLoader<Definitions.GameField>(this.availableGameFields).LoadDefinition("GameFields.json");
            //new ResourceLoader<Definitions.Star>(this.availableStars).LoadDefinition("Stars.json");
            new GameModesLoader(this.availableGameModes, this.availableGameFields).LoadDefinition("GameModes.json");
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

        private Model.GameField ConvertGameField(Definitions.GameField gameFieldDefinition)
        {
            var gameField = new Model.GameField() { Fields = gameFieldDefinition.Fields };
            var fields = gameField.Fields.ToDictionary(field => field.ID, field => field);
            gameField.Borders = ConvertBordersForGameField(gameFieldDefinition.Borders, fields);
            gameField.FieldObjects = ConvertFieldObjects(gameFieldDefinition.FieldObjects, fields);
            return gameField;
        }

        private List<Border> ConvertBordersForGameField(List<Definitions.Border> borderDefinitionList, Dictionary<string, Model.Field> fieldDict)
        {

            var borderList = new List<Model.Border>(); 
            foreach (var borderDefinition in borderDefinitionList)
            {
                borderList.Add(ConvertBorder(borderDefinition, fieldDict));
            }
            return borderList;
        }

        private Model.Border ConvertBorder(Definitions.Border borderDefinition, Dictionary<string, Model.Field> fields)
        {
            var border = new Model.Border() { BorderStatus = borderDefinition.BorderStatus };

            border.Field1 = fields[borderDefinition.Field1Ref];
            border.Field2 = fields[borderDefinition.Field2Ref];

            border.BorderType = ConvertBorderType(borderDefinition.BorderType); 

            return border;
        }

        private Model.BorderType ConvertBorderType(Definitions.BorderType borderTypeDefinition)
        {
            var borderType = new Model.BorderType() { Model = borderTypeDefinition.Model, Name = borderTypeDefinition.Name };


            return borderType;
        }

        private List<Model.FieldObject> ConvertFieldObjects(List<Definitions.FieldObject> fieldObjects, Dictionary<string, Model.Field> fieldDict)
        {
            var modelFieldObjects = new List<Model.FieldObject>();

            foreach (var fieldObject in fieldObjects)
            {
                var modelFieldObject = new Model.FieldObject()
                {
                    Name = fieldObject.Name,
                    Description = fieldObject.Description,
                    Model = fieldObject.Model,
                    Field = fieldDict[fieldObject.FieldReference],
                    UpdateMethod = fieldObject.UpdateMethod,
                    UpdateMethodParameters = fieldObject.UpdateMethodParameters
                };
                modelFieldObjects.Add(modelFieldObject);
                modelFieldObject.Field.FieldObjects.Add(modelFieldObject);
            }

            return modelFieldObjects;
        }

        private Model.GameMode ConvertGameMode(Definitions.GameMode selectedGameMode)
        {
            var gameMode = new Model.GameMode()
            {
                Name = selectedGameMode.Name,
                Description = selectedGameMode.Description,
                Creepers = selectedGameMode.Creepers
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