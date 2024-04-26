using System.Diagnostics;

namespace Assets.Scripts.Core.Model
{
    public class Creep 
    {
        public Creeper Creeper { get; set; }

        public float Value { get; set; }

        public bool CreeperChanged { get; set; } = false;

        public int PaintRadiusOld { get; set; }
    }
}