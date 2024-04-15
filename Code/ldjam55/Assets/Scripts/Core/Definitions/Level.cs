using Assets.Scripts.Core.Model;
using System.Collections.Generic;

namespace Assets.Scripts.Core.Definitions
{
    public class Level
    {

        public string Name { get; set; }
        public string Description { get; set; }

        public List<GameEndCondition> EndConditions { get; set; }
        public GameField GameField { get; set; }

        public List<UserAction> UserActions { get; set; }

    }
}