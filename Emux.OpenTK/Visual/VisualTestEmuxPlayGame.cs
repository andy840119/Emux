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
using osu.Framework.Graphics;
using OpenTK;

namespace Emux.OpenTK.Visual
{
    [TestFixture]
    public class VisualTestEmuxPlayGame : TestCase
    {
        public GameBoyContainer GameBoyContainer;
        public VisualTestEmuxPlayGame()
        {
            Add(GameBoyContainer = new GameBoyContainer
            {
                Scale = new Vector2(1.0f)
            });

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
        private readonly GameBoyScreen _displayScreen;

        private readonly Circle _powerLed;

        #region UI

        protected virtual Color4 BackgroundColor => new Color4(181,181,178,255);

        protected virtual Color4 BackgroundLineColor => new Color4(158,159,155,255);

        protected virtual Color4 DisplayBackgroundColor => new Color4(82,82,94,255);

        protected virtual Color4 DisplayScreenColor => new Color4(81,98,23,255);

        protected virtual Color4 DisplayTextColor => Color4.White;

        protected virtual Color4 Banner1Color => new Color4(110,19,79,255);

        protected virtual Color4 Banner2Color => new Color4(5,2,80,255);

        protected virtual Color4 LedOffColor => new Color4(38,17,22,255);

        protected virtual Color4 LedOnColor => new Color4(204,68,79,255);

        protected virtual Color4 TextColor => new Color4(13,24,124,255);

        #endregion

        public GameBoyContainer()
        {
            Name = "Gameboy";
            //Initial Ui
            AddInternal(new Container
            {
                Name="Gameboy body",
                Width = 275,
                Height = 450,
                Masking = true,
                CornerRadius = 10,
                Children = new Drawable[]
                {
                    new Container
                    {
                        Name = "Background area",
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Name = "Background",
                                Colour = BackgroundColor,
                                RelativeSizeAxes = Axes.Both
                            },
                            new Box
                            {
                                Name = "Center line",
                                Colour = BackgroundLineColor,
                                RelativeSizeAxes = Axes.X,
                                Height = 3,
                                Y = 20
                            },
                            new Box
                            {
                                Name = "Left line",
                                Colour = BackgroundLineColor,
                                Width = 3,
                                Height = 20,
                                X = 20,
                            },
                            new Box
                            {
                                Name = "Right line",
                                Colour = BackgroundLineColor,
                                Width = 3,
                                Height = 20,
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                                X = -20,
                            },
                            new FillFlowContainer
                            {
                                Name = "Logo",
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                                X = 16,
                                Y = 210,
                                Direction = FillDirection.Horizontal,
                                Children = new Drawable[]
                                {
                                    new SpriteText
                                    {
                                        Colour = TextColor,
                                        TextSize = 15,
                                        Text = "Nintendo",
                                        Scale = new Vector2(1.3f,1)
                                    },
                                    new SpriteText
                                    {
                                        Colour = TextColor,
                                        TextSize = 25,
                                        Text = "GAME BOY",
                                        X = 40,
                                        Y = 5,
                                    },
                                    new SpriteText
                                    {
                                        Colour = TextColor,
                                        TextSize = 10,
                                        Text = "TM",
                                        X = 120,
                                        Y = 5,
                                    },
                                }
                            },
                            new GameboySpeaker(BackgroundLineColor,5)
                            {
                                Name = "Speaker",
                                Width = 100,
                                Height = 40,
                                Spacing = new Vector2(10),
                                Anchor = Anchor.BottomRight,
                                Origin = Anchor.BottomRight,
                                X = 25,
                                Y = -60,
                                Rotation = -30
                            },
                            new Box
                            {
                                Name = "Phones",
                                Colour = BackgroundLineColor,
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                                Width = 30,
                                Height = 5,
                                Y = -5,
                            }
                        }
                    },
                    new Container
                    {
                        Name = "Screen area",
                        Width = 240,
                        Height = 175,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Masking = true,
                        Y = 35,
                        CornerRadius = 5,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Name = "Background",
                                Colour = DisplayBackgroundColor,
                                RelativeSizeAxes = Axes.Both,
                            },
                            new Container
                            {
                                Name = "Banner",
                                RelativeSizeAxes = Axes.X,
                                Y = 6,
                                Padding = new MarginPadding{Left = 10,Right = 10},
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Height = 2f,
                                        Colour = Banner1Color,
                                        Y = 1
                                    },
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Height = 2f,
                                        Colour = Banner2Color,
                                        Y = 6
                                    },
                                    new Container
                                    {
                                        Name = "Text",
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        X = 20,
                                        Width = 113,
                                        Height = 10,
                                        Children = new Drawable[]
                                        {
                                            new Box
                                            {
                                                Colour = DisplayBackgroundColor,
                                                RelativeSizeAxes = Axes.Both,
                                                X = -3
                                            },
                                            new SpriteText
                                            {
                                                TextSize = 9,
                                                Text = "DOT MATRIX WITH STEREO SOUND",
                                                Colour = DisplayTextColor,
                                            }
                                        }
                                    }
                                }
                            },
                             _powerLed = new Circle
                            {
                                Name = "Power led",
                                Width = 8,
                                Height = 8,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                X = 15,
                                Y = -12
                            },
                            new SpriteText
                            {
                                Name = "Battery text",
                                Text = "BATTERY",
                                TextSize = 9,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Colour = DisplayTextColor,
                                X = 10,
                            },
                            new Container
                            {
                                Name = "Screen",
                                Anchor = Anchor.Centre,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        Name = "Screen Background",
                                        Colour = DisplayScreenColor,
                                        Origin = Anchor.Centre,
                                        Width = 160 * 0.9f,
                                        Height = 144 * 0.9f,
                                    },
                                    _displayScreen = new GameBoyScreen
                                    {
                                        Name = "Screen Sprite",
                                        Origin = Anchor.Centre,
                                        Width = 160 * 0.9f,
                                        Height = 144 * 0.9f,
                                    }
                                }
                            },
                        }
                    },
                }
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
            _powerLed.Colour = LedOffColor;
            DeviceChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnDeviceLoaded(DeviceEventArgs e)
        {
            _powerLed.Colour = LedOnColor;
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

    public class GameboySpeaker : FillFlowContainer
    {
        public GameboySpeaker(Color4 speakerColor,float speakerWidth)
        {
            for(int i=0;i<5;i++)
            {
                this.Add(new Box
                {
                    Colour = speakerColor,
                    Width = speakerWidth,
                    RelativeSizeAxes = Axes.Y,
                });
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

    public class GameBoyScreen : Sprite, IVideoOutput
    {
        public GameBoyScreen()
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

            var image = SixLabors.ImageSharp.Image.LoadPixelData<Rgba32>(rawData, 160, 144);
            Texture.SetData(new TextureUpload(image));
        }
    }
}
