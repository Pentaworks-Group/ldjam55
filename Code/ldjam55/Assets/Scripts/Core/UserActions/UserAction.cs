namespace Assets.Scripts.Core.Model
{
    public class UserAction
    {
        public string Name { get; set; }
        public string IconName { get; set; }
        public int UsesRemaining { get; set; }
        public float Cooldown { get; set; } = 0f;
        public string ActionParamers { get; set; }
    }
}