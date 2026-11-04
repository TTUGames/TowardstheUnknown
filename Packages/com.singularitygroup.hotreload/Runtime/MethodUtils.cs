using System;
using System.Reflection;

namespace SingularityGroup.HotReload {
    static class MethodUtils {
#if ENABLE_MONO
        public static unsafe void DisableVisibilityChecks(MethodBase method) {
            if(IntPtr.Size == sizeof(long)) {
                var ptr = (Interop.MonoMethod64*)method.MethodHandle.Value.ToPointer();
                ptr->monoMethodFlags |= Interop.MonoMethodFlags.skip_visibility;
            } else {
                var ptr = (Interop.MonoMethod32*)method.MethodHandle.Value.ToPointer();
                ptr->monoMethodFlags |= Interop.MonoMethodFlags.skip_visibility;
            }
        }

        public static unsafe bool IsMethodInlined(MethodBase method) {
            if(IntPtr.Size == sizeof(long)) {
                var ptr = (Interop.MonoMethod64*)method.MethodHandle.Value.ToPointer();
                return (ptr -> monoMethodFlags & Interop.MonoMethodFlags.inline_info) == Interop.MonoMethodFlags.inline_info;
            } else {
                var ptr = (Interop.MonoMethod32*)method.MethodHandle.Value.ToPointer();
                return (ptr -> monoMethodFlags & Interop.MonoMethodFlags.inline_info) == Interop.MonoMethodFlags.inline_info;
            }
        }
#elif ENABLE_CORECLR
        // CoreCLR checks access per patch assembly instead, see IgnoresAccessChecksTo on the server
        public static void DisableVisibilityChecks(MethodBase method) { }

        public static bool IsMethodInlined(MethodBase method) {
#if UNITY_EDITOR
            return CoreClrInlining.IsMethodInlined(method);
#else
            // players run user code compiled for debugging, which the JIT doesn't inline
            return false;
#endif
        }
#else
        public static void DisableVisibilityChecks(MethodBase method) { }
        public static bool IsMethodInlined(MethodBase method) {
             return false; 
        }
#endif
    }
}
