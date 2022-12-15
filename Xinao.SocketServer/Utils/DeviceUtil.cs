using SuperSocket.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xinao.SocketServer.Database.Models;
using Xinao.SocketServer.Info;

namespace Xinao.SocketServer.Utils
{
    public class DeviceUtil
    {

        #region 沉降
        public static int GAUGE_MAX_DATA_LENGTH = 64;//返回数据长度限制
        public static int GAUGE_DTU_GATHER_DEFALUT_INTERVAL = 60;//DTU默认采样周期 秒
        public static int GAUGE_HEARTBEAT_INTERVAL = System.Configuration.ConfigurationManager.AppSettings["gaugeHeartBeatInterval"].ToInt32() * 1000;//设备间隔采样时间 毫秒
        public static int GAUGE_GATHER_BETWEEN_INTERVAL = System.Configuration.ConfigurationManager.AppSettings["gaugeGatherBetweenInterval"].ToInt32() * 1000;//设备间隔采样时间 秒
        public static int GAUGE_REFRESH_EXPIRATION_TIME = System.Configuration.ConfigurationManager.AppSettings["gaugeRefreshExpirationTime"].ToInt32();//配置过期时间 秒 超过该时间 重新向数据库获取
        public static bool IS_GAUGE_DATABASE_CACHE_ON = System.Configuration.ConfigurationManager.AppSettings["gaugeDatabaseCache"].ToBoolean();//读数据库缓存延时开关
        public static int GAUGE_SEND_SLEEP_TIME = System.Configuration.ConfigurationManager.AppSettings["gaugeSendSleepTime"].ToInt32();//两包数据发送最小间隔 毫秒
        public static int GAUGE_FIRST_TIME_SEND_INTERVAL = System.Configuration.ConfigurationManager.AppSettings["gaugeFirstTimeSendInterval"].ToInt32();//首次连接重发数据间隔 秒


        public static byte[] GetGaugeCmd(int? slaveId)
        {
            var cmd = new byte[8];
            cmd[0] = (byte)slaveId;
            cmd[1] = 4;
            cmd[2] = 0;
            cmd[3] = 0;
            cmd[4] = 0;
            cmd[5] = 6;
            var crc = Crc16(cmd, 6);
            cmd[6] = crc[0];
            cmd[7] = crc[1];

            return cmd;
        }

        //eg: 01 04 0C 48 E8 1F 20 48 E8 1E 80 48 E8 1D E0 50 24
        public static GaugeInfo ParseGauge(GaugeInfo info, string DtuCode)
        {
            info.SlaveId = info.ReadBuffer[0];

            info.Level = BitConverter.ToSingle(
                new byte[] { info.ReadBuffer[6], info.ReadBuffer[5], info.ReadBuffer[4], info.ReadBuffer[3] }, 0);
            info.Temprature = BitConverter.ToSingle(
                new byte[] { info.ReadBuffer[10], info.ReadBuffer[9], info.ReadBuffer[8], info.ReadBuffer[7] }, 0);
            info.Pressure = BitConverter.ToSingle(
                new byte[] { info.ReadBuffer[14], info.ReadBuffer[13], info.ReadBuffer[12], info.ReadBuffer[11] }, 0);

            LogUtil.LogGaugeData($"【沉降】数据解析完成->DtuCode: {DtuCode} SlaveId: {info.SlaveId} 液位: {info.Level} 温度: {info.Temprature} 压力: {info.Pressure}");

            return info;
        }
        #endregion

        #region 阴保
        public static int PROTECT_MAX_DATA_LENGTH = 64;//返回数据长度限制
        public static int PROTECT_NUMBER_OF_PROPERTIES = 5;//读取阴保有效设备的数量
        public static int PROTECT_DTU_GATHER_DEFALUT_INTERVAL = 60;//DTU默认采样周期 秒
        public static int PROTECT_HEARTBEAT_INTERVAL = System.Configuration.ConfigurationManager.AppSettings["protectHeartBeatInterval"].ToInt32() * 1000;//设备间隔采样时间 毫秒
        public static int PROTECT_REFRESH_EXPIRATION_TIME = System.Configuration.ConfigurationManager.AppSettings["protectRefreshExpirationTime"].ToInt32();//配置过期时间 秒 超过该时间 重新向数据库获取
        public static bool IS_PROTECT_DATABASE_CACHE_ON = System.Configuration.ConfigurationManager.AppSettings["protectDatabaseCache"].ToBoolean();//读数据库缓存延时开关
        public static int PROTECT_SEND_SLEEP_TIME = System.Configuration.ConfigurationManager.AppSettings["protectSendSleepTime"].ToInt32();//两包数据发送最小间隔 毫秒
        public static int PROTECT_FIRST_TIME_SEND_INTERVAL = System.Configuration.ConfigurationManager.AppSettings["protectFirstTimeSendInterval"].ToInt32();//首次连接重发数据间隔 秒


