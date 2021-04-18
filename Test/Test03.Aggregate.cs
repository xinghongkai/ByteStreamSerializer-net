using DotNetty.Buffers;
using Nuctech.NIS.ByteStream.Serializer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ByteStreamTest
{
    class SimpleDataTypeTest3
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
            bf.WriteByte(0xff);

            SimplDataType sdt = ByteStreamToObjectConverter.Deserialize<SimplDataType>(bf);


            Assert.AreEqual(20, sdt.type);            
            Assert.AreEqual(true, sdt.success);
            Assert.AreEqual(21, sdt.zb.x);
            Assert.AreEqual(22, sdt.zb.y);
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
            public bool success;

            [ByteStreamClassConvertIgnoreFilter(filter_indicator_field = "success", filter_value = false)]
            [ByteStreamParser(FieldType.Field_Class)]
            [ByteStreamClassSelector(class_type_select_basis = ByteStreamClassSelectorAttribute.ClassTypeSelectBasis.Specified_Class, class_full_type = typeof(ZuoBiao))]
            public ZuoBiao zb;


            [ByteStreamParser(FieldType.Field_Byte, FieldType.Field_Byte_Len)]
            public byte hash;
        }

        public class ZuoBiao
        {
            [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
            public int x;
            [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
            public int y;
        }
    }
}
