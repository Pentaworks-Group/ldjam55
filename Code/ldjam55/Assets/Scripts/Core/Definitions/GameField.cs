using System.Collections.Generic;

namespace Assets.Scripts.Core.Definitions
{
    public class GameField: GameFrame.Core.Definitions.BaseDefinition
    {
        public List<Field> Fields { get; set; }

        public List<Border> Borders { get; set; }

        public List<FieldObject> FieldObjects { get; set; }
    }
}