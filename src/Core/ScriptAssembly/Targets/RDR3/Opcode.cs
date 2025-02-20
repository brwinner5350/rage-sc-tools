namespace ScTools.ScriptAssembly.Targets.RDR3;

using ScTools.GameFiles;

using System;
using System.Buffers.Binary;
using System.Text;

public enum OpcodeV16 : byte
{
    NOP = 0x13,
    IADD = 0x43,
    ISUB = 0x10,
    IMUL = 0x1E,
    IDIV = 0x59,
    IMOD = 0x47,
    INOT = 0x05,
    INEG = 0x2B,
    IEQ = 0x0B,
    INE = 0x1C,
    IGT = 0x20,
    IGE = 0x7E,
    ILT = 0x35,
    ILE = 0x77,
    FADD = 0x71,
    FSUB = 0x51,
    FMUL = 0x24,
    FDIV = 0x0C,
    FMOD = 0x1B,
    FNEG = 0x65,
    FEQ = 0x7D,
    FNE = 0x57,
    FGT = 0x31,
    FGE = 0x16,
    FLT = 0x34,
    FLE = 0x84,
    VADD = 0x60,
    VSUB = 0x19,
    VMUL = 0x07,
    VDIV = 0x12,
    VNEG = 0x61,
    IAND = 0x69,
    IOR = 0x30,
    IXOR = 0x49,
    I2F = 0x36,
    F2I = 0x45,
    F2V = 0x38,
    PUSH_CONST_U8 = 0x6D,
    PUSH_CONST_U8_U8 = 0x6F,
    PUSH_CONST_U8_U8_U8 = 0x7B,
    PUSH_CONST_U32 = 0x37,
    PUSH_CONST_F = 0x86,
    DUP = 0x6A,
    DROP = 0x41,
    NATIVE = 0x03,
    ENTER = 0x22,
    LEAVE = 0x50,
    LOAD = 0x76,
    STORE = 0x32,
    STORE_REV = 0x3D,
    LOAD_N = 0x2D,
    STORE_N = 0x06,
    ARRAY_U8 = 0x63,
    ARRAY_U8_LOAD = 0x17,
    ARRAY_U8_STORE = 0x64,
    LOCAL_U8 = 0x4B,
    LOCAL_U8_LOAD = 0x66,
    LOCAL_U8_STORE = 0x67,
    STATIC_U8 = 0x89,
    STATIC_U8_LOAD = 0x54,
    STATIC_U8_STORE = 0x4E,
    IADD_U8 = 0x5C,
    IMUL_U8 = 0x14,
    IOFFSET = 0x56,
    IOFFSET_U8 = 0x80,
    IOFFSET_U8_LOAD = 0x27,
    IOFFSET_U8_STORE = 0x6C,
    PUSH_CONST_S16 = 0x25,
    IADD_S16 = 0x3B,
    IMUL_S16 = 0x7F,
    IOFFSET_S16 = 0x18,
    IOFFSET_S16_LOAD = 0x78,
    IOFFSET_S16_STORE = 0x8C,
    ARRAY_U16 = 0x40,
    ARRAY_U16_LOAD = 0x02,
    ARRAY_U16_STORE = 0x0A,
    LOCAL_U16 = 0x58,
    LOCAL_U16_LOAD = 0x01,
    LOCAL_U16_STORE = 0x44,
    STATIC_U16 = 0x46,
    STATIC_U16_LOAD = 0x3A,
    STATIC_U16_STORE = 0x5F,
    GLOBAL_U16 = 0x87,
    GLOBAL_U16_LOAD = 0x70,
    GLOBAL_U16_STORE = 0x4A,
    J = 0x68,
    JZ = 0x8B,
    IEQ_JZ = 0x15,
    INE_JZ = 0x72,
    IGT_JZ = 0x2E,
    IGE_JZ = 0x75,
    ILT_JZ = 0x8A,
    ILE_JZ = 0x23,
    CALL = 0x39,
    GLOBAL_U24 = 0x5D,
    GLOBAL_U24_LOAD = 0x85,
    GLOBAL_U24_STORE = 0x26,
    PUSH_CONST_U24 = 0x21,
    SWITCH = 0x3C,
    STRING = 0x04,
    STRINGHASH = 0x81,
    TEXT_LABEL_ASSIGN_STRING = 0x1F,
    TEXT_LABEL_ASSIGN_INT = 0x79,
    TEXT_LABEL_APPEND_STRING = 0x5E,
    TEXT_LABEL_APPEND_INT = 0x29,
    TEXT_LABEL_COPY = 0x1A,
    CATCH = 0x6E,
    THROW = 0x55,
    CALLINDIRECT = 0x8D,
    PUSH_CONST_M1 = 0x08,
    PUSH_CONST_0 = 0x2F,
    PUSH_CONST_1 = 0x09,
    PUSH_CONST_2 = 0x11,
    PUSH_CONST_3 = 0x1D,
    PUSH_CONST_4 = 0x42,
    PUSH_CONST_5 = 0x62,
    PUSH_CONST_6 = 0x4D,
    PUSH_CONST_7 = 0x0D,
    PUSH_CONST_FM1 = 0x4C,
    PUSH_CONST_F0 = 0x73,
    PUSH_CONST_F1 = 0x48,
    PUSH_CONST_F2 = 0x5B,
    PUSH_CONST_F3 = 0x2C,
    PUSH_CONST_F4 = 0x5A,
    PUSH_CONST_F5 = 0x7C,
    PUSH_CONST_F6 = 0x7A,
    PUSH_CONST_F7 = 0x33,
    LOCAL_LOAD_S = 0x0F,
    LOCAL_STORE_S = 0x00,
    LOCAL_STORE_SR = 0x52,
    STATIC_LOAD_S = 0x0E,
    STATIC_STORE_S = 0x4F,
    STATIC_STORE_SR = 0x74,
    LOAD_N_S = 0x88,
    STORE_N_S = 0x53,
    STORE_N_SR = 0x82,
    GLOBAL_LOAD_S = 0x83,
    GLOBAL_STORE_S = 0x3F,
    GLOBAL_STORE_SR = 0x2A,
    STATIC_U24 = 0x3E,
    STATIC_U24_LOAD = 0x6B,
    STATIC_U24_STORE = 0x28,
}

