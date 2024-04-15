using Assets.Scripts.Core.Model;
using System;
using System.Collections.Generic;

public class Level
{

    public String Name { get; set; }
    public String Description { get; set; }

    public List<GameEndCondition> EndConditions { get; set; }

}
