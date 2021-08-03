using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Tetris;
using static Tetris.TetrisGame;

namespace MeadowTetris
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Max7219 display;
        GraphicsLibrary graphics;
        //IDigitalInputPort portLeft;
        PushButton buttonLeft;
        //IDigitalInputPort portUp;
        PushButton buttonUp;
        //IDigitalInputPort portRight;
        PushButton buttonRight;
        //IDigitalInputPort portDown;
        PushButton buttonDown;
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
                AssignButtonHandlers();
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
            buttonLeft = new PushButton(Device.CreateDigitalInputPort(Device.Pins.D12, interruptMode: InterruptMode.EdgeFalling, resistorMode: ResistorMode.ExternalPullDown)); // , InterruptMode.EdgeBoth)); <- Caused it to hang without explicit external resistor mode set
            buttonUp = new PushButton(Device.CreateDigitalInputPort(Device.Pins.D13, interruptMode: InterruptMode.EdgeFalling, resistorMode: ResistorMode.ExternalPullDown));
            buttonRight = new PushButton(Device.CreateDigitalInputPort(Device.Pins.D07, interruptMode: InterruptMode.EdgeFalling, resistorMode: ResistorMode.ExternalPullDown));
            buttonDown = new PushButton(Device.CreateDigitalInputPort(Device.Pins.D11, interruptMode: InterruptMode.EdgeFalling, resistorMode: ResistorMode.ExternalPullDown));
            Console.WriteLine("Done initializing...");
        }

        private void AssignButtonHandlers()
        {
            Console.WriteLine($"Assinging button handlers");
            buttonLeft.Clicked += ButtonLeft_Clicked;
            buttonRight.Clicked += ButtonRight_Clicked;
            buttonUp.Clicked += ButtonUp_Clicked;
            buttonDown.Clicked += ButtonDown_Clicked;
        }

        private void ButtonLeft_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine($"Left");
            game.OnLeft();
        }
        private void ButtonRight_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine($"Right");
            game.OnRight();
        }
        private void ButtonUp_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine($"Up");
            game.OnRotate();
        }
        private void ButtonDown_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine($"Down");
            game.OnDrop();
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
                CheckForPieceDrop(tick);

                graphics.Clear();
                DrawTetrisField();
                graphics.Show();

                Thread.Sleep(50);
            }
        }

        void CheckForPieceDrop(int tick)
        {
            if (tick % (21 - game.Level) == 0)
            {
                game.OnDown(true);
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