public static class OpcodeV16Extensions
{
    public static bool IsInvalid(this OpcodeV16 opcode)
        => (byte)opcode >= OpcodeTraitsV16.NumberOfOpcodes;

    public static bool IsJump(this OpcodeV16 opcode)
        => opcode is OpcodeV16.J or OpcodeV16.JZ or
                     OpcodeV16.IEQ_JZ or OpcodeV16.INE_JZ or OpcodeV16.IGT_JZ or
                     OpcodeV16.IGE_JZ or OpcodeV16.ILT_JZ or OpcodeV16.ILE_JZ;

    public static bool IsControlFlow(this OpcodeV16 opcode)
        => opcode.IsJump() ||
           opcode is OpcodeV16.LEAVE or OpcodeV16.CALL or OpcodeV16.SWITCH or OpcodeV16.THROW or OpcodeV16.CALLINDIRECT;

    /// <returns>
    /// The byte size of a instruction with this <paramref name="opcode"/>; or, <c>0</c> if the size is variable (i.e. <paramref name="opcode"/> is <see cref="Opcode.CALL"/> or <see cref="Opcode.SWITCH"/>).
    /// </returns>
    public static int ConstantByteSize(this OpcodeV16 opcode) => OpcodeTraitsV16.ConstantByteSize(opcode);

    public static byte GetU8Operand(this OpcodeV16 opcode, ReadOnlySpan<byte> bytecode, int operandIndex = 0)
    {
        ThrowIfOpcodeDoesNotMatch(opcode, bytecode);

        // TODO: check opcodes with U8 operands
        //if (opcode is Opcode.PUSH_CONST_U8)
        //{
        return bytecode[1 + operandIndex];
        //}
        //else
        //{
        //    throw new ArgumentException($"The opcode {opcode} does not have a U8 operand.", nameof(opcode));
        //}
    }

    public static short GetS16Operand(this OpcodeV16 opcode, ReadOnlySpan<byte> bytecode)
    {
        ThrowIfOpcodeDoesNotMatch(opcode, bytecode);

        // TODO: check opcodes with U16 operands
        //if (opcode is Opcode.PUSH_CONST_U16)
        //{
        return BinaryPrimitives.ReadInt16LittleEndian(bytecode[1..]);
        //}
        //else
        //{
        //    throw new ArgumentException($"The opcode {opcode} does not have a U16 operand.", nameof(opcode));
        //}
    }

