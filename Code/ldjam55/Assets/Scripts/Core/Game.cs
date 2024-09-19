using System;
using System.Collections.Generic;
using System.Linq;

using Assets.Scripts.Core.Definitions.Loaders;
using Assets.Scripts.Core.Model;

using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Core
{
    public class Game : GameFrame.Core.Game<GameState, PlayerOptions, SavedGamePreview>
    {
        private readonly Dictionary<String, Definitions.GameMode> availableGameModes = new Dictionary<String, Definitions.GameMode>();
        private readonly Dictionary<String, Definitions.GameField> availableGameFields = new Dictionary<String, Definitions.GameField>();

        public static Definitions.GameMode SelectedGameMode { get; set; }

        public UnityEvent GameLoadedEvent { get; set; } = new UnityEvent();
        public bool IsRunning { get; set; } = false;

        public int GameSpeed { get; set; } = 1;

        public List<AudioClip> EffectsClipList { get; set; }

        public IList<Definitions.GameMode> AvailableGameModes
        {
            get
            {
                if (this.availableGameModes.Count == 0)
                {
                    LoadGameDefinitions();
                }

                return this.availableGameModes.Values.ToList();
            }
        }

        public UserActionHandler UserActionHandler { get; private set; } = new UserActionHandler();

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

            Debug.Log("InitGameState: " + availableGameFields.Count);
            var gameState = new GameState()
            {
                CreatedOn = DateTime.Now,
                CurrentScene = Constants.SceneNames.Game,
                Mode = ConvertGameMode(SelectedGameMode),
            };

            if (SelectedGameMode.StartLevel != null)
            {
                gameState.CurrentLevel = GetLevel(SelectedGameMode.StartLevel);
            }
            if (gameState.CurrentLevel == null)
            {
                gameState.CurrentLevel = ConvertLevel(SelectedGameMode.Levels[0]);
            }

            return gameState;
        }

        public Level GetLevel(string levelName)
        {
            foreach (var level in SelectedGameMode.Levels)
            {
                if (level.Name == levelName)
                {
                    return ConvertLevel(level);
                }
            }

            return null;
        }

        public void StartNextLevel()
        {
            var nL = State.CurrentLevel.NextLevel;
            var level = GetLevel(nL);
            StartLevel(level);
        }

        public void RestartLevel()
        {
            IsRunning = false;

            var nL = State.CurrentLevel.Name;
            var level = GetLevel(nL);

            StartLevel(level);
        }

        public void StartCurrenLevelWithField(Definitions.GameField field)
        {
            foreach (var level in SelectedGameMode.Levels)
            {
                if (level.Name == State.CurrentLevel.Name)
                {
                    var oldGameField = level.GameField;

                    level.GameField = field;

                    var convLevel = ConvertLevel(level);

                    StartLevel(convLevel);

                    level.GameField = oldGameField;

                    break;
                }
            }
        }

        public void StartLevel(Level level)
        {
            State.CurrentLevel = level;
            ChangeScene(State.CurrentScene);
        }

        public Level GetNextLevel()
        {
            var nL = State.CurrentLevel.NextLevel;
            return GetLevel(nL);
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
                AmbienceVolume = 0.70f,
                BackgroundVolume = 0.70f,
            };
        }

        protected override void OnGameStart()
        {
            LoadGameDefinitions();
            InitializeAudioClips();

            EffectsClipList = new List<AudioClip>()
            {
                GameFrame.Base.Resources.Manager.Audio.Get("Slime_3"),
                GameFrame.Base.Resources.Manager.Audio.Get("Slime_4"),
                GameFrame.Base.Resources.Manager.Audio.Get("Slime_5"),
                GameFrame.Base.Resources.Manager.Audio.Get("Slime_6"),
                GameFrame.Base.Resources.Manager.Audio.Get("Slime_7"),
                GameFrame.Base.Resources.Manager.Audio.Get("Slime_8"),
                GameFrame.Base.Resources.Manager.Audio.Get("Slime_9"),
                GameFrame.Base.Resources.Manager.Audio.Get("Slime_10"),
                GameFrame.Base.Resources.Manager.Audio.Get("Slime_11"),
                GameFrame.Base.Resources.Manager.Audio.Get("Slime_12"),
            };

            GameLoadedEvent.Invoke();
        }

        private void LoadGameDefinitions()
        {
            var filePath = $"{Application.streamingAssetsPath}/GameFields.json";
            var filePath2 = $"{Application.streamingAssetsPath}/GameModes.json";

            new GameFrame.Core.Definitions.Loaders.DefinitionLoader<Definitions.GameField>(this.availableGameFields).LoadAssets(filePath, ValidateGameFields);

            new GameModesLoader(this.availableGameModes, availableGameFields).LoadAssets(filePath2);
        }

        private Boolean ValidateGameFields(List<Definitions.GameField> fields)
        {
            var isSuccessful = true;

            Debug.Log($"Testing GameFields: {fields?.Count}");

            foreach (var field in fields)
            {
                if (field.Fields == null)
                {
                    isSuccessful = false;
                    Debug.Log("Loaded gamefield with no fields: " + field.Reference);
                }
            }

            return isSuccessful;
        }

        private void InitializeAudioClips()
        {
            InitializeBackgroundAudio();
        }

        private void InitializeBackgroundAudio()
        {
            var backgroundAudioClips = new List<AudioClip>()
            {
                //TODO: change when scene changes
                GameFrame.Base.Resources.Manager.Audio.Get("Music_1"),
/*                GameFrame.Base.Resources.Manager.Audio.Get("Music_2"),
                GameFrame.Base.Resources.Manager.Audio.Get("Music_3"),
                GameFrame.Base.Resources.Manager.Audio.Get("Music_4"),
                GameFrame.Base.Resources.Manager.Audio.Get("Music_5")*/
            };

            GameFrame.Base.Audio.Background.Play(backgroundAudioClips);

            var ambienceAudioClips = new List<AudioClip>()
            {
                GameFrame.Base.Resources.Manager.Audio.Get("Forest_Sounds"),
            };

            GameFrame.Base.Audio.Ambience.Play(ambienceAudioClips);
        }

        private Model.GameField ConvertGameField(Definitions.GameField gameFieldDefinition)
        {
            Debug.Log("Converting field: " + gameFieldDefinition.Reference);
            if (gameFieldDefinition.Fields == null)
            {
                Debug.Log("Fields: " + gameFieldDefinition.Fields);
                Debug.Log("other fields: " + availableGameFields[gameFieldDefinition.Reference].Fields);
            }
            var gameField = new Model.GameField()
            {
                IsReferenced = gameFieldDefinition.IsReferenced,
                Reference = gameFieldDefinition.Reference
            };

            gameField.Fields = ConvertFields(gameFieldDefinition.Fields);
            var fields = gameField.Fields.ToDictionary(field => field.ID, field => field);
            gameField.Borders = ConvertBordersForGameField(gameFieldDefinition.Borders, fields);
            gameField.FieldObjects = ConvertFieldObjects(gameFieldDefinition.FieldObjects, fields);

            return gameField;
        }

        private List<Field> ConvertFields(List<Definitions.Field> fieldDefinitionList)
        {
            if (fieldDefinitionList == null)
            {
                Debug.Log("Null fieldDefinitionList");

                return new();
            }
            var fieldList = new List<Model.Field>();

            foreach (var borderDefinition in fieldDefinitionList)
            {
                var field = new Model.Field()
                {
                    Coords = borderDefinition.Coords,
                    Height = borderDefinition.Height,
                    ID = borderDefinition.ID,
                };

                fieldList.Add(field);
            }

            return fieldList;
        }

        private List<Border> ConvertBordersForGameField(List<Definitions.Border> borderDefinitionList, Dictionary<string, Model.Field> fieldDict)
        {
            var borderList = new List<Model.Border>();

            if (borderDefinitionList?.Count > 0)
            {
                foreach (var borderDefinition in borderDefinitionList)
                {
                    borderList.Add(ConvertBorder(borderDefinition, fieldDict));
                }
            }

            return borderList;
        }

        private Model.Border ConvertBorder(Definitions.Border borderDefinition, Dictionary<string, Model.Field> fields)
        {
            var border = new Model.Border() { BorderStatus = borderDefinition.BorderStatus, Methods = borderDefinition.Methods };

            border.Field1 = fields[borderDefinition.Field1Ref];
            border.Field2 = fields[borderDefinition.Field2Ref];

            border.Field1.Borders.Add(border);
            border.Field2.Borders.Add(border);

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
                EndConditions = CopyGameEndCondition(level.EndConditions),
                UserActions = CopyUserActions(level.UserActions),
                NextLevel = level.NextLevel
            };
            Debug.Log("description:" + level.GameField.Description + " isReferenced: " + level.GameField.IsReferenced);
            //level.GameField = availableGameFields[level.GameField.Reference];
            modelLevel.GameField = ConvertGameField(level.GameField);

            return modelLevel;
        }

        private List<UserAction> CopyUserActions(List<UserAction> actions)
        {
            var cps = new List<UserAction>();

            foreach (var action in actions)
            {
                cps.Add(new UserAction()
                {
                    Name = action.Name,
                    ActionParamers = action.ActionParamers,
                    Cooldown = action.Cooldown,
                    UsesRemaining = action.UsesRemaining
                });
            }

            return cps;
        }

        private List<GameEndCondition> CopyGameEndCondition(List<GameEndCondition> conditions)
        {
            var cps = new List<GameEndCondition>();

            foreach (var action in conditions)
            {
                cps.Add(new GameEndCondition()
                {
                    Name = action.Name,
                    CurrentCount = action.CurrentCount,
                    Description = action.Description,
                    Done = action.Done,
                    IsWin = action.IsWin,
                    WinCount = action.WinCount
                });
            }

            return cps;
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
                MinNewCreep = selectedGameMode.MinNewCreep,
                NothingFlowRate = selectedGameMode.NothingFlowRate
            };

            return gameMode;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void GameStart()
        {
            Base.Core.Game.Startup();
        }
    }
}