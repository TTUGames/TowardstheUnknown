using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace SingularityGroup.HotReload.Editor {
    internal static class HotReloadBuildHelper {
        /// <summary>
        /// Should HotReload runtime be included in the current build?
        /// </summary>
        public static bool IncludeInThisBuild() {
            return IsAllBuildSettingsSupported();
        }

        /// <summary>
        /// The scripting backend a player needs for Hot Reload: Mono, or CoreCLR on Unity 7, which has no Mono.
        /// </summary>
        public static ScriptingImplementation SupportedScriptingBackend {
            get {
#if UNITY_7000_0_OR_NEWER
                return ScriptingImplementation.CoreCLR;
#else
                return ScriptingImplementation.Mono2x;
#endif
            }
        }

        /// <summary>
        /// Get scripting backend for the current platform.
        /// </summary>
        /// <returns>Scripting backend</returns>
        public static ScriptingImplementation GetCurrentScriptingBackend() {
#pragma warning disable CS0618
            return PlayerSettings.GetScriptingBackend(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
#pragma warning restore CS0618
        }

        public static ManagedStrippingLevel GetCurrentStrippingLevel() {
#pragma warning disable CS0618
            return PlayerSettings.GetManagedStrippingLevel(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
#pragma warning restore CS0618
        }

        public static bool WillIncludeRuntimeInBuild() {
#if UNITY_7000_0_OR_NEWER
            return IsInstrumentedManagedCodeVariant();
#elif UNITY_6000_6_OR_NEWER
            return EditorUserBuildSettings.development || IsInstrumentedManagedCodeVariant();
#else
            return EditorUserBuildSettings.development;
#endif
        }

        public static void EnsureRuntimeIncludedInBuild() {
            if (WillIncludeRuntimeInBuild()) {
                // avoid changing ProjectSettings when not needed
                return;
            }
#if UNITY_7000_0_OR_NEWER
            // Development Build no longer defines DEVELOPMENT_BUILD, the variant is the only thing that includes us.
            SetInstrumentedManagedCodeVariant();
#else
            EditorUserBuildSettings.development = true;
#endif
        }

#if UNITY_6000_6_OR_NEWER
        /// Name of the Managed Code Variant, or null when Unity cannot tell us.
        public static string GetManagedCodeVariantName() {
            try {
                return PlayerSettings.GetManagedCodeVariant(CurrentNamedBuildTarget()).ToString();
            } catch {
                return null;
            }
        }

        /// Does the Managed Code Variant define UNITY_INCLUDE_INSTRUMENTATION?
        /// Release is the only variant that does not.
        public static bool IsInstrumentedManagedCodeVariant() {
            try {
                return PlayerSettings.GetManagedCodeVariant(CurrentNamedBuildTarget()) != ManagedCodeVariant.Release;
            } catch {
                // unknown platform or a Unity version that changed the api. Do not claim the build is broken.
                return true;
            }
        }

        static void SetInstrumentedManagedCodeVariant() {
            try {
                var namedBuildTarget = CurrentNamedBuildTarget();
                if (PlayerSettings.GetManagedCodeVariant(namedBuildTarget) == ManagedCodeVariant.Release) {
                    // Instrumented is the cheapest variant that defines UNITY_INCLUDE_INSTRUMENTATION
                    PlayerSettings.SetManagedCodeVariant(namedBuildTarget, ManagedCodeVariant.Instrumented);
                }
            } catch (Exception ex) {
                Log.Warning(ex.Message);
            }
        }

#if UNITY_7000_0_OR_NEWER
        /// <summary>
        /// Are the player's scripts compiled without optimizations?
        /// </summary>
        /// <remarks>
        /// CoreCLR inlines optimized methods into their callers, Unity's own invokers of event methods like
        /// Update included, so a patch of such a method never runs. CoreCLR never inlines code compiled for debugging.
        /// (Mono differs: it inlines small methods into managed callers even then, and only the debugger agent
        /// that Script Debugging starts stops it. It never inlines event methods, which Unity invokes through calli.)
        /// The Debug variant compiles for debugging. Script Debugging in a Development Build compiles the same scripts
        /// with any variant (byte-identical in 7000.0.0a10), but it doesn't count here: Unity doesn't document it,
        /// and it also ships the development engine, which is slower, shows the Development Build label and console and
        /// listens on the network. Requiring only the Debug variant keeps the On-Device check and its fix the same.
        /// </remarks>
        public static bool CompilesScriptsForDebugging() {
            try {
                return PlayerSettings.GetManagedCodeVariant(CurrentNamedBuildTarget()) == ManagedCodeVariant.Debug;
            } catch {
                // unknown platform or a Unity version that changed the api. Do not claim the build is broken.
                return true;
            }
        }

        public static void EnsureScriptsCompiledForDebugging() {
            if (CompilesScriptsForDebugging()) {
                // avoid changing ProjectSettings when not needed
                return;
            }
            try {
                // the Debug variant also defines UNITY_INCLUDE_INSTRUMENTATION, so the runtime is included too
                PlayerSettings.SetManagedCodeVariant(CurrentNamedBuildTarget(), ManagedCodeVariant.Debug);
            } catch (Exception ex) {
                Log.Warning(ex.Message);
            }
        }
#endif

        static NamedBuildTarget CurrentNamedBuildTarget() {
            return NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        }
#endif

        public static void SetCurrentScriptingBackend(ScriptingImplementation to) {
#pragma warning disable CS0618
            // only set it if default is not correct (avoid changing ProjectSettings when not needed)
            if (GetCurrentScriptingBackend() != to) {
                PlayerSettings.SetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup, to);
            }
#pragma warning restore CS0618
        }
        
        public static void SetCurrentStrippingLevel(ManagedStrippingLevel to) {
#pragma warning disable CS0618
            // only set it if default is not correct (avoid changing ProjectSettings when not needed)
            if (GetCurrentStrippingLevel() != to) {
                PlayerSettings.SetManagedStrippingLevel(EditorUserBuildSettings.selectedBuildTargetGroup, to);
            }
#pragma warning restore CS0618
        }

        /// Is the current build target supported?
        /// main thread only
        public static bool IsBuildTargetSupported() {
            var buildTarget = EditorUserBuildSettings.selectedBuildTargetGroup;  
            return Array.IndexOf(unsupportedBuildTargets, buildTarget) == -1;
        }
        
        /// Are all the settings supported?
        /// main thread only
        static bool IsAllBuildSettingsSupported() {
            if (!IsBuildTargetSupported()) {
                return false;
            }

            // need way to give it settings object, dont want to give serializedobject
            var options = HotReloadSettingsEditor.LoadSettingsOrDefault();
            var so = new SerializedObject(options);
            
            // check all projeect options
            foreach (var option in HotReloadSettingsTab.allOptions) {
                var projectOption = option as ProjectOptionBase;
                if (projectOption != null) {
                    // if option is required, build can't use hot reload
                    if (projectOption.IsRequiredForBuild() && !projectOption.GetValue(so)) {
                        return false;
                    }
                }
            }

            return GetCurrentScriptingBackend() == SupportedScriptingBackend
                && GetCurrentStrippingLevel() == ManagedStrippingLevel.Disabled
#if UNITY_7000_0_OR_NEWER
                && CompilesScriptsForDebugging()
#endif
                && WillIncludeRuntimeInBuild();
        }

        /// <summary>
        /// Some platforms are not supported because they don't have the Mono (or CoreCLR) scripting backend.
        /// </summary>
        /// <remarks>
        /// Only list the platforms that definately don't have Mono scripting.
        /// </remarks>
        private static readonly BuildTargetGroup[] unsupportedBuildTargets = new [] {
            BuildTargetGroup.iOS, // mono support was removed many years ago
            BuildTargetGroup.WebGL, // has never had mono
        };
        
#pragma warning disable CS0618
        /// Does the platform have the <see cref="SupportedScriptingBackend"/>?
        public static bool IsSupportedScriptingBackendAvailable(BuildTargetGroup buildTarget) {
            var supportedBackend = SupportedScriptingBackend;
            var backend = PlayerSettings.GetScriptingBackend(buildTarget);
            if (backend == supportedBackend) {
                return true;
            }
            // Switching to Mono makes Unity strip Mono-incompatible architectures (e.g. ARM64 on Android),
            // and switching back does not re-add them. Save them so we can restore them afterwards.
            var androidArchitectures = PlayerSettings.Android.targetArchitectures;
            try {
                // GetDefaultScriptingBackend returns IL2CPP for Unity 6 which goes against Unity documentation.
                // Have to use a workaround approach instead
                PlayerSettings.SetScriptingBackend(buildTarget, supportedBackend);
                return PlayerSettings.GetScriptingBackend(buildTarget) == supportedBackend;
            } catch {
                return false;
            } finally {
                PlayerSettings.SetScriptingBackend(buildTarget, backend);
                // restore after the backend, otherwise Unity strips the architectures again
                if (PlayerSettings.Android.targetArchitectures != androidArchitectures) {
                    PlayerSettings.Android.targetArchitectures = androidArchitectures;
                }
            }
        }
#pragma warning restore CS0618
    }
}