    public static ushort GetU16Operand(this OpcodeV16 opcode, ReadOnlySpan<byte> bytecode)
    {
        ThrowIfOpcodeDoesNotMatch(opcode, bytecode);

        // TODO: check opcodes with U16 operands
        //if (opcode is Opcode.PUSH_CONST_U16)
        //{
        return BinaryPrimitives.ReadUInt16LittleEndian(bytecode[1..]);
        //}
        //else
        //{
        //    throw new ArgumentException($"The opcode {opcode} does not have a U16 operand.", nameof(opcode));
        //}
    }

    public static uint GetU24Operand(this OpcodeV16 opcode, ReadOnlySpan<byte> bytecode)
    {
        ThrowIfOpcodeDoesNotMatch(opcode, bytecode);

        // TODO: check opcodes with U24 operands
        //if (opcode is )
        //{                var lo = inst[0];
        var lo = bytecode[1];
        var mi = bytecode[2];
        var hi = bytecode[3];

        return (uint)(hi << 16 | mi << 8 | lo);
        //}
        //else
        //{
        //    throw new ArgumentException($"The opcode {opcode} does not have a U32 operand.", nameof(opcode));
        //}
    }

    public static uint GetU32Operand(this OpcodeV16 opcode, ReadOnlySpan<byte> bytecode)
    {
        ThrowIfOpcodeDoesNotMatch(opcode, bytecode);

        // TODO: check opcodes with U32 operands
        //if (opcode is )
        //{
        return BinaryPrimitives.ReadUInt32LittleEndian(bytecode[1..]);
        //}
        //else
        //{
        //    throw new ArgumentException($"The opcode {opcode} does not have a U32 operand.", nameof(opcode));
        //}
    }

    public static float GetFloatOperand(this OpcodeV16 opcode, ReadOnlySpan<byte> bytecode)
    {
        ThrowIfOpcodeDoesNotMatch(opcode, bytecode);
        if (opcode is not OpcodeV16.PUSH_CONST_F)
        {
            throw new ArgumentException($"The opcode {opcode} does not have a FLOAT operand.", nameof(opcode));
        }

        return BinaryPrimitives.ReadSingleLittleEndian(bytecode[1..]);
    }

    public static int GetSwitchNumberOfCases(this OpcodeV16 opcode, ReadOnlySpan<byte> bytecode)
    {
        ThrowIfOpcodeDoesNotMatch(opcode, bytecode);
        ThrowIfNotExpectedOpcode(OpcodeV16.SWITCH, bytecode);
        return bytecode[1];
    }

    public static SwitchCasesEnumerator GetSwitchOperands(this OpcodeV16 opcode, ReadOnlySpan<byte> bytecode)
    {
        ThrowIfOpcodeDoesNotMatch(opcode, bytecode);
        return new(bytecode);
    }

    public static (byte ParamCount, ushort FrameSize) GetEnterOperands(this OpcodeV16 opcode, ReadOnlySpan<byte> bytecode)
    {
        ThrowIfOpcodeDoesNotMatch(opcode, bytecode);
        ThrowIfNotExpectedOpcode(OpcodeV16.ENTER, bytecode);
        return (bytecode[1], BinaryPrimitives.ReadUInt16LittleEndian(bytecode[2..]));
    }

    public static string? GetEnterFunctionName(this OpcodeV16 opcode, ReadOnlySpan<byte> bytecode)
    {
        ThrowIfOpcodeDoesNotMatch(opcode, bytecode);
        ThrowIfNotExpectedOpcode(OpcodeV16.ENTER, bytecode);

        if (bytecode[4] > 0)
        {
            var nameSlice = bytecode[5..^1];
            while (nameSlice[0] == 0xFF) { nameSlice = nameSlice[1..]; }

            return Encoding.UTF8.GetString(nameSlice);
        }

        return null;
    }

    public static (byte ParamCount, byte ReturnCount) GetLeaveOperands(this OpcodeV16 opcode, ReadOnlySpan<byte> bytecode)
    {
        ThrowIfOpcodeDoesNotMatch(opcode, bytecode);
        ThrowIfNotExpectedOpcode(OpcodeV16.LEAVE, bytecode);
        return (bytecode[1], bytecode[2]);
    }