        public static byte[] GetProtectCmd(int? slaveId)
        {
            var cmd = new byte[8];
            cmd[0] = (byte)slaveId;
            cmd[1] = 3;
            cmd[2] = 0;
            cmd[3] = 0;
            cmd[4] = 0;
            cmd[5] = 7;
            var crc = Crc16(cmd, 6);
            cmd[6] = crc[0];
            cmd[7] = crc[1];

            return cmd;
        }

        //eg: 01 03 0E E0 AD E0 AD E0 AD E0 AD E0 AD E0 AD E0 AD 65 2E
        public static ProtectInfo ParseProtect(ProtectInfo info, string DtuCode)
        {
            info.SlaveId = info.ReadBuffer[0];

            info.ElectricPotential = BitConverter.ToInt16(new byte[] { info.ReadBuffer[4], info.ReadBuffer[3] }, 0);
            info.NaturePotential = BitConverter.ToInt16(new byte[] { info.ReadBuffer[6], info.ReadBuffer[5] }, 0);
            info.DcCurrent = BitConverter.ToInt16(new byte[] { info.ReadBuffer[8], info.ReadBuffer[7] }, 0);
            info.AcInterferenceVoltage = BitConverter.ToInt16(new byte[] { info.ReadBuffer[10], info.ReadBuffer[9] }, 0);
            info.AcStrayVoltage = BitConverter.ToInt16(new byte[] { info.ReadBuffer[12], info.ReadBuffer[11] }, 0);

            LogUtil.LogProtectData($"【阴保】数据解析完成(原始值)->DtuCode: {DtuCode} SlaveId: {info.SlaveId} 阴保电位: {info.ElectricPotential} 自然电位: {info.NaturePotential} 直流电流: {info.DcCurrent} 交流干扰电压: {info.AcInterferenceVoltage} 交流杂散电压: {info.AcStrayVoltage}");

            return info;
        }
        #endregion

        #region 位移
        public static int TIME_TRY_TO_CONNECT = 2;
        public static int MOVE_MAX_DATA_LENGTH = 64;//返回数据长度限制
        public static int MOVE_HEARTBEAT_INTERVAL = System.Configuration.ConfigurationManager.AppSettings["moveHeartBeatInterval"].ToInt32() * 1000;//设备间隔采样时间 毫秒
        public static int MOVE_REFRESH_EXPIRATION_TIME = System.Configuration.ConfigurationManager.AppSettings["moveRefreshExpirationTime"].ToInt32();//配置过期时间 秒 超过该时间 重新向数据库获取
        public static bool IS_MOVE_DATABASE_CACHE_ON = System.Configuration.ConfigurationManager.AppSettings["moveDatabaseCache"].ToBoolean();//读数据库缓存延时开关

        public static string GetMoveDtuCode(string data)
        {
            if (string.IsNullOrEmpty(data))
                return null;

            var regex = new Regex("\\$HB(?<code>.*)\\*HB");

            var code = regex.Match(data).Groups["code"].Value;

            return code;
        }

        //eg: $HUASI,GET,TMDATA,1,1,2020,04,09,18,10,29,0000,280618,3,288001,12.20,21.46,-0.142690,-0.981184,-0.130071,-246.8404,-178.3741,1365.2286,0.00,288002,8.13,21.35,0.177074,-0.976333,0.124174,-313.7042,-105.1029,874.4208,0.00,288003,10.68,22.35,-0.387279,-0.772509,0.503234,-251.6172,-193.6397,386.2544,0.00*1A
        public static List<MoveInfo> ParseMove(string data, string DtuCode)
        {
            var list = new List<MoveInfo>();

            var regex = new Regex("\\$HUASI,GET,TMDATA,([^,]*,){10}(?<number>[^,]*),(?<datas>.*\\*)[A-Fa-f0-9]{2}");

            var matches = regex.Match(data);

            try
            {
                var number = int.Parse(matches.Groups["number"].Value);
                var datas = matches.Groups["datas"].Value;

                list = ParseMove(datas, number, DtuCode);
            }
            catch
            {
                return list;
            }

            return list;
        }

