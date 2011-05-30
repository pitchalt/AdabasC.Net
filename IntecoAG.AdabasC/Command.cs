using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace IntecoAG.AdabasC
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ctrl_block
    {
        public byte cb_call_type;
        public byte cb_reserved;
        public byte cb_cmd_code_f;
        public byte cb_cmd_code_l;
        public uint cb_cmd_id;
        public ushort cb_file_nr;
        public ushort cb_return_code;         /* Return Code        */
        public Int32 cb_isn;                 /* ISN                */
        public Int32 cb_isn_ll;              /* ISN Lower Limit    */
        public uint cb_isn_quantity;        /* ISN Quantity       */
        /*
        #define CB_FBID (0)                      ID of format buffer in CB
        #define CB_RBID (1)                      ID of record buffer in CB
        #define CB_SBID (2)                      ID of search buffer in CB
        #define CB_VBID (3)                      ID of value  buffer in CB
        #define CB_IBID (4)                      ID of ISN    buffer in CB
        */
        public ushort cb_fm_buf_lng;          /* I/O Buffer Lengths */
        public ushort cb_rb_buf_lng;          /* I/O Buffer Lengths */
        public ushort cb_sb_buf_lng;          /* I/O Buffer Lengths */
        public ushort cb_vb_buf_lng;          /* I/O Buffer Lengths */
        public ushort cb_ib_buf_lng;          /* I/O Buffer Lengths */

        public byte cb_cop1;                  /* Command Option 1 */
        public byte cb_cop2;                  /* Command Option 2 */

        public ulong cb_add1;        /* Addition 1 field */
        public uint  cb_add2;        /* Addition 2 field */
        public ulong cb_add3;        /* Addition 3 field */
        public ulong cb_add4;        /* Addition 4 field */
        public ulong cb_add5;        /* Addition 5 field */

        public uint cb_cmd_time;                  /* Command Time */

        public uint cb_user_area;              /* User Area    */
    };
    //
    internal static class AdaLnkX {
        [DllImport("adalnkx.dll")]
        internal static extern int adabas(ref ctrl_block cb, IntPtr fb, IntPtr rb, IntPtr sb, IntPtr vb, IntPtr ib);
    }
    //
    public class AdabasException : Exception 
    {
        public AdabasException(Int32 ret_code) 
            : base("Adabas response: " + ret_code)
        {
        }
    }
    //
    public unsafe class CommandBase
    {
        protected ushort dbid;
        internal ctrl_block m_cb;
        //
        public CommandBase(ushort dbid, ushort file)
        {
            this.dbid = dbid;
            m_cb.cb_call_type = 0x30;
            m_cb.cb_return_code = dbid;
            m_cb.cb_file_nr = file;
        }
        protected int CallAdabas(BufferBase fb, BufferBase rb, BufferBase sb, BufferBase vb, BufferBase ib)
        {
            m_cb.cb_return_code = dbid;
            fixed (Byte* fbp = fb.m_buffer,
                         rbp = rb.m_buffer,
                         sbp = sb.m_buffer,
                         vbp = vb.m_buffer,
                         ibp = ib.m_buffer)
            {
                return AdaLnkX.adabas(ref this.m_cb, (IntPtr)fbp, (IntPtr)rbp, (IntPtr)sbp, (IntPtr)vbp, (IntPtr)ibp);
            }
        }
        public int cmd_OP()
        {
            m_cb.cb_return_code = dbid;
            m_cb.cb_cmd_code_f = (byte)ANSIChars.Let_O;
            m_cb.cb_cmd_code_l = (byte)ANSIChars.Let_P;
            m_cb.cb_fm_buf_lng = 0;
            m_cb.cb_rb_buf_lng = 0;
            AdaLnkX.adabas(ref this.m_cb, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            return m_cb.cb_return_code;
        }
        public int cmd_CL()
        {
            m_cb.cb_return_code = dbid;
            m_cb.cb_cmd_code_f = (byte)ANSIChars.Let_C;
            m_cb.cb_cmd_code_l = (byte)ANSIChars.Let_L;
            m_cb.cb_fm_buf_lng = 0;
            m_cb.cb_rb_buf_lng = 0;
            AdaLnkX.adabas(ref this.m_cb, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            return m_cb.cb_return_code;
        }
    }
    //
    public interface IRecord
    {
        Int32 Isn
        {
            get;
            set;
        }
        Byte[] FormatBuffer
        {
            get;
        }
        void Read(BufferRecord buffer);
    }
    //
    abstract public class CommandReadPhysical<T> : CommandBase, IEnumerator<T>
        where T : IRecord, new()
    {
        protected T m_current;
        protected BufferBase m_fb;
        protected BufferRecord m_rb;
        protected Boolean m_is_first;
        //
        public CommandReadPhysical(UInt16 dbid, UInt16 file)
            : base(dbid, file)
        {
            m_current = new T();
            m_cb.cb_cmd_code_f = (byte)ANSIChars.Let_L;
            m_cb.cb_cmd_code_l = (byte)ANSIChars.Let_2;
            m_cb.cb_cmd_id = 0xffffffff;
            m_fb = new BufferBase(m_current.FormatBuffer);
            m_cb.cb_fm_buf_lng = m_fb.Length;
            m_rb = new BufferRecord();
            m_cb.cb_rb_buf_lng = m_rb.Length;
            Reset();
        }
        public T Current
        {
            get
            {
                if (m_is_first)
                    throw new InvalidOperationException("Invalid sequence use MoveNext first");
                return m_current;
            }
        }
        Object IEnumerator.Current
        {
            get
            {
                if (m_is_first)
                    throw new InvalidOperationException("Invalid sequence use MoveNext first");
                return m_current;
            }
        }
        public void Reset()
        {
            m_cb.cb_isn = 0;
            m_is_first = true;
        }
        public Boolean MoveNext()
        {
            return false;
        }
        public void Dispose()
        {
        }
    }
    public unsafe class CommandReadPhysicalMF<T> : CommandReadPhysical<T>, IEnumerator<T>
        where T : IRecord, new()
    {
        BufferMF m_ib;
        Int32 m_multi_fetch;
        Int32 m_record;
        Int32 m_pos;
        Boolean m_is_read_required;
        //
        public CommandReadPhysicalMF(UInt16 dbid, UInt16 file, UInt16 multi_fetch)
            : base(dbid, file)
        {
            m_multi_fetch = multi_fetch;
            m_ib = new BufferMF(multi_fetch);
            m_cb.cb_ib_buf_lng = m_ib.Length;
            m_cb.cb_cop1 = (byte)ANSIChars.Let_M;
            Reset();
        }
        public new void Reset()
        {
            base.Reset();
            m_cb.cb_isn_ll = m_multi_fetch;
            m_pos = 0;
            m_record = 0;
            m_is_read_required = true;
        }
        public new Boolean MoveNext()
        {
            MFResult mr;
            if (m_is_read_required) {
                m_cb.cb_return_code = dbid;
                fixed (Byte* fbp = m_fb.m_buffer,
                             rbp = m_rb.m_buffer,
                             ibp = m_ib.m_buffer)
                {
                    m_cb.cb_call_type = 0x30;
                    AdaLnkX.adabas(ref this.m_cb, (IntPtr)fbp, (IntPtr)rbp, IntPtr.Zero, IntPtr.Zero, (IntPtr)ibp);
                }
                if (m_cb.cb_return_code == 3)
                    return false;
                m_pos = 0;
                m_record = 0;
                m_is_read_required = false;
                if (m_cb.cb_return_code != 0)
                    throw new AdabasException(m_cb.cb_return_code);
            }
            if (m_record >= m_ib.Count)
                return false;
            //
            mr = m_ib[m_record];
            if (mr.ret_code == 3) 
                return false;
            if (mr.ret_code!= 0)
                throw new AdabasException(m_cb.cb_return_code);
            m_rb.Position = m_pos;
            m_current.Read(m_rb);
            m_current.Isn = mr.isn;
            //
            m_record++;
            m_pos += mr.len;
            if (m_record == m_ib.Count)
            {
                m_pos = 0;
                m_record = 0;
                m_is_read_required = true;
            }
            m_is_first = false;
            return true;
        }
    }
        //
    public class File<T> : IEnumerable<T>
            where T : IRecord, new()
    {
        UInt16 m_dbid;
        UInt16 m_file;
        //
        public File(UInt16 dbid, UInt16 file)
        {
            m_dbid = dbid;
            m_file = file;
        }
        public IEnumerator<T> ReadPhysical(Int32 multi_fetch )
        {
            if (multi_fetch != 0 )
                return new CommandReadPhysicalMF<T>(m_dbid, m_file, (UInt16) multi_fetch);
            else
                return new CommandReadPhysicalMF<T>(m_dbid, m_file, 1);
        }
        //
        public IEnumerator<T> GetEnumerator()
        {
            return this.ReadPhysical(100);
        }
        //
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.ReadPhysical(100);
        }
    }
}
