﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Yomiage.SDK.VoiceEffects
{
    /// <summary>
    /// マスター音声効果
    /// </summary>
    public class MasterEffectValue : VoiceEffectValueBase
    {
        /// <summary>
        /// 短ポーズの設定値 [ms]
        /// </summary>
        public double ShortPause { get; set; }
        /// <summary>
        /// 長ポーズの設定値 [ms]
        /// </summary>
        public double LongPause { get; set; }
        /// <summary>
        /// 文末ポーズの設定値 [ms]
        /// </summary>
        public double EndPause { get; set; }

    }
}