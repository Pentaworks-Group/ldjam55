namespace Assets.Scripts.Core.Model
{
    public class GameEndCondition
    {
        public string Name;
        public bool IsWin;
        public int WinCount = 0;
        public int CurrentCount = 0;
        public bool Done = false;
    }
}