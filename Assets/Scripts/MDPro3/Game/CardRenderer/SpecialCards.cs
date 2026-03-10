
using MDPro3.Duel.YGOSharp;
using Newtonsoft.Json;
using System;
using System.IO;

namespace MDPro3
{
    public static class SpecialCards
    {
        [Serializable]
        private class SpecialCardLists
        {
            public int[] FinalAttackBlueEyes;
            public int[] FinalAttackDarkM;
            public int[] FinalAttackRedEyes;
            public int[] FinalAttackObelisk;
            public int[] FinalAttackRa;
            public int[] FinalAttackSlifer;

            public int[] LevelZeroMonsters;

            public SpecialCardLists()
            {
                FinalAttackBlueEyes = new int[]
                {
                    89631139,   //青眼白龙
                    53347303,   //青眼光龙
                    22804410,   //渊眼白龙
                    38517737,   //青眼亚白龙
                    30576089,   //青眼喷气龙
                    9433350,    //罪 青眼白龙
                    53183600,   //青眼卡通龙
                    23995346,   //青眼究极龙
                    43228023,   //青眼究极亚龙
                    56532353,   //真青眼究极龙
                    2129638,    //青眼双爆裂龙
                    11443677    //青眼暴君龙
                };

                FinalAttackDarkM = new int[]
                {
                    46986414,   //黑魔术师
                    92377303,   //黑衣大贤者
                    342673,       //黑色魔术师-黑魔术师
                    21296502,   //卡通黑魔术师
                    29436665,   //黑魔导执行官
                    35191415,   //黑幻想之魔术师
                    38033121,   //黑魔术少女
                    90960358,   //卡通黑魔术少女
                    50237654    //超魔导师-黑魔术师徒
                };

                FinalAttackRedEyes = new int[]
                {
                    74677422,   //真红眼黑龙
                    96561011,   //真红眼暗龙
                    64335804,   //真红眼黑钢龙
                    18491580,   //真红眼亚黑龙
                    55343236,   //罪 真红眼黑龙
                    6556909,     //真红之魂
                };

                FinalAttackObelisk = new int[]
                {
                    10000000   //巨神兵
                };

                FinalAttackRa = new int[]
                {
                    10000010,   //翼神龙
                    10000080,   //蛋
                    10000090,   //不死鸟
                };

                FinalAttackSlifer = new int[]
                {
                    10000020   //天空龙
                };

                LevelZeroMonsters = new int[]
                {
                    1686814,
                    90884403,
                    26973555,
                    43490025,
                    65305468,
                    52653092,
                };
            }
        }

        private const string PATH_JSON = Program.PATH_DATA + "SpecialCards.json";
        private static SpecialCardLists lists;
        private static SpecialCardLists Lists
        {
            get
            {
                if (lists == null)
                    Initialize();
                return lists;
            }
        }

        private static void Initialize()
        {
            if (!File.Exists(PATH_JSON))
            {
                lists = new SpecialCardLists();
                SaveJson(lists);
                return;
            }

            var json = File.ReadAllText(PATH_JSON);
            try
            {
                lists = JsonConvert.DeserializeObject<SpecialCardLists>(json);
            }
            catch (JsonReaderException ex)
            {
                MessageManager.Cast("Failed to parse SpecialCards.json: " + ex.Message);
                lists = new SpecialCardLists();
            }
            finally
            {
                lists ??= new SpecialCardLists();
            }
        }

        private static void SaveJson(SpecialCardLists lists)
        {
            var json = JsonConvert.SerializeObject(lists, Formatting.Indented);
            File.WriteAllText(PATH_JSON, json);
        }

        public static bool IsLevelZeroMonster(this Card data)
        {
            var code = data.GetOriginalID();
            return Array.Exists(Lists.LevelZeroMonsters, c => c == code);
        }

        public static FinalAttackType GetFinalAttackType(int code)
        {
            var data = CardsManager.Get(code);
            var id = data.GetOriginalID();

            if (Array.Exists(Lists.FinalAttackBlueEyes, c => c == id))
                return FinalAttackType.BlueEyes;
            if (Array.Exists(Lists.FinalAttackDarkM, c => c == id))
                return FinalAttackType.DarkM;
            if (Array.Exists(Lists.FinalAttackRedEyes, c => c == id))
                return FinalAttackType.RedEyes;
            if (Array.Exists(Lists.FinalAttackObelisk, c => c == id))
                return FinalAttackType.Obelisk;
            if (Array.Exists(Lists.FinalAttackRa, c => c == id))
                return FinalAttackType.Ra;
            if (Array.Exists(Lists.FinalAttackSlifer, c => c == id))
                return FinalAttackType.Slifer;

            return FinalAttackType.Normal;
        }

        public enum FinalAttackType
        {
            Normal,
            BlueEyes,
            DarkM,
            RedEyes,
            Obelisk,
            Ra,
            Slifer,
        }

    }
}
