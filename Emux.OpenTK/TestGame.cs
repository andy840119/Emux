using System.Reflection;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.IO.Stores;

namespace Emux.OpenTK
{
    internal class TestGame : Game
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Resources.AddStore(new NamespacedResourceStore<byte[]>(new DllResourceStore(Assembly.GetExecutingAssembly().Location), "Resources"));
        }
    }
}
