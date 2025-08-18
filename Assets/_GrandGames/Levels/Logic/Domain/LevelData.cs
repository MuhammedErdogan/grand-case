namespace _GrandGames.Levels.Logic.Domain
{
    [System.Serializable]
    public class LevelData
    {
        public int level;
        public string levelId;
        public string difficulty;
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