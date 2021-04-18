using DotNetty.Buffers;
using Nuctech.NIS.ByteStream.Serializer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ByteStreamTest
{
    class SimpleDataTypeTest6
    {
        [Test]
        public void CommonTest()
        {

            PooledByteBufferAllocator pbba = new PooledByteBufferAllocator();
            IByteBuffer bf = pbba.CompositeBuffer();
            bf.WriteUnsignedShort(20);
            bf.WriteBoolean(true);
            bf.WriteInt(32);
            bf.WriteInt(1);
            bf.WriteInt(2);
            bf.WriteInt(3);
            bf.WriteInt(4);
            bf.WriteInt(5);
            bf.WriteInt(6);
            bf.WriteByte(0xff);

            SimplDataType sdt = ByteStreamToObjectConverter.Deserialize<SimplDataType>(bf);


            Assert.AreEqual(20, sdt.head);
            Assert.AreEqual(true, sdt.result);
            Assert.AreEqual(32, sdt.length);
            Assert.AreEqual(0xff, sdt.XOR);

            //System.Console.ReadLine();
        }

        /// <summary>
        ///    head  result  len     x   y   x   y   x   y      hash
        /// |ushort|bool    |int  |int|int|int|int|int|int|  byte |
        ///    2      1        4     4   4   4   4   4   4     1
        /// </summary>
        public class SimplDataType
        {
            public SimplDataType()
            {
            }

            [ByteStreamParser(FieldType.Field_Ushort, FieldType.Field_Ushort_Len)]
            public ushort head;

            [ByteStreamParser(FieldType.Field_Bool, FieldType.Field_Bool_Len)]
            public bool result;

            [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
            public int length;


            [ByteStreamParser(FieldType.Field_Class, length_field_name:"{length}-8")] // 总长 - 头 - 校验码
            [ByteStreamClassSelector(class_type_select_basis = ByteStreamClassSelectorAttribute.ClassTypeSelectBasis.Specified_Class, class_full_type = typeof(ZuoBiao2D))]
            public ZuoBiao2D two_d;


            [ByteStreamParser(FieldType.Field_Byte, FieldType.Field_Byte_Len)]
            public byte XOR;
        }

        public class ZuoBiao2D
        {
            public ZuoBiao2D(IByteBuffer bf, int len)
            {
                // 构造函数会收到字节流和长度，须字定义反序列化方式
                bf.ReadBytes(len);
            }
        }
        public class ZuoBiao3D
        {
        }

        public class People
        {
        }
    }
}
