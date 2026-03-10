using System;
using System.Data;
using MDPro3.Utility;
using MDPro3.Duel.YGOSharp;
using System.Diagnostics;
using static YgomSystem.UI.ViewController;
using MDPro3.Net;

namespace MDPro3.Duel.YGOSharp
{
    public class Card
    {
        public const int MAX_BATTLE_POWER_DISPLAY = 99_999_999;
        public const int UNKNOWN_BATTLE_VALUE = -2;

        public int Id;
        public int Ot;
        public int Alias;
        public long Setcode;
        public int Type;

        public int Level;
        public int LScale;
        public int RScale;
        public int LinkMarker;

        public int Attribute;
        public int Race;
        public int Attack;
        public int Defense;
        public int rAttack;
        public int rDefense;
        public int Reason;
        public int ReasonCard;

        public Int64 Category;
        public string Name;
        public string Desc;
        public string[] Str;

        public string packShortName = "";
        public string packFullName = "";
        public string reality = "";
        public string strSetName = "";
        public int year = 0;
        public int month = 0;
        public int day = 0;
        public bool isPre = false;

        public enum LevelType
        {
            Level,
            Rank,
            Link
        }

        public Card()
        {
            Id = 0;
            Str = new string[16];
            Name = CardsManager.nullName;
            Desc = CardsManager.nullString;
        }

        public Card Clone()
        {
            Card r = new Card();
            r.Id = Id;
            r.Ot = Ot;
            r.Alias = Alias;
            r.Setcode = Setcode;
            r.Type = Type;
            r.Level = Level;
            r.LScale = LScale;
            r.RScale = RScale;
            r.LinkMarker = LinkMarker;
            r.Attribute = Attribute;
            r.Race = Race;
            r.Attack = Attack;
            r.Defense = Defense;
            r.rAttack = rAttack;
            r.rDefense = rDefense;
            r.Category = Category;
            r.Name = Name;
            r.Desc = Desc;
            r.Str = new string[Str.Length];
            r.isPre = isPre;

            for (int ii = 0; ii < Str.Length; ii++)
            {
                r.Str[ii] = Str[ii];
            }
            return r;
        }

        public void CloneTo(Card r)
        {
            r.Id = Id;
            r.Ot = Ot;
            r.Alias = Alias;
            r.Setcode = Setcode;
            r.Type = Type;
            r.Level = Level;
            r.LScale = LScale;
            r.RScale = RScale;
            r.Attribute = Attribute;
            r.Race = Race;
            r.Attack = Attack;
            r.Defense = Defense;
            r.rAttack = rAttack;
            r.rDefense = rDefense;
            r.Category = Category;
            r.Name = Name;
            r.Desc = Desc;
            r.Str = new string[Str.Length];
            r.isPre = isPre;

            for (int ii = 0; ii < Str.Length; ii++)
            {
                r.Str[ii] = Str[ii];
            }
        }

        public static Card Get(int id)
        {
            return CardsManager.GetCard(id);
        }

        internal Card(IDataRecord reader)
        {
            Str = new string[16];
            Id = (int)reader.GetInt64(0);
            Ot = reader.GetInt32(1);
            Alias = (int)reader.GetInt64(2);
            Setcode = reader.GetInt64(3);
            Type = (int)reader.GetInt64(4);
            Attack = reader.GetInt32(5);
            Defense = reader.GetInt32(6);
            rAttack = this.Attack;
            rDefense = this.Defense;
            long Level_raw = reader.GetInt64(7);
            Level = (int)Level_raw & 0xff;
            LScale = (int)((Level_raw >> 0x18) & 0xff);
            RScale = (int)((Level_raw >> 0x10) & 0xff);
            LinkMarker = this.Defense;
            Race = reader.GetInt32(8);
            Attribute = reader.GetInt32(9);
            Category = reader.GetInt64(10);
            Name = reader.GetString(12);
            Desc = reader.GetString(13);

            for (int ii = 0; ii < 0x10; ii++)
            {
                Str[ii] = reader.GetString(14 + ii);
            }
        }



        #region Tools

        public bool HasType(CardType type)
        {
            return (Type & (int)type) != 0;
        }

        public bool HasType(CardType type1, CardType type2)
        {
            return HasType(type1) || HasType(type2);
        }

        public bool HasLinkMarker(CardLinkMarker dir)
        {
            return (LinkMarker & (int)dir) != 0;
        }

        public bool IsExtraCard()
        {
            return (HasType(CardType.Fusion) || HasType(CardType.Synchro) || HasType(CardType.Xyz) || HasType(CardType.Link));
        }

