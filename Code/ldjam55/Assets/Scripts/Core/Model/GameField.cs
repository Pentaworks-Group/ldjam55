using System.Collections.Generic;

namespace Assets.Scripts.Core.Model
{
    public class GameField
    {
        public List<Field> Fields { get; set; }

        public List<Border> Borders { get; set; }
    }
}