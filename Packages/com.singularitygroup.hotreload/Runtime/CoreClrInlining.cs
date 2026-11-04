#if ENABLE_CORECLR && UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace SingularityGroup.HotReload {
    /// <summary>
    /// Which patched methods the CoreCLR JIT may have inlined into code compiled before the patch, or compiled callers of
    /// as if the call never returns (Unity 7). Mono flags inlined methods itself (see <see cref="MethodUtils.IsMethodInlined"/>).
    /// </summary>
    /// <remarks>
    /// CoreCLR keeps no record of what it inlined, so methods small enough to inline count as inlined. The JIT only
    /// inlines optimized code, and goes by the assembly of each method for that (see <see cref="Optimized"/>).
    /// </remarks>
    static class CoreClrInlining {
        // JitExtDefaultPolicyMaxIL, the JIT's IL size limit for methods without profile data, which collectible code never has
        const int MaxInlineILSize = 128;

        public static bool IsMethodInlined(MethodBase method) {
            return Optimized(method) && SmallEnoughToInline(method);
        }

        /// A patch must not apply then: the method always threw, and a caller compiled as if the call never returns
        /// would crash when it returns.
        public static bool CallersAssumeNeverReturns(MethodBase method) {
            return Optimized(method) && SmallEnoughToInline(method) && !HasReturn(method);
        }

        /// <summary>
        /// Whether the JIT optimized the method's code, which it does unless the [Debuggable] attribute of the method's
        /// assembly disables the optimizer. The JIT only inlines such a method into callers, or compiles them as if the
        /// call never returns, and only does it in callers it optimizes too.
        /// </summary>
        /// <remarks>
        /// The C# compiler adds the attribute to the assemblies Unity compiles: in Debug Mode it disables the optimizer,
        /// in Release mode it doesn't. The server adds it to the patch assemblies in Debug Mode (DebuggablePatch), so they
        /// match. A patch assembly in Release mode has none, and CoreCLR optimizes it like the user's code.
        /// The editor's code optimization setting can differ from the loaded code, which only changes on a compile.
        /// </remarks>
        static bool Optimized(MethodBase method) {
            var attribute = (DebuggableAttribute)Attribute.GetCustomAttribute(method.Module.Assembly, typeof(DebuggableAttribute));
            return attribute == null || !attribute.IsJITOptimizerDisabled;
        }

        static bool SmallEnoughToInline(MethodBase method) {
            var flags = method.MethodImplementationFlags;
            if ((flags & MethodImplAttributes.NoInlining) != 0) {
                return false;
            }
            if ((flags & MethodImplAttributes.AggressiveInlining) != 0) {
                return true;
            }
            var body = method.GetMethodBody();
            return body != null && body.GetILAsByteArray().Length <= MaxInlineILSize;
        }

        // built on first use: Debug Mode never scans IL
        static Dictionary<short, OpCode> opCodes;

        static Dictionary<short, OpCode> CreateOpCodes() {
            var result = new Dictionary<short, OpCode>();
            foreach (var field in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)) {
                var opCode = (OpCode)field.GetValue(null);
                result[opCode.Value] = opCode;
            }
            return result;
        }

        /// The JIT takes a method without a ret as one that never returns (CALLEE_DOES_NOT_RETURN). Unknown IL counts as a ret.
        static bool HasReturn(MethodBase method) {
            var il = method.GetMethodBody()?.GetILAsByteArray();
            if (il == null) {
                return true;
            }
            if (opCodes == null) {
                opCodes = CreateOpCodes();
            }
            for (var i = 0; i < il.Length;) {
                short value = il[i++];
                if (value == 0xFE && i < il.Length) {
                    value = unchecked((short)(0xFE00 | il[i++]));
                }
                OpCode opCode;
                if (!opCodes.TryGetValue(value, out opCode)) {
                    return true;
                }
                if (opCode.Value == OpCodes.Ret.Value) {
                    return true;
                }
                switch (opCode.OperandType) {
                    case OperandType.InlineNone:
                        break;
                    case OperandType.ShortInlineBrTarget:
                    case OperandType.ShortInlineI:
                    case OperandType.ShortInlineVar:
                        i += 1;
                        break;
                    case OperandType.InlineVar:
                        i += 2;
                        break;
                    case OperandType.InlineI8:
                    case OperandType.InlineR:
                        i += 8;
                        break;
                    case OperandType.InlineSwitch:
                        if (i + 4 > il.Length) {
                            return true;
                        }
                        i += 4 + 4 * BitConverter.ToInt32(il, i);
                        break;
                    default:
                        i += 4;
                        break;
                }
            }
            return false;
        }
    }
}
#endif
