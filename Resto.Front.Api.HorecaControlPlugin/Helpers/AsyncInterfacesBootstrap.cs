using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Resto.Front.Api.HorecaControlPlugin.Helpers
{
    /// <summary>
    /// linq2db 5.x ссылается на Microsoft.Bcl.AsyncInterfaces 7.0.0.0, пакеты DI 10.x — на 10.0.0.9.
    /// Без unify на Host 8.4 / V8Preview5: TypeLoadException (DisposeAsync у DataConnection).
    /// bindingRedirect в dll.config на Host 9.x вешает AppDomain — поэтому unify через AssemblyResolve.
    /// </summary>
    internal static class AsyncInterfacesBootstrap
    {
        private static readonly string[] RedirectAssemblyNames =
        {
            "Microsoft.Bcl.AsyncInterfaces",
            "System.Threading.Tasks.Extensions",
        };

        private static int _attached;

        [ModuleInitializer]
        internal static void Initialize()
        {
            Attach();
        }

        internal static void Attach()
        {
            if (System.Threading.Interlocked.Exchange(ref _attached, 1) != 0)
                return;

            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

            // Прогрев до загрузки linq2db / DataConnection.
            foreach (var name in RedirectAssemblyNames)
                TryLoad(name);
        }

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            AssemblyName requested;
            try
            {
                requested = new AssemblyName(args.Name);
            }
            catch
            {
                return null;
            }

            if (!RedirectAssemblyNames.Any(n =>
                    string.Equals(n, requested.Name, StringComparison.OrdinalIgnoreCase)))
                return null;

            return TryLoad(requested.Name);
        }

        private static Assembly TryLoad(string assemblyName)
        {
            foreach (var loaded in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (string.Equals(loaded.GetName().Name, assemblyName, StringComparison.OrdinalIgnoreCase))
                    return loaded;
            }

            var baseDir = Path.GetDirectoryName(typeof(AsyncInterfacesBootstrap).Assembly.Location);
            if (string.IsNullOrEmpty(baseDir))
                return null;

            var path = Path.Combine(baseDir, assemblyName + ".dll");
            if (!File.Exists(path))
                return null;

            try
            {
                return Assembly.LoadFrom(path);
            }
            catch
            {
                return null;
            }
        }
    }
}

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    internal sealed class ModuleInitializerAttribute : Attribute
    {
    }
}
