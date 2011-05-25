using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace IntecoAG.AdabasC
{
    public class BufferBase
    {
        public Byte[] m_buffer;

        public BufferBase(UInt16 len) 
        {
            m_buffer = new Byte[len];
        }
        public BufferBase(Byte[] buffer)
        {
            m_buffer = buffer;
        }
        public UInt16 Length
        {
            get
            {
                return (UInt16)m_buffer.Length;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MFResult
    {
        public Int32  len;
        public Int32  ret_code;
        public Int32  isn;
        public Int32  index;
    };
    unsafe class BufferMF : BufferBase
    {
        public BufferMF(Int32 count): base((UInt16)(4 + 16 * count)) { }
        //
        public Int32 Count 
        {
            get 
            {
                fixed (Byte* bp = m_buffer ) {
                    return *(Int32 *)bp;
                }
            }
            set 
            {
                fixed (Byte* bp = m_buffer ) {
                    *(Int32 *)bp = value;
                }
            }
        }
        public MFResult this[Int32 index] {
            get
            {
                if (index >= Count || index < 0)
                    throw new IndexOutOfRangeException();
                fixed (Byte* bp = m_buffer)
                {
                    MFResult* rp = (MFResult*)(bp + 4);
                    return rp[index];
                }
            }
        }
    }
    static class PackedByteDecoder { 
        public static UInt32[] m_decoder_table;
        static PackedByteDecoder() 
        { 
            UInt32 i,j;
            m_decoder_table = new UInt32[256];
            for (i = 0; i < 10; i++)
            {
                for (j = 0; j < 10; j++)
                    m_decoder_table[(i << 4) | j] = i * 10 + j;
                m_decoder_table[(i << 4) | 0xC] = i;
                m_decoder_table[(i << 4) | 0xD] = i;
            }
        }
    }
    unsafe public class BufferRecord : BufferBase
    {
        Int32 m_pos;
        //
        public BufferRecord()
            : base(UInt16.MaxValue)
        {
            m_pos = 0;
        }
        public BufferRecord(UInt16 len)
            : base(len)
        {
            m_pos = 0;
        }
        public BufferRecord(Byte[] buffer)
            : base(buffer)
        {
            m_pos = 0;
        }
        //
        public Int32 Position {
            get
            {
                return m_pos;
            }
            set 
            {
                m_pos = value;
            }
        }
        //
        public Int64 ReadInt64()
        {
            fixed (Byte* bp = &m_buffer[m_pos])
            {
                m_pos += 8;
                return *(Int64*)bp;
            }
        }
        public UInt64 ReadUInt64()
        {
            fixed (Byte* bp = &m_buffer[m_pos])
            {
                m_pos += 8;
                return *(UInt64*)bp;
            }
        }
        public Int32 ReadInt32()
        {
            fixed (Byte* bp = &m_buffer[m_pos])
            {
                m_pos += 4;
                return *(Int32*)bp;
            }
        }
        public UInt32 ReadUInt32()
        {
            fixed (Byte* bp = &m_buffer[m_pos])
            {
                m_pos += 4;
                return *(UInt32*)bp;
            }
        }
        public Int16 ReadInt16()
        {
            fixed (Byte* bp = &m_buffer[m_pos])
            {
                m_pos += 2;
                return *(Int16*)bp;
            }
        }
        public UInt16 ReadUInt16()
        {
            fixed (Byte* bp = &m_buffer[m_pos])
            {
                m_pos += 2;
                return *(UInt16*)bp;
            }
        }
        public SByte ReadSByte()
        {
            fixed (Byte* bp = &m_buffer[m_pos])
            {
                m_pos += 1;
                return *(SByte*)bp;
            }
        }
        public Byte ReadByte()
        {
            fixed (Byte* bp = &m_buffer[m_pos])
            {
                m_pos += 1;
                return *(Byte*)bp;
            }
        }
        public Byte[] ReadByteArray(Int32 size)
        {
            Byte[] res = new Byte[size];
            Int32 i;
            fixed (Byte* src = &m_buffer[m_pos],
                         dst = &res[0])
            {
                for (i = 0; i < size; i++)
                    dst[i] = src[i];
            }
            m_pos += size;
            return res;
        }
        public String ReadANSIString(Int32 size, Encoding enc)
        {
            fixed (Byte* bp = &m_buffer[0])
            {
                String res = new String((SByte*)bp, m_pos, size, enc);
                m_pos += size;
                return res;
            }
        }
        public String ReadUnicodeString(Int32 size)
        {
            throw new NotImplementedException();
        }
        public Decimal ReadPackDecimal(Int32 size, Byte scale)
        {
            Int32 i;
            UInt32 hig = 0, mid = 0, low = 0;
            UInt64 irh, irm, irl;
            Boolean sign;
            if (scale > 28)
                throw new ArgumentOutOfRangeException("Scale", scale, "not support > 28");
            fixed (Byte* cp = &m_buffer[m_pos]) 
            {
                fixed (UInt32* dt = &PackedByteDecoder.m_decoder_table[0])
                {
                    for (i = 0; i < size; i++)
                    {
                        if (cp[i] != 0 && dt[cp[i]] == 0)
                            throw new FormatException("Unknow byte: " + dt[cp[i]] + " in position: " + i);
                        if (i == 0)
                            low = dt[cp[0]];
                        else
                        {
                            UInt32 mult;
                            if (i == size - 1)
                                mult = 10;
                            else
                                mult = 100;
                            irl = (UInt64)low * mult + dt[cp[i]];
                            if (mid != 0)
                                irm = (UInt64)mid * mult + (irl >> 32);
                            else
                                irm = irl >> 32;
                            if (hig != 0)
                                irh = (UInt64)hig * mult + (irm >> 32);
                            else
                                irh = irm >> 32;
                            low = (UInt32)(irl & 0x00000000ffffffff);
                            mid = (UInt32)(irm & 0x00000000ffffffff);
                            hig = (UInt32)(irh & 0x00000000ffffffff);
                        }
                    }
                    if ((cp[size - 1] & 0xf) == 0xC)
                        sign = false;
                    else if ((cp[size - 1] & 0xf) == 0xD)
                        sign = true;
                    else
                        throw new FormatException("Unknow byte: " + dt[cp[i]] + " in position: " + i);
                }
            }
            return new Decimal((Int32)low, (Int32)mid, (Int32)hig, sign, scale);
        }
        public Decimal ReadUnPackDecimal(Int32 size, Byte scale)
        {
            Int32 i;
            UInt32 hig = 0, mid = 0, low = 0;
            UInt64 irh, irm, irl;
            Boolean sign;
            UInt32 dig;
            if (scale > 28)
                throw new ArgumentOutOfRangeException("Scale", scale, "not support > 28");
            fixed (Byte* cp = &m_buffer[m_pos])
            {
                sign = false;
                for (i = 0; i < size; i++)
                {
                    if ((cp[i] & 0xf0) == 0x30)
                        dig = (UInt32) cp[i] - 0x30;
                    else
                        if (i == size - 1 && (cp[i] & 0xf0) == 0x70)
                        {
                            sign = true;
                            dig = (UInt32) cp[i] - 0x70;
                        }
                        else
                            throw new FormatException("Unknow byte: " + cp[i] + " in position: " + i);
                    if (dig > 9)
                        throw new FormatException("Unknow byte: " + cp[i] + " in position: " + i);
                    if (i == 0)
                        low = dig;
                    else
                    {
                        irl = (UInt64)low * 10 + dig;
                        if (mid != 0)
                            irm = (UInt64)mid * 10 + (irl >> 32);
                        else
                            irm = irl >> 32;
                        if (hig != 0)
                            irh = (UInt64)hig * 10 + (irm >> 32);
                        else
                            irh = irm >> 32;
                        low = (UInt32)(irl & 0x00000000ffffffff);
                        mid = (UInt32)(irm & 0x00000000ffffffff);
                        hig = (UInt32)(irh & 0x00000000ffffffff);
                    }
                }
            }
            return new Decimal((Int32)low, (Int32)mid, (Int32)hig, sign, scale);
        }
    }
}