        public bool IsSameCard(Card data)
        {
            return GetOriginalID() == data.GetOriginalID();
        }

        public int GetLinkCount()
        {
            int returnValue = 0;
            for (int i = 0; i < 9; i++)
                if (((LinkMarker >> i) & 1u) > 0 && i != 4)
                    returnValue++;
            return returnValue;
        }

        public int GetOriginalID()
        {
            if (Alias > 0)
                return Alias;
            return Id;
        }

        public LevelType GetLevelType()
        {
            if (HasType(CardType.Link))
                return LevelType.Link;
            else if (HasType(CardType.Xyz))
                return LevelType.Rank;
            else
                return LevelType.Level;
        }

        public bool IsHighLevel()
        {
            if (HasType(CardType.Link) && GetLinkCount() > 2)
                return true;
            if (HasType(CardType.Xyz) && Level > 3)
                return true;
            if (Level > 6)
                return true;
            return false;
        }

        public bool IsAttribute(CardAttribute attribute)
        {
            return (Attribute & (uint)attribute) > 0;
        }

        public int GetGenesysPoint()
        {
            return OnlineService.GetGenesysPoint(GetOriginalID());
        }

        #endregion

        #region String

        public string GetAttackString()
        {
            return FormatBattleValue(Attack);
        }

        public string GetDefenseString()
        {
            return FormatBattleValue(Defense);
        }

        public static int NormalizeBattleValue(int value, bool keepUnknown = true)
        {
            if (value == UNKNOWN_BATTLE_VALUE)
                return keepUnknown ? UNKNOWN_BATTLE_VALUE : 0;
            if (value < 0)
                return MAX_BATTLE_POWER_DISPLAY;
            if (value > MAX_BATTLE_POWER_DISPLAY)
                return MAX_BATTLE_POWER_DISPLAY;
            return value;
        }

        public static string FormatBattleValue(int value)
        {
            var normalized = NormalizeBattleValue(value, true);
            return normalized == UNKNOWN_BATTLE_VALUE ? "?" : normalized.ToString();
        }

        private static readonly string PendulumSeparatorLine = new string('─', 14);

        public string GetDescription(bool withSetName = false)
        {
            if (!HasType(CardType.Pendulum))
                return (withSetName ? GetSetNameWithColor() : string.Empty) + Desc;

            var setName = withSetName ? GetSetNameWithColor() : string.Empty;

            var texts = GetDescriptionSplit();
            var pendulumText = texts[0]?.Trim('\r', '\n');
            var monsterText = texts[1]?.Trim('\r', '\n');

            var language = Language.GetConfig();

            // Apply "official-like" panel layout for ALL languages except Korean:
            // Monster text (no header) -> separator -> [Pendulum Effect] -> pendulum text
            if (language != Language.Korean)
            {
                var pendulumHeader = NormalizeBracketLabel(InterString.Get("【灵摆效果】"));

                var result = setName + (monsterText ?? string.Empty);

                if (!string.IsNullOrEmpty(pendulumText))
                {
                    if (!string.IsNullOrEmpty(monsterText))
                        result += "\n" + PendulumSeparatorLine + "\n";
                    else if (!string.IsNullOrEmpty(result))
                        result += "\n";

                    result += pendulumHeader + "\n" + pendulumText;
                }

                return result;
            }

            // Korean: keep the old behavior (already working for you)
            string monsterHeader = InterString.Get("【怪兽效果】");
            if (!HasType(CardType.Effect))
                monsterHeader = InterString.Get("【怪兽描述】");

            var pendulumHeaderK = NormalizeBracketLabel(InterString.Get("【灵摆效果】"));
            monsterHeader = NormalizeBracketLabel(monsterHeader);

            return setName
                + pendulumHeaderK + "\n" + (pendulumText ?? string.Empty)
                + "\n" + monsterHeader + "\n" + (monsterText ?? string.Empty);
        }

        private static string NormalizeBracketLabel(string label)
        {
            if (string.IsNullOrEmpty(label))
                return string.Empty;

            // Remove extra spaces inside brackets: "[ xxx ]" -> "[xxx]" / "【 xxx 】" -> "【xxx】"
            label = label.Replace("[ ", "[")
                         .Replace(" ]", "]")
                         .Replace("【 ", "【")
                         .Replace(" 】", "】");

            // Ensure bracket glyphs match current UI language.
            // TrimEnd so we don't accidentally embed the " ] " trailing space inside the label itself.
            var left = Language.GetLeftBracket();
            var right = Language.GetRightBracket().TrimEnd();

            label = label.Replace("【", left).Replace("】", right)
                         .Replace("[", left).Replace("]", right);

            return label.Trim();
        }

