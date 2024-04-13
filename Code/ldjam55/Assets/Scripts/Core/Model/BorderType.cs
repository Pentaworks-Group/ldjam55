using System;

namespace Assets.Scripts.Core.Model
{
    public class BorderType
    {
        public string Model { get; set; }
        public Action<Border> UpdateBorder { get; set; }
    }
}