        private static List<MoveInfo> ParseMove(string datas, int number, string DtuCode)
        {
            var regex = new Regex(RepeatInfoString(number));

            var list = new List<MoveInfo>();

            var matches = regex.Match(datas);

            for (int i = 0; i < number; i++)
            {
                var match = matches.Groups[$"data{i}"].Value;

                if (string.IsNullOrEmpty(match))
                    continue;

                var data = match.Split(',');

                var info = new MoveInfo()
                {
                    DeviceCode = data[0],
                    Voltage = float.Parse(data[1]),
                    Tempratrue = float.Parse(data[2]),
                    DeviceXSpeed = float.Parse(data[3]),
                    DeviceYSpeed = float.Parse(data[4]),
                    DeviceZSpeed = float.Parse(data[5]),
                    DeviceXData = float.Parse(data[6]),
                    DeviceYData = float.Parse(data[7]),
                    DeviceZData = float.Parse(data[8]),
                    DeviceAngle = float.Parse(data[9])
                };

                LogUtil.LogMoveData($"【位移】数据解析完成->DtuCode: {DtuCode} DeviceCode: {info.DeviceCode} 电压: {info.Voltage} 温度: {info.Tempratrue} 加速度XYZ: {info.DeviceXSpeed} {info.DeviceYSpeed} {info.DeviceZSpeed} 坐标XYZ: {info.DeviceXData} {info.DeviceYData} {info.DeviceZData} 旋转角: {info.DeviceAngle}");
                list.Add(info);
            }

            return list;
        }

        private static string RepeatInfoString(int times)
        {
            var sb = new StringBuilder();

            for(int i = 0; i < times; i++)
            {
                sb.Append($"(?<data{i}>(([^,^\\*]*)[,\\*])" + "{9}[^,^\\*]*)[,\\*]");
            }

            return sb.ToString();
        }

        public static double MoveDistance(float x, float y, float ox, float oy)
        {
            var distance = Math.Sqrt((x - ox)*(x - ox) + (y - oy)*(y - oy));
            return Math.Round(distance, 5);
        }

        public static string MoveDirection(float x, float y, float ox, float oy, float angle)
        {
            double radian = Math.Atan2(x - ox, y - oy); // x采集-x埋设，y采集-y埋设
            double angel = radian * 180.0 / Math.PI; // 与Y轴正向的夹角
            double northAnagel = angle % 360 - 90 + angel; // 与正北顺时针的夹角

            northAnagel = Math.Round(northAnagel, 4);

            string direction = "";
            if (northAnagel == 0.0)
            {
                direction = "正北向";
            }
            else if (northAnagel == 90.0)
            {
                direction = "正东向";
            }
            else if (northAnagel == 180.0)
            {
                direction = "正南向";
            }
            else if (northAnagel == -90.0)
            {
                direction = "正西向";
            }
            else if (northAnagel > 0 && northAnagel < 90)
            {
                direction = "北偏东 " + northAnagel;
            }
            else if (northAnagel > 90 && northAnagel < 180)
            {
                direction = "东偏南 " + Math.Round((northAnagel - 90),5) ;
            }
            else if (northAnagel > 180 && northAnagel < 270)
            {
                direction = "南偏西 " + Math.Round((northAnagel - 180),5) ;
            }
            else if (northAnagel > 270 && northAnagel < 360)
            {
                direction = "西偏北 " + Math.Round((northAnagel - 270), 5);
            }
            else if (northAnagel > 360 && northAnagel < 450)
            {
                direction = "北偏东" + Math.Round((northAnagel - 360), 5);
            }
            else if (northAnagel < 0 && northAnagel > -90)
            {
                direction = "北偏西 " + Math.Round((-1.0 * (northAnagel)), 5);
            }
            else if (northAnagel < -90 && northAnagel > -180)
            {
                direction = "西偏南 " + Math.Round((-1.0 * (northAnagel + 90)), 5);
            }
            else if (northAnagel < -180 && northAnagel > -270)
            {
                direction = "南偏东" + Math.Round((-1.0 * (northAnagel + 180)), 5);
            }
            else
            {
                direction = northAnagel.ToString();
            }

            return direction + "°";
        }
        #endregion

        #region  CRC16 校验

