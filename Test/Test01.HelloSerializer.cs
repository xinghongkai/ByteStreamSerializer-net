using DotNetty.Buffers;
using Nuctech.NIS.ByteStream.Serializer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ByteStreamTest
{
    class SimpleDataTypeTest1
    {
        [Test]
        public void CommonTest()
        {
            PooledByteBufferAllocator pbba = new PooledByteBufferAllocator();
            IByteBuffer bf = pbba.CompositeBuffer();
            bf.WriteUnsignedShort(20);
            bf.WriteInt(21);
            bf.WriteBoolean(false);
            bf.WriteByte(0xff);

            SimplDataType sdt = ByteStreamToObjectConverter.Deserialize<SimplDataType>(bf);


            Assert.AreEqual(20, sdt.type);
            Assert.AreEqual(21, sdt.id);
            Assert.AreEqual(false, sdt.gender);
            Assert.AreEqual(0xff, sdt.hash);

            //System.Console.ReadLine();
        }

        /// <summary>
        /// |ushort|int|bool|byte|
        /// </summary>
        public class SimplDataType
        {
            /// <summary>
            /// 字段的顺序须和字节流一致
            /// </summary>
            [ByteStreamParser(FieldType.Field_Ushort, FieldType.Field_Ushort_Len)]
            public ushort type;

            [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
            public int id;

            [ByteStreamParser(FieldType.Field_Bool, FieldType.Field_Bool_Len)]
            public bool gender;

            [ByteStreamParser(FieldType.Field_Byte, FieldType.Field_Byte_Len)]
            public byte hash;
        }
    }
}
