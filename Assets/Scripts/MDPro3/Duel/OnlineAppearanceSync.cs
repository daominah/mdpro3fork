using MDPro3.Duel.YGOSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDPro3
{
    public struct OnlineAppearanceData
    {
        public int Case;
        public int Protector;
        public int Field;
        public int Grave;
        public int Stand;
        public int Mate;
        public int Face;
        public int Frame;
    }

    public static class OnlineAppearanceSync
    {
        private const int DefaultCase = 1080001;
        private const int DefaultProtector = 1070001;
        private const int DefaultField = 1090001;
        private const int DefaultGrave = 1100001;
        private const int DefaultStand = 1110001;
        private const int DefaultMate = 1000001;
        private const int DefaultFace = 1010001;
        private const int DefaultFrame = 1030001;

        public static bool IsSyncMessage(string content)
        {
            return TryExtractPayload(content, out _);
        }

        public static string BuildMessage(Deck deck)
        {
            var data = BuildValidatedData(deck);
            var ints = new int[]
            { data.Case, data.Protector, data.Field, data.Grave, data.Stand, data.Mate, data.Face, data.Frame };
            return ZeroWidthIntsCodec.Encode(ints);
        }

        public static string BuildMessageForLocalPlayer(Deck deck)
        {
            var source = BuildSourceDeckForLocalPlayer(deck);
            return BuildMessage(source);
        }

        public static bool TryParse(string content, out OnlineAppearanceData data)
        {
            data = default;
            if (!TryExtractPayload(content, out var payload))
                return false;

            data = new OnlineAppearanceData
            {
                Case = EnsureValidCode(payload[0], Items.ItemType.Case, DefaultCase),
                Protector = EnsureValidCode(payload[1], Items.ItemType.Protector, DefaultProtector),
                Field = EnsureValidCode(payload[2], Items.ItemType.Mat, DefaultField),
                Grave = EnsureValidCode(payload[3], Items.ItemType.Grave, DefaultGrave),
                Stand = EnsureValidCode(payload[4], Items.ItemType.Stand, DefaultStand),
                Mate = EnsureValidCode(payload[5], Items.ItemType.Mate, DefaultMate),
                Face = EnsureValidCode(payload[6], Items.ItemType.Face, DefaultFace),
                Frame = EnsureValidCode(payload[7], Items.ItemType.Frame, DefaultFrame),
            };
            return true;
        }

        private static bool TryExtractPayload(string content, out int[] payload)
        {
            payload = ZeroWidthIntsCodec.Decode(content);
            if (payload == null) return false;
            return true;
        }

        public static bool IsValid(OnlineAppearanceData data)
        {
            return IsValidCode(data.Case, Items.ItemType.Case) &&
                   IsValidCode(data.Protector, Items.ItemType.Protector) &&
                   IsValidCode(data.Field, Items.ItemType.Mat) &&
                   IsValidCode(data.Grave, Items.ItemType.Grave) &&
                   IsValidCode(data.Stand, Items.ItemType.Stand) &&
                   IsValidCode(data.Mate, Items.ItemType.Mate) &&
                   IsValidCode(data.Face, Items.ItemType.Face) &&
                   IsValidCode(data.Frame, Items.ItemType.Frame);
        }

        private static OnlineAppearanceData BuildValidatedData(Deck deck)
        {
            var source = deck ?? new Deck();
            var faceFallback = GetDefaultCode(Items.ItemType.Face, DefaultFace);
            var frameFallback = GetDefaultCode(Items.ItemType.Frame, DefaultFrame);
            return new OnlineAppearanceData
            {
                Case = EnsureValidCode(source.Case, Items.ItemType.Case, DefaultCase),
                Protector = EnsureValidCode(source.Protector, Items.ItemType.Protector, DefaultProtector),
                Field = EnsureValidCode(source.Field, Items.ItemType.Mat, DefaultField),
                Grave = EnsureValidCode(source.Grave, Items.ItemType.Grave, DefaultGrave),
                Stand = EnsureValidCode(source.Stand, Items.ItemType.Stand, DefaultStand),
                Mate = EnsureValidCode(source.Mate, Items.ItemType.Mate, DefaultMate),
                Face = EnsureValidCode(ReadConfigCode("DuelFace0", faceFallback), Items.ItemType.Face, faceFallback),
                Frame = EnsureValidCode(ReadConfigCode("DuelFrame0", frameFallback), Items.ItemType.Frame, frameFallback),
            };
        }

        private static Deck BuildSourceDeckForLocalPlayer(Deck deck)
        {
            var source = deck ?? new Deck();
            if (!Config.GetBool("OverrideDeckAppearance", false))
                return source;

            var result = new Deck
            {
                Case = source.Case,
                Protector = ReadConfigCode("DuelProtector0", source.Protector),
                Field = ReadConfigCode("DuelField0", source.Field),
                Grave = ReadConfigCode("DuelGrave0", source.Grave),
                Stand = ReadConfigCode("DuelStand0", source.Stand),
                Mate = ReadConfigCode("DuelMate0", source.Mate),
            };
            return result;
        }

        private static int ReadConfigCode(string key, int fallback)
        {
            var raw = Config.Get(key, fallback.ToString());
            return int.TryParse(raw, out var code) ? code : fallback;
        }

        private static int EnsureValidCode(int code, Items.ItemType type, int fallback)
        {
            var normalized = NormalizeSpecialCode(code, type, fallback);
            if (IsSupportedSpecialCode(normalized, type))
                return normalized;
            return IsValidCode(normalized, type) ? normalized : fallback;
        }

        private static int NormalizeSpecialCode(int code, Items.ItemType type, int fallback)
        {
            if (code == Items.CODE_RANDOM)
            {
                if (Program.items != null)
                    return Program.items.GetRandomItem(type).id;
                return fallback;
            }

            if (IsSupportedSpecialCode(code, type))
                return code;

            if (code == Items.CODE_SAME || code == Items.CODE_NONE || code == Items.CODE_DIY)
                return fallback;

            return code;
        }

        private static bool IsValidCode(int code, Items.ItemType type)
        {
            if (IsSupportedSpecialCode(code, type))
                return true;

            var items = Program.items;
            if (items == null)
                return false;

            return type switch
            {
                Items.ItemType.Case => ContainsItem(items.cases, code),
                Items.ItemType.Face => ContainsItem(items.faces, code),
                Items.ItemType.Frame => ContainsItem(items.frames, code),
                Items.ItemType.Protector => ContainsItem(items.protectors, code),
                Items.ItemType.Mat => ContainsItem(items.mats, code),
                Items.ItemType.Grave => ContainsItem(items.graves, code),
                Items.ItemType.Stand => ContainsItem(items.stands, code),
                Items.ItemType.Mate => ContainsItem(items.mates, code),
                _ => false,
            };
        }

        private static bool IsSupportedSpecialCode(int code, Items.ItemType type)
        {
            return code switch
            {
                Items.CODE_DIY => type == Items.ItemType.Face || type == Items.ItemType.Frame,
                Items.CODE_SAME => type == Items.ItemType.Grave || type == Items.ItemType.Stand,
                Items.CODE_NONE => type == Items.ItemType.Stand || type == Items.ItemType.Mate,
                _ => false,
            };
        }

        private static int GetDefaultCode(Items.ItemType type, int fallback)
        {
            var items = Program.items;
            if (items == null)
                return fallback;

            return type switch
            {
                Items.ItemType.Face => GetFirstItemId(items.faces, fallback),
                Items.ItemType.Frame => GetFirstItemId(items.frames, fallback),
                _ => fallback,
            };
        }

        private static int GetFirstItemId(List<Items.Item> list, int fallback)
        {
            return list != null && list.Count > 0 ? list[0].id : fallback;
        }

        private static bool ContainsItem(List<Items.Item> list, int id)
        {
            if (list == null)
                return false;

            return list.Any(item => item.id == id);
        }
    }

    public static class ZeroWidthIntsCodec
    {
        // 定义4个零宽字符表示2比特
        private const char ZW_00 = '\u200B'; // 零宽空格 -> 00
        private const char ZW_01 = '\u200C'; // 零宽非连接符 -> 01
        private const char ZW_10 = '\u200D'; // 零宽连接符 -> 10
        private const char ZW_11 = '\u200E'; // 从左到右标记 -> 11

        // 起始标记：两个连续ZW_10 (U+200D U+200D)
        private const string START_MARKER = "\u200D\u200D";

        // 映射表：2比特值 -> 字符
        private static readonly char[] Bit2Char = new char[4] { ZW_00, ZW_01, ZW_10, ZW_11 };
        // 反向映射：字符 -> 2比特值
        private static readonly Dictionary<char, byte> Char2Bit = new()
        {{ ZW_00, 0 }, { ZW_01, 1 }, { ZW_10, 2 }, { ZW_11, 3 }};

        public static string Encode(int[] ints)
        {
            if (ints == null || ints.Length != 8)
                throw new ArgumentException("需要长度为8的int数组");

            // 1. 将8个int转换为字节数组（小端序）
            byte[] bytes = new byte[32];
            for (int i = 0; i < 8; i++)
            {
                byte[] intBytes = BitConverter.GetBytes(ints[i]);
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(intBytes);
                Array.Copy(intBytes, 0, bytes, i * 4, 4);
            }

            // 2. 将字节数组转换为比特流（使用BitArray，索引0为最低位）
            System.Collections.BitArray bits = new(bytes);

            // 3. 每次取2比特，映射为字符
            StringBuilder sb = new();
            sb.Append(START_MARKER);
            for (int i = 0; i < bits.Length; i += 2)
            {
                // 构建2比特值 (低位在前，即bit i为低位，bit i+1为高位)
                int value = (bits[i] ? 1 : 0) | ((bits[i + 1] ? 1 : 0) << 1);
                sb.Append(Bit2Char[value]);
            }

            return sb.ToString();
        }

        public static int[] Decode(string hiddenMessage)
        {
            if (string.IsNullOrEmpty(hiddenMessage))
                return null;

            // 查找起始标记
            int markerIndex = hiddenMessage.IndexOf(START_MARKER, StringComparison.Ordinal);
            if (markerIndex == -1)
                return null;

            string dataPart = hiddenMessage[(markerIndex + START_MARKER.Length)..];

            // 收集比特
            List<bool> bits = new();
            foreach (char c in dataPart)
            {
                if (!Char2Bit.TryGetValue(c, out byte twoBits))
                {
                    // 由于我们预期只有映射字符，所以如果出现未知字符，可视为无效消息。
                    return null;
                }
                // 将2比特拆分为两个布尔值，注意低位先存
                bits.Add((twoBits & 1) != 0);      // 低位
                bits.Add((twoBits & 2) != 0);      // 高位
            }

            // 校验比特数：应为256
            if (bits.Count != 256)
            {
                // 可能数据损坏
                return null;
            }

            // 将比特数组转回字节数组
            byte[] bytes = new byte[32];
            for (int i = 0; i < bits.Count; i++)
            {
                if (bits[i])
                {
                    int byteIndex = i / 8;
                    int bitIndex = i % 8;
                    bytes[byteIndex] |= (byte)(1 << bitIndex);
                }
            }

            // 解析8个int
            int[] result = new int[8];
            for (int i = 0; i < 8; i++)
            {
                byte[] intBytes = new byte[4];
                Array.Copy(bytes, i * 4, intBytes, 0, 4);
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(intBytes);
                result[i] = BitConverter.ToInt32(intBytes, 0);
            }

            return result;
        }
    }

}
