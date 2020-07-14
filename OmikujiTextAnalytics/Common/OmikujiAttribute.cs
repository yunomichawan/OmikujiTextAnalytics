using System;

namespace OmikujiTextAnalytics.Common
{
    /// <summary>
    /// おみくじ属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class OmikujiAttribute : Attribute
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="category"></param>
        public OmikujiAttribute(string category)
        {
            this.Category = category;
        }

        /// <summary>
        /// カテゴリ
        /// </summary>
        private string Category { get; set; }

        /// <summary>
        /// カテゴリキー
        /// </summary>
        public string CategoryCode => OmikujiConst.CD_OMIKUJI_CATEGORY + this.Category;
    }
}
