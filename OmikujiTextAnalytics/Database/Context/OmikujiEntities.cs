using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using OmikujiTextAnalytics.Database.Model;

namespace OmikujiTextAnalytics.Database.Context
{
    public class OmikujiEntities: DbContext
    {
        /// <summary>
        /// 各コード（大吉等をコードで管理）
        /// </summary>
        public DbSet<MCode> Codes { get; set; }

        /// <summary>
        /// おみくじ内容
        /// </summary>
        public DbSet<MOmikuji> Omikujis { get; set; }

        /// <summary>
        /// おみくじ分析結果
        /// </summary>
        public DbSet<TOmikujiAnalytics> OmikujiAnalytics { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public OmikujiEntities()
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            // SQLServerに接続
            optionsBuilder.UseSqlServer(@"接続文字列");
        }

    }
}