        private static readonly byte[] aucCRCHi = {
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
             0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
             0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40
         };
        private static readonly byte[] aucCRCLo = {
             0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06, 0x07, 0xC7,
             0x05, 0xC5, 0xC4, 0x04, 0xCC, 0x0C, 0x0D, 0xCD, 0x0F, 0xCF, 0xCE, 0x0E,
             0x0A, 0xCA, 0xCB, 0x0B, 0xC9, 0x09, 0x08, 0xC8, 0xD8, 0x18, 0x19, 0xD9,
             0x1B, 0xDB, 0xDA, 0x1A, 0x1E, 0xDE, 0xDF, 0x1F, 0xDD, 0x1D, 0x1C, 0xDC,
             0x14, 0xD4, 0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2, 0x12, 0x13, 0xD3,
             0x11, 0xD1, 0xD0, 0x10, 0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3, 0xF2, 0x32,
             0x36, 0xF6, 0xF7, 0x37, 0xF5, 0x35, 0x34, 0xF4, 0x3C, 0xFC, 0xFD, 0x3D,
             0xFF, 0x3F, 0x3E, 0xFE, 0xFA, 0x3A, 0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38,
             0x28, 0xE8, 0xE9, 0x29, 0xEB, 0x2B, 0x2A, 0xEA, 0xEE, 0x2E, 0x2F, 0xEF,
             0x2D, 0xED, 0xEC, 0x2C, 0xE4, 0x24, 0x25, 0xE5, 0x27, 0xE7, 0xE6, 0x26,
             0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0, 0xA0, 0x60, 0x61, 0xA1,
             0x63, 0xA3, 0xA2, 0x62, 0x66, 0xA6, 0xA7, 0x67, 0xA5, 0x65, 0x64, 0xA4,
             0x6C, 0xAC, 0xAD, 0x6D, 0xAF, 0x6F, 0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB,
             0x69, 0xA9, 0xA8, 0x68, 0x78, 0xB8, 0xB9, 0x79, 0xBB, 0x7B, 0x7A, 0xBA,
             0xBE, 0x7E, 0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C, 0xB4, 0x74, 0x75, 0xB5,
             0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71, 0x70, 0xB0,
             0x50, 0x90, 0x91, 0x51, 0x93, 0x53, 0x52, 0x92, 0x96, 0x56, 0x57, 0x97,
             0x55, 0x95, 0x94, 0x54, 0x9C, 0x5C, 0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E,
             0x5A, 0x9A, 0x9B, 0x5B, 0x99, 0x59, 0x58, 0x98, 0x88, 0x48, 0x49, 0x89,
             0x4B, 0x8B, 0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D, 0x4D, 0x4C, 0x8C,
             0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42, 0x43, 0x83,
             0x41, 0x81, 0x80, 0x40
         };
        private static byte[] Crc16(byte[] pucFrame, int usLen)// 低8 高8
        {
            int i = 0;
            byte[] res = new byte[2] { 0xFF, 0xFF };

            UInt16 iIndex = 0x0000;

            while (usLen-- > 0)
            {
                iIndex = (UInt16)(res[0] ^ pucFrame[i++]);
                res[0] = (byte)(res[1] ^ aucCRCHi[iIndex]);  // 低
                res[1] = aucCRCLo[iIndex]; // 高
            }
            return res;
        }

        public static bool checkCrc16(byte[] data)
        {
            if (data.Length < 2)
                return false;

            var crc = Crc16(data, data.Length - 2);

            if (crc[0] == data[data.Length - 2] && crc[1] == data[data.Length - 1])
                return true;

            return false;
        }
        #endregion

        #region xor校验

        public static bool CheckXor(string cmd)
        {
            var regex = new Regex("\\$(?<content>HUASI,GET,TMDATA,.*)\\*(?<checkSum>[A-Fa-f0-9]{2})");

            if (!regex.IsMatch(cmd))
                return false;

            var matches = regex.Match(cmd);
            string getXor = "";

            try { getXor = GetXor(Encoding.ASCII.GetBytes(matches.Groups["content"].Value)).ToString("X2"); } catch { return false; }

            if (getXor != matches.Groups["checkSum"].Value) { return false; }

            return true;
        }

        public static byte GetXor(byte[] Cmd)
        {
            byte check = (byte)(Cmd[0] ^ Cmd[1]);
            for (int i = 2; i < Cmd.Length; i++)
            {
                check = (byte)(check ^ Cmd[i]);
            }
            return check;
        }

        #endregion
    }
}
