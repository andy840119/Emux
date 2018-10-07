using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics;

namespace Emux.OpenTK.Emux
{
    public class Settings
    {
        public static Color4 GBColor0 { get; set; } =new Color4(255, 224, 248,208);

        public static Color4 GBColor1 { get; set; } = new Color4(255, 136, 192, 112);

        public static Color4 GBColor2 { get; set; } = new Color4(255, 52, 104,86);

        public static Color4 GBColor3 { get; set; } = new Color4(255, 8, 24, 32);

        public static bool ForceOriginalGameBoy { get; set; } = false;
    }
}
