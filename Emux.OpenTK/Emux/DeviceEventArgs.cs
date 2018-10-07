using System;
using System.Collections.Generic;
using System.Text;

namespace Emux.OpenTK.Emux
{
    public class DeviceEventArgs : EventArgs
    {
        public DeviceEventArgs(GameBoy.GameBoy device)
        {
            Device = device;
        }

        public GameBoy.GameBoy Device
        {
            get;
        }
    }
}
