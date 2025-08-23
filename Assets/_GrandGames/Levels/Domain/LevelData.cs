namespace _GrandGames.Levels.Domain
{
    [System.Serializable]
    public class LevelData
    {
        public int level;
        public string levelId;
        public Difficulty difficulty;
        public int gridSize;
        public string[][] board;
    }

    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }
}