
namespace MDPro3.Utility
{
    public static class Language
    {
        public const string ConfigName = "Language";
        public const string CardConfigName = "CardLanguage";
        public const string English = "en-US";
        public const string Spanish = "es-ES";
        public const string Japanese = "ja-JP";
        public const string Korean = "ko-KR";
        public const string SimplifiedChinese = "zh-CN";
        public const string TraditionalChinese = "zh-TW";
        public const string Portuguese = "pt-PT";
        private const string MasterDuelPortuguese = "pt-BR";

        public static string GetConfig()
        {
            return Config.Get(ConfigName, SimplifiedChinese);
        }

        public static string GetCardConfig()
        {
            return Config.Get(CardConfigName, SimplifiedChinese);
        }

        public static bool NeedBlankToAddWord()
        {
            var config = GetConfig();
            if (config == English || config == Spanish || config == Korean || config == Portuguese)
                return true;
            return false;
        }

        public static bool UseLatin()
        {
            var config = GetConfig();
            if (config == English || config == Spanish || config == Portuguese)
                return true;
            return false;
        }

        public static bool CardUseLatin()
        {
            var config = GetCardConfig();
            if (config == English || config == Spanish || config == Portuguese)
                return true;
            return false;
        }

        public static bool CardNeedSmallBracket()
        {
            var config = GetCardConfig();
            if (config == English || config == Spanish || config == Portuguese || config == Korean)
                return true;
            return false;
        }

        public static string GetMasterDuelLanguage(string language)
        {
            if (language == Portuguese)
                return MasterDuelPortuguese;
            return language;
        }

    }
}
