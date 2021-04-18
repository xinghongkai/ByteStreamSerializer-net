using DotNetty.Buffers;
using Nuctech.NIS.ByteStream.Serializer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ByteStreamTest
{
    class SimpleDataTypeTest5
    {
        [Test]
        public void CommonTest()
        {

            PooledByteBufferAllocator pbba = new PooledByteBufferAllocator();
            IByteBuffer bf = pbba.CompositeBuffer();
            bf.WriteUnsignedShort(20);

            bf.WriteBoolean(true);
            bf.WriteUnsignedShort(2);
            bf.WriteInt(21);bf.WriteInt(22);
            bf.WriteInt(23);bf.WriteInt(24);

            bf.WriteUnsignedShort(10);

            bf.WriteInt(25);bf.WriteInt(28);bf.WriteInt(25);
            bf.WriteInt(26);bf.WriteInt(29);bf.WriteInt(26);
            bf.WriteInt(27);bf.WriteInt(30);bf.WriteInt(27);
            bf.WriteInt(28);bf.WriteInt(25);bf.WriteInt(28);
            bf.WriteInt(29);bf.WriteInt(26);bf.WriteInt(29);
            bf.WriteInt(30);bf.WriteInt(27);bf.WriteInt(30);
            bf.WriteInt(25);bf.WriteInt(28);bf.WriteInt(25);
            bf.WriteInt(26);bf.WriteInt(29);bf.WriteInt(26);
            bf.WriteInt(27);bf.WriteInt(30);bf.WriteInt(27); 
            bf.WriteInt(28);bf.WriteInt(29);bf.WriteInt(30);
            
            
            bf.WriteByte(0xff);

            SimplDataType sdt = ByteStreamToObjectConverter.Deserialize<SimplDataType>(bf);


            Assert.AreEqual(20, sdt.type);
            Assert.AreEqual(true, sdt.three_D);
            Assert.AreEqual(0xff, sdt.hash);

            //System.Console.ReadLine();
        }

        /// <summary>
        ///             2d点数 x   y   x   y   x    y  3d点数    x   y   z   x   y   z   x   y   z
        /// |ushort|bool|uint|int|int|int|int|int|int|  uint   |int|int|int|int|int|int|int|int|int|byte|
        /// </summary>
        public class SimplDataType
        {
            [ByteStreamParser(FieldType.Field_Ushort, FieldType.Field_Ushort_Len)]
            public ushort type;

            [ByteStreamParser(FieldType.Field_Bool, FieldType.Field_Bool_Len)]
            public bool   three_D;

            [ByteStreamParser(FieldType.Field_Ushort, FieldType.Field_Ushort_Len)]
            public ushort two_d_cnts;


            //[ByteStreamClassConvertIgnoreFilter(filter_indicator_field = "two_d_cnts", filter_value = 0)]
            [ByteStreamParser(FieldType.Field_List)]
            [ByteStreamClassConvertListSize(list_size_basis = ByteStreamClassConvertListSizeAttribute.ListSizeBasis.Calculated,list_size_indicator_field = "two_d_cnts")]
            [ByteStreamListElementSizeParser(list_element_size_basis = ByteStreamListElementSizeParserAttribute.ListElementSizeBasis.RefObject,list_element_type = typeof(ZuoBiao2D))]
            public List<object> two_d_points = new List<object>() { };



            [ByteStreamParser(FieldType.Field_Ushort, FieldType.Field_Ushort_Len)]
            public ushort three_d_cnts;


            [ByteStreamParser(FieldType.Field_List)]
            [ByteStreamClassConvertListSize(list_size_basis = ByteStreamClassConvertListSizeAttribute.ListSizeBasis.Calculated,list_size_indicator_field = "three_d_cnts")]
            [ByteStreamListElementSizeParser(list_element_size_basis = ByteStreamListElementSizeParserAttribute.ListElementSizeBasis.RefObject,list_element_type = typeof(ZuoBiao3D))]
            public List<object> three_d_points= new List<object>() { };

            [ByteStreamParser(FieldType.Field_Byte, FieldType.Field_Byte_Len)]
            public byte hash;
        }

        public class ZuoBiao2D
        {
            [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
            public int x;
            [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
            public int y;


            //public People teacher;
        }
        public class ZuoBiao3D
        {
            [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
            public int x;
            [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
            public int y;
            [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
            public int z;

            //public People student;
        }

        public class People
        {
            string name;
            string home;
        }
    }
}
