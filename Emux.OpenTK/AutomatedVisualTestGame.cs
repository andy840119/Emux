using System;
using System.Collections.Generic;
using System.Text;
using osu.Framework.Testing;

namespace Emux.OpenTK
{
    internal class AutomatedVisualTestGame : TestGame
    {
        public AutomatedVisualTestGame()
        {
            Add(new TestBrowserTestRunner(new TestBrowser()));
        }
    }
}
