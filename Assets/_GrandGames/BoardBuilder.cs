using System;
using _GrandGames.Levels.Domain;
using Cysharp.Threading.Tasks;

namespace _GrandGames
{
    [Serializable]
    public class BoardBuilder
    {
        public async UniTask<char[,]> BuildAsync(LevelData levelData)
        {
            var size = levelData.gridSize;
            var board = new char[size, size];

            for (var r = 0; r < size; r++)
            {
                for (var c = 0; c < size; c++)
                {
                    board[r, c] = levelData.board[r][c][0];
                }
            }

            await UniTask.Yield(); // Simulate async work
            return board;
        }
    }
}