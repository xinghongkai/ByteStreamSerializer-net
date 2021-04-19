using DotNetty.Buffers;
using Nuctech.NIS.ByteStream.Serializer;
using Nuctech.NIS.Service.DeviceAccess.ETD;
using NUnit.Framework;
using System;
using System.IO;
using System.Text;

namespace ByteStreamTest
{
    public class SimpleDataTypeTest8
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test_Package_simple_Type()
        {


            TRPackage tr = new TRPackage()
            {
                identifier = (ushort)CommandIdentifier.Hello,
                content_identifier = 0
            };

            IByteBuffer bf = ByteStreamToObjectConverter.Serialize<TRPackage>(tr);

            byte[] content = new byte[bf.WriterIndex];
            bf.ReadBytes(content);

            tr.xor_check_code = TRPackage.getChekcCode(content);
            bf.WriteUnsignedShort(tr.xor_check_code);


            bf.ResetReaderIndex();


            TRPackage ar = ByteStreamToObjectConverter.Deserialize<TRPackage>(bf);

            Assert.AreEqual(ar.delimiter, 0xefef);
            Assert.AreEqual(ar.packageid, tr.packageid);
            Assert.AreEqual(ar.identifier, tr.identifier);
            Assert.AreEqual(ar.data_len, tr.data_len);
            Assert.AreEqual(ar.content_identifier, tr.content_identifier);
            Assert.AreEqual(ar.content_len, tr.content_len);
            Assert.AreEqual(ar.xor_check_code, tr.xor_check_code);
            Assert.AreEqual(ar.content_obj, null);
            //System.Console.ReadLine();
        }

    }
}