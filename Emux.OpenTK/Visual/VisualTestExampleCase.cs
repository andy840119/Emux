using NUnit.Framework;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using OpenTK.Graphics;

namespace Emux.OpenTK.Visual
{
    [TestFixture]
    public class VisualTestExampleCase : TestCase
    {
        public VisualTestExampleCase()
        {
            //open panel
            AddStep("Test case Step", ()=> Add(new Box
            {
                Width = 300,
                Height = 300,
                Colour = Color4.White
            }));
        }
    }
}
