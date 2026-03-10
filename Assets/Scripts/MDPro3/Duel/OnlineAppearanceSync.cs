using System;
using System.Collections.Generic;
using MDPro3.Duel.YGOSharp;

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
        public const string Prefix = "mdp3acc:v2:";
        public const string LegacyPrefix = "/mdp3acc:v1:";

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
            return $"{Prefix}{data.Case},{data.Protector},{data.Field},{data.Grave},{data.Stand},{data.Mate},{data.Face},{data.Frame}";
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

            var parts = payload.Split(',');
            if (parts.Length != 6 && parts.Length != 8)
                return false;

            if (!int.TryParse(parts[0], out var deckCase))
                return false;
            if (!int.TryParse(parts[1], out var protector))
                return false;
            if (!int.TryParse(parts[2], out var field))
                return false;
            if (!int.TryParse(parts[3], out var grave))
                return false;
            if (!int.TryParse(parts[4], out var stand))
                return false;
            if (!int.TryParse(parts[5], out var mate))
                return false;

            var defaultFace = GetDefaultCode(Items.ItemType.Face, DefaultFace);
            var defaultFrame = GetDefaultCode(Items.ItemType.Frame, DefaultFrame);
            var face = defaultFace;
            var frame = defaultFrame;
            if (parts.Length >= 8)
            {
                if (!int.TryParse(parts[6], out face))
                    return false;
                if (!int.TryParse(parts[7], out frame))
                    return false;
            }

            data = new OnlineAppearanceData
            {
                Case = EnsureValidCode(deckCase, Items.ItemType.Case, DefaultCase),
                Protector = EnsureValidCode(protector, Items.ItemType.Protector, DefaultProtector),
                Field = EnsureValidCode(field, Items.ItemType.Mat, DefaultField),
                Grave = EnsureValidCode(grave, Items.ItemType.Grave, DefaultGrave),
                Stand = EnsureValidCode(stand, Items.ItemType.Stand, DefaultStand),
                Mate = EnsureValidCode(mate, Items.ItemType.Mate, DefaultMate),
                Face = EnsureValidCode(face, Items.ItemType.Face, defaultFace),
                Frame = EnsureValidCode(frame, Items.ItemType.Frame, defaultFrame),
            };
            return true;
        }

        private static bool TryExtractPayload(string content, out string payload)
        {
            payload = null;
            if (string.IsNullOrEmpty(content))
                return false;

            if (content.StartsWith(Prefix, StringComparison.Ordinal))
            {
                payload = content.Substring(Prefix.Length).Trim().TrimEnd('\0');
                return true;
            }

            if (content.StartsWith(LegacyPrefix, StringComparison.Ordinal))
            {
                payload = content.Substring(LegacyPrefix.Length).Trim().TrimEnd('\0');
                return true;
            }

            return false;
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

            for (var i = 0; i < list.Count; i++)
                if (list[i].id == id)
                    return true;

            return false;
        }
    }
}
