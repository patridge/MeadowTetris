using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Hid;
using Meadow.Peripherals.Sensors.Hid;
using Tetris;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Max7219 display;
        GraphicsLibrary graphics;
        AnalogJoystick joystick;
        TetrisGame game = new TetrisGame(8, 24);

        public MeadowApp()
        {
            Console.WriteLine("Tetris");
            Init();

            Console.WriteLine("Start game");
            StartGameLoop();
        }

        int tick = 0;
        void StartGameLoop()
        {
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

        async Task CheckInput(int tick)
        {
            if (tick % (21 - game.Level) == 0)
            {
                game.OnDown(true);
            }

            //var pos = await joystick.Position.GetPosition(); // Old...doesn't work anymore.
            //var pos = await joystick.Read(); // Returns a JoystickPosition, which doesn't work with DigitalJoystickPosition.
            var pos = joystick.DigitalPosition;
            //if (pos == AnalogJoystick.DigitalJoystickPosition.Left)
            if (pos == DigitalJoystickPosition.Left)
            {
                game.OnLeft();
            }
            if (pos == DigitalJoystickPosition.Right)
            {
                game.OnRight();
            }
            if (pos == DigitalJoystickPosition.Up)
            {
                game.OnDown();
            }
            if (pos == DigitalJoystickPosition.Down)
            {
                game.OnRotate();
                await Task.Delay(500);
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

        void Init()
        {
            Console.WriteLine("Init");
            display = new Max7219(
                device: Device,
                spiBus: Device.CreateSpiBus(),
                csPin: Device.Pins.D01,
                deviceCount: 4,
                maxMode: Max7219.Max7219Type.Display);

            graphics = new GraphicsLibrary(display);
            graphics.CurrentFont = new Font4x8();
            graphics.Rotation = GraphicsLibrary.RotationType._180Degrees;
            joystick = new AnalogJoystick(Device, Device.Pins.A00, Device.Pins.A01, null, true);
        }
    }
}