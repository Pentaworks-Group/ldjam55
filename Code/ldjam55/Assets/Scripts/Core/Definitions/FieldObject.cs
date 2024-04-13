namespace Assets.Scripts.Core.Definitions
{
    public class FieldObject : GameFrame.Core.Definitions.BaseDefinition
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Model { get; set; }

        public string FieldReference { get; set; }

        public string UpdateMethod { get; set; }

        public string UpdateMethodParameters { get; set; }
    }
}