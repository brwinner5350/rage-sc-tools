﻿namespace ScTools.ScriptLang.CodeGen.Targets.RDR3;

using ScTools.ScriptAssembly.Targets;
using ScriptAssembly.Targets.RDR3;

public partial class CodeEmitter
{
    /// <summary>
    /// Optimizes instructions emitted by <see cref="CodeEmitter"/> using pattern matching.
    /// </summary>
    private class PatternOptimizer
    {
        private readonly IPattern[] patterns = new IPattern[]
        {
            new CombinedStoreOrLoadPattern(OpcodeV16.LOCAL_U8, OpcodeV16.LOCAL_U8_STORE, OpcodeV16.LOCAL_U8_LOAD),
            new CombinedStoreOrLoadPattern(OpcodeV16.LOCAL_U16, OpcodeV16.LOCAL_U16_STORE, OpcodeV16.LOCAL_U16_LOAD),

            new CombinedStoreOrLoadPattern(OpcodeV16.STATIC_U8, OpcodeV16.STATIC_U8_STORE, OpcodeV16.STATIC_U8_LOAD),
            new CombinedStoreOrLoadPattern(OpcodeV16.STATIC_U16, OpcodeV16.STATIC_U16_STORE, OpcodeV16.STATIC_U16_LOAD),
            new CombinedStoreOrLoadPattern(OpcodeV16.STATIC_U24, OpcodeV16.STATIC_U24_STORE, OpcodeV16.STATIC_U24_LOAD),

            new CombinedStoreOrLoadPattern(OpcodeV16.GLOBAL_U16, OpcodeV16.GLOBAL_U16_STORE, OpcodeV16.GLOBAL_U16_LOAD),
            new CombinedStoreOrLoadPattern(OpcodeV16.GLOBAL_U24, OpcodeV16.GLOBAL_U24_STORE, OpcodeV16.GLOBAL_U24_LOAD),

            new CombinedStoreOrLoadPattern(OpcodeV16.IOFFSET_U8, OpcodeV16.IOFFSET_U8_STORE, OpcodeV16.IOFFSET_U8_LOAD),
            new CombinedStoreOrLoadPattern(OpcodeV16.IOFFSET_S16, OpcodeV16.IOFFSET_S16_STORE, OpcodeV16.IOFFSET_S16_LOAD),

            new CombinedStoreOrLoadPattern(OpcodeV16.ARRAY_U8, OpcodeV16.ARRAY_U8_STORE, OpcodeV16.ARRAY_U8_LOAD),
            new CombinedStoreOrLoadPattern(OpcodeV16.ARRAY_U16, OpcodeV16.ARRAY_U16_STORE, OpcodeV16.ARRAY_U16_LOAD),

            new AddMulS16Pattern(),
            new AddMulU8Pattern(),

            new PushConstU8Pattern(),

            new IntCompareAndJZPattern(),
        };

        public void Optimize(InstructionBuffer instBuffer)
        {
            for (int i = 0; i < instBuffer.NumberOfInstructions; i++)
            {
                var inst = instBuffer.GetRef(i);
                if (instBuffer.IsEmpty(inst))
                {
                    continue;
                }

                bool anyMatch = false;
                for (int patternIdx = 0; patternIdx < patterns.Length; patternIdx++)
                {
                    var pattern = patterns[patternIdx];
                    if (pattern.MatchAndOptimize(instBuffer, inst))
                    {
                        anyMatch = true;
                        break;
                    }
                }

                if (anyMatch)
                {
                    // re-match all patterns on the instruction that was just optimized in case a new pattern appeared
                    i--;
                }
            }
        }

        private static InstructionReference? GetFirstNonEmptyInstruction(InstructionBuffer instBuffer, int start)
        {
            for (int i = start; i < instBuffer.NumberOfInstructions; i++)
            {
                var inst = instBuffer.GetRef(i);
                if (!instBuffer.IsEmpty(inst))
                {
                    return inst;
                }
            }

            return null;
        }

        private interface IPattern
        {
            bool MatchAndOptimize(InstructionBuffer instBuffer, InstructionReference instruction);
        }

        private sealed class CombinedStoreOrLoadPattern : IPattern
        {
            public OpcodeV16 Target { get; }
            public OpcodeV16 StoreReplacement { get; }
            public OpcodeV16 LoadReplacement { get; }

