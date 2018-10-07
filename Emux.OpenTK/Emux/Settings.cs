using System;
using System.Collections.Generic;
using System.Text;
using Emux.GameBoy.Graphics;

namespace Emux.OpenTK.Emux
{
    public class Settings
    {
        public static Color GBColor0 { get; set; } =new Color(224, 248, 208);

        public static Color GBColor1 { get; set; } = new Color( 136, 192, 112);

        public static Color GBColor2 { get; set; } = new Color( 52, 104,86);

        public static Color GBColor3 { get; set; } = new Color( 8, 24, 32);

        public static bool ForceOriginalGameBoy { get; set; } = false;
    }
}
