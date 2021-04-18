using DotNetty.Buffers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Nuctech.NIS.ByteStream.Serializer
{
    public class ByteStreamToObjectConverter
    {
        public static T Deserialize<T>(IByteBuffer nettybytes)
        {
            T byte_stream_obj = System.Activator.CreateInstance<T>();

            DeserializeInner(byte_stream_obj, nettybytes);

            return byte_stream_obj;
        }

        private static void DeserializeInner(object byte_stream_obj, IByteBuffer nettybytes)
        {
            Type byte_stream_type = byte_stream_obj.GetType();
            object[] laAttributes = byte_stream_type.GetCustomAttributes(typeof(ByteStreamParserAttribute), false);


            if (laAttributes == null && laAttributes.Count() <= 0)
                return;


            FieldInfo[] laFields =
                byte_stream_type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);


            foreach (FieldInfo lofield in laFields)
            {
                // 查看是否满足忽略条件
                object[] ignoreattributes = lofield.GetCustomAttributes(typeof(ByteStreamClassConvertIgnoreFilterAttribute), false);
                bool bIgnore = false;
                if (ignoreattributes != null && ignoreattributes.Count() > 0)
                {
                    foreach (Attribute ig in ignoreattributes)
                    {
                        ByteStreamClassConvertIgnoreFilterAttribute ignoreattribute = (ByteStreamClassConvertIgnoreFilterAttribute)ig;
                        FieldInfo len_field_info = byte_stream_type.GetField(ignoreattribute.filter_indicator_field);
                        object ilen = len_field_info.GetValue(byte_stream_obj);
                        if (ilen.Equals(ignoreattribute.filter_value))
                        {
                            bIgnore = true;
                            break;
                        }
                    }
                }
                if (bIgnore) continue;



                object[] laFieldsAttributes = lofield.GetCustomAttributes(typeof(ByteStreamParserAttribute), false);

                foreach (Attribute loAtt in laFieldsAttributes)
                {
                    ByteStreamParserAttribute loDefectTrack = (ByteStreamParserAttribute)loAtt;
                    if (loDefectTrack.FieldTypeName == FieldType.Field_Class)
                    {
                        // 如果是类名，则动态创建对象，递归初始化
                        /*
                         * /// 这里分为两种情况：
                         * 1、TR集成时，基本都有长度字段，那么对于类的解析，类中成员可以简单通过偏移实现
                         * 2、金属门集成时，content字段没有长度， 需要通过包的总长减去包头和包尾，得到长度len，
                         * 对于content的解析是从content开始处读取固定的长度len.
                         * */
                        //if(loDefectTrack.LenthFieldName == null && loDefectTrack.LenthFieldName.Length<=0)
                        //{
                        // 第一种情况
                        object[] classSelector = lofield.GetCustomAttributes(typeof(ByteStreamClassSelectorAttribute), false);
                        if (classSelector != null && classSelector.Count() > 0)
                        {
                            // 如果有类选择器，则获取类型字段,并创建对象
                            ByteStreamClassSelectorAttribute bcsa = (ByteStreamClassSelectorAttribute)classSelector.ElementAt(0);

                            if (bcsa.class_type_select_basis == ByteStreamClassSelectorAttribute.ClassTypeSelectBasis.Indicator_Field)
                            {
                                FieldInfo class_field_info = byte_stream_type.GetField(bcsa.class_indicator_field);

                                object iclasstype_id = class_field_info.GetValue(byte_stream_obj);

                                foreach (object bytestream in classSelector)
                                {
                                    ByteStreamClassSelectorAttribute innerbcsa = (ByteStreamClassSelectorAttribute)bytestream;
                                    if (iclasstype_id.ToString().Equals(innerbcsa.class_indicator.ToString()))
                                    {
                                        if (loDefectTrack.LenthFieldName == null && loDefectTrack.LenthFieldName.Length <= 0)
                                        {
                                            //第一种情况
                                            dynamic obj = innerbcsa.class_full_type.Assembly.CreateInstance(innerbcsa.class_full_type.FullName);
                                            lofield.SetValue(byte_stream_obj, obj);
                                            DeserializeInner(obj, nettybytes);
                                            break;
                                        }
                                        else if (loDefectTrack.LenthFieldName != null && loDefectTrack.LenthFieldName.Length > 0)
                                        {
                                            // 第二种情况，首先计算类占用的字节数，然后再调用有参数的构造函数，参数为IByteBuffer nettybytes,int length
                                            // 1、计算所占用的字节长度
                                            // 2、调用指定类型的构造函数，构造函数完成字节读取、反序列化，不需要再递归调用DeserializeInner
                                            int length = (int)ExpresstionClass.ExpValue(ReplaceProWithValue(loDefectTrack.LenthFieldName, byte_stream_obj));
                                            var constructors = innerbcsa.class_full_type.GetConstructors();
                                            foreach (ConstructorInfo ci in constructors)
                                            {
                                                ParameterInfo[] pis = ci.GetParameters();
                                                if (pis.Length == 2
                                                    /*&& pis.ElementAt(1).ParameterType.Equals(typeof(IByteBuffer)) 
                                                     * && pis.ElementAt(1).ParameterType.Equals(typeof(int))*/)
                                                {
                                                    List<object> o = new List<object> { nettybytes, length };
                                                    lofield.SetValue(byte_stream_obj, ci.Invoke(o.ToArray()));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else if (bcsa.class_type_select_basis == ByteStreamClassSelectorAttribute.ClassTypeSelectBasis.Specified_Class)
                            {
                                if (loDefectTrack.LenthFieldName == null && loDefectTrack.LenthFieldName.Length <= 0)
                                {
                                    //第一种情况
                                    dynamic obj = bcsa.class_full_type.Assembly.CreateInstance(bcsa.class_full_type.FullName);
                                    lofield.SetValue(byte_stream_obj, obj);
                                    DeserializeInner(obj, nettybytes);
                                    break;
                                }
                                else if (loDefectTrack.LenthFieldName != null && loDefectTrack.LenthFieldName.Length > 0)
                                {
                                    // 第二种情况，首先计算类占用的字节数，然后再调用有参数的构造函数，参数为IByteBuffer nettybytes,int length
                                    // 1、计算所占用的字节长度
                                    // 2、调用指定类型的构造函数，构造函数完成字节读取、反序列化，不需要再递归调用DeserializeInner
                                    int length = (int)ExpresstionClass.ExpValue(ReplaceProWithValue(loDefectTrack.LenthFieldName, byte_stream_obj));
                                    var constructors = bcsa.class_full_type.GetConstructors();
                                    foreach (ConstructorInfo ci in constructors)
                                    {
                                        ParameterInfo[] pis = ci.GetParameters();
                                        if (pis.Length == 2
                                            /*&& 
                                            pis.ElementAt(1).ParameterType.FullName.Equals(typeof(IByteBuffer).FullName) && 
                                            pis.ElementAt(1).ParameterType.FullName.Equals(typeof(int).FullName)*/)
                                        {
                                            List<object> o = new List<object> { nettybytes, length };
                                            lofield.SetValue(byte_stream_obj, ci.Invoke(o.ToArray()));
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (loDefectTrack.FieldTypeName == FieldType.Field_List)
                    {
                        // 获取列表长度
                        //ByteStreamClassConvertListSizeAttribute 
                        object[] classSelector = lofield.GetCustomAttributes(typeof(ByteStreamClassConvertListSizeAttribute), false);
                        int iListSize = 0;
                        if (classSelector != null && classSelector.Count() > 0)
                        {
                            ByteStreamClassConvertListSizeAttribute listsize = (ByteStreamClassConvertListSizeAttribute)classSelector.ElementAt(0);

                            if (listsize.list_size_basis == ByteStreamClassConvertListSizeAttribute.ListSizeBasis.Predefined)
                            {
                                iListSize = listsize.list_size_value;
                            }
                            else if (listsize.list_size_basis == ByteStreamClassConvertListSizeAttribute.ListSizeBasis.Calculated)
                            {
                                FieldInfo list_size_field_info = byte_stream_type.GetField(listsize.list_size_indicator_field);
                                iListSize = Convert.ToInt32(list_size_field_info.GetValue(byte_stream_obj));
                            }
                        }

                        if (iListSize <= 0) continue;


                        // 获取list字段的add方法
                        BindingFlags flag = BindingFlags.Instance | BindingFlags.Public;
                        MethodInfo methodInfo = lofield.FieldType.GetMethod("Add", flag);
                        // 获取list字段值
                        object olist = lofield.GetValue(byte_stream_obj);



                        // 获取list元素的类型和长度   ByteStreamListElementSizeParserAttribute
                        object[] elementSizeSelector = lofield.GetCustomAttributes(typeof(ByteStreamListElementSizeParserAttribute), false);
                        if (elementSizeSelector == null || elementSizeSelector.Count() <= 0) continue;

                        ByteStreamListElementSizeParserAttribute sizeParser = (ByteStreamListElementSizeParserAttribute)elementSizeSelector.ElementAt(0);

                        for (int i = 0; i < iListSize; i++)
                        {
                            if (sizeParser.list_element_size_basis == ByteStreamListElementSizeParserAttribute.ListElementSizeBasis.PreDefined)
                            {
                                //动态创建对象，并初始化
                                object vo = GetValueTypeValue(sizeParser.list_element_type, nettybytes, (uint)sizeParser.list_element_length);
                                //添加到对象
                                methodInfo.Invoke(olist, new object[] { vo });//相当于List<T>调用Add方法
                            }
                            else if (sizeParser.list_element_size_basis == ByteStreamListElementSizeParserAttribute.ListElementSizeBasis.RefObject)
                            {
                                //动态创建对象，并初始化
                                object subobj = sizeParser.list_element_type.Assembly.CreateInstance(sizeParser.list_element_type.FullName);
                                DeserializeInner(subobj, nettybytes);
                                //添加到对象
                                methodInfo.Invoke(olist, new object[] { subobj });//相当于List<T>调用Add方法
                            }
                            else
                                break;
                        }
                    }
                    else if (loDefectTrack.FieldTypeName == FieldType.Field_String)
                    {
                        // 字符串比较特殊，多一种0X7F方式，还需要与TR研发人员对接下，目前还是不确定如何转成有长度字节序列
                        if (loDefectTrack.LenType == eHowtoSetLen.Accord_0X7F)
                        {
                            //http://www.csref.cn/vs100/method/System-IO-BinaryWriter-Write-12.html
                            int strlen = nettybytes.ReadByte() & 0x7f;
                            strlen += nettybytes.ReadByte() & 0x80; // 网站中此处写成*了， 有问题吧？


                            byte[] strbyte = new byte[strlen];
                            nettybytes.ReadBytes(strbyte, 0, strlen);
                            lofield.SetValue(byte_stream_obj, Encoding.ASCII.GetString(strbyte));

                        }
                        #region unused

                        else if (loDefectTrack.LenType == eHowtoSetLen.Accord_Number)
                        {
                            byte[] str_b = new byte[loDefectTrack.FieldLen];
                            nettybytes.ReadBytes(str_b, 0,(int)loDefectTrack.FieldLen);
                            string str_s = Encoding.ASCII.GetString(str_b);

                            lofield.SetValue(byte_stream_obj, str_s);
                        }
                        else if (loDefectTrack.LenType == eHowtoSetLen.Accord_Property)
                        {
                            // 如果是其它类型的字段，则根据指定属性值的长度来源，暂无需求
                            byte[] str_b = new byte[loDefectTrack.FieldLen];
                            nettybytes.ReadBytes(str_b, 0,(int)loDefectTrack.FieldLen);
                            string str_s = Encoding.ASCII.GetString(str_b);

                            lofield.SetValue(byte_stream_obj, str_s);
                        }
                        
                        #endregion
                    }
                    else
                    {
                        // 其它类型目前仅支持特定长度和属性
                        if (loDefectTrack.LenType == eHowtoSetLen.Accord_Number)
                        {
                            // 如果是其它类型的字段，则根据指定字段的长度来源
                            Type t = lofield.FieldType;
                            object vo = GetValueTypeValue(t, nettybytes, loDefectTrack.FieldLen);
                            lofield.SetValue(byte_stream_obj, vo);
                        }
                        #region unused
                        /*
                        else if (loDefectTrack.LenType == eHowtoSetLen.Accord_Property)
                        {
                            // 如果是其它类型的字段，则根据指定属性值的长度来源
                        }
                        else
                        {
                            // 
                        }*/
                        #endregion
                    }
                }
            }
        }

        private static object GetValueTypeValue(Type t, IByteBuffer nettybytes, uint readlen)
        {
            if (t.Equals(typeof(bool))) return nettybytes.ReadBoolean();
            else if (t.Equals(typeof(short))) return nettybytes.ReadShort();
            else if (t.Equals(typeof(ushort))) return nettybytes.ReadUnsignedShort();
            else if (t.Equals(typeof(int))) return nettybytes.ReadInt();
            else if (t.Equals(typeof(uint))) return nettybytes.ReadUnsignedInt();
            else if (t.Equals(typeof(long))) return nettybytes.ReadLong();
            else if (t.Equals(typeof(ulong))) return nettybytes.ReadLong(); // Unsignedlong??
            else if (t.Equals(typeof(float))) return nettybytes.ReadFloat();
            else if (t.Equals(typeof(double))) return nettybytes.ReadDouble();
            else if (t.Equals(typeof(byte))) return nettybytes.ReadByte();
            else if (t.Equals(typeof(byte[]))) return nettybytes.ReadBytes((int)readlen);
            else return t.Assembly.CreateInstance(t.FullName);
        }

        public static string ReplaceProWithValue(string regex, object o)
        {
            int start = 0, end = 0;
            IList<KeyValuePair<string, string>> value = new List<KeyValuePair<string, string>>();
            for (int i = 0; i < regex.Length; i++)
            {
                if (regex.ElementAt(i) == '{')
                    start = i;
                if (regex.ElementAt(i) == '}')
                {
                    end = i;
                    value.Add(new KeyValuePair<string, string>(regex.Substring(start + 1, end - start - 1), regex.Substring(start, end - start + 1)));
                }
            }

            foreach (var v in value)
            {
                string propertyname = v.Key;
                string decoratedpropertyname = v.Value;
                string pvalue = string.Empty;
                pvalue = ParseField(propertyname, o);
                regex = regex.Replace(decoratedpropertyname, pvalue);
            }
            return regex;
        }



        private static string ParseField(string propertyname, object o)
        {
            string pvalue = string.Empty;
            int dotindex = propertyname.IndexOf('.');
            if (dotindex < 0)
            {
                foreach (var cbp in o.GetType().GetFields())
                {
                    if (cbp.Name.Equals(propertyname))
                    {
                        if (cbp.FieldType.Name.Equals("Enum"))
                            pvalue = Convert.ToInt32(cbp.GetValue(o)).ToString();
                        else
                            pvalue = cbp.GetValue(o)?.ToString();
                        break;
                    }
                }
            }
            else
            {
                string pfirstproperty = propertyname.Substring(0, dotindex);
                string leftproperty = propertyname.Substring(dotindex + 1);
                foreach (var cbp in o.GetType().GetFields())
                {
                    if (cbp.Name.Equals(pfirstproperty))
                    {
                        pvalue = ParseProperty(leftproperty, cbp.GetValue(o));
                        break;
                    }
                }
            }
            return pvalue;
        }

        private static string ParseProperty(string propertyname, object o)
        {
            string pvalue = string.Empty;
            int dotindex = propertyname.IndexOf('.');
            if (dotindex < 0)
            {
                foreach (var cbp in o.GetType().GetProperties())
                {
                    if (cbp.Name.Equals(propertyname))
                    {
                        if (cbp.PropertyType.BaseType.Name.Equals("Enum"))
                            pvalue = Convert.ToInt32(cbp.GetValue(o, null)).ToString();
                        else
                            pvalue = cbp.GetValue(o, null)?.ToString();
                        break;
                    }
                }
            }
            else
            {
                string pfirstproperty = propertyname.Substring(0, dotindex);
                string leftproperty = propertyname.Substring(dotindex + 1);
                foreach (var cbp in o.GetType().GetProperties())
                {
                    if (cbp.Name.Equals(pfirstproperty))
                    {
                        pvalue = ParseProperty(leftproperty, cbp.GetValue(o, null));
                        break;
                    }
                }
            }
            return pvalue;
        }
    }
}
