using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Emux.GameBoy.Graphics;
using Emux.OpenTK.Emux;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using OpenTK.Graphics;

namespace Emux.OpenTK.Visual
{
    [TestFixture]
    public class VisualTestEmuxPlayGame : TestCase
    {
        public readonly DeviceManager DeviceManager = new DeviceManager();
        private GameBoy.GameBoy _currentDevice;

        public VisualTestEmuxPlayGame()
        {
            OnDeviceChanged();
            InitialTestStep();
        }

        void OnDeviceChanged()
        {
            _currentDevice = DeviceManager.CurrentDevice;
        }

        void InitialTestStep()
        {
            //load 
            AddStep("Load game", () =>
            {
                string romPath = "";
                DeviceManager.LoadDevice(romPath, Path.ChangeExtension(romPath, ".sav"));
            });

            //run game
            AddStep("Run", () =>
            {
                if(_currentDevice != null && _currentDevice.Cpu.Running)
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
    }

    public class EmuxGameContainer : Container, IVideoOutput
    {
        public void RenderFrame(byte[] pixelData)
        {
            ///TODO : generate texture and renderer to container
        }
    }
}
