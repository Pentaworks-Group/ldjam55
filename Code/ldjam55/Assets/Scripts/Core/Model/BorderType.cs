using System;

namespace Assets.Scripts.Core.Definitions
{
    public class BorderType : GameFrame.Core.Definitions.BaseDefinition
    {
        public string Name { get; set; }
        public string Model { get; set; }
        public string UpdateMethod { get; set; }

        public string UpdateMethodArguments { get; set; }
    }
}