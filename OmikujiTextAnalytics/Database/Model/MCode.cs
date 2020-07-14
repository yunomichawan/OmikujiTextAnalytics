using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OmikujiTextAnalytics.Database.Model
{
    /// <summary>
    /// コードマスタ
    /// </summary>
    [Table("m_code")]
    public class MCode
    {
        /// <summary>
        /// プライマリキー(Code+Category)
        /// </summary>
        [Key]
        [StringLength(8)]
        [Column("key")]
        public string Key { get; set; }

        /// <summary>
        /// コード
        /// </summary>
        [Column("code")]
        [StringLength(4)]
        public string Code { get; set; }

        /// <summary>
        /// カテゴリ
        /// </summary>
        [Column("category")]
        [StringLength(4)]
        public string Category { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        [Column("value")]
        [StringLength(100)]
        public string Value { get; set; }
    }
}
