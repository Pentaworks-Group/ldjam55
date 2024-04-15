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


        public GameState GetInitGameState()
        {
            return InitializeGameState();
        }
        protected override GameState InitializeGameState()
        {
            if (SelectedGameMode == default)
            {
                if (this.availableGameModes.Count > 0)
                {
                    SelectDefaultGameMode();
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
                Mode = ConvertGameMode(SelectedGameMode),
            };

            if (SelectedGameMode.StartLevel != null)
            {
                foreach (var level in SelectedGameMode.Levels) {
                    if (level.Name == SelectedGameMode.StartLevel)
                    {
                        gameState.CurrentLevel = ConvertLevel(level);
                    }
                }
            }
            if (gameState.CurrentLevel == null) {
                gameState.CurrentLevel = ConvertLevel(SelectedGameMode.Levels[0]);
            }

            return gameState;
        }

        private void SelectDefaultGameMode()
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
            var gameField = new Model.GameField() { Fields = gameFieldDefinition.Fields, IsReferenced = gameFieldDefinition.IsReferenced, Reference = gameFieldDefinition.Reference };
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
            var border = new Model.Border() { BorderStatus = borderDefinition.BorderStatus, Methods = borderDefinition.Methods };

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
                    Material = fieldObject.Material,
                    Hidden = fieldObject.Hidden,
                    Field = fieldDict[fieldObject.FieldReference],
                    Methods = fieldObject.Methods
                };
                modelFieldObjects.Add(modelFieldObject);
                modelFieldObject.Field.FieldObjects.Add(modelFieldObject);
            }

            return modelFieldObjects;
        }

        private List<Model.Level> ConvertLevels(List<Definitions.Level> levels)
        {
            var modelLevels = new List<Model.Level>();

            foreach (var level in levels)
            {
                Level modelLevel = ConvertLevel(level);
                modelLevels.Add(modelLevel);
            }

            return modelLevels;
        }

        private Level ConvertLevel(Definitions.Level level)
        {
            var modelLevel = new Model.Level()
            {
                Name = level.Name,
                Description = level.Description,
                EndConditions = level.EndConditions,
                UserActions = level.UserActions
            };
            modelLevel.GameField = ConvertGameField(level.GameField);
            return modelLevel;
        }

        private Model.GameMode ConvertGameMode(Definitions.GameMode selectedGameMode)
        {
            var gameMode = new Model.GameMode()
            {
                Name = selectedGameMode.Name,
                Description = selectedGameMode.Description,
                Creepers = selectedGameMode.Creepers,
                StartLevel = selectedGameMode.StartLevel,
                FlowSpeed = selectedGameMode.FlowSpeed,
                MinFlow = selectedGameMode.MinFlow,
                NothingFlowRate = selectedGameMode.NothingFlowRate,
                Levels = ConvertLevels(selectedGameMode.Levels),
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