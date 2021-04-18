using DotNetty.Buffers;
using Nuctech.NIS.ByteStream.Serializer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ByteStreamTest
{
    class SimpleDataTypeTest4
    {

        [Test]
        public void CommonTest()
        {

            PooledByteBufferAllocator pbba = new PooledByteBufferAllocator();
            IByteBuffer bf = pbba.CompositeBuffer();
            bf.WriteUnsignedShort(20);
            //bf.WriteBoolean(true);
            //bf.WriteInt(21); // false时跳过解析
            bf.WriteBoolean(true);
            bf.WriteInt(21); // true时跳过解析
            bf.WriteInt(22);
            bf.WriteInt(23);
            bf.WriteByte(0xff);

            SimplDataType sdt = ByteStreamToObjectConverter.Deserialize<SimplDataType>(bf);


            Assert.AreEqual(20, sdt.type);
            Assert.AreEqual(true, sdt.three_D);
            Assert.AreEqual(0xff, sdt.hash);

            //System.Console.ReadLine();
        }

        /// <summary>
        /// |ushort|bool|int|int|byte|
        /// </summary>
        public class SimplDataType
        {
            [ByteStreamParser(FieldType.Field_Ushort, FieldType.Field_Ushort_Len)]
            public ushort type;

            [ByteStreamParser(FieldType.Field_Bool, FieldType.Field_Bool_Len)]
            public bool three_D;

            [ByteStreamParser(FieldType.Field_Class)]
            [ByteStreamClassSelector(class_indicator_field = "three_D", class_indicator = true, class_full_type = typeof(ZuoBiao3D))]
            [ByteStreamClassSelector(class_indicator_field = "three_D", class_indicator = false, class_full_type = typeof(ZuoBiao2D))]
            public object zb;


            [ByteStreamParser(FieldType.Field_Byte, FieldType.Field_Byte_Len)]
            public byte hash;
        }

        public class ZuoBiao2D
        {
            [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
            public int x;
            [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
            public int y;
        }
        public class ZuoBiao3D
        {
            [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
            public int x;
            [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
            public int y;
            [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
            public int z;
        }
    }
}
