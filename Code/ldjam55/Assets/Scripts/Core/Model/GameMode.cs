using System;
using System.Collections.Generic;

namespace Assets.Scripts.Core.Model
{
    public class GameMode
    {
        public String Name { get; set; }
        public String Description { get; set; }

        public List<Creeper> Creepers { get; set; }
        public List<GameEndCondition> EndConditions { get; set; }
        public float NothingFlowRate { get; set; } = 0.1f;
        public float FlowSpeed { get; set; } = 0.5f;
        public float MinFlow { get; set; } = 0.0001f;
    }
}
