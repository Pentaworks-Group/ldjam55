using Assets.Scripts.Core.Model;
using System.Collections.Generic;

namespace Assets.Scripts.Core.Definitions
{
    public class GameMode : GameFrame.Core.Definitions.GameMode
    {
        public GameField GameField {  get; set; }

        public List<Model.Creeper> Creepers { get; set; }

        public List<GameEndCondition> EndConditions { get; set; }

        public float NothingFlowRate { get; set; } = 0.1f;
        public float FlowSpeed { get; set; } = 0.5f;
        public float MinFlow { get; set; } = 0.0001f;
    }
}
