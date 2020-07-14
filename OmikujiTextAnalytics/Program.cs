using Azure;
using Azure.AI.TextAnalytics;
using OmikujiTextAnalytics.Common;
using OmikujiTextAnalytics.Database.Context;
using OmikujiTextAnalytics.Database.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OmikujiTextAnalytics
{
    class Program
    {
        static void Main(string[] args)
        {
            OmikujiTest omikujiTest = new OmikujiTest();
            // データベースの準備、データの準備
            omikujiTest.InitSchema();
            omikujiTest.AddOmikuji();

            // おみくじ内容を自然言語処理で分析
            omikujiTest.AnalysOmikuji();

            // おまけ：他の分析方法を検証してみる
            Console.WriteLine("おまけ：他の分析も試そう");
            // 固有名詞の抽出
            omikujiTest.EntityRecognition();
            // キーフレーズの抽出
            omikujiTest.KeyPhraseExtraction();


        }
    }

    public class OmikujiTest
    {
        // key情報
        private static readonly AzureKeyCredential credentials = new AzureKeyCredential("<replace-with-your-text-analytics-key-here>");
        private static readonly Uri endpoint = new Uri("<replace-with-your-text-analytics-endpoint-here>");

        /// <summary>
        /// おみくじDB
        /// </summary>
        private OmikujiEntities OmikijiEntities { get; set; }

        /// <summary>
        /// Text Analytics API
        /// </summary>
        private TextAnalyticsClient Client { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public OmikujiTest()
        {
            // インスタンス生成
            this.OmikijiEntities = new OmikujiEntities();

            // 日本設定でText Analytics APIのインスタンス生成
            TextAnalyticsClientOptions options = new TextAnalyticsClientOptions { DefaultLanguage = "ja", DefaultCountryHint = "jp" };
            this.Client = new TextAnalyticsClient(endpoint, credentials, options);
        }

        /// <summary>
        /// スキーマ初期化
        /// </summary>
        public void InitSchema()
        {
            this.OmikijiEntities.Database.EnsureDeleted();
            this.OmikijiEntities.Database.EnsureCreated();
        }

        /// <summary>
        /// おみくじデータ追加
        /// </summary>
        public void AddOmikuji()
        {
            MOmikuji omikuji = this.GetMOmikuji();
            List<MCode> codes = this.GetCodes();

            this.OmikijiEntities.Omikujis.Add(omikuji);
            this.OmikijiEntities.Codes.AddRange(codes);
            this.OmikijiEntities.SaveChanges();
        }

        /// <summary>
        /// おみくじ分析
        /// </summary>
        public void AnalysOmikuji()
        {
            MOmikuji omikuji = this.OmikijiEntities.Omikujis.First();
            Console.WriteLine($"おみくじ結果：{this.GetCodeText(omikuji.RankCode)}");
            this.SentimentAnalysOmikuji(omikuji);
        }

        /// <summary>
        /// センチメント分析
        /// </summary>
        /// <param name="omikuji"></param>
        private void SentimentAnalysOmikuji(MOmikuji omikuji)
        {
            Type type = typeof(MOmikuji);
            PropertyInfo[] propertyInfos = type.GetProperties();

            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                // プロパティがおみくじ属性を持つ場合、分析実施
                OmikujiAttribute attribute = propertyInfo.GetCustomAttributes<OmikujiAttribute>().FirstOrDefault();
                if (attribute != null)
                {
                    // 分析内容取得
                    string omikujiText = propertyInfo.GetGetMethod().Invoke(omikuji, null).ToString();
                    this.AnalysOmikujiText(omikuji, omikujiText, attribute.CategoryCode);
                }
            }
        }

        /// <summary>
        /// おみくじ内容分析
        /// </summary>
        /// <param name="omikuji"></param>
        /// <param name="omikujiText"></param>
        /// <param name="categoryCode"></param>
        private void AnalysOmikujiText(MOmikuji omikuji, string omikujiText, string categoryCode)
        {
            // 分析実行
            DocumentSentiment sentiment = this.Client.AnalyzeSentiment(omikujiText);

            // おみくじ分析結果生成
            TOmikujiAnalytics omikujiAnalytics = new TOmikujiAnalytics(omikuji.Id, omikujiText, categoryCode, sentiment);
            Console.WriteLine($"{this.GetCodeText(categoryCode)}：");

            this.OmikijiEntities.OmikujiAnalytics.Add(omikujiAnalytics);
            // 分析結果をコンソールに出力
            this.ConsoleOutput(omikujiText, sentiment.Sentiment, sentiment.ConfidenceScores);
            this.OmikijiEntities.SaveChanges();

            // 分析結果が複数ある場合（文1,文2…)、文章全体の分析結果を親とし、さらに細かく分析し子要素を作成
            if (sentiment.Sentences.Count > 1)
            {
                // 文章をさらに分析（句読点がついている場合に文1,文2…となる)
                foreach (SentenceSentiment sentence in sentiment.Sentences)
                {
                    // 最初の分析結果に紐づけ、おみくじ分析結果生成
                    TOmikujiAnalytics child = new TOmikujiAnalytics(omikuji.Id, omikujiAnalytics.Id, categoryCode, sentence);
                    this.OmikijiEntities.OmikujiAnalytics.Add(child);
                    // 分析結果をコンソールに出力
                    this.ConsoleOutput(sentence.Text, sentence.Sentiment, sentence.ConfidenceScores);
                }

                this.OmikijiEntities.SaveChanges();
            }
        }

        /// <summary>
        /// 分析結果をコンソールに出力
        /// </summary>
        /// <param name="text"></param>
        /// <param name="sentiment"></param>
        /// <param name="scores"></param>
        private void ConsoleOutput(string text, TextSentiment sentiment, SentimentConfidenceScores scores)
        {
            Console.WriteLine($"\tText: \"{text}\"");
            Console.WriteLine($"\tSentence sentiment: {sentiment}");
            Console.WriteLine($"\tPositive score: {scores.Positive:0.00}");
            Console.WriteLine($"\tNegative score: {scores.Negative:0.00}");
            Console.WriteLine($"\tNeutral score: {scores.Neutral:0.00}\n");
        }

        /// <summary>
        /// 名前付きエンティティの認識
        /// </summary>
        public void EntityRecognition()
        {
            Console.WriteLine("・固有名詞の認識。");

            string recognitionText = "今日は天気が良く富士山がきれいに見えた。";
            var responese = this.Client.RecognizeEntities(recognitionText);
            
            Console.WriteLine($"対象文章：{recognitionText}");
            foreach (CategorizedEntity entity in responese.Value)
            {
                Console.WriteLine($"\tText: {entity.Text},\tCategory: {entity.Category},\tSub-Category: {entity.SubCategory}");
                Console.WriteLine($"\t\tScore: {entity.ConfidenceScore:F2}\n");
            }
        }

        /// <summary>
        /// キーフレーズの抽出
        /// </summary>
        public void KeyPhraseExtraction()
        {
            Console.WriteLine("・キーフレーズの抽出");
            var response = this.Client.ExtractKeyPhrases("今日は天気が良く富士山がきれいに見えた。");
            foreach (string keyphrase in response.Value)
            {
                Console.WriteLine($"\t{keyphrase}");
            }
        }

        /// <summary>
        /// コードからテキスト取得
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private string GetCodeText(string code)
        {
            MCode mCode = this.OmikijiEntities.Codes.FirstOrDefault(c => c.Key.Equals(code));
            return mCode == null ? "" : mCode.Value;
        }

        #region データ準備

        /// <summary>
        /// おみくじデータ追加
        /// </summary>
        /// <returns></returns>
        private MOmikuji GetMOmikuji()
        {
            MOmikuji omikuji = new MOmikuji();
            omikuji.RankCode = OmikujiConst.CD_OMIKUJI_KEKKA + OmikujiConst.KEKKA_SUEKICHI;
            omikuji.Body = "暦の上ではうららかな春を迎えながらも依然として冬の様な時候。しかしながら冬ごもりの草木が芽を出すのは目前にある。苦あれば、楽あり。くよくよせずに春を待つがよい。";
            omikuji.Ganbo = "叶う。されど無理な願いはよせ";
            omikuji.Machibito = "遅けれど来たる";
            omikuji.Shitsubutsu = "長引けど出る";
            omikuji.Ryoko = "あせらぬが吉";
            omikuji.Syobai = "後程利益多し";
            omikuji.Hogaku = "北東の方よし";
            omikuji.Gakugyo = "人にたよるな。自分でなせば叶う。";
            omikuji.Arasoigoto = "よくよく見定めよ";
            omikuji.Tenkyo = "さわりなし吉";
            omikuji.Syussan = "思いのほかかるし";
            omikuji.Byoki = "危き様なるも癒ゆ";
            omikuji.Endan = "良縁ありまとまる";

            return omikuji;
        }

        /// <summary>
        /// マスタデータ追加
        /// </summary>
        /// <returns></returns>
        private List<MCode> GetCodes()
        {
            List<MCode> codes = new List<MCode>();
            // 結果追加
            codes.Add(new MCode { Key = OmikujiConst.CD_OMIKUJI_KEKKA + OmikujiConst.KEKKA_DAIKICHI, Code = OmikujiConst.KEKKA_DAIKICHI, Category = OmikujiConst.CD_OMIKUJI_KEKKA, Value = "大吉" });
            codes.Add(new MCode { Key = OmikujiConst.CD_OMIKUJI_KEKKA + OmikujiConst.KEKKA_CHUIKICHI, Code = OmikujiConst.KEKKA_CHUIKICHI, Category = OmikujiConst.CD_OMIKUJI_KEKKA, Value = "中吉" });
            codes.Add(new MCode { Key = OmikujiConst.CD_OMIKUJI_KEKKA + OmikujiConst.KEKKA_SUEKICHI, Code = OmikujiConst.KEKKA_SUEKICHI, Category = OmikujiConst.CD_OMIKUJI_KEKKA, Value = "末吉" });
            codes.Add(new MCode { Key = OmikujiConst.CD_OMIKUJI_KEKKA + OmikujiConst.KEKKA_SYOKICHI, Code = OmikujiConst.KEKKA_SYOKICHI, Category = OmikujiConst.CD_OMIKUJI_KEKKA, Value = "小吉" });
            codes.Add(new MCode { Key = OmikujiConst.CD_OMIKUJI_KEKKA + OmikujiConst.KEKKA_KICHI, Code = OmikujiConst.KEKKA_KICHI, Category = OmikujiConst.CD_OMIKUJI_KEKKA, Value = "吉" });
            codes.Add(new MCode { Key = OmikujiConst.CD_OMIKUJI_KEKKA + OmikujiConst.KEKKA_KYO, Code = OmikujiConst.KEKKA_KYO, Category = OmikujiConst.CD_OMIKUJI_KEKKA, Value = "凶" });
            codes.Add(new MCode { Key = OmikujiConst.CD_OMIKUJI_KEKKA + OmikujiConst.KEKKA_DAIKYO, Code = OmikujiConst.KEKKA_DAIKYO, Category = OmikujiConst.CD_OMIKUJI_KEKKA, Value = "大凶" });
            // カテゴリ追加
            codes.Add(new MCode { Key = OmikujiConst.CD_OMIKUJI_CATEGORY + OmikujiConst.CATEGORY_BODY, Code = OmikujiConst.CATEGORY_BODY, Category = OmikujiConst.CD_OMIKUJI_CATEGORY, Value = "本文" });
            codes.Add(new MCode { Key = OmikujiConst.CD_OMIKUJI_CATEGORY + OmikujiConst.CATEGORY_GANBO, Code = OmikujiConst.CATEGORY_GANBO, Category = OmikujiConst.CD_OMIKUJI_CATEGORY, Value = "願望" });
            codes.Add(new MCode { Key = OmikujiConst.CD_OMIKUJI_CATEGORY + OmikujiConst.CATEGORY_MACHIBITO, Code = OmikujiConst.CATEGORY_MACHIBITO, Category = OmikujiConst.CD_OMIKUJI_CATEGORY, Value = "待人" });
            codes.Add(new MCode { Key = OmikujiConst.CD_OMIKUJI_CATEGORY + OmikujiConst.CATEGORY_SHITSUBUTSU, Code = OmikujiConst.CATEGORY_SHITSUBUTSU, Category = OmikujiConst.CD_OMIKUJI_CATEGORY, Value = "失物" });
            codes.Add(new MCode { Key = OmikujiConst.CD_OMIKUJI_CATEGORY + OmikujiConst.CATEGORY_RYOKO, Code = OmikujiConst.CATEGORY_RYOKO, Category = OmikujiConst.CD_OMIKUJI_CATEGORY, Value = "旅行" });
            codes.Add(new MCode { Key = OmikujiConst.CD_OMIKUJI_CATEGORY + OmikujiConst.CATEGORY_SYOBAI, Code = OmikujiConst.CATEGORY_SYOBAI, Category = OmikujiConst.CD_OMIKUJI_CATEGORY, Value = "商売" });
            codes.Add(new MCode { Key = OmikujiConst.CD_OMIKUJI_CATEGORY + OmikujiConst.CATEGORY_HOGAKU, Code = OmikujiConst.CATEGORY_HOGAKU, Category = OmikujiConst.CD_OMIKUJI_CATEGORY, Value = "方角" });
            codes.Add(new MCode { Key = OmikujiConst.CD_OMIKUJI_CATEGORY + OmikujiConst.CATEGORY_GAKUGYO, Code = OmikujiConst.CATEGORY_GAKUGYO, Category = OmikujiConst.CD_OMIKUJI_CATEGORY, Value = "学業" });
            codes.Add(new MCode { Key = OmikujiConst.CD_OMIKUJI_CATEGORY + OmikujiConst.CATEGORY_ARASOIGOTO, Code = OmikujiConst.CATEGORY_ARASOIGOTO, Category = OmikujiConst.CD_OMIKUJI_CATEGORY, Value = "争事" });
            codes.Add(new MCode { Key = OmikujiConst.CD_OMIKUJI_CATEGORY + OmikujiConst.CATEGORY_TENKYO, Code = OmikujiConst.CATEGORY_TENKYO, Category = OmikujiConst.CD_OMIKUJI_CATEGORY, Value = "転居" });
            codes.Add(new MCode { Key = OmikujiConst.CD_OMIKUJI_CATEGORY + OmikujiConst.CATEGORY_SYUSSAN, Code = OmikujiConst.CATEGORY_SYUSSAN, Category = OmikujiConst.CD_OMIKUJI_CATEGORY, Value = "出産" });
            codes.Add(new MCode { Key = OmikujiConst.CD_OMIKUJI_CATEGORY + OmikujiConst.CATEGORY_BYOKI, Code = OmikujiConst.CATEGORY_BYOKI, Category = OmikujiConst.CD_OMIKUJI_CATEGORY, Value = "病気" });
            codes.Add(new MCode { Key = OmikujiConst.CD_OMIKUJI_CATEGORY + OmikujiConst.CATEGORY_ENDAN, Code = OmikujiConst.CATEGORY_ENDAN, Category = OmikujiConst.CD_OMIKUJI_CATEGORY, Value = "縁談" });

            return codes;
        }

        #endregion

    }
}
