using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Emux.GameBoy.Cartridge;
using Emux.GameBoy.Cheating;
using Emux.GameBoy.Graphics;
using Emux.NAudio;
using Emux.OpenTK.Emux;
using NAudio.Wave;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Testing;
using osu.Framework.Timing;
using OpenTK.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using IClock = Emux.GameBoy.Cpu.IClock;

namespace Emux.OpenTK.Visual
{
    [TestFixture]
    public class VisualTestEmuxPlayGame : TestCase
    {
        public GameBoyContainer GameBoyContainer;
        public VisualTestEmuxPlayGame()
        {
            Add(GameBoyContainer = new GameBoyContainer());

            InitialTestStep();
        }

        void InitialTestStep()
        {
            //load 
            AddStep("Load game", () =>
            {
                string romPath = "Resources/ROM/Tetris.gb";
                GameBoyContainer.LoadDevice(romPath, Path.ChangeExtension(romPath, ".sav"));
            });

            //run game
            AddStep("Run", () =>
            {
                if(GameBoyContainer.CurrentDevice != null && !GameBoyContainer.CurrentDevice.Cpu.Running)
                {
                    GameBoyContainer.CurrentDevice.Cpu.Run();
                }
            });

            //reset and reload game
            AddStep("Reset", () =>
            {
                GameBoyContainer.CurrentDevice.Reset();
            });

            //stop game
            AddStep("Step", () =>
            {
                GameBoyContainer.CurrentDevice.Cpu.Step();
            });

            //break game
            AddStep("Break", () =>
            {
                GameBoyContainer.CurrentDevice.Cpu.Break();
            });

            //set breakPoint
            AddStep("Set BreakPoint", () =>
            {
                GameBoyContainer.CurrentDevice.Cpu.Step();
            });

            //clean all breakpoint
            AddStep("Reset BreakPoint", () =>
            {
                GameBoyContainer.CurrentDevice.Cpu.ClearBreakpoints();
            });
        }
    }

    public class GameBoyContainer : Container
    {
        public event EventHandler<DeviceEventArgs> DeviceLoaded;
        public event EventHandler<DeviceEventArgs> DeviceUnloaded;
        public event EventHandler DeviceChanged;

        private GameBoy.GameBoy _currentDevice;
        private StreamedExternalMemory _currentExternalMemory;

        private readonly EmuxGameContainer _displayScreen;

        public GameBoyContainer()
        {
            //Initial Ui
            AddInternal(_displayScreen = new EmuxGameContainer
            {
                Width = 160 * 4,
                Height = 144 * 4
            });

            AudioMixer = new GameBoyNAudioMixer();
            var player = new DirectSoundOut();
            player.Init(AudioMixer);
            player.Play();

            GamesharkController = new GamesharkController();
            Breakpoints = new Dictionary<ushort, BreakpointInfo>();
        }

        public GameBoyNAudioMixer AudioMixer
        {
            get;
        }

        public GameBoy.GameBoy CurrentDevice
        {
            get { return _currentDevice; }
            private set
            {
                if (_currentDevice != value)
                {
                    _currentDevice = value;
                    if (value != null)
                    {
                        AudioMixer.Connect(value.Spu);
                        GamesharkController.Device = value;

                        //Connect output
                        _currentDevice.Gpu.VideoOutput = _displayScreen;
                    }
                    OnDeviceChanged();
                }
            }
        }

        public GamesharkController GamesharkController
        {
            get;
        }

        public IDictionary<ushort, BreakpointInfo> Breakpoints
        {
            get;
        }

        public void UnloadDevice()
        {
            var device = _currentDevice;
            if (device != null)
            {
                device.Terminate();
                _currentExternalMemory.Dispose();
                _currentDevice = null;
                OnDeviceUnloaded(new DeviceEventArgs(device));
            }
        }

        public void LoadDevice(string romFilePath, string ramFilePath)
        {
            UnloadDevice();
            _currentExternalMemory = new StreamedExternalMemory(File.Open(ramFilePath, FileMode.OpenOrCreate));
            var cartridge = new EmulatedCartridge(File.ReadAllBytes(romFilePath), _currentExternalMemory);
            _currentExternalMemory.SetBufferSize(cartridge.ExternalRamSize);
            CurrentDevice = new GameBoy.GameBoy(cartridge, RecreateTicker(), !Settings.ForceOriginalGameBoy);
            ApplyColorPalettes();
            OnDeviceLoaded(new DeviceEventArgs(CurrentDevice));
        }

        GameBoyTicker RecreateTicker()
        {
            RemoveAll(x => x is GameBoyTicker);
            var ticker = new GameBoyTicker();
            Add(ticker);
            return ticker;
        }

        protected override void Dispose(bool isDisposing)
        {
            //unload device
            UnloadDevice();
            base.Dispose(isDisposing);
        }

        protected virtual void OnDeviceChanged()
        {
            DeviceChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnDeviceLoaded(DeviceEventArgs e)
        {
            DeviceLoaded?.Invoke(this, e);
        }

        protected virtual void OnDeviceUnloaded(DeviceEventArgs e)
        {
            DeviceUnloaded?.Invoke(this, e);
        }

        private static Color ConvertColor(Color color)
        {
            return color;
        }

        private void ApplyColorPalettes()
        {
            if (CurrentDevice != null)
            {
                CurrentDevice.Gpu.Color0 = ConvertColor(Settings.GBColor0);
                CurrentDevice.Gpu.Color1 = ConvertColor(Settings.GBColor1);
                CurrentDevice.Gpu.Color2 = ConvertColor(Settings.GBColor2);
                CurrentDevice.Gpu.Color3 = ConvertColor(Settings.GBColor3);
            }
        }
    }

    public class GameBoyTicker : Container , IClock
    {
        public event EventHandler Tick;

        private bool _tick;
        private double _lastTickleTime;

        public void Start()
        {
            _tick = true;
        }

        public void Stop()
        {
            _tick = false;
        }

        protected override void Update()
        {
            
            if (_tick)
            {
                var time = this.Time.Current;

                //Limit to 60HZ par second
                if ((time - _lastTickleTime) > (1.0f / 60.0f) * 1000)
                {
                    Tick?.Invoke(this, EventArgs.Empty);
                    _lastTickleTime = time;
                }
            }
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
            var rawData = new byte[160 * 144 * sizeof(int)];

            for (int i = 0, j = 0; j < pixelData.Length; i += 4, j += 3)
            {
                rawData[i] = pixelData[j];
                rawData[i + 1] = pixelData[j + 1];
                rawData[i + 2] = pixelData[j + 2];
                rawData[i + 3] = 255;
            }

            var image = Image.LoadPixelData<Rgba32>(rawData, 160, 144);
            Texture.SetData(new TextureUpload(image));
        }
    }
}
