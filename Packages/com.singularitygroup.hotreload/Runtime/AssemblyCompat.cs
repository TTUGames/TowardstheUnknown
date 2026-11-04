using System;
using System.Collections.Generic;
using System.Reflection;

namespace SingularityGroup.HotReload {
    /// <summary>
    /// Assembly APIs whose plain .NET versions misbehave on Unity 7 (CoreCLR), where all user code lives in one
    /// collectible AssemblyLoadContext that Unity replaces on every code reload.
    /// On Mono these are exactly the plain .NET calls.
    /// </summary>
    static class AssemblyCompat {
        /// Loads an assembly next to the user code, so it binds to the user's types.
        /// On CoreCLR, Assembly.Load(byte[]) loads into the contextual reflection load context: Unity's user
        /// context on the editor's main thread, but an isolated context of its own in a player, or on a thread
        /// that doesn't carry Unity's contextual reflection scope.
        public static Assembly LoadFromBytes(byte[] rawAssembly, byte[] rawSymbolStore) {
#if ENABLE_CORECLR
            return UnityEngine.Assemblies.CurrentAssemblies.LoadFromBytes(rawAssembly, rawSymbolStore);
#else
            return Assembly.Load(rawAssembly, rawSymbolStore);
#endif
        }

        /// Assemblies of the current code generation. On CoreCLR, AppDomain.GetAssemblies() also
        /// returns assemblies of load contexts that are still unloading after a code reload.
        public static IReadOnlyList<Assembly> GetLoadedAssemblies() {
#if ENABLE_CORECLR
            return UnityEngine.Assemblies.CurrentAssemblies.GetLoadedAssemblies();
#else
            return AppDomain.CurrentDomain.GetAssemblies();
#endif
        }

        /// Path of the assembly file. On CoreCLR, Unity loads some assemblies from streams, so Assembly.Location
        /// can be empty; Unity knows their path, but can only tell us on the main thread.
        public static string GetLocation(Assembly assembly) {
#if ENABLE_CORECLR
            var location = assembly.Location;
            if (!string.IsNullOrEmpty(location) || global::System.Threading.Thread.CurrentThread.ManagedThreadId != ThreadUtility.mainThreadId) {
                return location;
            }
            return UnityEngine.AssemblyExtension.GetLoadedAssemblyPath(assembly) ?? "";
#else
            return assembly.Location;
#endif
        }
    }
}
