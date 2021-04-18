using DotNetty.Buffers;
using Nuctech.NIS.ByteStream.Serializer;
using Nuctech.NIS.Service.DeviceAccess.ETD;
using NUnit.Framework;
using System;
using System.IO;
using System.Text;

namespace ByteStreamTest
{
    public class SimpleDataTypeTest7
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test_Package_simple_Type()
        {
            PooledByteBufferAllocator pbba = new PooledByteBufferAllocator();
            IByteBuffer bf = pbba.CompositeBuffer();

            bf.WriteByte(0xef);
            bf.WriteByte(0xef); // 超始标识
            bf.SetUnsignedInt(bf.WriterIndex, 2000);// 数据包ID
            bf.SetWriterIndex(bf.WriterIndex + sizeof(UInt32));
            bf.WriteUnsignedShort((ushort)CommandIdentifier.Hello);// 命令标识
            bf.WriteInt(6);     // 数据长度
            bf.WriteUnsignedShort(2); // 内容标识
            bf.WriteInt(0);           // 内容长度
            bf.WriteUnsignedShort(20);

            TRPackage ar = ByteStreamToObjectConverter.Deserialize<TRPackage>(bf);

            Assert.AreEqual(ar.delimiter, 0xefef);
            Assert.AreEqual(ar.packageid, 2000);
            Assert.AreEqual(ar.identifier, (ushort)CommandIdentifier.Hello);
            Assert.AreEqual(ar.data_len, 6);
            Assert.AreEqual(ar.content_identifier, 2);
            Assert.AreEqual(ar.content_len, 0);
            Assert.AreEqual(ar.content_obj, null);
            //System.Console.ReadLine();
        }

        [Test]
        public void Test_content_maintaince_string()
        {
            PooledByteBufferAllocator pbba = new PooledByteBufferAllocator();
            IByteBuffer bf = pbba.CompositeBuffer();

            bf.WriteByte(0xef);// 1
            bf.WriteByte(0xef); // 超始标识 1
            bf.SetUnsignedInt(bf.WriterIndex, 2000);// 数据包ID 4
            bf.SetWriterIndex(bf.WriterIndex + sizeof(UInt32)); 
            bf.WriteUnsignedShort((ushort)CommandIdentifier.Report);// 命令标识 2
            bf.WriteInt(6);     // 数据长度 4
            bf.WriteUnsignedShort((ushort)PackageDataIdnetifier.Maintenance); // 内容标识 2
            bf.WriteInt(100);           // 内容长度 4
            ////////////////写入内容/////////////////////
            bf.WriteByte(0x01); // 1

            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms, encoding: Encoding.ASCII);
            string bstring;
            bstring = "codes";
            bw.Write(bstring);
            ms.Seek(0, SeekOrigin.Begin);

            // Read the string as raw bytes using FileStream...
            // The first series of bytes is the UTF7 encoded length of the
            // string. In this case, however, it is just the first two bytes.
            int len = ms.ReadByte() & 0x7f;
            len += ms.ReadByte() & 0x80;

            byte[] code_str = new byte[len];
            ms.Read(code_str, 0, len);

            ms.Seek(0, SeekOrigin.Begin);
            bf.WriteByte(ms.ReadByte());
            bf.WriteByte(ms.ReadByte());



            string convertred = Encoding.ASCII.GetString(code_str);


            bf.WriteBytes(code_str);


            ms.Seek(0, SeekOrigin.Begin);
            bf.WriteByte(ms.ReadByte());
            bf.WriteByte(ms.ReadByte());
            bf.WriteBytes(code_str);

            ms.Close();
            ////////////////写入内容/////////////////////
            bf.WriteUnsignedShort(20); //2

            TRPackage ar = ByteStreamToObjectConverter.Deserialize<TRPackage>(bf);