            public CombinedStoreOrLoadPattern(OpcodeV16 target, OpcodeV16 storeReplacement, OpcodeV16 loadReplacement)
            {
                Target = target;
                StoreReplacement = storeReplacement;
                LoadReplacement = loadReplacement;
            }

            public bool MatchAndOptimize(InstructionBuffer instBuffer, InstructionReference instruction)
            {
                var first = GetFirstNonEmptyInstruction(instBuffer, instruction.Index);
                if (first is null || instBuffer.GetOpcode(first) != Target)
                {
                    return false;
                }

                var second = GetFirstNonEmptyInstruction(instBuffer, first.Index + 1);
                if (second is null || instBuffer.GetOpcode(second) is not (OpcodeV16.STORE or OpcodeV16.LOAD))
                {
                    return false;
                }

                var replacement = instBuffer.GetOpcode(second) switch { OpcodeV16.STORE => StoreReplacement, OpcodeV16.LOAD => LoadReplacement, _ => throw new NotSupportedException() };

                var newBytes = instBuffer.GetBytes(first);
                newBytes[0] = (byte)replacement;
                instBuffer.Update(first, newBytes);
                instBuffer.Remove(second);
                return true;
            }
        }

        /// <summary>
        /// Merges a PUSH_CONST_S16 instruction followed by IADD/IMUL to IADD_S16/IMUL_S16.
        /// </summary>
        public sealed class AddMulS16Pattern : IPattern
        {
            public bool MatchAndOptimize(InstructionBuffer instBuffer, InstructionReference instruction)
            {
                var first = GetFirstNonEmptyInstruction(instBuffer, instruction.Index);
                if (first is null || instBuffer.GetOpcode(first) is not OpcodeV16.PUSH_CONST_S16)
                {
                    return false;
                }

                var second = GetFirstNonEmptyInstruction(instBuffer, first.Index + 1);
                if (second is null || instBuffer.GetOpcode(second) is not (OpcodeV16.IADD or OpcodeV16.IMUL))
                {
                    return false;
                }

                var replacement = instBuffer.GetOpcode(second) switch { OpcodeV16.IADD => OpcodeV16.IADD_S16, OpcodeV16.IMUL => OpcodeV16.IMUL_S16, _ => throw new NotImplementedException() };

                var newBytes = instBuffer.GetBytes(first);
                newBytes[0] = (byte)replacement;
                instBuffer.Update(first, newBytes);
                instBuffer.Remove(second);
                return true;
            }
        }

        /// <summary>
        /// Merges a PUSH_CONST_U8 instruction followed by IADD/IMUL to IADD_U8/IMUL_U8.
        /// </summary>
        public sealed class AddMulU8Pattern : IPattern
        {
            public bool MatchAndOptimize(InstructionBuffer instBuffer, InstructionReference instruction)
            {
                var first = GetFirstNonEmptyInstruction(instBuffer, instruction.Index);
                if (first is null || instBuffer.GetOpcode(first) is not (OpcodeV16.PUSH_CONST_U8
                                                                        or OpcodeV16.PUSH_CONST_U8_U8
                                                                        or OpcodeV16.PUSH_CONST_U8_U8_U8
                                                                        or (>= OpcodeV16.PUSH_CONST_0 and <= OpcodeV16.PUSH_CONST_7)))
                {
                    return false;
                }

                var second = GetFirstNonEmptyInstruction(instBuffer, first.Index + 1);
                if (second is null || instBuffer.GetOpcode(second) is not (OpcodeV16.IADD or OpcodeV16.IMUL))
                {
                    return false;
                }

                var replacement = instBuffer.GetOpcode(second) switch { OpcodeV16.IADD => OpcodeV16.IADD_U8, OpcodeV16.IMUL => OpcodeV16.IMUL_U8, _ => throw new NotImplementedException() };
                var newOpBytes = new List<byte>(capacity: 2) { (byte)replacement, 0 };

                instBuffer.Remove(second);
                switch (instBuffer.GetOpcode(first))
                {
                    case OpcodeV16.PUSH_CONST_0:
                    case OpcodeV16.PUSH_CONST_1:
                    case OpcodeV16.PUSH_CONST_2:
                    case OpcodeV16.PUSH_CONST_3:
                    case OpcodeV16.PUSH_CONST_4:
                    case OpcodeV16.PUSH_CONST_5:
                    case OpcodeV16.PUSH_CONST_6:
                    case OpcodeV16.PUSH_CONST_7:
                        newOpBytes[1] = instBuffer.GetOpcode(first).AsInteger<OpcodeV16, byte>();
                        instBuffer.Update(first, newOpBytes);
                        break;
                    case OpcodeV16.PUSH_CONST_U8:
                        newOpBytes[1] = instBuffer.GetByte(first, 1);
                        instBuffer.Update(first, newOpBytes);
                        break;
                    case OpcodeV16.PUSH_CONST_U8_U8:
                        newOpBytes[1] = instBuffer.GetByte(first, 2);
                        var newPushU8Bytes = instBuffer.GetBytes(first);
                        newPushU8Bytes[0] = (byte)OpcodeV16.PUSH_CONST_U8;
                        newPushU8Bytes.RemoveAt(newPushU8Bytes.Count - 1);
                        instBuffer.Update(first, newPushU8Bytes);
                        instBuffer.InsertAfter(first, newOpBytes);
                        break;
                    case OpcodeV16.PUSH_CONST_U8_U8_U8:
                        newOpBytes[1] = instBuffer.GetByte(first, 3);
                        var newPushU8U8Bytes = instBuffer.GetBytes(first);
                        newPushU8U8Bytes[0] = (byte)OpcodeV16.PUSH_CONST_U8_U8;
                        newPushU8U8Bytes.RemoveAt(newPushU8U8Bytes.Count - 1);
                        instBuffer.Update(first, newPushU8U8Bytes);
                        instBuffer.InsertAfter(first, newOpBytes);
                        break;
                }

                return true;
            }
        }

