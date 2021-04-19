using DotNetty.Buffers;
using Nuctech.NIS.ByteStream.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuctech.NIS.Service.DeviceAccess.ETD
{
    /// <summary>
    /// ETD解析规则详《EDT协议解析图示.png》
    /// 示例
    /// 
    ///ef ef 01 00 00 00 cf 00 06 00 00 00 14 18 00 00 00 00 33 f7
    ///|--------------------------------------------------------------------------------------------------------
    ///|名称 |起始标识 |   数据包ID   |命令标识          |数据长度     |            数据      |      校验码    |
    ///|     |         |              |                  |             |----------------------|                |
    ///|     |         |              |                  |             |内容标识 |  内容长度  |                |
    ///|-----|---------|--------------|------------------|-------------|---------|------------|----------------|
    ///|长度-| 2	   |      4	      |  2	             | 4	       | 2	     |      4	  |       2        |
    ///|-----|---------|--------------|------------------|-------------|---------|------------|----------------|
    ///|内容-|ef ef	   |01 00 00 00	  |cf 00	         |06 00 00 00  |	14 18|00 00 00 00 |	    33 f7      |
    ///|-----|---------|--------------|------------------|-------------|---------|------------|----------------|
    ///|说明-|固定     |     自增     |0x00CF代表查询指令| 数据总长度6 | 0x1814查|询状态   无 |      异或校验  |
    ///--------------------------------------------------------------------------------------------------------
    /// </summary>
    /// 
    [ByteStreamParser(FieldType.Field_Class)]
    public class TRPackage
    {
        private static uint s_package_id = 0;
        public TRPackage() { }
        public TRPackage(IByteBuffer nettybytes)
        {
            _Buffer = nettybytes;
        }
        // 包分隔符，用于字节流截断
        [ByteStreamParser(FieldType.Field_Ushort, FieldType.Field_Ushort_Len, true)]
        public ushort delimiter = 0xEFEF;
        /*
         * 0x00000000~0xFFFFFFFF循环
         * 说明：
         * 1、数据包ID用于区分先后发送的相同标识的数据包，也可用于数据包个数计数。
         * 2、从1开始连续计数。数据包ID的生成方应保证ID的唯一性。
         * 3、在通讯链路中，由通讯的主动方决定数据包ID。若双方均可为通讯的主动方时，双方可自行分别计数。
         * 4、接受端向发送端发送返回数据包时，返回数据包的ID应与对应的发送数据包的ID保持一致。
         *    即返回包的ID与收到的ID保持一致。若需返回多个数据包时，各数据包的ID可由通讯双方单独约定。
         * 5、数据包ID的使用在各项目中可根据情况自行约定。不强制要求。但最后遵循以上原则。
         * */
        [ByteStreamParser(FieldType.Field_UInt, FieldType.Field_UInt_Len)]
        public uint packageid = s_package_id++;

        /*
         * 数据包标识由通讯双方约定。除常用系统指令外，其他指令均可使用。
         * 在一次通讯中，接收端向发送端返回数据包时，若返回的为非系统指令，
         * 则返回数据包的标识应与请求标识一致
         * */
        [ByteStreamParser(FieldType.Field_Ushort, FieldType.Field_Ushort_Len)]
        public ushort identifier = (ushort)CommandIdentifier.Hello;

        /*
         * 数据内容长度=内容标识（2字节）+内容长度（4字节）+内容
         */

        [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
        public int data_len = 6;



        [ByteStreamParser(FieldType.Field_Ushort, FieldType.Field_Ushort_Len)]
        [ByteStreamClassConvertIgnoreFilter(filter_indicator_field = "data_len", filter_value = 0)]
        public ushort content_identifier=(ushort)PackageDataIdnetifier.Get_WorkStatus;



        [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
        [ByteStreamClassConvertIgnoreFilter(filter_indicator_field = "data_len", filter_value = 0)]
        public int content_len=0;

        /*
         * 数据内容对象
         * */
        [ByteStreamClassConvertIgnoreFilter(filter_indicator_field = "data_len", filter_value = 0)]
        [ByteStreamClassConvertIgnoreFilter(filter_indicator_field = "content_len", filter_value = 0)]
        [ByteStreamParser(FieldType.Field_Class)]
        [ByteStreamClassSelector(class_indicator_field = "content_identifier",class_indicator = 0x1802, class_full_type = typeof(TRCloudWorkStatus) )]
        [ByteStreamClassSelector(class_indicator_field = "content_identifier",class_indicator = 0x1803, class_full_type = typeof(TRCloudAnalyseResult))]
        [ByteStreamClassSelector(class_indicator_field = "content_identifier",class_indicator = 0x1804, class_full_type = typeof(TRCloudDiagnose))]
        [ByteStreamClassSelector(class_indicator_field = "content_identifier",class_indicator = 0x1805, class_full_type = typeof(TRCloudMaintenance))]
        public PackageData content_obj;


        /*
         * XOR check code
         * */

        [ByteStreamParser(FieldType.Field_Ushort, FieldType.Field_Ushort_Len)]
        public ushort xor_check_code;


        /*
         * 数据包
         */
        private IByteBuffer _Buffer;

        /*
         * 数据包
         * */
        public byte[] _bBuf;
        /*
         * 数据内容
         * */
        public byte[] data_content;

        public static bool IsValid(ushort check_code, byte[] byffer)
        {
			int n = byffer.Length / 2;
			byte[] tmpByte = new byte[2];
			for (int i = 0; i < n - 1; i++)
			{
				tmpByte[0] ^= byffer[2 * i];
				tmpByte[1] ^= byffer[2 * i + 1];
			}
			return check_code == BitConverter.ToUInt16(tmpByte, 0);		
        }
        public static ushort getChekcCode(byte[] byffer)
        {
			int n = byffer.Length / 2;
			byte[] tmpByte = new byte[2];
			for (int i = 0; i < n - 1; i++)
			{
				tmpByte[0] ^= byffer[2 * i];
				tmpByte[1] ^= byffer[2 * i + 1];
			}
			return BitConverter.ToUInt16(tmpByte, 0);		
        }
    }




    [ByteStreamParser(FieldType.Field_Class)]
    public class PackageData
    {
        public PackageData() { }
        
        public PackageData(PackageDataIdnetifier identifer, int dl, byte[] pd)
        {
            data_identifier = identifer;
            data_len = dl;
            data = pd;
        }

       public PackageData(PackageDataIdnetifier identifer)
        {
            data_identifier = identifer;
        }

        public PackageData(IByteBuffer buffer)
        {
            _Buffer = buffer;
        }



        //[ByteStreamParser(FieldType.Field_Ushort, FieldType.Field_Ushort_Len)]
        public PackageDataIdnetifier data_identifier;



        //[ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
        public int data_len;
        public byte[] data;
        private IByteBuffer _Buffer;
    }

    public class TRCloudWorkStatus : PackageData
    {
        public TRCloudWorkStatus() : base(PackageDataIdnetifier.WorkStatus){ }
        public TRCloudWorkStatus(int dl, byte[] pd) : base(PackageDataIdnetifier.WorkStatus, dl, pd)
        {

        }

        [ByteStreamParser(FieldType.Field_Byte, FieldType.Field_Byte_Len)]
        public byte work_status;//WorkStatus



        [ByteStreamParser(FieldType.Field_Short, FieldType.Field_Short_Len)]
        public short e_temperature; // 环境温度



        [ByteStreamParser(FieldType.Field_Short, FieldType.Field_Short_Len)]
        public short e_humidity;    // 湿度


        [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
        public Int32 e_pressure;    // 气压，单位kPa


        [ByteStreamParser(FieldType.Field_Short, FieldType.Field_Short_Len)]
        public short s_temperature; // 采样温度


        [ByteStreamParser(FieldType.Field_Short, FieldType.Field_Short_Len)]
        public short p_temperature; // 单模式温度


        [ByteStreamParser(FieldType.Field_Short, FieldType.Field_Short_Len)]
        public short n_tempreature; // 双模式温度


        [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
        public int   time;          // 本次运行时长，单位小时


        [ByteStreamParser(FieldType.Field_Float, FieldType.Field_Float_Len)]
        public float longitude;     // 经度


        [ByteStreamParser(FieldType.Field_Float, FieldType.Field_Float_Len)]
        public float latitude;      // 纬度


        //http://www.csref.cn/vs100/method/System-IO-BinaryWriter-Write-12.html
        [ByteStreamParser(FieldType.Field_String, eHowtoSetLen.Accord_0X7F)]
        public string firmware;     // 固件版本   


        //http://www.csref.cn/vs100/method/System-IO-BinaryWriter-Write-12.html
        [ByteStreamParser(FieldType.Field_String, eHowtoSetLen.Accord_0X7F)]
        public string detector;     // 探测器版本 


        [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
        public int pressure_inner;  // 内部气压


        [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
        public int pressure_outter; // 外部气压


        [ByteStreamParser(FieldType.Field_Float, FieldType.Field_Float_Len)]
        public float temperature_ion_positive;// 正温度


        [ByteStreamParser(FieldType.Field_Float, FieldType.Field_Float_Len)]
        public float temperature_ion_negative;// 负温度


        [ByteStreamParser(FieldType.Field_Float, FieldType.Field_Float_Len)]
        public float temperature_cal;// 标定温度


        [ByteStreamParser(FieldType.Field_Float, FieldType.Field_Float_Len)]
        public float pump_mobile;    // 内部流量


        [ByteStreamParser(FieldType.Field_Float, FieldType.Field_Float_Len)]
        public float pump_sample;    // 采样流量


        [ByteStreamParser(FieldType.Field_Float, FieldType.Field_Float_Len)]
        public float pump_assist;    // 辅助流量


        [ByteStreamParser(FieldType.Field_Float, FieldType.Field_Float_Len)]
        public float pump_clean;     // 清洁流量


        [ByteStreamParser(FieldType.Field_Float, FieldType.Field_Float_Len)]
        public float voltage;        // 电压


        [ByteStreamParser(FieldType.Field_Float, FieldType.Field_Float_Len)]
        public float electric_current;// 电流


        [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
        public int total_running_time;//累计运行时间


        [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
        public int analyse_count;   // 当前检测次数


        [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
        public int alarm_count;     // 当前报警次数


        [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
        public int total_analyse_count; // 累计检测次数


        [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
        public int total_alarm_count;   // 累计报警次数


        [ByteStreamParser(FieldType.Field_Long, FieldType.Field_Long_Len)]
        public long last_ana_time;  // 最近一次检测时间，日期，作为long型变量传输，UTC表示自 1970 年 1 月 1 日午夜 12:00:00 以来已经过的时间的以1秒为间隔的间隔数。


        [ByteStreamParser(FieldType.Field_Long, FieldType.Field_Long_Len)]
        public long last_cal_time;  // 最近一次标定时间，日期，作为long型变量传输，UTC表示自 1970 年 1 月 1 日午夜 12:00:00 以来已经过的时间的以1秒为间隔的间隔数。


        [ByteStreamParser(FieldType.Field_Long, FieldType.Field_Long_Len)]
        public long last_ver_time;  // 最近一次验证时间，日期，作为long型变量传输，UTC表示自 1970 年 1 月 1 日午夜 12:00:00 以来已经过的时间的以1秒为间隔的间隔数。
        
        
        [ByteStreamParser(FieldType.Field_Ushort, FieldType.Field_Ushort_Len)]
        public ushort device_type; //EnumTRDeviceType
        
        
        [ByteStreamParser(FieldType.Field_Long, FieldType.Field_Long_Len)]
        public long product_date;   // 最近一次验证时间，日期，作为long型变量传输，UTC表示自 1970 年 1 月 1 日午夜 12:00:00 以来已经过的时间的以1秒为间隔的间隔数。
        
        
        [ByteStreamParser(FieldType.Field_Ushort, FieldType.Field_Ushort_Len)]
        public ushort lib_mode;//EnumLibraryMode
    }

    /// <summary>
    public class  TRCloudAnalyseResult: PackageData
    {
        public TRCloudAnalyseResult():base(PackageDataIdnetifier.AnalyseResults) { }
        public TRCloudAnalyseResult(int dl, byte[] pd):base(PackageDataIdnetifier.AnalyseResults, dl, pd)
        {
            
        }

        [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
        public int record_id; // 记录编号
       
        
        [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
        public int type;      // 记录类型 检测=0，标定=1
        
        
        [ByteStreamParser(FieldType.Field_Long, FieldType.Field_Long_Len)]
        public long time;     // // 最近一次验证时间，日期，作为long型变量传输，UTC表示自 1970 年 1 月 1 日午夜 12:00:00 以来已经过的时间的以1秒为间隔的间隔数。
        
        
        [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
        public int analysis_count; // 检测次数统计
        
        
        [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
        public int alarm_count; // 报警次数统计
        
        
        [ByteStreamParser(FieldType.Field_Bool, FieldType.Field_Bool_Len)]
        public bool is_alarm; // 是否报警
       
        
        [ByteStreamParser(FieldType.Field_String, eHowtoSetLen.Accord_0X7F)]
        public string soft_version; // 软件版本号
        
        
        [ByteStreamParser(FieldType.Field_String, eHowtoSetLen.Accord_0X7F)] 
        public string alg_version;  // 算法版本号
        
        
        [ByteStreamParser(FieldType.Field_String, eHowtoSetLen.Accord_0X7F)] 
        public string lib_version;  // 物质版本号
        
        
        [ByteStreamParser(FieldType.Field_String, eHowtoSetLen.Accord_0X7F)]
        public string user_name;    // 使用用户


        /// <summary>
        /// ByteStreamClassSelector特性目前只支持动态选择对象类型，那么像下面这种情况，类型是确定的如何处理呢？
        /// 这里可以添加一个固定值的字段，如下，添加对象创建需求
        ///public int predefined_class = 0;
        ///[ByteStreamParser(FieldType.Field_Class)]
        ///[ByteStreamClassSelector(class_indicator_field = "predefined_class", class_indicator = 0, class_full_type = typeof(AlarmResult))]
        ///public AlarmResult contraband_list;
        /// </summary>
        /// 


        [ByteStreamParser(FieldType.Field_Class)]
        [ByteStreamClassSelector(class_type_select_basis = ByteStreamClassSelectorAttribute.ClassTypeSelectBasis.Specified_Class, class_full_type = typeof(AlarmResult))]
        public AlarmResult contraband_list;
    }

    public class AlarmResult
    {
        public AlarmResult() { }
        [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
        public int count; // 报警物质数量


        [ByteStreamClassConvertIgnoreFilter(filter_indicator_field = "count", filter_value = 0)]
        [ByteStreamParser(FieldType.Field_List)]
        [ByteStreamClassConvertListSize(
            list_size_basis = ByteStreamClassConvertListSizeAttribute.ListSizeBasis.Calculated,
            list_size_indicator_field = "count")]
        [ByteStreamListElementSizeParser(
            list_element_size_basis = ByteStreamListElementSizeParserAttribute.ListElementSizeBasis.RefObject,
            list_element_length = FieldType.Field_Float_Len,
            list_element_type =typeof(TRAlarmInfo))]
        public List<TRAlarmInfo> alarm_info_list = new List<TRAlarmInfo>() { };
    }

    public class TRAlarmInfo
    {
        public TRAlarmInfo() { }
        [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
        public int mass_type; // 0:Explosive;1,Drug
        
        
        [ByteStreamParser(FieldType.Field_String, eHowtoSetLen.Accord_0X7F)]
        public string mass_name; // 名称
        
        
        [ByteStreamParser(FieldType.Field_Float, FieldType.Field_Float_Len)]
        public float idtensity; // 强度
        
        
        [ByteStreamParser(FieldType.Field_Int, FieldType.Field_Int_Len)]
        public int peak_number; // 实际峰数量


        [ByteStreamParser(FieldType.Field_List)]
        [ByteStreamListElementSizeParser( list_element_size_basis = ByteStreamListElementSizeParserAttribute.ListElementSizeBasis.PreDefined, list_element_length = FieldType.Field_Float_Len,list_element_type =typeof(float))]
        [ByteStreamClassConvertListSize( list_size_basis = ByteStreamClassConvertListSizeAttribute.ListSizeBasis.Predefined, list_size_value =5)]
        public List<float> peak_pos=new List<float>() { };// 峰值


        [ByteStreamParser(FieldType.Field_List)]
        [ByteStreamListElementSizeParser(list_element_size_basis = ByteStreamListElementSizeParserAttribute.ListElementSizeBasis.PreDefined, list_element_length = FieldType.Field_Float_Len,list_element_type =typeof(float))]
        [ByteStreamClassConvertListSize(list_size_basis = ByteStreamClassConvertListSizeAttribute.ListSizeBasis.Predefined, list_size_value = 5)]
        public List<float> peak_value = new List<float>() { }; // 峰高
    }

   public class TRCloudDiagnose : PackageData
    {
        public TRCloudDiagnose() : base(PackageDataIdnetifier.Diagnosis){ }
        public TRCloudDiagnose(int dl, byte[] pd) : base(PackageDataIdnetifier.Diagnosis, dl, pd)
        {

        }
        
        
        [ByteStreamParser(FieldType.Field_Ushort, FieldType.Field_Ushort_Len)]
        public byte dia_status; // eDiaStatus
        
        
        
        [ByteStreamParser(FieldType.Field_String,eHowtoSetLen.Accord_0X7F)]
        public string codes;    // 故障码
        
        
        
        [ByteStreamParser(FieldType.Field_String, eHowtoSetLen.Accord_0X7F)]
        public string user;     // 当前用户
    }

   public class TRCloudMaintenance : PackageData
    {
        public TRCloudMaintenance(): base(PackageDataIdnetifier.Maintenance) { }
        public TRCloudMaintenance(int dl, byte[] pd) : base(PackageDataIdnetifier.Maintenance, dl, pd)
        {

        }
        
        
        [ByteStreamParser(FieldType.Field_Byte, FieldType.Field_Byte_Len)]
        public byte maintain_status; //eMaintainStatus


        [ByteStreamParser(FieldType.Field_String, eHowtoSetLen.Accord_0X7F)]
        public string codes; // 维护码
        
        
        [ByteStreamParser(FieldType.Field_String, eHowtoSetLen.Accord_0X7F)]
        public string users; // 当前用户
    }

   public class TRCloudActionGetWorkStatus : PackageData
    {
        public TRCloudActionGetWorkStatus(): base(PackageDataIdnetifier.Get_WorkStatus) { }
        public TRCloudActionGetWorkStatus(int dl, byte[] pd) : base(PackageDataIdnetifier.Get_WorkStatus, dl, pd)
        {

        }
    }

   public class TRCloudActionAnalyse : PackageData
    {
        public TRCloudActionAnalyse(): base(PackageDataIdnetifier.Action_Analyse) { }
        public TRCloudActionAnalyse(int dl, byte[] pd) : base(PackageDataIdnetifier.Action_Analyse, dl, pd)
        {

        }
    }

    public class TRCloudActionGasStart : PackageData
    {
        public TRCloudActionGasStart(): base(PackageDataIdnetifier.Action_GasStart) { }
        public TRCloudActionGasStart(int dl, byte[] pd) : base(PackageDataIdnetifier.Action_GasStart, dl, pd)
        {

        }
    }

    public class TRCloudActionGasStop : PackageData
    {
        public TRCloudActionGasStop(): base(PackageDataIdnetifier.Action_GasStop) { }
        public TRCloudActionGasStop(int dl, byte[] pd) : base(PackageDataIdnetifier.Action_GasStop, dl, pd)
        {

        }
    }

    public class TRCloudActionSelfCali : PackageData
    {
        public TRCloudActionSelfCali(): base(PackageDataIdnetifier.Action_SelfCali) { }
        public TRCloudActionSelfCali(int dl, byte[] pd) : base(PackageDataIdnetifier.Action_SelfCali, dl, pd)
        {

        }
    }
    public class TRCloudActionShut : PackageData
    {
        public TRCloudActionShut() : base(PackageDataIdnetifier.Action_Shut){ }
        public TRCloudActionShut(int dl, byte[] pd) : base(PackageDataIdnetifier.Action_Shut, dl, pd)
        {

        }
    }

    public enum eMaintainStatus
    {
        Normal = 0x00,    //正常
        Maintain = 0x01,   //需要维护
        DealWith = 0x11,   //进行了维护操作
    }

    public enum eDiaStatus
    {
        Normal = 0x00,     //正常
        Error = 0x01       //故障
    }

    public enum CommandIdentifier
    {
        None = 0xffff,  
        /*
         * 心跳握手指令
         * 上位机PC主动发送，设备接收后，返回回执命令Success；
         * 该命令可用作心跳指令，用来测试通讯是否断开连接，间隔频率不宜太高，防止和其他指令冲突
         * */
        Hello = 0x0000,    
        // 心跳回执 
        Success = 0x005F, 
        /* 
         * 查询命令标识
         * 上位机PC主动查询下位机设备状态信息时，发送数据包的命令标识。
         * */
        Command = 0x00CF,
        /*
         * 上报信息标识
         * 1、当下位机设备接收到查询命令时，返回的数据包
         * 2、设备状态发生变化时，也会主动上传状态信息
         * 注：上位机只需要负责连接，设备状态改变时会自动上传信息（包括报警结果以及曲线）
         * */
        Report = 0x0301,  
    }

    public enum PackageDataIdnetifier
    {
        // ==========================================================
        /*
         * 下位机返回数据内容标识，即设备发往PC端的数据的标识
         * */
        // ==========================================================
        /*
         *  上传最新设备状态/每次状态发生改变上传
         *  TRCloudWorkStatus
         * */
        WorkStatus = 0x1802,

        /*
         * 每次检测完成后，上传检测结果信息：是否报警，报警结果是什么
         * 每次检测后上传
         * */
        AnalyseResults = 0x1803,

        /*
         * 设备故障信息
         * */
        Diagnosis  = 0x1804,

        /*
         * 设备维护信息
         * */
        Maintenance = 0x1805,

        //=============================================================
        /*
         * PC端往下位机设备发送命令，下位机上传数据
         * */
        //=============================================================
        /**
         * PC端主动查询当前设备状态信息，设备收到该命令，上传WorkStatus指令
         * */
        Get_WorkStatus = 0x1814,
        Action_Analyse = 0x1901,// 进行一次普通分析操作
        Action_GasStart = 0x1911, //开始气体监测模式
        Action_GasStop = 0x1912, // 结束气体监测模式
        Action_SelfCali = 0x1921, //自校准操作
        Action_Shut = 0x19F1 //关机
    }


    /// 设备运行模式
    /// </summary>
    public enum EnumLibraryMode
    {
        Dual = 0x00,// 毒品和爆炸物
        Explosive = 0x01,// 爆炸物
        Narcotic = 0x02,//毒品
    }


    /// <summary>
    /// 设备类型
    /// </summary>
    public enum EnumTRDeviceType
    {
        TR1000DC = 0x0d,
        TR1000QC = 0x0e,
        TR1000DCA = 0x0f,
        TR2000DBA = 0x16,
        TR2000DC = 0x17,
        TR2000DE = 0x1f,
    }

    public enum WorkStatus
    {
        None = 0x00,//初始状态            
        Heat = 0x01,//正在预热            
        HeatTimeOut = 0x02,//预热超时            
        TempBalance = 0x03,//温度平衡            
        SelfCheck = 0x04,//正在自检            
        SelfCheckSuccess = 0x05,//自检成功            
        SelfCheckFailed = 0x06,//自检失败            
        Initalizing = 0x07,//设备初始化          
        //
        ReadyForAnalyze = 0x10,//就绪                
        Analyzing = 0x11,//正在分析            
        Monitoring = 0x12,//正在监测气体        
        EarlyAlarm = 0x13,//提前报警            
        Pass = 0x14,//分析结果-通过       
        Alarm = 0x15,//分析结果-报警       
        WaitForConfirmAnalyzeResult = 0x16,//等待确认结果-报警   
        WaitForConfirmAnalyzeResult_NoAlarm = 0x17,//等待确认结果-通过   
        //
        Calibrating = 0x20,//正在标定            
        ReadyForCalibrate = 0x21,//等待标定            
        CalibratingSuccess = 0x22,//标定通过            
        CalibratingFailed = 0x23,//标定失败            
        //
        Verifying = 0x30,//正在验证            
        ReadyForVerify = 0x31,//等待验证            
        VerifySuccess = 0x32,//验证通过            
        VerifyFailed = 0x33,//验证失败            
        //
        Cleaning = 0x40,//正在清洁            
        DeepCleaning = 0x41,//深度清洁            
        //
        CoronaCalCheck = 0x50,//正在校准            
        //
        Generating = 0x60,//正在生成报告        
        ReadyForGenerat = 0x61,//等待生成报告        
        GenerateSuccess = 0x62,//生成报告成功        
        GenerateFailed = 0x63,//生成报告失败        
        //
        Downtime = 0xA0,//设备宕机            
        //
        Error = 0xFF,//设备故障            
        //
        ShutDown = 0xFD,//设备关机          

    }
}
