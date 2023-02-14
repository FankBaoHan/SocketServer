using Org.BouncyCastle.Asn1.Mozilla;
using SqlSugar;
using System;

namespace Xinao.SocketServer.Database.Models
{
	[SugarTable("base_warn_data")]
	public class BaseWarnData
	{

		[SugarColumn(IsPrimaryKey = true)]
		public string id { get; set; }

		/// <summary>
		/// 传感器id
		/// </summary>
		public string device_id { get; set; }

		/// <summary>
		/// 沉降 位移 阴保 数据表id
		/// </summary>
		public string c_id { get; set; }

		/// <summary>
		/// 管道id
		/// </summary>
		public string pipeline_id { get; set; }

		/// <summary>
		/// 管道名称
		/// </summary>
		public string pipeline_name { get; set; }

		/// <summary>
		/// dtu
		/// </summary>
		public string dtu_id { get; set; }

		/// <summary>
		/// dtu
		/// </summary>
		public string dtu_name { get; set; }

		/// <summary>
		/// 所属设备 1 沉降 2 位移 3 阴保
		/// </summary>
		public int? dtu_type { get; set; }

		/// <summary>
		/// 设备名称
		/// </summary>
		public string device_name { get; set; }

		/// <summary>
		/// 设备序列号
		/// </summary>
		public string device_code { get; set; }

		/// <summary>
		/// 告警类型 1 正常告警 2 掉线告警
		/// </summary>
		public int? alarm_type { get; set; }

		/// <summary>
		/// 告警内容
		/// </summary>
		public string alarm_content { get; set; }

		/// <summary>
		/// 报警是否处理；0未处理；1已处理
		/// </summary>
		public bool? is_deal { get; set; }

		/// <summary>
		/// 处理时间
		/// </summary>
		public DateTime? gmt_deal { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public DateTime? gmt_create { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public bool? deleted { get; set; }
	}
}
