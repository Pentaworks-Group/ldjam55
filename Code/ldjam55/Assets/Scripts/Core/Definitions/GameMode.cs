using System.Collections.Generic;

namespace Assets.Scripts.Core.Definitions
{
    public class GameMode : GameFrame.Core.Definitions.GameMode
    {
        public GameField GameField {  get; set; }

        public List<Model.Creeper> Creepers { get; set; }
    }
}
