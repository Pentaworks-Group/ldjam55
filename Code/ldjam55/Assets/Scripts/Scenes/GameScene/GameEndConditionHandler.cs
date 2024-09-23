using Assets.Scripts.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Scenes.GameScene
{
    public class GameEndConditionHandler
    {
        private Dictionary<string, GameEndCondition> conditions;

        private List<Action<GameEndCondition>> actions = new();

        public GameEndConditionHandler()
        {
            Init();
        }

        public void RegisterListener(Action<GameEndCondition> listener)
        {
            //Init();
            actions.Add(listener);
        }

        public void Init()
        {
            //if (isInitied)
            //{
            //    return;
            //}
            conditions = Base.Core.Game.State.CurrentLevel.EndConditions.ToDictionary(con => con.Name);
            actions = new();
            //isInitied = true;
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
                foreach (var listener in actions)
                {
                    listener.Invoke(gameEndCondition);
                }
            }
        }

        internal void RegisterListener(object v)
        {
            throw new NotImplementedException();
        }
    }
}