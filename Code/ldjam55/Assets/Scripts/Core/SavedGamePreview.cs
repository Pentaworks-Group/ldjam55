using System;

namespace Assets.Scripts.Core
{
    public class SavedGamePreview : GameFrame.Core.Persistence.SavedGamePreview<GameState>
    {
        public String CreatedOn { get; set; }
        public String SavedOn { get; set; }
        public String GameMode { get; set; }
        public String TimeElapsed { get; set; }

        public override void Init(GameState gameState, String key)
        {
            base.Init(gameState, key);

            CreatedOn = String.Format("{0:G}", gameState.CreatedOn);
            SavedOn = String.Format("{0:G}", gameState.SavedOn);
            GameMode = gameState.Mode.Name;
            TimeElapsed = String.Format("{0:#0.0}s", gameState.TimeElapsed);
        }
    }
}