        public string GetMonsterDescription(bool render = false)
        {
            if (HasType(CardType.Pendulum))
                return GetDescriptionSplit(render)[1];
            else
                return Desc;
        }

        public string GetPendulumDescription(bool render = false)
        {
            if (HasType(CardType.Pendulum))
                return GetDescriptionSplit(render)[0];
            else
                return string.Empty;
        }

        /// <summary>
        /// 仅在卡片是灵摆卡时使用
        /// </summary>
        /// <param name="render"></param>
        /// <returns></returns>
        public string[] GetDescriptionSplit(bool render = false)
        {
            var returnValue = new string[2];
            returnValue[0] = string.Empty;
            returnValue[1] = string.Empty;
            var lines = Desc.Replace("\r", "").Split('\n');
            var language = render ? Language.GetCardConfig() : Language.GetConfig();

            // Chinese Japanese Korean
            int beforePendulum = 1;
            int splitLines = 1;
            string symbol = "【";
            int monsterStart = 0;

            if (language == Language.English
                || language == Language.Portuguese
                || language == Language.French
                || language == Language.German
                || language == Language.Italian)
            {
                beforePendulum = 2;
                splitLines = 2;
                symbol = "[";
            }
            else if (language == Language.Spanish)
            {
                beforePendulum = 2;
                splitLines = 2;
            }
            else if (language == Language.TraditionalChinese)
            {
                beforePendulum = 0;
            }

            for (int i = beforePendulum; i < lines.Length; i++)
                if (lines[i].StartsWith(symbol))
                {
                    monsterStart = i;
                    break;
                }

            for (int i = beforePendulum; i < lines.Length; i++)
            {
                if (i <= monsterStart - splitLines)
                {
                    if (monsterStart - i == splitLines)
                        returnValue[0] += lines[i];
                    else
                        returnValue[0] += lines[i] + Program.STRING_LINE_BREAK;
                }
                else if (i > monsterStart)
                {
                    if (i == lines.Length - 1)
                        returnValue[1] += lines[i];
                    else
                        returnValue[1] += lines[i] + Program.STRING_LINE_BREAK;
                }
            }
            if (language == Language.Spanish)
                returnValue[0] = returnValue[0].Replace("-n/a-", string.Empty);
            return returnValue;
        }

        /// <summary>
        /// 获取卡片的字段。
        /// </summary>
        /// <returns>e.g: "No.|未来皇 霍普\r\n"</returns>
        public string GetSetName()
        {
            return StringHelper.GetSetName(Setcode);
        }

        /// <summary>
        /// 获取卡片的字段，用于卡片描述中，不为空时以换行符结尾。
        /// </summary>
        /// <returns>e.g: "<color=#FFF000>系列：No.|未来皇 霍普</color>\r\n"</returns>
        public string GetSetNameWithColor()
        {
            var returnValue = GetSetName();
            if (returnValue.Length > 0)
            {
                returnValue = $"<color=#FFF000>{StringHelper.GetUnsafe(1329)}{returnValue}</color>{Program.STRING_LINE_BREAK}";
            }
            return returnValue;
        }

        /// <summary>
        /// 获取卡片的字段，用于卡片详情页中。
        /// </summary>
        /// <returns>e.g: "【No.|未来皇 霍普】"</returns>
        public string GetSetNameWithBracket()
        {
            var returnValue = GetSetName();
            if (returnValue.Length > 0)
            {
                returnValue = $"{Language.GetLeftBracket()}{returnValue}{Language.GetRightBracket()}";
            }
            return returnValue;
        }

        public string GetIdWithBracket()
        {
            var re = $"{Language.GetLeftBracket()}{Id}";
            if (Alias != 0 && Alias != Id)
            {
                re += $"/{Alias}";
            }
            re += Language.GetRightBracket();
            return re;
        }

        public string GetAttributeString(bool render = false)
        {
            var type = render ? 1 : 0;
            if (render && isPre)
                type = 2;
            return StringHelper.Attribute(Attribute, type);
        }

        public string GetRaceString(bool render = false)
        {
            var type = render ? 1 : 0;
            if (render && isPre)
                type = 2;
            return StringHelper.Race(Race, type);
        }

        public string GetMainTypeString(bool render = false)
        {
            var type = render ? 1 : 0;
            if (render && isPre)
                type = 2;
            return StringHelper.MainType(Type, type);
        }

        public string GetSecondType(bool render = false)
        {
            var type = render ? 1 : 0;
            if (render && isPre)
                type = 2;
            return StringHelper.SecondType(Type, type);
        }

