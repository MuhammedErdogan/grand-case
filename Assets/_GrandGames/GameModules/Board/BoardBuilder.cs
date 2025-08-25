using System;
using _GrandGames.GameModules.Level.Domain;
using Cysharp.Threading.Tasks;

namespace _GrandGames.GameModules.Board
{
    [Serializable]
    public class BoardBuilder
    {
        public async UniTask<char[,]> BuildAsync(LevelData levelData)
        {
            var size = levelData.GridSize;
            var board = new char[size, size];

            for (var r = 0; r < size; r++)
            {
                for (var c = 0; c < size; c++)
                {
                    board[r, c] = levelData.Board[r][c][0];
                }
            }

            await UniTask.Yield(); // Simulate async work
            return board;
        }
    }
}