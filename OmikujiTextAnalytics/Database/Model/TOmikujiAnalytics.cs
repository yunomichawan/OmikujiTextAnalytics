using Azure.AI.TextAnalytics;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;

namespace OmikujiTextAnalytics.Database.Model
{
    /// <summary>
    /// おみくじ分析結果
    /// </summary>
    [Table("t_omikuji_analytics")]
    public class TOmikujiAnalytics
    {
        /// <summary>
        /// プライマリキー
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// 分析したおみくじID
        /// </summary>
        [Column("omikuji_id")]
        public int OmikujiId { get; set; }

        /// <summary>
        /// おみくじ分析結果ID（分析したテキストが複数の文章で構成されていた場合に使用）
        /// </summary>
        [AllowNull]
        [Column("parent_omikuji_analytics_id")]
        public int? ParentOmikujiAnalyticsId { get; set; }

        /// <summary>
        /// おみくじのカテゴリ（本文、願望等）
        /// </summary>
        [StringLength(8)]
        [Column("category_code")]
        public string CategoryCode { get; set; }

        /// <summary>
        /// 分析したテキスト
        /// </summary>
        [StringLength(200)]
        [Column("analys_text")]
        public string AnalysText { get; set; }

        /// <summary>
        /// 分析したテキストの感情
        /// </summary>
        [StringLength(10)]
        [Column("text_sentiment")]
        public string TextSentiment { get; set; }

        /// <summary>
        /// 分析結果：ポジティブ値
        /// </summary>
        [Column("positive")]
        public double Positive { get; set; }

        /// <summary>
        /// 分析結果：ネガティブ値
        /// </summary>
        [Column("negative")]
        public double Negative { get; set; }

        /// <summary>
        /// 分析結果：ニュートラル値
        /// </summary>
        [Column("neutral")]
        public double Neutral { get; set; }

        /// <summary>
        /// 空コンストラクタ
        /// </summary>
        public TOmikujiAnalytics()
        {

        }

        /// <summary>
        /// 親データ用コンストラクタ
        /// </summary>
        /// <param name="omikujiId"></param>
        /// <param name="text"></param>
        /// <param name="categoryCode"></param>
        /// <param name="sentiment"></param>
        public TOmikujiAnalytics(int omikujiId, string text, string categoryCode, DocumentSentiment sentiment)
        {
            this.OmikujiId = omikujiId;
            this.ParentOmikujiAnalyticsId = null;
            this.AnalysText = text;
            this.CategoryCode = categoryCode;
            this.TextSentiment = sentiment.Sentiment.ToString();
            this.Positive = sentiment.ConfidenceScores.Positive;
            this.Neutral = sentiment.ConfidenceScores.Neutral;
            this.Negative = sentiment.ConfidenceScores.Negative;
        }

        /// <summary>
        /// 子データ用コンストラクタ
        /// </summary>
        /// <param name="omikujiId"></param>
        /// <param name="parentId"></param>
        /// <param name="categoryCode"></param>
        /// <param name="sentiment"></param>
        public TOmikujiAnalytics(int omikujiId, int parentId, string categoryCode, SentenceSentiment sentiment)
        {
            this.OmikujiId = omikujiId;
            this.ParentOmikujiAnalyticsId = parentId;
            this.CategoryCode = categoryCode;
            this.AnalysText = sentiment.Text;
            this.TextSentiment = sentiment.Sentiment.ToString();
            this.Positive = sentiment.ConfidenceScores.Positive;
            this.Neutral = sentiment.ConfidenceScores.Neutral;
            this.Negative = sentiment.ConfidenceScores.Negative;
        }
    }
}