            Assert.AreEqual(ar.delimiter, 0xefef);
            Assert.AreEqual(ar.packageid, 2000);
            Assert.AreEqual(ar.identifier, (ushort)CommandIdentifier.Report);
            Assert.AreEqual(ar.data_len, 6);
            Assert.AreEqual(ar.content_identifier, (ushort)PackageDataIdnetifier.Maintenance);
            Assert.AreEqual(ar.content_len, 100);
            Assert.AreEqual(ar.content_obj.GetType(), typeof(TRCloudMaintenance));
            Assert.AreEqual(20,ar.xor_check_code);
            //System.Console.ReadLine();
        }

        [Test]
        public void Test_content_diagnose_string()
        {
            PooledByteBufferAllocator pbba = new PooledByteBufferAllocator();
            IByteBuffer bf = pbba.CompositeBuffer();

            bf.WriteByte(0xef);// 1
            bf.WriteByte(0xef); // 超始标识 1
            bf.SetUnsignedInt(bf.WriterIndex, 2000);// 数据包ID 4
            bf.SetWriterIndex(bf.WriterIndex + sizeof(UInt32));
            bf.WriteUnsignedShort((ushort)CommandIdentifier.Report);// 命令标识 2
            bf.WriteInt(6);     // 数据长度 4
            bf.WriteUnsignedShort((ushort)PackageDataIdnetifier.Diagnosis); // 内容标识 2
            bf.WriteInt(100);           // 内容长度 4
            ////////////////写入内容/////////////////////
            bf.WriteByte(0x01); // 1

            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms, encoding: Encoding.ASCII);
            string bstring;
            bstring = "codes";
            bw.Write(bstring);
            ms.Seek(0, SeekOrigin.Begin);

            // Read the string as raw bytes using FileStream...
            // The first series of bytes is the UTF7 encoded length of the
            // string. In this case, however, it is just the first two bytes.
            int len = ms.ReadByte() & 0x7f;
            len += ms.ReadByte() & 0x80;

            byte[] code_str = new byte[len];
            ms.Read(code_str, 0, len);

            ms.Seek(0, SeekOrigin.Begin);
            bf.WriteByte(ms.ReadByte());
            bf.WriteByte(ms.ReadByte());



            string convertred = Encoding.ASCII.GetString(code_str);


            bf.WriteBytes(code_str);


            ms.Seek(0, SeekOrigin.Begin);
            bf.WriteByte(ms.ReadByte());
            bf.WriteByte(ms.ReadByte());
            bf.WriteBytes(code_str);

            ms.Close();
            ////////////////写入内容/////////////////////
            bf.WriteUnsignedShort(20); //2

            TRPackage ar = ByteStreamToObjectConverter.Deserialize<TRPackage>(bf);

            Assert.AreEqual(ar.delimiter, 0xefef);
            Assert.AreEqual(ar.packageid, 2000);
            Assert.AreEqual(ar.identifier, (ushort)CommandIdentifier.Report);
            Assert.AreEqual(ar.data_len, 6);
            Assert.AreEqual(ar.content_identifier, (ushort)PackageDataIdnetifier.Diagnosis);
            Assert.AreEqual(ar.content_len, 100);
            Assert.AreEqual(ar.content_obj.GetType(), typeof(TRCloudDiagnose));
            Assert.AreEqual(20, ar.xor_check_code);
            //System.Console.ReadLine();
        }
        [Test]
        public void Test_content_analysis_result_list()
        {
            PooledByteBufferAllocator pbba = new PooledByteBufferAllocator();
            IByteBuffer bf = pbba.CompositeBuffer();

            bf.WriteByte(0xef);// 1
            bf.WriteByte(0xef); // 超始标识 1
            bf.SetUnsignedInt(bf.WriterIndex, 2000);// 数据包ID 4
            bf.SetWriterIndex(bf.WriterIndex + sizeof(UInt32));
            bf.WriteUnsignedShort((ushort)CommandIdentifier.Report);// 命令标识 2
            bf.WriteInt(6);     // 数据长度 4
            bf.WriteUnsignedShort((ushort)PackageDataIdnetifier.AnalyseResults); // 内容标识 2
            bf.WriteInt(100);           // 内容长度 4
            ////////////////写入内容/////////////////////
            bf.WriteInt(23); // recordid
            bf.WriteInt(24); // type
            bf.WriteLong(25); // time
            bf.WriteInt(26); // analysiscount
            bf.WriteInt(27); // alarmcount
            bf.WriteBoolean(true); // isalarm


            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms, encoding: Encoding.ASCII);
            string bstring;
            bstring = "codes";
            bw.Write(bstring);
            ms.Seek(0, SeekOrigin.Begin);

            // Read the string as raw bytes using FileStream...
            // The first series of bytes is the UTF7 encoded length of the
            // string. In this case, however, it is just the first two bytes.
            int len = ms.ReadByte() & 0x7f;
            len += ms.ReadByte() & 0x80;

            byte[] code_str = new byte[len];
            ms.Read(code_str, 0, len);

            ms.Seek(0, SeekOrigin.Begin);
            bf.WriteByte(ms.ReadByte());
            bf.WriteByte(ms.ReadByte());
            bf.WriteBytes(code_str); // softversion


            ms.Seek(0, SeekOrigin.Begin);
            bf.WriteByte(ms.ReadByte());
            bf.WriteByte(ms.ReadByte());
            bf.WriteBytes(code_str);// algversion

            ms.Seek(0, SeekOrigin.Begin);
            bf.WriteByte(ms.ReadByte());
            bf.WriteByte(ms.ReadByte());
            bf.WriteBytes(code_str);// libversion


            ms.Seek(0, SeekOrigin.Begin);
            bf.WriteByte(ms.ReadByte());
            bf.WriteByte(ms.ReadByte());
            bf.WriteBytes(code_str);// username


            // alarm result
            bf.WriteInt(1); // count

            // alarm info
            bf.WriteInt(28);// masstype


            ms.Seek(0, SeekOrigin.Begin);
            bf.WriteByte(ms.ReadByte());
            bf.WriteByte(ms.ReadByte());
            bf.WriteBytes(code_str);//  massname
            bf.WriteFloat(29); // identensity

            bf.WriteInt(30);

            bf.WriteFloat(31);
            bf.WriteFloat(32);
            bf.WriteFloat(33);
            bf.WriteFloat(34);
            bf.WriteFloat(35);


            bf.WriteFloat(36);
            bf.WriteFloat(37);
            bf.WriteFloat(38);
            bf.WriteFloat(39);
            bf.WriteFloat(40);



            ms.Close();
            ////////////////写入内容/////////////////////
            bf.WriteUnsignedShort(20); //2

            TRPackage ar = ByteStreamToObjectConverter.Deserialize<TRPackage>(bf);

            Assert.AreEqual(ar.delimiter, 0xefef);
            Assert.AreEqual(ar.packageid, 2000);
            Assert.AreEqual(ar.identifier, (ushort)CommandIdentifier.Report);
            Assert.AreEqual(ar.data_len, 6);
            Assert.AreEqual(ar.content_identifier, (ushort)PackageDataIdnetifier.AnalyseResults);
            Assert.AreEqual(ar.content_len, 100);
            Assert.AreEqual(ar.content_obj.GetType(), typeof(TRCloudAnalyseResult));
            Assert.AreEqual(20, ar.xor_check_code);
            //System.Console.ReadLine();
        }




        [Test]
        public void Test_content_workstatus()
        {
            PooledByteBufferAllocator pbba = new PooledByteBufferAllocator();
            IByteBuffer bf = pbba.CompositeBuffer();

            bf.WriteByte(0xef);// 1
            bf.WriteByte(0xef); // 超始标识 1
            bf.SetUnsignedInt(bf.WriterIndex, 2000);// 数据包ID 4
            bf.SetWriterIndex(bf.WriterIndex + sizeof(UInt32));
            bf.WriteUnsignedShort((ushort)CommandIdentifier.Report);// 命令标识 2
            bf.WriteInt(6);     // 数据长度 4
            bf.WriteUnsignedShort((ushort)PackageDataIdnetifier.WorkStatus); // 内容标识 2
            bf.WriteInt(100);           // 内容长度 4
            ////////////////写入内容/////////////////////
            bf.WriteByte(0x01); // 1 // workstatu
            bf.WriteShort(1);
            bf.WriteShort(2);
            bf.WriteInt(3);
            bf.WriteShort(4); //采样温度
            bf.WriteShort(5);
            bf.WriteShort(6);
            bf.WriteInt(7); // 本次运行时长

            bf.WriteFloat(8);
            bf.WriteFloat(9);


            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms, encoding: Encoding.ASCII);
            string bstring;
            bstring = "codes";
            bw.Write(bstring);
            ms.Seek(0, SeekOrigin.Begin);

            // Read the string as raw bytes using FileStream...
            // The first series of bytes is the UTF7 encoded length of the
            // string. In this case, however, it is just the first two bytes.
            int len = ms.ReadByte() & 0x7f;
            len += ms.ReadByte() & 0x80;

            byte[] code_str = new byte[len];
            ms.Read(code_str, 0, len);

            ms.Seek(0, SeekOrigin.Begin);
            bf.WriteByte(ms.ReadByte());
            bf.WriteByte(ms.ReadByte());



            string convertred = Encoding.ASCII.GetString(code_str);


            bf.WriteBytes(code_str);


            ms.Seek(0, SeekOrigin.Begin);
            bf.WriteByte(ms.ReadByte());
            bf.WriteByte(ms.ReadByte());
            bf.WriteBytes(code_str); // 探测器版本

            bf.WriteInt(10);
            bf.WriteInt(11);


            bf.WriteFloat(12);
            bf.WriteFloat(13);
            bf.WriteFloat(14);
            bf.WriteFloat(15);
            bf.WriteFloat(16);
            bf.WriteFloat(17);
            bf.WriteFloat(18);
            bf.WriteFloat(19);
            bf.WriteFloat(20);


            bf.WriteInt(21);
            bf.WriteInt(22);
            bf.WriteInt(23);
            bf.WriteInt(24);
            bf.WriteInt(25);


            bf.WriteLong(26);
            bf.WriteLong(27);
            bf.WriteLong(28);
            bf.WriteUnsignedShort(29);
            bf.WriteLong(30);
            bf.WriteUnsignedShort(31);

            ms.Close();
            ////////////////写入内容/////////////////////
            bf.WriteUnsignedShort(20); //2

            TRPackage ar = ByteStreamToObjectConverter.Deserialize<TRPackage>(bf);

            Assert.AreEqual(ar.delimiter, 0xefef);
            Assert.AreEqual(ar.packageid, 2000);
            Assert.AreEqual(ar.identifier, (ushort)CommandIdentifier.Report);
            Assert.AreEqual(ar.data_len, 6);
            Assert.AreEqual(ar.content_identifier, (ushort)PackageDataIdnetifier.WorkStatus);
            Assert.AreEqual(ar.content_len, 100);
            Assert.AreEqual(ar.content_obj.GetType(), typeof(TRCloudWorkStatus));
            Assert.AreEqual(20, ar.xor_check_code);
            //System.Console.ReadLine();
        }
    }
}