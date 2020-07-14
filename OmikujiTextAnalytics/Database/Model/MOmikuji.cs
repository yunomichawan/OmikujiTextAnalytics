
using OmikujiTextAnalytics.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OmikujiTextAnalytics.Database.Model
{
    /// <summary>
    /// おみくじマスタ
    /// </summary>
    [Table("m_omikuji")]
    public class MOmikuji
    {
        /// <summary>
        /// プライマリキー
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// 大吉、中吉等のコード
        /// </summary>
        [Column("rank_code")]
        [StringLength(8)]
        public string RankCode { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        [Omikuji(OmikujiConst.CATEGORY_BODY)]
        [Column("body")]
        [StringLength(200)]
        public string Body { get; set; }

        /// <summary>
        /// 願望
        /// </summary>
        [Omikuji(OmikujiConst.CATEGORY_GANBO)]
        [Column("ganbo")]
        [StringLength(200)]
        public string Ganbo { get; set; }

        /// <summary>
        /// 待人
        /// </summary>
        [Omikuji(OmikujiConst.CATEGORY_MACHIBITO)]
        [Column("machibito")]
        [StringLength(200)]
        public string Machibito { get; set; }

        /// <summary>
        /// 失物
        /// </summary>
        [Omikuji(OmikujiConst.CATEGORY_SHITSUBUTSU)]
        [Column("shitsubutsu")]
        [StringLength(200)]
        public string Shitsubutsu { get; set; }

        /// <summary>
        /// 旅行
        /// </summary>
        [Omikuji(OmikujiConst.CATEGORY_RYOKO)]
        [Column("ryoko")]
        [StringLength(200)]
        public string Ryoko { get; set; }

        /// <summary>
        /// 商売
        /// </summary>
        [Omikuji(OmikujiConst.CATEGORY_SYOBAI)]
        [Column("syobai")]
        [StringLength(200)]
        public string Syobai { get; set; }

        /// <summary>
        /// 方角
        /// </summary>
        [Omikuji(OmikujiConst.CATEGORY_HOGAKU)]
        [Column("hogaku")]
        [StringLength(200)]
        public string Hogaku { get; set; }

        /// <summary>
        /// 学業
        /// </summary>
        [Omikuji(OmikujiConst.CATEGORY_GAKUGYO)]
        [Column("gakugyo")]
        [StringLength(200)]
        public string Gakugyo { get; set; }

        /// <summary>
        /// 争事
        /// </summary>
        [Omikuji(OmikujiConst.CATEGORY_ARASOIGOTO)]
        [Column("arasoigoto")]
        [StringLength(200)]
        public string Arasoigoto { get; set; }

        /// <summary>
        /// 転居
        /// </summary>
        [Omikuji(OmikujiConst.CATEGORY_TENKYO)]
        [Column("tenkyo")]
        [StringLength(200)]
        public string Tenkyo { get; set; }

        /// <summary>
        /// 出産
        /// </summary>
        [Omikuji(OmikujiConst.CATEGORY_SYUSSAN)]
        [Column("syussan")]
        [StringLength(200)]
        public string Syussan { get; set; }

        /// <summary>
        /// 病気
        /// </summary>
        [Omikuji(OmikujiConst.CATEGORY_BYOKI)]
        [Column("byoki")]
        [StringLength(200)]
        public string Byoki { get; set; }

        /// <summary>
        /// 縁談
        /// </summary>
        [Omikuji(OmikujiConst.CATEGORY_ENDAN)]
        [Column("endan")]
        [StringLength(200)]
        public string Endan { get; set; }
    }
}
