using System;
using System.Collections.Generic;

namespace Assets.Scripts.Core.Model
{
    public class GameMode
    {
        public String Name { get; set; }
        public String Description { get; set; }


        public List<Creeper> Creepers { get; set; }
    }
}