    public static (byte ParamCount, byte ReturnCount, ushort CommandIndex) GetNativeOperands(this OpcodeV16 opcode, ReadOnlySpan<byte> bytecode)
    {
        ThrowIfOpcodeDoesNotMatch(opcode, bytecode);
        ThrowIfNotExpectedOpcode(OpcodeV16.NATIVE, bytecode);

        var paramReturnCounts = bytecode[0];
        var nativeIndexHi = bytecode[2];
        var nativeIndexLo = bytecode[3];

        var paramCount = paramReturnCounts >> 2 & 0x3F;
        var returnCount = paramReturnCounts & 0x3;
        var nativeIndex = nativeIndexHi << 8 | nativeIndexLo;

        return ((byte)paramCount, (byte)returnCount, (ushort)nativeIndex);
    }

    public static byte GetTextLabelLength(this OpcodeV16 opcode, ReadOnlySpan<byte> bytecode)
    {
        ThrowIfOpcodeDoesNotMatch(opcode, bytecode);
        if ((OpcodeV16)bytecode[0] is not (OpcodeV16.TEXT_LABEL_ASSIGN_STRING or OpcodeV16.TEXT_LABEL_ASSIGN_INT or
                                        OpcodeV16.TEXT_LABEL_APPEND_STRING or OpcodeV16.TEXT_LABEL_APPEND_INT))
        {
            throw new ArgumentException($"The instruction opcode is not a TEXT_LABEL_ASSIGN/APPEND opcode.", nameof(bytecode));
        }
        return bytecode[1];
    }

    internal static void ThrowIfOpcodeDoesNotMatch(OpcodeV16 opcode, ReadOnlySpan<byte> bytecode)
    {
        if ((byte)opcode != bytecode[0])
        {
            throw new ArgumentException($"The opcode {opcode} does not match the bytecode {bytecode[0]:X2}.", nameof(bytecode));
        }
    }

    internal static void ThrowIfNotExpectedOpcode(OpcodeV16 expectedOpcode, ReadOnlySpan<byte> bytecode)
    {
        if (bytecode[0] != (byte)expectedOpcode)
        {
            throw new ArgumentException($"The instruction opcode is not {expectedOpcode}.", nameof(bytecode));
        }
    }

    public readonly record struct SwitchCase(uint Value, short JumpOffset, int CaseIndex)
    {
        public int OffsetWithinInstruction => 2 + CaseIndex * 6;
        public int GetJumpTargetAddress(int switchBaseAddress) => switchBaseAddress + OffsetWithinInstruction + 6 + JumpOffset;
    }

    public ref struct SwitchCasesEnumerator
    {
        private readonly ReadOnlySpan<byte> bytecode;
        private SwitchCase current;
        private int index;

        public SwitchCasesEnumerator(ReadOnlySpan<byte> bytecode)
        {
            ThrowIfNotExpectedOpcode(OpcodeV16.SWITCH, bytecode);

            this.bytecode = bytecode;
            current = default;
            index = 0;
        }

        public SwitchCase Current => current;

        public SwitchCasesEnumerator GetEnumerator() => this;

        public bool MoveNext()
        {
            var numCases = bytecode[1];
            if (index >= numCases)
            {
                current = default;
                return false;
            }

            var caseOffset = 2 + index * 6;
            var caseValue = BinaryPrimitives.ReadUInt32LittleEndian(bytecode[caseOffset..(caseOffset + 4)]);
            var caseJumpOffset = BinaryPrimitives.ReadInt16LittleEndian(bytecode[(caseOffset + 4)..]);

            current = new(caseValue, caseJumpOffset, index);
            index++;
            return true;
        }
    }

    /// <returns>
    /// The number of operands required by <see cref="opcode"/>; or, <c>-1</c> if it accepts a variable number of operands (i.e. <paramref name="opcode"/> is <see cref="OpcodeV16.SWITCH"/>).
    /// </returns>
    public static int NumberOfOperands(this OpcodeV16 opcode)
        => (int)opcode < OpcodeTraitsV16.NumberOfOpcodes ? NumberOfOperandsTable[(int)opcode] : throw new ArgumentException($"Unknown opcode '{opcode}'", nameof(opcode));

    private static readonly sbyte[] NumberOfOperandsTable = new sbyte[OpcodeTraitsV16.NumberOfOpcodes]
    {
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,1,2,3,1,1,0,0,3,2,2,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,0,
        1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
        1,1,1,1,0,0,1,1,1,1,0,0,0,0,1,-1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
    };

}