        /// <summary>
        /// Merges sequential PUSH_CONST_U8 instructions to PUSH_CONST_U8_U8 and PUSH_CONST_U8_U8_U8.
        /// </summary>
        public sealed class PushConstU8Pattern : IPattern
        {
            public bool MatchAndOptimize(InstructionBuffer instBuffer, InstructionReference instruction)
            {
                var first = GetFirstNonEmptyInstruction(instBuffer, instruction.Index);
                if (first is null || instBuffer.GetOpcode(first) is not (OpcodeV16.PUSH_CONST_U8 or OpcodeV16.PUSH_CONST_U8_U8))
                {
                    return false;
                }

                var second = GetFirstNonEmptyInstruction(instBuffer, first.Index + 1);
                if (second is null || instBuffer.GetOpcode(second) is not OpcodeV16.PUSH_CONST_U8)
                {
                    return false;
                }

                List<byte> newBytes = instBuffer.GetBytes(first);
                switch (instBuffer.GetOpcode(first))
                {
                    case OpcodeV16.PUSH_CONST_U8:
                        newBytes[0] = (byte)OpcodeV16.PUSH_CONST_U8_U8;
                        newBytes.Add(instBuffer.GetByte(second, 1));
                        break;
                    case OpcodeV16.PUSH_CONST_U8_U8:
                        newBytes[0] = (byte)OpcodeV16.PUSH_CONST_U8_U8_U8;
                        newBytes.Add(instBuffer.GetByte(second, 1));
                        break;
                }

                instBuffer.Update(first, newBytes);
                instBuffer.Remove(second);
                return true;
            }
        }

        /// <summary>
        /// Merges a IEQ/INE/IGT/IGE/ILT/ILE instruction followed by JZ to IEQ_JZ/INE_JZ/IGT_JZ/IGE_JZ/ILT_JZ/ILE_JZ.
        /// </summary>
        public sealed class IntCompareAndJZPattern : IPattern
        {
            public bool MatchAndOptimize(InstructionBuffer instBuffer, InstructionReference instruction)
            {
                var first = GetFirstNonEmptyInstruction(instBuffer, instruction.Index);
                if (first is null || instBuffer.GetOpcode(first) is not (>= OpcodeV16.IEQ and <= OpcodeV16.ILE))
                {
                    return false;
                }

                var second = GetFirstNonEmptyInstruction(instBuffer, first.Index + 1);
                if (second is null || instBuffer.GetOpcode(second) is not OpcodeV16.JZ)
                {
                    return false;
                }

                var replacement = instBuffer.GetOpcode(first) - OpcodeV16.IEQ + OpcodeV16.IEQ_JZ;

                var newBytes = instBuffer.GetBytes(second);
                newBytes[0] = (byte)replacement;
                instBuffer.Update(second, newBytes);
                instBuffer.Remove(first);
                return true;
            }
        }
    }
}
