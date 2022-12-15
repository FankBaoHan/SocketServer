using SqlSugar;

namespace Xinao.SocketServer.Database.Models
{
    [SugarTable("base_dtu")]
    public class BaseDtu
    {
        /// <summary>
        /// 唯一标识
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string id { get; set; }

        /// <summary>
        /// 管道ID
        /// </summary>
        public string pipeline_id { get; set; }

        /// <summary>
        /// 管道名称
        /// </summary>
        public string pipeline_name { get; set; }

        /// <summary>
        /// DTU名称
        /// </summary>
        public string dtu_name { get; set; }

        /// <summary>
        /// DTU编码：序列号（位移DTU不需要该值）
        /// </summary>
        public string dtu_code { get; set; }

        /// <summary>
        /// 采集频率：单位（秒）（位移不需要，对沉降、阴保有效）
        /// </summary>
        public int? collect_rate { get; set; }

        /// <summary>
        /// 所属类型：1:沉降；2：位移；3：阴保
        /// </summary>
        public int? types { get; set; }

        /// <summary>
        /// 0正常1删除
        /// </summary>
        public bool? deleted { get; set; }
    }
}