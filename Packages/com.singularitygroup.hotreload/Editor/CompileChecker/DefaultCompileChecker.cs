using System;
using System.IO;
using System.Threading.Tasks;
using SingularityGroup.HotReload.Editor.Localization;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace SingularityGroup.HotReload.Editor {
    class DefaultCompileChecker : ICompileChecker {
        // careful when modifying - also used on the server
        static string recompileFilePath = PackageConst.LibraryCachePath + "/recompile.txt";
        public bool hasCompileErrors { get; private set;  }
        static bool compiling;
        bool compiledEditorAssembly;
        bool recompile;
        readonly string compilationSessionId;
        public string CompilationSessionId => compilationSessionId;
        public DefaultCompileChecker() {
            if (MultiplayerPlaymodeHelper.IsClone) {
                CompilationPipeline.compilationStarted += OnCompilationStartedClone;
                CompilationPipeline.assemblyCompilationFinished += DetectCompileErrors;
                CompilationPipeline.compilationFinished += OnCompilationFinishedClone;
                return;
            }
            compilationSessionId = HotReloadState.CompileSessionId;
            CompilationPipeline.compilationStarted += OnCompilationStarted;
            CompilationPipeline.assemblyCompilationFinished += DetectCompileErrors;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
            Task.Run(() => {
                try {
                    var compileSessionId = File.ReadAllText(recompileFilePath);
                    if (compileSessionId == compilationSessionId) {
                        ThreadUtility.RunOnMainThread(() => {
                            recompile = true;
                            _onCompilationFinished?.Invoke();
                        });
                    } else {
                        ThreadUtility.RunOnMainThread(OnCompilationRequestFinished);
                    }
                } catch (DirectoryNotFoundException) {
                    //dir doesn't exist -> no recompile required
                    ThreadUtility.RunOnMainThread(OnCompilationRequestFinished);
                } catch (FileNotFoundException) {
                    //file doesn't exist -> no recompile required
                    ThreadUtility.RunOnMainThread(OnCompilationRequestFinished);
                } catch (Exception ex) {
                    Log.Warning(Translations.Errors.WarningCompileCheckerIssue, ex.GetType().Name, ex.Message);
                    ThreadUtility.RunOnMainThread(OnCompilationRequestFinished);
                }
            });
        }
        
        void DetectCompileErrors(string assemblyPath, CompilerMessage[] messages) {
            // player script compiles (builds, Addressables) fire the same events but write elsewhere and don't reload the domain.
            // their errors don't affect the editor, so they must not trigger a recompile
            if (assemblyPath == null || !assemblyPath.Replace('\\', '/').Contains("Library/ScriptAssemblies/")) {
                return;
            }
            compiledEditorAssembly = true;
            // sticky for the whole compilation: an error in any assembly means it failed, even if a later assembly compiles clean
            for (int i = 0; i < messages.Length; i++) {
                if (messages[i].type == CompilerMessageType.Error) {
                    hasCompileErrors = true;
                    return;
                }
            }
        }

        void OnCompilationStarted(object _) {
            compiling = true;
            hasCompileErrors = false;
            compiledEditorAssembly = false;
            var newCompilationSessionId = Guid.NewGuid().ToString();
            HotReloadState.CompileSessionId = newCompilationSessionId;
            var dirName = Path.GetDirectoryName(PackageConst.LibraryCachePath);
            if (dirName != null) {
                Directory.CreateDirectory(dirName);
            }
            File.WriteAllText(recompileFilePath, newCompilationSessionId);
        }

        void OnCompilationStartedClone(object _) {
            compiling = true;
            hasCompileErrors = false;
        }
        
        void OnCompilationFinishedClone(object _) {
            compiling = false;
        }
        
        public void OnCompilationRequestFinished() {
            if (compiling || HotReloadState.CompileSessionId != compilationSessionId) {
                return;
            }
            try {
                File.Delete(recompileFilePath);
            } catch { /*_*/ }
        }

        void OnCompilationFinished(object _) {
            compiling = false;
            //Don't recompile on compile errors
            if (hasCompileErrors) {
                File.Delete(recompileFilePath);
            } else if (compiledEditorAssembly) {
                // persisted patches target the old assemblies; the upcoming domain reload must not reapply them
                HotReloadState.RecompiledBeforeDomainReload = true;
            } else if (!HotReloadState.RecompiledBeforeDomainReload) {
                // player-only compile: no domain reload follows to delete the file, and the server pauses patching while it exists.
                // skipped when an earlier editor compile still waits for its reload (e.g. a build with Auto Refresh off):
                // that reload needs the file to request the compile and clear persisted patches
                File.Delete(recompileFilePath);
            }
        }

        Action _onCompilationFinished;
        public event Action onCompilationFinished {
            add {
                if(recompile && value != null) {
                    value();
                }
                _onCompilationFinished += value;
            }
            remove {
                _onCompilationFinished -= value;
            }
        }
    }
}