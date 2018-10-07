using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Emux.GameBoy.Graphics;
using Emux.OpenTK.Emux;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Testing;
using OpenTK.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Emux.OpenTK.Visual
{
    [TestFixture]
    public class VisualTestEmuxPlayGame : TestCase
    {
        public readonly DeviceManager DeviceManager = new DeviceManager();
        private GameBoy.GameBoy _currentDevice;
        private EmuxGameContainer _displayScreen;

        public VisualTestEmuxPlayGame()
        {
            Add(_displayScreen = new EmuxGameContainer
            {
                Width = 160 * 4,
                Height = 144 * 4
            });
            InitialTestStep();
        }

        void OnDeviceChanged()
        {
            _currentDevice = DeviceManager.CurrentDevice;
            _currentDevice.Gpu.VideoOutput = _displayScreen;
        }

        void InitialTestStep()
        {
            //load 
            AddStep("Load game", () =>
            {
                string romPath = "Resources/ROM/Tetris.gb";
                DeviceManager.LoadDevice(romPath, Path.ChangeExtension(romPath, ".sav"));
                OnDeviceChanged();
            });

            //run game
            AddStep("Run", () =>
            {
                if(_currentDevice != null && !_currentDevice.Cpu.Running)
                {
                    _currentDevice.Cpu.Run();
                }
            });

            //reset and reload game
            AddStep("Reset", () =>
            {
                _currentDevice.Reset();
            });

            //stop game
            AddStep("Step", () =>
            {
                _currentDevice.Cpu.Step();
            });

            //break game
            AddStep("Break", () =>
            {
                _currentDevice.Cpu.Break();
            });

            //set breakPoint
            AddStep("Set BreakPoint", () =>
            {
                _currentDevice.Cpu.Step();
            });

            //clean all breakpoint
            AddStep("Reset BreakPoint", () =>
            {
                _currentDevice.Cpu.ClearBreakpoints();
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            //unload device
            DeviceManager.UnloadDevice();
            base.Dispose(isDisposing);
        }
    }

    public class EmuxGameContainer : Sprite, IVideoOutput
    {
        public EmuxGameContainer()
        {
            Texture = new Texture(160, 144);
        }

        public void RenderFrame(byte[] pixelData)
        {
            var rgbaByte = new List<byte>();
            for (int i = 0; i < pixelData.Length; i += 3)
            {
                byte R = pixelData[i];
                byte G = pixelData[i + 1];
                byte B = pixelData[i + 2];
                byte A = 255;

                rgbaByte.Add(R);
                rgbaByte.Add(G);
                rgbaByte.Add(B);
                rgbaByte.Add(A);
            }
            var image = Image.LoadPixelData<Rgba32>(rgbaByte.ToArray(), 160, 144);
            
            Texture.SetData(new TextureUpload(image));
        }
    }
}
