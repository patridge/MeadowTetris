using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;
using Tetris;
using static Tetris.TetrisGame;

namespace MeadowTetris
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Max7219 display;
        GraphicsLibrary graphics;
        IDigitalInputPort portLeft;
        IDigitalInputPort portUp;
        IDigitalInputPort portRight;
        IDigitalInputPort portDown;
        TetrisGame game = new TetrisGame(8, 24);
        GameData gameData;

        public MeadowApp()
        {
            Console.WriteLine("Tetris");

            _ = Task.Run(async () =>
            {
                await Initialize();
            }).ContinueWith(async (_) =>
            {
                await DisplayPreGame();
                StartGameLoop();
            });
        }

        async Task Initialize()
        {
            game.GameLost += Game_GameLost;
            Console.WriteLine("Initializing files.");
            gameData = await GameData.LoadGameData();
            Console.WriteLine("Done initializing files...");

            Console.WriteLine("Initializing hardware...");
            display = new Max7219(
                device: Device,
                spiBus: Device.CreateSpiBus(),
                csPin: Device.Pins.D01,
                deviceCount: 4,
                maxMode: Max7219.Max7219Type.Display);

            graphics = new GraphicsLibrary(display);
            graphics.CurrentFont = new Font4x8();
            graphics.Rotation = GraphicsLibrary.RotationType._180Degrees;
            portLeft = Device.CreateDigitalInputPort(Device.Pins.D12);
            portUp = Device.CreateDigitalInputPort(Device.Pins.D13);
            portRight = Device.CreateDigitalInputPort(Device.Pins.D07);
            portDown = Device.CreateDigitalInputPort(Device.Pins.D11);
            Console.WriteLine("Done initializing...");
        }

        private async void Game_GameLost(object sender, GameLostEventArgs e)
        {
            if (gameData.TrySetHighScore(e.Score))
            {
                Console.WriteLine($"New high score: {gameData.HighScore}");
                await GameData.WriteGameData(gameData);
            }
        }

        int tick = 0;
        void StartGameLoop()
        {
            Console.WriteLine("Start game...");
            while (true)
            {
                tick++;
                CheckInput(tick);

                graphics.Clear();
                DrawTetrisField();
                graphics.Show();

                Thread.Sleep(50);
            }
        }

        void CheckInput(int tick)
        {
            if (tick % (21 - game.Level) == 0)
            {
                game.OnDown(true);
            }

            if (portLeft.State == true)
            {
                game.OnLeft();
            }
            else if (portRight.State == true)
            {
                game.OnRight();
            }
            else if (portUp.State == true)
            {
                game.OnRotate();
            }
            else if (portDown.State == true)
            {
                game.OnDown();
            }
        }

        void DrawTetrisField()
        {
            graphics.DrawText(0, 0, $"{game.LinesCleared}");
            int yOffset = 8;
            //draw current piece
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (game.IsPieceLocationSet(i, j))
                    {
                        graphics.DrawPixel((game.CurrentPiece.X + i),
                        game.CurrentPiece.Y + j + yOffset);
                    }
                }
            }

            //draw gamefield
            for (int i = 0; i < game.Width; i++)
            {
                for (int j = 0; j < game.Height; j++)
                {
                    if (game.IsGameFieldSet(i, j))
                    {
                        graphics.DrawPixel(i, j + yOffset);
                    }
                }
            }
        }

        async Task DisplayPreGame()
        {
            Console.WriteLine("Show high score");
            graphics.Clear();
            graphics.DrawText(0, 0, "HS");
            graphics.DrawText(0, 8, gameData.HighScore.ToString());
            graphics.Show();
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}