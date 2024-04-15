using Assets.Scripts.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts.Scenes.GameScene
{
    public class GameEndConditionHandler
    {
        private Dictionary<string, GameEndCondition> conditions;

        private Action<GameEndCondition> winAction;
        private Action<GameEndCondition> loseAction;

        private static GameEndConditionHandler _Instance;
        public static GameEndConditionHandler Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new GameEndConditionHandler();
                }
                return _Instance;
            }
        }

        public void Init(Action<GameEndCondition> winAction, Action<GameEndCondition> loseAction)
        {
            this.winAction = winAction;
            this.loseAction = loseAction;
            conditions = Base.Core.Game.State.CurrentLevel.EndConditions.ToDictionary(con => con.Name);
        }


        public void RegisterCondition(string name, int winCount, bool isWin)
        {
            conditions[name] = new GameEndCondition() { Name = name, WinCount = winCount, IsWin = isWin };
        }

        private bool IncrementCount(GameEndCondition gameEndCondition)
        {
            gameEndCondition.CurrentCount++;
            if (gameEndCondition.CurrentCount >= gameEndCondition.WinCount)
            {
                gameEndCondition.Done = true;
            }
            return gameEndCondition.Done;
        }

        public void IncreaseCount(string name)
        {
            var gameEndCondition = conditions[name];
            if (IncrementCount(gameEndCondition))
            {
                if (gameEndCondition.IsWin)
                {
                    winAction(gameEndCondition);
                }
                else
                {
                    loseAction(gameEndCondition);
                }
            }
        }

    }
}