        public string GetSpellTrapType(bool render = false)
        {
            var type = 0;
            if (render)
            {
                type = 1;
                if (isPre)
                    type = 2;
            }

            return GetSpellTrapType(Type, type);
        }

        public static string GetSpellTrapType(int cardType, int type = 0)
        {
            if ((cardType & (int)CardType.Spell) > 0)
            {
                if ((cardType & (int)CardType.Field) > 0)
                    return InterString.Get("场地魔法", type);
                else if ((cardType & (int)CardType.QuickPlay) > 0)
                    return InterString.Get("速攻魔法", type);
                else if ((cardType & (int)CardType.Continuous) > 0)
                    return InterString.Get("永续魔法", type);
                else if ((cardType & (int)CardType.Equip) > 0)
                    return InterString.Get("装备魔法", type);
                else if ((cardType & (int)CardType.Ritual) > 0)
                    return InterString.Get("仪式魔法", type);
                else
                    return InterString.Get("通常魔法", type);
            }
            else if ((cardType & (int)CardType.Trap) > 0)
            {
                if ((cardType & (int)CardType.Continuous) > 0)
                    return InterString.Get("永续陷阱", type);
                else if ((cardType & (int)CardType.Counter) > 0)
                    return InterString.Get("反击陷阱", type);
                else
                    return InterString.Get("通常陷阱", type);
            }
            return string.Empty;
        }

        public string GetTypeForUI()
        {
            var re = string.Empty;
            if (Id == 0)
                return re;
            var bracketLeft = Language.GetLeftBracket();
            var bracketRight = Language.GetRightBracket();
            if (HasType(CardType.Monster))
                re = $"{bracketLeft}{InterString.Get("[?]族", GetRaceString())}{Program.STRING_SLASH}{GetSecondType()}{bracketRight}";
            else
                re = $"{bracketLeft}{StringHelper.MainType(Type)}{bracketRight}";
            return re;
        }

        public string GetTypeForRushDuelRender()
        {
            var re = string.Empty;
            if (Id == 0)
                return re;

            var bracketLeft = "【";
            var bracketRight = "】";
            if (Language.NeedSmallBracket(isPre ? Language.GetPrereleaseConfig() : Language.GetCardConfig()))
            {
                bracketLeft = "[";
                bracketRight = "] ";
            }

            if (HasType(CardType.Monster))
            {
                re = $"{bracketLeft}{InterString.Get("[?]族", GetRaceString(true), isPre ? 2 : 1)}{Program.STRING_SLASH}{GetSecondType(true)}{bracketRight}";
            }
            else
            {
                var type = 1;
                if (isPre)
                    type = 2;
                re = bracketLeft;
                if (HasType(CardType.Spell))
                    re += InterString.Get("魔法卡", type);
                else
                    re += InterString.Get("陷阱卡", type);
                var secondType = GetSecondType(true);
                if (secondType != StringHelper.GetUnsafe(1054, type))
                    re += Program.STRING_SLASH + secondType + GetSpellTrapTypeIconCode();
                re += bracketRight;
            }

            re = re.Replace(Program.STRING_SLASH,
                (isPre ? Language.UseLatin(Language.GetPrereleaseConfig()) : Language.CardUseLatin())
                ? CardRenderer.SMALL_SLASH : CardRenderer.BIG_SLASH);

            return re;
        }

        public string GetSpellTypeForOCGRender()
        {
            var re = string.Empty;
            if (Id == 0 || HasType(CardType.Monster))
                return re;

            var bracketLeft = "【";
            var bracketRight = "】";
            if (Language.NeedSmallBracket(isPre ? Language.GetPrereleaseConfig() : Language.GetCardConfig()))
            {
                bracketLeft = "[";
                bracketRight = "]";
            }
            re = bracketLeft;
            if (HasType(CardType.Spell))
                re += InterString.Get("魔法卡", isPre ? 2 : 1);
            else
                re += InterString.Get("陷阱卡", isPre ? 2 : 1);
            re += GetSpellTrapTypeIconCode() + bracketRight;

            return re;
        }

        private string GetSpellTrapTypeIconCode()
        {
            var re = string.Empty;
            if (HasType(CardType.Equip))
                re += "<Sprite=0>";
            if (HasType(CardType.QuickPlay))
                re += "<Sprite=1>";
            if (HasType(CardType.Field))
                re += "<Sprite=2>";
            if (HasType(CardType.Ritual))
                re += "<Sprite=3>";
            if (HasType(CardType.Continuous))
                re += "<Sprite=4>";
            if (HasType(CardType.Counter))
                re += "<Sprite=5>";
            return re;
        }

        #endregion

    }
}
