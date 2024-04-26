using Assets.Scripts.Core.Model;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Core.Definitions
{
    public class GameMode : GameFrame.Core.Definitions.GameMode
    {

        public List<Model.Creeper> Creepers { get; set; }
        public List<Level> Levels { get; set; }
        public String StartLevel { get; set; }
        public float NothingFlowRate { get; set; } = 0.1f;
        public float FlowSpeed { get; set; } = 0.5f;
        public float MinFlow { get; set; } = 0.0001f;
        public float MinNewCreep { get; set; } = 0.01f;
    }
}
