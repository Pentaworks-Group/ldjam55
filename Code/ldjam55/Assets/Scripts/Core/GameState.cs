using Assets.Scripts.Core.Model;
using System;

namespace Assets.Scripts.Core
{
    public class GameState : GameFrame.Core.GameState
    {
        public GameMode Mode { get; set; }
        public Double TimeElapsed { get; set; }


        public Level CurrentLevel { get; set; }

    }
}
