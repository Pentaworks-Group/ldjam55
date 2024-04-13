using System;
using System.Collections.Generic;


namespace Assets.Scripts.Core.Model
{
    public class Field
    {
        public String ID { get; set; }
        public GameFrame.Core.Math.Vector2 Coords { get; set; }
        public int Height { get; set; }

        public List<FieldObject> Objects { get; set; }

        public Creep Creep { get; set; }




    }
}
