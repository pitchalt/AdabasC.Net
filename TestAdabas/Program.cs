using System;
//using System.Collections.Generic;
using System.Text;
using IntecoAG.AdabasC;
using System.Runtime.InteropServices;

namespace TestAdabas
{
    class TestRecord : IRecord {
        Int32 m_isn;
        String m_id;
        String m_first;
        String m_midle;
        String m_last;
        //
        static Byte[] m_fb = {
            (byte) ANSIChars.Let_A, 
            (byte) ANSIChars.Let_A, 
            (byte) ANSIChars.Let_Comma, 
            (byte) ANSIChars.Let_A, 
            (byte) ANSIChars.Let_B, 
            (byte) ANSIChars.Let_Dot
        };
        static Encoding m_enc = Encoding.GetEncoding(0);
        //
        public TestRecord() 
        { 
        }
        //
        public new String ToString()
        {
            return "TestRecord : isn(" + m_isn + ") id:" + m_id + ">" + m_first + m_midle + m_last;
        }
        //
        public void Read(BufferRecord rb) 
        {
            m_id = rb.ReadANSIString(8, m_enc);
            m_first = rb.ReadANSIString(20, m_enc);
            m_midle = rb.ReadANSIString(20, m_enc);
            m_last = rb.ReadANSIString(20, m_enc);
        }
        public Int32 Isn 
        {
            get 
            {
                return m_isn;
            }
            set
            {
                m_isn = value;
            }
        }
        public Byte[] FormatBuffer 
        {
            get 
            {
                return m_fb;
            }
        }
    }
    //
    class Program
    {
        static void TestReadPhysicalMF()
        {
            int i;
            int count = 0;
            DateTime d1;
            DateTime d2;
            //
            BufferRecord br = new BufferRecord();
            File<TestRecord> af = new File<TestRecord>(12, 11);
            //
            d1 = DateTime.Now;
            for (i = 0; i < 500; i++)
                foreach (TestRecord tr in af) 
                {
                    count++;
//                Console.WriteLine(tr.ToString());
                }
            //
            d2 = DateTime.Now;
            System.Console.WriteLine("Read count: " + count);
            System.Console.WriteLine(d1 + " pass: " + i + " " + d2);
        }
        static void TestBufferPack() 
        {
            int i;
            DateTime d1;
            DateTime d2;
            BufferRecord br = new BufferRecord();
            br.m_buffer[0] = 0x98;
            br.m_buffer[1] = 0x76;
            br.m_buffer[2] = 0x54;
            br.m_buffer[3] = 0x32;
            br.m_buffer[4] = 0x10;
            br.m_buffer[5] = 0x98;
            br.m_buffer[6] = 0x76;
            br.m_buffer[7] = 0x54;
            br.m_buffer[8] = 0x32;
            br.m_buffer[9] = 0x10;
            br.m_buffer[10] = 0x98;
            br.m_buffer[11] = 0x76;
            br.m_buffer[12] = 0x54;
            br.m_buffer[13] = 0x3C;
            System.Console.WriteLine(br.ReadPackDecimal(14, 0));
            //
            br = new BufferRecord();
            br.m_buffer[0] = 0x98;
            br.m_buffer[1] = 0x76;
            br.m_buffer[2] = 0x54;
            br.m_buffer[3] = 0x32;
            br.m_buffer[4] = 0x10;
            br.m_buffer[5] = 0x98;
            br.m_buffer[6] = 0x76;
            br.m_buffer[7] = 0x54;
            br.m_buffer[8] = 0x32;
            br.m_buffer[9] = 0x10;
            br.m_buffer[10] = 0x98;
            br.m_buffer[11] = 0x76;
            br.m_buffer[12] = 0x54;
            br.m_buffer[13] = 0x3D;
            System.Console.WriteLine(br.ReadPackDecimal(14, 0));
            //
            br = new BufferRecord();
            br.m_buffer[0] = 0x98;
            br.m_buffer[1] = 0x76;
            br.m_buffer[2] = 0x54;
            br.m_buffer[3] = 0x32;
            br.m_buffer[4] = 0x10;
            br.m_buffer[5] = 0x98;
            br.m_buffer[6] = 0x76;
            br.m_buffer[7] = 0x54;
            br.m_buffer[8] = 0x32;
            br.m_buffer[9] = 0x10;
            br.m_buffer[10] = 0x98;
            br.m_buffer[11] = 0x76;
            br.m_buffer[12] = 0x54;
            br.m_buffer[13] = 0x3D;
            System.Console.WriteLine(br.ReadPackDecimal(14, 7));
            //
            Decimal lr = 0;
            Int32 count1 = 100000000;
            //
            br.m_buffer[11] = 0x7C;
            d1 = DateTime.Now;
            for (i = 0; i < count1; i++)
                lr = br.ReadPackDecimal(12, 7);
            d2 = DateTime.Now;
            System.Console.WriteLine("Result: " + lr);
            System.Console.WriteLine(d1 + " pass: " + i + " " + d2);
            //
            br.m_buffer[7] = 0x5D;
            d1 = DateTime.Now;
            for (i = 0; i < count1; i++)
                lr = br.ReadPackDecimal(8, 7);
            d2 = DateTime.Now;
            System.Console.WriteLine("Result: " + lr);
            System.Console.WriteLine(d1 + " pass: " + i + " " + d2);
            //
            br.m_buffer[3] = 0x3C;
            d1 = DateTime.Now;
            for (i = 0; i < count1; i++)
                lr = br.ReadPackDecimal(4, 7);
            d2 = DateTime.Now;
            System.Console.WriteLine("Result: " + lr);
            System.Console.WriteLine(d1 + " pass: " + i + " " + d2);
            //
            br = new BufferRecord();
            br.m_buffer[0] = (Byte)ANSIChars.Let_9;
            br.m_buffer[1] = (Byte)ANSIChars.Let_8;
            br.m_buffer[2] = (Byte)ANSIChars.Let_7;
            br.m_buffer[3] = (Byte)ANSIChars.Let_6;
            br.m_buffer[4] = (Byte)ANSIChars.Let_5;
            br.m_buffer[5] = (Byte)ANSIChars.Let_4;
            br.m_buffer[6] = (Byte)ANSIChars.Let_3;
            br.m_buffer[7] = (Byte)ANSIChars.Let_2;
            br.m_buffer[8] = (Byte)ANSIChars.Let_1;
            br.m_buffer[9] = (Byte)ANSIChars.Let_0;
            br.m_buffer[10] = (Byte)ANSIChars.Let_9;
            br.m_buffer[11] = (Byte)ANSIChars.Let_8;
            br.m_buffer[12] = (Byte)ANSIChars.Let_7;
            br.m_buffer[13] = (Byte)ANSIChars.Let_6;
            br.m_buffer[14] = (Byte)ANSIChars.Let_5;
            br.m_buffer[15] = (Byte)ANSIChars.Let_4;
            br.m_buffer[16] = (Byte)ANSIChars.Let_3;
            br.m_buffer[17] = (Byte)ANSIChars.Let_2;
            br.m_buffer[18] = (Byte)ANSIChars.Let_1;
            br.m_buffer[19] = (Byte)ANSIChars.Let_0;
            br.m_buffer[20] = (Byte)ANSIChars.Let_9;
            br.m_buffer[21] = (Byte)ANSIChars.Let_8;
            br.m_buffer[22] = (Byte)ANSIChars.Let_7;
            br.m_buffer[23] = (Byte)ANSIChars.Let_6;
            br.m_buffer[24] = (Byte)ANSIChars.Let_5;
            br.m_buffer[25] = (Byte)ANSIChars.Let_4;
            br.m_buffer[26] = (Byte)ANSIChars.Let_3;
            System.Console.WriteLine(br.ReadUnPackDecimal(27, 7));
            //
            br = new BufferRecord();
            br.m_buffer[0] = (Byte)ANSIChars.Let_9;
            br.m_buffer[1] = (Byte)ANSIChars.Let_8;
            br.m_buffer[2] = (Byte)ANSIChars.Let_7;
            br.m_buffer[3] = (Byte)ANSIChars.Let_6;
            br.m_buffer[4] = (Byte)ANSIChars.Let_5;
            br.m_buffer[5] = (Byte)ANSIChars.Let_4;
            br.m_buffer[6] = (Byte)ANSIChars.Let_3;
            br.m_buffer[7] = (Byte)ANSIChars.Let_2;
            br.m_buffer[8] = (Byte)ANSIChars.Let_1;
            br.m_buffer[9] = (Byte)ANSIChars.Let_0;
            br.m_buffer[10] = (Byte)ANSIChars.Let_9;
            br.m_buffer[11] = (Byte)ANSIChars.Let_8;
            br.m_buffer[12] = (Byte)ANSIChars.Let_7;
            br.m_buffer[13] = (Byte)ANSIChars.Let_6;
            br.m_buffer[14] = (Byte)ANSIChars.Let_5;
            br.m_buffer[15] = (Byte)ANSIChars.Let_4;
            br.m_buffer[16] = (Byte)ANSIChars.Let_3;
            br.m_buffer[17] = (Byte)ANSIChars.Let_2;
            br.m_buffer[18] = (Byte)ANSIChars.Let_1;
            br.m_buffer[19] = (Byte)ANSIChars.Let_0;
            br.m_buffer[20] = (Byte)ANSIChars.Let_9;
            br.m_buffer[21] = (Byte)ANSIChars.Let_8;
            br.m_buffer[22] = (Byte)ANSIChars.Let_7;
            br.m_buffer[23] = (Byte)ANSIChars.Let_6;
            br.m_buffer[24] = (Byte)ANSIChars.Let_5;
            br.m_buffer[25] = (Byte)ANSIChars.Let_4;
            br.m_buffer[26] = (Byte)0x73;
            System.Console.WriteLine(br.ReadUnPackDecimal(27, 7));
        }
        static void Main(string[] args)
        {
            TestReadPhysicalMF();
            TestBufferPack();
            Console.ReadKey();
        }
    }
}
