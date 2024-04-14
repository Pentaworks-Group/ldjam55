using Assets.Scripts.Core.Model;
using System.Collections.Generic;

namespace Assets.Scripts.Core.Definitions
{
    public class Border
    {
        public string Field1Ref { get; set; }

        public string Field2Ref { get; set; }

        public BorderType BorderType { get; set; }

        public BorderStatus BorderStatus { get; set; }
        public List<MethodBundle> Methods { get; set; }
    }
}