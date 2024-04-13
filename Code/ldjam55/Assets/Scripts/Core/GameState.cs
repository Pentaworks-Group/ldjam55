using System;
using System.Collections.Generic;

using Assets.Scripts.Core.Model;

namespace Assets.Scripts.Core
{
    public class GameState : GameFrame.Core.GameState
    {
        public GameMode Mode { get; set; }
        public Double TimeElapsed { get; set; }
    }
}