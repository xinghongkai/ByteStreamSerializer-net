using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nuctech.NIS.ByteStream.Serializer
{
    /// <summary>
    /// 特性对类和属性进行标注，用于对字节流按顺序进行拆分，并对类的成员进行赋值
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class | 
        AttributeTargets.Method |
        AttributeTargets.Field,
        AllowMultiple = false)]
    public class ByteStreamParserAttribute: Attribute
    {
        private bool  bHead;
        /// <summary>
        /// 字段类型，用于对拆分的字节序列进行类型转换
        /// Byte	   1	    1个字节
        /// Byte[]     动态       字节数组，传输时在之前应当有一个字段说明数组的长度
        /// Bool	   1	    1字节表示的布尔值
        /// Short	   2	    2字节整型变量
        /// Ushort	   2	    2字节无符号整型变量
        /// Int	       4	    4字节整型变量
        /// UInt	   4	    4字节无符号整型变量
        /// Long	   8	    8字节整型变量
        /// Ulong	   8	    8字节无符号整型变量
        /// Float	   4	    4字节浮点数
        /// Double	   8	    8字节浮点数
        /// DateTime   8	    日期，作为long型变量传输，表示自 1970 年 1 月 1 日午夜 12:00:00 以来已经过的时间的以1秒为间隔的间隔数。
        /// String     动态    传输时在之前应当有一个字段说明字符长度。字符统一为ASCII编码
        /// </summary>
        private string field_type;     
        private uint   field_len;
        private string field_customer_function;// 特殊的类型转换函数
        private eHowtoSetLen how_to_set_len;// 
        private string property_name; // 当通过属性指定占用字节长度是，需要指定属性名称,需要说明的一点是，对于Class这种可以是表达式，表达式是最简单的四则运算

        /// <summary>
        /// 指定特定长度
        /// </summary>
        /// <param name="param_field_type"></param>
        /// <param name="param_field_len"></param>
        /// <param name="bh"></param>
        /// <param name="customer_function"></param>
        public ByteStreamParserAttribute(
                string param_field_type,
                uint param_field_len=0,
                bool bh=false,
                string customer_function="")
        {
            bHead = bh;
            field_type= param_field_type;
            field_len= param_field_len;
            field_customer_function = customer_function;
            if(param_field_type == FieldType.Field_String)
            {
                how_to_set_len = eHowtoSetLen.Accord_0X7F;
            }
            else
            {
                how_to_set_len = eHowtoSetLen.Accord_Number;
            }
        }
        public ByteStreamParserAttribute(
       string param_field_type,
       eHowtoSetLen string_length_type,
       uint param_field_len = 0,
       bool bh = false,
       string customer_function = "")
        {
            bHead = bh;
            field_type = param_field_type;
            field_len = param_field_len;
            field_customer_function = customer_function;
            how_to_set_len = string_length_type;
        }

        public ByteStreamParserAttribute(
        string param_field_type,
        string length_field_name,
        bool bh = false,
        string customer_function = "")
        {
            bHead = bh;
            field_type = param_field_type;
            field_customer_function = customer_function;
            how_to_set_len = eHowtoSetLen.Accord_Property;
            property_name = length_field_name;
        }

        public ByteStreamParserAttribute(
                string param_field_type,
                eHowtoSetLen string_length_type,
                string length_field_name,
                bool bh = false,
                string customer_function = "")
        {
            field_type = param_field_type;
            how_to_set_len = string_length_type;
            property_name = length_field_name;
            bHead = bh;
            field_customer_function = customer_function;
        }

        public bool Head
        {
            get { return bHead; }
        }

        public string FieldTypeName
        {
            get
            {
                return field_type;
            }
        }

        public uint FieldLen
        {
            get { return field_len; }
        }

        public string CustomerTypeConverterFun
        {
            get { return field_customer_function; }
        }

        public eHowtoSetLen LenType
        {
            get { return how_to_set_len; }
        }

        public string LenthFieldName
        {
            get { return property_name; }
        }

    }

    /// <summary>
    /// 类选择器，根据字节流中类型字段不同取值，转换为不同的对象
    /// </summary>
    [AttributeUsage(AttributeTargets.Class |AttributeTargets.Method |AttributeTargets.Field,AllowMultiple = false)]
    public class ByteStreamListElementSizeParserAttribute : Attribute
    {
        public enum ListElementSizeBasis
        {
            RefObject,  // 如果元素是引用对象，则通过引用对象递归进行反序列化
            PreDefined  // 如果元素是值对象，则预指定长度
        }
        public ListElementSizeBasis list_element_size_basis; 
        public int list_element_length;       // 类型字段取值
        public Type list_element_type;


        public ByteStreamListElementSizeParserAttribute()
        {
        }
    }

    /// <summary>
    /// 类选择器，根据字节流中类型字段不同取值，转换为不同的对象
    /// </summary>
    [AttributeUsage(AttributeTargets.Class |AttributeTargets.Method |AttributeTargets.Field,AllowMultiple = true)]
    public class ByteStreamClassSelectorAttribute : Attribute
    {
        /// <summary>
        ///  根据相关字段的值决定类的类型，还是指定特定类型
        /// </summary>
        public enum ClassTypeSelectBasis
        {
            Indicator_Field,
            Specified_Class
        }
        public string class_indicator_field; // 类型字段的名称
        public object    class_indicator;       // 类型字段取值
        public Type   class_full_type;       // 特定取值对应的类型
        public ClassTypeSelectBasis class_type_select_basis = ClassTypeSelectBasis.Indicator_Field;

        public ByteStreamClassSelectorAttribute()
        {
        }
    }


    /// <summary>
    /// 根据不同的条件决定是否读取指定的属性，这在字节流中经常出现的如长度字段，当长度为0时，后续内容不再读取之类
    /// </summary>
    [AttributeUsage(AttributeTargets.Class |AttributeTargets.Method |AttributeTargets.Field,AllowMultiple = true)]
    public class ByteStreamClassConvertIgnoreFilterAttribute : Attribute
    {
        public string filter_indicator_field; // 前面的字段，只支持可转换为整形的字段
        public object filter_value;              // 比较值

        public ByteStreamClassConvertIgnoreFilterAttribute()
        {
        }
    }

    /// <summary>
    /// 字节流中传递多个对象时， 根据字节流中的取值，决定后面会有多少对象需要循环读取
    /// 也有采用定长占位的情况，在ETD集成中有这种情况
    /// </summary>
    [AttributeUsage(AttributeTargets.Class |AttributeTargets.Method |AttributeTargets.Field,AllowMultiple = false)]
    public class ByteStreamClassConvertListSizeAttribute : Attribute
    {
        public enum ListSizeBasis
        {
            Predefined, // 读取list_size_indicator_field的值
            Calculated  // 读取list_size_value
        }
        public string list_size_indicator_field; // 前面的字段，只支持可转换为整形的字段
        public int list_size_value;              // 比较值
        public ListSizeBasis list_size_basis;

        public ByteStreamClassConvertListSizeAttribute()
        {
        }
    }

    public class  FieldType
    {
        public const string Field_Class    = "Class"; // 字节偏移来做，根据属性进行偏移
        public const string Field_Byte     = "Byte";	   
        public const string Field_Bool     = "Bool";
        public const string Field_Short    = "Short";
        public const string Field_Ushort   = "Ushort";
        public const string Field_Int      = "Int";
        public const string Field_UInt     = "UInt";
        public const string Field_Long     = "Long";
        public const string Field_Ulong    = "Ulong";
        public const string Field_Float    = "Float";
        public const string Field_Double   = "Double";
        public const string Field_String   = "String";
        public const string Field_List     = "List";



        public const int Field_Class_Len = 0;
        public const int Field_Byte_Len = 1;
        public const int Field_Bytes_Len = 0;
        public const int Field_Bool_Len = 1;
        public const int Field_Short_Len = 2;
        public const int Field_Ushort_Len = 2;
        public const int Field_Int_Len = 4;
        public const int Field_UInt_Len = 4;
        public const int Field_Long_Len = 8;
        public const int Field_Ulong_Len = 8;
        public const int Field_Float_Len = 4;
        public const int Field_Double_Len = 8;
        public const int Field_DateTime_Len = 8;
        public const int Field_String_Len = 0; // 字符串是有长度的，通过解析字节流获取长度。
        public const int Field_List_Len = 0;
    }

    /// <summary>
    /// 如何获取字段占用的字节数
    /// </summary>
    public enum eHowtoSetLen
    {
        Accord_Number, // 强制指定
        Accord_Property, // 通过类内特定属性指定，这里属性必须是上一个属性，上一个属性的值目前仅支持通过ByteStreamParserAttribute计算出来
        Accord_0X7F, // 通过长度字符串读取长度
    }
}
