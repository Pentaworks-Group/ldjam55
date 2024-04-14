using Assets.Scripts.Core.Model;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Scenes.GameScene
{
    public class GameEndConditionHandler
    {
        private Dictionary<string, GameEndCondition> conditions = new Dictionary<string, GameEndCondition>();

        private readonly Action<GameEndCondition> winAction;
        private readonly Action<GameEndCondition> loseAction;


        public GameEndConditionHandler(Action<GameEndCondition> winAction, Action<GameEndCondition> loseAction)
        {
            this.winAction = winAction;
            this.loseAction = loseAction;
            foreach (var condition in conditions)
            {

            }
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