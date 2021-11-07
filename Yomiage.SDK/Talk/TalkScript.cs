﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using Yomiage.SDK.Common;
using Yomiage.SDK.Config;
using Yomiage.SDK.VoiceEffects;

namespace Yomiage.SDK.Talk
{
    /// <summary>
    /// 文章の読み上げ指示情報
    /// フレーズ編集情報
    /// </summary>
    public class TalkScript
    {
        /// <summary>
        /// ユーザーが打ち込んだ文字列そのもの
        /// </summary>
        public string OriginalText { get; set; }
        /// <summary>
        /// ボイスプリセットタグで指定がある場合に一時的に使用される。
        /// </summary>
        [JsonIgnore]
        public string PresetName { get; set; }
        /// <summary>
        /// 複数のmoraとポーズからなるアクセント句のリスト。
        /// </summary>
        public List<Section> Sections { get; set; } = new List<Section>();
        /// <summary>
        /// 文末のポーズと音声効果。モーラは無いかわりに、？や！などの記号情報が入る。
        /// </summary>
        [JsonPropertyName("E")]
        public EndSection EndSection { get; set; } = new EndSection();

        /// <summary>
        /// モーラのトータル数
        /// </summary>
        public int MoraCount
        {
            get => this.Sections.Select(s => s.Moras.Count).Sum();
        }

        /// <summary>
        /// 読み（全部カタカナ）を取得する。
        /// </summary>
        public string GetYomi(bool withSpace = true)
        {
            string yomi = "";
            this.Sections.ForEach(s =>
            {
                s.Moras.ForEach(m =>
                {
                    yomi += m.Character;
                });
                if (withSpace)
                {
                    yomi += "　";
                }
            });
            yomi += EndSection.EndSymbol;
            return yomi;
        }

        /// <summary>
        /// nullになっている部分を全て埋める
        /// </summary>
        public void Fill(EngineConfig config, int shortPause, int longPause)
        {
            var section = new VoiceEffectValue(config);
            var mora = new VoiceEffectValue(config);
            Sections.ForEach(s => s.Fill(section, mora, shortPause, longPause));
            EndSection.Fill(section, mora, config, shortPause, longPause);
            var curve = new VoiceEffectValue(config);
            EndSection.FillCurve(curve, config);
            for (int i = Sections.Count - 1; i >= 0; i--)
            {
                Sections[i].FillCurve(curve, config);
            }
        }

        /// <summary>
        /// 不要なパラメータを削除する。
        /// </summary>
        /// <param name="engineConfig"></param>
        public void RemoveUnnecessaryParameters(EngineConfig engineConfig)
        {
            Sections.ForEach(s =>
            {
                s.RemoveUnnecessaryParameters(engineConfig);
                s.Moras.ForEach(m => m.RemoveUnnecessaryParameters(engineConfig));
            });
            EndSection.RemoveUnnecessaryParameters(engineConfig);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="PromptString"></param>
        /// <returns></returns>
        public string GetOriginalTextWithPresetName(string PromptString = "＞")
        {
            if (!string.IsNullOrWhiteSpace(PresetName))
            {
                return PresetName + PromptString + OriginalText;
            }
            return OriginalText;
        }

        /// <summary>
        /// Jsonに保存する用のテキストを取得する。
        /// </summary>
        public string GetPhraseJsonText_toSave()
        {
            this.OriginalText = null;
            var copy = JsonUtil.DeepClone(this);
            var jsonText = JsonUtil.SerializeToString(copy);
            if (jsonText.Contains("!"))
            {
                return jsonText;
            }
            return jsonText.Replace("\"", "!");
        }

        /// <summary>
        /// GetPhraseJsonText_toSave() で作った保存用テキストから
        /// 読み上げ指示情報　を復元する。
        /// </summary>
        public static TalkScript GetTalkScript_fromPhraseJsonText(string jsonText)
        {
            if (jsonText.Contains("\""))
            {
                return JsonUtil.DeserializeFromString<TalkScript>(jsonText);
            }
            return JsonUtil.DeserializeFromString<TalkScript>(jsonText.Replace("!", "\""));
        }

    }
}
