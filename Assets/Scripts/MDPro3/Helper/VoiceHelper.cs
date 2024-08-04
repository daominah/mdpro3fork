using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace MDPro3
{
    public static class VoiceHelper
    {


        public const string voicePath = "Sound/Voice";
        public const string customVoicePath = "Sound/CustomVoice";
        public const string jsonPath = "Data/locales/";

        public static string language = "zh-CN";
        static string hero = "0601";
        static string rival = "0001";
        static VoicesData heroVoices;
        static LinesData heroLines;
        static VoicesData rivalVoices;
        static LinesData rivalLines;

        public static void LoadVoicesAndInfo()
        {
            var dataPath = jsonPath + language + "/voice/V" + hero + ".json";
            var txt = File.ReadAllText(dataPath);
            heroVoices = JsonConvert.DeserializeObject<VoicesData>(txt);

            dataPath = dataPath.Replace("/voice/V", "/voice/SN");
            txt = File.ReadAllText(dataPath);
            heroLines = JsonConvert.DeserializeObject<LinesData>(txt);
        }

    }

    [Serializable]
    public class VoiceInfo
    {
        public string fullName;
        public string shortName;
        public string voiceIdx;
        public int[] cards;
        public int[] duelists;
        public int[] skills;
        public int[] situations;
        public int[] engineparams;
        public object groupingKeys;
        public int subCategoryIndex;
        public int patternIndex;
    }

    [Serializable]
    public class VoiceInfoEntry
    {
        public Dictionary<string, VoiceInfo> rawKvp;
    }

    [Serializable]
    public class VoicesData
    {
        public int DummyFlag;
        public Dictionary<string, int> NumVoices;
        public int labelver;
        public VoiceInfoEntry BeforeDuel;
        public VoiceInfoEntry DuelStart;
        public VoiceInfoEntry TurnStart;
        public VoiceInfoEntry Draw;
        public VoiceInfoEntry DestinyDraw;
        public VoiceInfoEntry BeforeCardEffect;
        public VoiceInfoEntry CardEffect;
        public VoiceInfoEntry MainMagicTrap;
        public VoiceInfoEntry MainMonsterEffect;
        public VoiceInfoEntry BeforeSummon;
        public VoiceInfoEntry Summon;
        public VoiceInfoEntry None;
        public VoiceInfoEntry MainMonsterSummon;
        public VoiceInfoEntry BattleStart;
        public VoiceInfoEntry BeforeAttackNormal;
        public VoiceInfoEntry BeforeAttackFinish;
        public VoiceInfoEntry Attack;
        public VoiceInfoEntry DirectAttack;
        public VoiceInfoEntry MainMonsterAttack;
        public VoiceInfoEntry CardSet;
        public VoiceInfoEntry TurnEnd;
        public VoiceInfoEntry Damage;
        public VoiceInfoEntry FinishDamage;
        public VoiceInfoEntry CostDamage;
        public VoiceInfoEntry BigDamage;
        public VoiceInfoEntry AfterDamage;
        public VoiceInfoEntry AfterBigDamage;
        public VoiceInfoEntry Win;
        public VoiceInfoEntry Lose;
        public VoiceInfoEntry Taunt;
        public VoiceInfoEntry Surprise;
        public VoiceInfoEntry Title;
        public VoiceInfoEntry Skill;
        public VoiceInfoEntry Chat;
        public VoiceInfoEntry CharaChange;
        public VoiceInfoEntry SwitchToPartner;
        public VoiceInfoEntry BeforeMainSummon;
        public VoiceInfoEntry RidingDuelStart;
        public VoiceInfoEntry CoinTossOfMagicTrap;
        public VoiceInfoEntry CoinTossOfMonster;
        public VoiceInfoEntry BeforeDimensionDuel;
        public VoiceInfoEntry DimensionDuelStart;
        public VoiceInfoEntry Transformation;
        public VoiceInfoEntry ActionDuelStart;
        public VoiceInfoEntry ActionCard;
        public VoiceInfoEntry BeforeMainReincarnationSummon;
        public VoiceInfoEntry MainMonsterReincarnationSummon;
        public VoiceInfoEntry RushDuelStart;
        public VoiceInfoEntry RidingRushDuelStart;

    }


    [Serializable]
    public class LineInfo
    {
        public int face;
        public int frame;
        public int cutin;
        public int[] card;
        public int[] duelist;
        public string text;
    }

    [Serializable]
    public class LinesData
    {
        public string _GRP_;
        public string _LNG_;
        public Dictionary<string, LineInfo> info;
    }
}


