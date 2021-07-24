using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Meadow;

namespace MeadowTetris
{
    class GameData
    {
        public int HighScore { get; set; } = 0;
        public bool TrySetHighScore(int newScore)
        {
            if (newScore > HighScore)
            {
                HighScore = newScore;
                return true;
            }
            return false;
        }

        static string settingsFile = Path.Combine(MeadowOS.FileSystem.DataDirectory, "settings.txt");

        public static async Task WriteGameData(GameData gameData)
        {
            try
            {
                await File.WriteAllLinesAsync(settingsFile, gameData.Serialize()).ConfigureAwait(false);
                Console.WriteLine($"Saved game data.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing game data: {ex.Message}");
            }
        }
        static async Task WriteInitialGameData()
        {
            await WriteGameData(new GameData()).ConfigureAwait(false);
            Console.WriteLine("Created new saved game data.");
        }

        public static async Task<GameData> LoadGameData()
        {
            if (!File.Exists(settingsFile))
            {
                Console.WriteLine("No saved game data found.");
                await WriteInitialGameData();
            }

            GameData gameData = null;
            try
            {
                var gameDataLines = (await File.ReadAllLinesAsync(settingsFile).ConfigureAwait(false));
                gameData = GameData.Deserialize(gameDataLines.ToList());
                Console.WriteLine($"Found game data:\n{string.Join("\n\t", gameData.Serialize())}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading game data: {ex.Message}");
            }
            return gameData ?? new GameData();
        }


        List<string> Serialize()
        {
            return new List<string>() {
                $"{nameof(HighScore)}:{HighScore}",
            };
        }
        static GameData Deserialize(List<string> textGameData)
        {
            if (textGameData == null || !textGameData.Any())
            {
                throw new ArgumentException("Game data null or empty.", nameof(textGameData));
            }

            var gameData = new GameData();
            var highScoreLine = textGameData[0];
            var highScoreLabel = $"{nameof(HighScore)}:";
            int scoreIndex = highScoreLine.IndexOf(highScoreLabel) + highScoreLabel.Length;
            int scoreLength = highScoreLine.Length - highScoreLabel.Length;
            int.TryParse(highScoreLine.Substring(scoreIndex, scoreLength), out var highScore);
            gameData.HighScore = highScore;
            return gameData;
        }
    }
}
