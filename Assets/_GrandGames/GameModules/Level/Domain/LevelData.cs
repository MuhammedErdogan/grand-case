namespace _GrandGames.GameModules.Level.Domain
{
    [System.Serializable]
    public class LevelData
    {
        public int Level;
        public string LevelId;
        public Difficulty Difficulty;
        public int GridSize;
        public string[][] Board;
    }

    public enum Difficulty : byte
    {
        Easy,
        Medium,
        Hard
    }
}