using System.Collections.Generic;

namespace Assets.Scripts.Core.Definitions
{
    public class FieldObject : GameFrame.Core.Definitions.BaseDefinition
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Model { get; set; }
        public string Material { get; set; }
        public bool Hidden { get; set; }

        public string FieldReference { get; set; }

        public List<MethodBundle> Methods { get; set; }
    }
}