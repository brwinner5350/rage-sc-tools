namespace ScTools.Decompiler.IR;

using System;
using System.Collections.Immutable;
using ScTools.ScriptAssembly.Targets;
using ScTools.ScriptAssembly.Targets.RDR3;

public sealed class IRDisassemblerRDR3
{
    public static IRCode Disassemble(ScTools.GameFiles.RDR3.Script script) => new IRDisassemblerRDR3(script).Disassemble();

    private ScTools.GameFiles.RDR3.Script Script { get; }

    private IRDisassemblerRDR3(ScTools.GameFiles.RDR3.Script sc)
    {
        Script = sc ?? throw new ArgumentNullException(nameof(sc));
    }

    private IRCode Disassemble()
    {
        var sc = new IRCode();
        if (Script.CodeLength == 0)
        {
            return sc;
        }

        foreach (var inst in Script.EnumerateInstructions())
        {
            DisassembleInstruction(sc, inst.Address, inst.Bytes);
        }
        sc.AppendInstruction(new IREndOfScript((int)Script.CodeLength));
        return sc;
    }

    private void DisassembleInstruction(IRCode script, int ip, ReadOnlySpan<byte> inst)
    {
        var opcode = (OpcodeV16)inst[0];

        switch (opcode)
        {
            case OpcodeV16.NOP:
                script.AppendInstruction(new IRNop(ip));
                break;
            case OpcodeV16.LEAVE:
                var leave = opcode.GetLeaveOperands(inst);
                script.AppendInstruction(new IRLeave(ip, leave.ParamCount, leave.ReturnCount));
                break;
            case OpcodeV16.ENTER:
                var enter = opcode.GetEnterOperands(inst);
                script.AppendInstruction(new IREnter(ip, enter.ParamCount, enter.FrameSize - 2, opcode.GetEnterFunctionName(inst)));
                break;
            case OpcodeV16.PUSH_CONST_U8:
                script.AppendInstruction(new IRPushInt(ip, opcode.GetU8Operand(inst)));
                break;
            case OpcodeV16.PUSH_CONST_U8_U8:
                script.AppendInstruction(new IRPushInt(ip, opcode.GetU8Operand(inst, 0)));
                script.AppendInstruction(new IRPushInt(ip, opcode.GetU8Operand(inst, 1)));
                break;
            case OpcodeV16.PUSH_CONST_U8_U8_U8:
                script.AppendInstruction(new IRPushInt(ip, opcode.GetU8Operand(inst, 0)));
                script.AppendInstruction(new IRPushInt(ip, opcode.GetU8Operand(inst, 1)));
                script.AppendInstruction(new IRPushInt(ip, opcode.GetU8Operand(inst, 2)));
                break;
            case OpcodeV16.PUSH_CONST_S16:
                script.AppendInstruction(new IRPushInt(ip, opcode.GetS16Operand(inst)));
                break;
            case OpcodeV16.PUSH_CONST_U24:
                script.AppendInstruction(new IRPushInt(ip, unchecked((int)opcode.GetU24Operand(inst))));
                break;
            case OpcodeV16.PUSH_CONST_U32:
                script.AppendInstruction(new IRPushInt(ip, unchecked((int)opcode.GetU32Operand(inst))));
                break;
            case OpcodeV16.PUSH_CONST_F:
                script.AppendInstruction(new IRPushFloat(ip, opcode.GetFloatOperand(inst)));
                break;
            case OpcodeV16.NATIVE:
                var native = opcode.GetNativeOperands(inst);
                var commandHash = Script.NativeHash(native.CommandIndex);
                script.AppendInstruction(new IRNativeCall(ip, native.ParamCount, native.ReturnCount, commandHash));
                break;
            case OpcodeV16.J:
                var jumpOffset = unchecked((int)opcode.GetS16Operand(inst));
                var jumpAddress = ip + 3 + jumpOffset;
                script.AppendInstruction(new IRJump(ip, jumpAddress));
                break;
            case OpcodeV16.JZ:
                var jzOffset = unchecked((int)opcode.GetS16Operand(inst));
                var jzAddress = ip + 3 + jzOffset;
                script.AppendInstruction(new IRJumpIfZero(ip, jzAddress));
                break;
            case OpcodeV16.IEQ_JZ:
                script.AppendInstruction(new IRIEqual(ip));
                goto case OpcodeV16.JZ;
            case OpcodeV16.INE_JZ:
                script.AppendInstruction(new IRINotEqual(ip));
                goto case OpcodeV16.JZ;
            case OpcodeV16.IGT_JZ:
                script.AppendInstruction(new IRIGreaterThan(ip));
                goto case OpcodeV16.JZ;
            case OpcodeV16.IGE_JZ:
                script.AppendInstruction(new IRIGreaterOrEqual(ip));
                goto case OpcodeV16.JZ;
            case OpcodeV16.ILT_JZ:
                script.AppendInstruction(new IRILessThan(ip));
                goto case OpcodeV16.JZ;
            case OpcodeV16.ILE_JZ:
                script.AppendInstruction(new IRILessOrEqual(ip));
                goto case OpcodeV16.JZ;

            case OpcodeV16.CALL:
                var callAddr = unchecked((int)opcode.GetU24Operand(inst));
                script.AppendInstruction(new IRCall(ip, callAddr));
                break;
            case OpcodeV16.SWITCH:
                var cases = ImmutableArray.CreateBuilder<IRSwitchCase>(opcode.GetSwitchNumberOfCases(inst));
                foreach (var c in opcode.GetSwitchOperands(inst))
                {
                    cases.Add(new(unchecked((int)c.Value), c.GetJumpTargetAddress(ip)));
                }
                script.AppendInstruction(new IRSwitch(ip, cases.MoveToImmutable()));
                break;

            case OpcodeV16.STRING: script.AppendInstruction(new IRPushStringFromStringTable(ip)); break;
            case OpcodeV16.IADD: script.AppendInstruction(new IRIAdd(ip)); break;
            case OpcodeV16.ISUB: script.AppendInstruction(new IRISub(ip)); break;
            case OpcodeV16.IMUL: script.AppendInstruction(new IRIMul(ip)); break;
            case OpcodeV16.IDIV: script.AppendInstruction(new IRIDiv(ip)); break;
            case OpcodeV16.IMOD: script.AppendInstruction(new IRIMod(ip)); break;
            case OpcodeV16.INOT: script.AppendInstruction(new IRINot(ip)); break;
            case OpcodeV16.INEG: script.AppendInstruction(new IRINeg(ip)); break;
            case OpcodeV16.IEQ: script.AppendInstruction(new IRIEqual(ip)); break;
            case OpcodeV16.INE: script.AppendInstruction(new IRINotEqual(ip)); break;
            case OpcodeV16.IGT: script.AppendInstruction(new IRIGreaterThan(ip)); break;
            case OpcodeV16.IGE: script.AppendInstruction(new IRIGreaterOrEqual(ip)); break;
            case OpcodeV16.ILT: script.AppendInstruction(new IRILessThan(ip)); break;
            case OpcodeV16.ILE: script.AppendInstruction(new IRILessOrEqual(ip)); break;
            case OpcodeV16.FADD: script.AppendInstruction(new IRFAdd(ip)); break;
            case OpcodeV16.FSUB: script.AppendInstruction(new IRFSub(ip)); break;
            case OpcodeV16.FMUL: script.AppendInstruction(new IRFMul(ip)); break;
            case OpcodeV16.FDIV: script.AppendInstruction(new IRFDiv(ip)); break;
            case OpcodeV16.FMOD: script.AppendInstruction(new IRFMod(ip)); break;
            case OpcodeV16.FNEG: script.AppendInstruction(new IRFNeg(ip)); break;
            case OpcodeV16.FEQ: script.AppendInstruction(new IRFEqual(ip)); break;
            case OpcodeV16.FNE: script.AppendInstruction(new IRFNotEqual(ip)); break;
            case OpcodeV16.FGT: script.AppendInstruction(new IRFGreaterThan(ip)); break;
            case OpcodeV16.FGE: script.AppendInstruction(new IRFGreaterOrEqual(ip)); break;
            case OpcodeV16.FLT: script.AppendInstruction(new IRFLessThan(ip)); break;
            case OpcodeV16.FLE: script.AppendInstruction(new IRFLessOrEqual(ip)); break;
            case OpcodeV16.VADD: script.AppendInstruction(new IRVAdd(ip)); break;
            case OpcodeV16.VSUB: script.AppendInstruction(new IRVSub(ip)); break;
            case OpcodeV16.VMUL: script.AppendInstruction(new IRVMul(ip)); break;
            case OpcodeV16.VDIV: script.AppendInstruction(new IRVDiv(ip)); break;
            case OpcodeV16.VNEG: script.AppendInstruction(new IRVNeg(ip)); break;
            case OpcodeV16.IAND: script.AppendInstruction(new IRIAnd(ip)); break;
            case OpcodeV16.IOR: script.AppendInstruction(new IRIOr(ip)); break;
            case OpcodeV16.IXOR: script.AppendInstruction(new IRIXor(ip)); break;
            case OpcodeV16.I2F: script.AppendInstruction(new IRIntToFloat(ip)); break;
            case OpcodeV16.F2I: script.AppendInstruction(new IRFloatToInt(ip)); break;
            case OpcodeV16.F2V: script.AppendInstruction(new IRFloatToVector(ip)); break;
            case OpcodeV16.DUP: script.AppendInstruction(new IRDup(ip)); break;
            case OpcodeV16.DROP: script.AppendInstruction(new IRDrop(ip)); break;
            case OpcodeV16.LOAD: script.AppendInstruction(new IRLoad(ip)); break;
            case OpcodeV16.STORE: script.AppendInstruction(new IRStore(ip)); break;
            case OpcodeV16.STORE_REV: script.AppendInstruction(new IRStoreRev(ip)); break;
            case OpcodeV16.LOAD_N: script.AppendInstruction(new IRLoadN(ip)); break;
            case OpcodeV16.STORE_N: script.AppendInstruction(new IRStoreN(ip)); break;

            case OpcodeV16.IADD_U8:
                script.AppendInstruction(new IRPushInt(ip, opcode.GetU8Operand(inst)));
                script.AppendInstruction(new IRIAdd(ip));
                break;
            case OpcodeV16.IADD_S16:
                script.AppendInstruction(new IRPushInt(ip, opcode.GetS16Operand(inst)));
                script.AppendInstruction(new IRIAdd(ip));
                break;
            case OpcodeV16.IMUL_U8:
                script.AppendInstruction(new IRPushInt(ip, opcode.GetU8Operand(inst)));
                script.AppendInstruction(new IRIMul(ip));
                break;
            case OpcodeV16.IMUL_S16:
                script.AppendInstruction(new IRPushInt(ip, opcode.GetS16Operand(inst)));
                script.AppendInstruction(new IRIMul(ip));
                break;

            case OpcodeV16.IOFFSET:
                script.AppendInstruction(new IRIAdd(ip));
                break;
            case OpcodeV16.IOFFSET_U8:
            case OpcodeV16.IOFFSET_U8_LOAD:
            case OpcodeV16.IOFFSET_U8_STORE:
                script.AppendInstruction(new IRPushInt(ip, opcode.GetU8Operand(inst)));
                script.AppendInstruction(new IRIAdd(ip));
                if (opcode is OpcodeV16.IOFFSET_U8_LOAD) goto case OpcodeV16.LOAD;
                if (opcode is OpcodeV16.IOFFSET_U8_STORE) goto case OpcodeV16.STORE;
                break;
            case OpcodeV16.IOFFSET_S16:
            case OpcodeV16.IOFFSET_S16_LOAD:
            case OpcodeV16.IOFFSET_S16_STORE:
                script.AppendInstruction(new IRPushInt(ip, opcode.GetS16Operand(inst)));
                script.AppendInstruction(new IRIAdd(ip));
                if (opcode is OpcodeV16.IOFFSET_S16_LOAD) goto case OpcodeV16.LOAD;
                if (opcode is OpcodeV16.IOFFSET_S16_STORE) goto case OpcodeV16.STORE;
                break;

            case OpcodeV16.LOCAL_U8:
            case OpcodeV16.LOCAL_U8_LOAD:
            case OpcodeV16.LOCAL_U8_STORE:
                script.AppendInstruction(new IRLocalRef(ip, opcode.GetU8Operand(inst)));
                if (opcode is OpcodeV16.LOCAL_U8_LOAD) goto case OpcodeV16.LOAD;
                if (opcode is OpcodeV16.LOCAL_U8_STORE) goto case OpcodeV16.STORE;
                break;
            case OpcodeV16.STATIC_U8:
            case OpcodeV16.STATIC_U8_LOAD:
            case OpcodeV16.STATIC_U8_STORE:
                script.AppendInstruction(new IRStaticRef(ip, opcode.GetU8Operand(inst)));
                if (opcode is OpcodeV16.STATIC_U8_LOAD) goto case OpcodeV16.LOAD;
                if (opcode is OpcodeV16.STATIC_U8_STORE) goto case OpcodeV16.STORE;
                break;


            case OpcodeV16.LOCAL_U16:
            case OpcodeV16.LOCAL_U16_LOAD:
            case OpcodeV16.LOCAL_U16_STORE:
                script.AppendInstruction(new IRLocalRef(ip, opcode.GetU16Operand(inst)));
                if (opcode is OpcodeV16.LOCAL_U16_LOAD) goto case OpcodeV16.LOAD;
                if (opcode is OpcodeV16.LOCAL_U16_STORE) goto case OpcodeV16.STORE;
                break;
            case OpcodeV16.STATIC_U16:
            case OpcodeV16.STATIC_U16_LOAD:
            case OpcodeV16.STATIC_U16_STORE:
                script.AppendInstruction(new IRStaticRef(ip, opcode.GetU16Operand(inst)));
                if (opcode is OpcodeV16.STATIC_U16_LOAD) goto case OpcodeV16.LOAD;
                if (opcode is OpcodeV16.STATIC_U16_STORE) goto case OpcodeV16.STORE;
                break;
            case OpcodeV16.GLOBAL_U16:
            case OpcodeV16.GLOBAL_U16_LOAD:
            case OpcodeV16.GLOBAL_U16_STORE:
                script.AppendInstruction(new IRGlobalRef(ip, opcode.GetU16Operand(inst)));
                if (opcode is OpcodeV16.GLOBAL_U16_LOAD) goto case OpcodeV16.LOAD;
                if (opcode is OpcodeV16.GLOBAL_U16_STORE) goto case OpcodeV16.STORE;
                break;

            case OpcodeV16.STATIC_U24:
            case OpcodeV16.STATIC_U24_LOAD:
            case OpcodeV16.STATIC_U24_STORE:
                script.AppendInstruction(new IRStaticRef(ip, (int)opcode.GetU24Operand(inst)));
                if (opcode is OpcodeV16.STATIC_U24_LOAD) goto case OpcodeV16.LOAD;
                if (opcode is OpcodeV16.STATIC_U24_STORE) goto case OpcodeV16.STORE;
                break;
            case OpcodeV16.GLOBAL_U24:
            case OpcodeV16.GLOBAL_U24_LOAD:
            case OpcodeV16.GLOBAL_U24_STORE:
                script.AppendInstruction(new IRGlobalRef(ip, (int)opcode.GetU24Operand(inst)));
                if (opcode is OpcodeV16.GLOBAL_U24_LOAD) goto case OpcodeV16.LOAD;
                if (opcode is OpcodeV16.GLOBAL_U24_STORE) goto case OpcodeV16.STORE;
                break;

            case OpcodeV16.ARRAY_U8:
            case OpcodeV16.ARRAY_U8_LOAD:
            case OpcodeV16.ARRAY_U8_STORE:
                script.AppendInstruction(new IRArrayItemRef(ip, opcode.GetU8Operand(inst)));
                if (opcode is OpcodeV16.ARRAY_U8_LOAD) goto case OpcodeV16.LOAD;
                if (opcode is OpcodeV16.ARRAY_U8_STORE) goto case OpcodeV16.STORE;
                break;
            case OpcodeV16.ARRAY_U16:
            case OpcodeV16.ARRAY_U16_LOAD:
            case OpcodeV16.ARRAY_U16_STORE:
                script.AppendInstruction(new IRArrayItemRef(ip, opcode.GetU16Operand(inst)));
                if (opcode is OpcodeV16.ARRAY_U16_LOAD) goto case OpcodeV16.LOAD;
                if (opcode is OpcodeV16.ARRAY_U16_STORE) goto case OpcodeV16.STORE;
                break;

            case OpcodeV16.TEXT_LABEL_ASSIGN_STRING: script.AppendInstruction(new IRTextLabelAssignString(ip, opcode.GetTextLabelLength(inst))); break;
            case OpcodeV16.TEXT_LABEL_ASSIGN_INT: script.AppendInstruction(new IRTextLabelAssignInt(ip, opcode.GetTextLabelLength(inst))); break;
            case OpcodeV16.TEXT_LABEL_APPEND_STRING: script.AppendInstruction(new IRTextLabelAppendString(ip, opcode.GetTextLabelLength(inst))); break;
            case OpcodeV16.TEXT_LABEL_APPEND_INT: script.AppendInstruction(new IRTextLabelAppendInt(ip, opcode.GetTextLabelLength(inst))); break;
            case OpcodeV16.TEXT_LABEL_COPY: script.AppendInstruction(new IRTextLabelCopy(ip)); break;
            case OpcodeV16.CALLINDIRECT: script.AppendInstruction(new IRCallIndirect(ip)); break;
            case OpcodeV16.PUSH_CONST_M1:
                script.AppendInstruction(new IRPushInt(ip, -1));
                break;
            case OpcodeV16.PUSH_CONST_0:
                script.AppendInstruction(new IRPushInt(ip, 0));
                break;
            case OpcodeV16.PUSH_CONST_1:
                script.AppendInstruction(new IRPushInt(ip, 1));
                break;
            case OpcodeV16.PUSH_CONST_2:
                script.AppendInstruction(new IRPushInt(ip, 2));
                break;
            case OpcodeV16.PUSH_CONST_3:
                script.AppendInstruction(new IRPushInt(ip, 3));
                break;
            case OpcodeV16.PUSH_CONST_4:
                script.AppendInstruction(new IRPushInt(ip, 4));
                break;
            case OpcodeV16.PUSH_CONST_5:
                script.AppendInstruction(new IRPushInt(ip, 5));
                break;
            case OpcodeV16.PUSH_CONST_6:
                script.AppendInstruction(new IRPushInt(ip, 6));
                break;
            case OpcodeV16.PUSH_CONST_7:
                script.AppendInstruction(new IRPushInt(ip, 7));
                break;
            case OpcodeV16.PUSH_CONST_FM1:
                script.AppendInstruction(new IRPushFloat(ip, -1.0f));
                break;
            case OpcodeV16.PUSH_CONST_F0:
                script.AppendInstruction(new IRPushFloat(ip, 0.0f));
                break;
            case OpcodeV16.PUSH_CONST_F1:
                script.AppendInstruction(new IRPushFloat(ip, 1.0f));
                break;
            case OpcodeV16.PUSH_CONST_F2:
                script.AppendInstruction(new IRPushFloat(ip, 2.0f));
                break;
            case OpcodeV16.PUSH_CONST_F3:
                script.AppendInstruction(new IRPushFloat(ip, 3.0f));
                break;
            case OpcodeV16.PUSH_CONST_F4:
                script.AppendInstruction(new IRPushFloat(ip, 4.0f));
                break;
            case OpcodeV16.PUSH_CONST_F5:
                script.AppendInstruction(new IRPushFloat(ip, 5.0f));
                break;
            case OpcodeV16.PUSH_CONST_F6:
                script.AppendInstruction(new IRPushFloat(ip, 6.0f));
                break;
            case OpcodeV16.PUSH_CONST_F7:
                script.AppendInstruction(new IRPushFloat(ip, 7.0f));
                break;

            case OpcodeV16.CATCH: script.AppendInstruction(new IRCatch(ip)); break;
            case OpcodeV16.THROW: script.AppendInstruction(new IRThrow(ip)); break;

            default:
                throw new NotImplementedException(opcode.ToString());
        }
    }
}
