using System.Text;

namespace _GrandGames.GameModules.Board
{
    public static class BoardFormatter
    {
        public static string ToText(char[,] board)
        {
            var rows = board.GetLength(0);
            var cols = board.GetLength(1);
            var sb = new StringBuilder(rows * (cols + 1));

            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    sb.Append(board[r, c]);
                }

                if (r < rows - 1)
                {
                    sb.Append('\n');
                }
            }

            return sb.ToString();
        }
    }
}