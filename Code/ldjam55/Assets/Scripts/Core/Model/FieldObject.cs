using System;

namespace Assets.Scripts.Core.Model
{
    public class FieldObject
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Model { get; set; }
        public string Material { get; set; }
        public Field Field { get; set; }

        public string UpdateMethod { get; set; }

        public string UpdateMethodParameters { get; set; }

        public Action UpdateMethodAction { get; set; }
    }
}