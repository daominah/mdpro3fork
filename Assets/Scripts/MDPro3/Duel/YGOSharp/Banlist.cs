using System;
using System.Collections.Generic;
using System.Linq;

namespace MDPro3.Duel.YGOSharp
{
    public class Banlist
    {
        public IList<int> BannedIds { get; private set; }
        public IList<int> LimitedIds { get; private set; }
        public IList<int> SemiLimitedIds { get; private set; }
        public IList<int> UnlimitedIds { get; private set; }
        public bool WhitelistOnly { get; private set; }
        private uint _hash = 0x7dfcee6a;
        public uint Hash
        {
            get => _hash;
            private set => _hash = value;
        }
        public string Name = "";

        public Dictionary<string, uint> CreditLimits { get; private set; }
        public Dictionary<int, Dictionary<string, int>> CardCredits { get; private set; }

        public Banlist()
        {
            BannedIds = new List<int>();
            LimitedIds = new List<int>();
            SemiLimitedIds = new List<int>();
            UnlimitedIds = new List<int>();
            WhitelistOnly = false;
        }

        public void AddCreditLimit(string key, uint limit)
        {
            CreditLimits ??= new();

            if (CreditLimits.ContainsKey(key)) return;
            CreditLimits[key] = limit;
            uint keyHash = CreditHash(key);
            UpdateHash(ref _hash, keyHash, limit, 0x43524544u);
        }

        public void AddCardCredit(int cardId, string key, int value)
        {
            CardCredits ??= new();

            if (!CardCredits.ContainsKey(cardId))
                CardCredits[cardId] = new Dictionary<string, int>();
            CardCredits[cardId][key] = value;
            uint keyHash = CreditHash(key);
            UpdateHash(ref _hash, (uint)cardId, keyHash, value);
        }

        public int GetQuantity(int cardId)
        {
            int al = 0;
            try
            {
                al = CardsManager.Get(cardId).Alias;
            }
            catch (Exception)
            {
            }
            if (al==0)
            {
                if (BannedIds.Contains(cardId))
                    return 0;
                if (LimitedIds.Contains(cardId))
                    return 1;
                if (SemiLimitedIds.Contains(cardId))
                    return 2;
                if (UnlimitedIds.Contains(cardId))
                    return 3;
                return WhitelistOnly ? 0 : 3;
            }
            else
            {
                if (BannedIds.Contains(al))
                    return 0;
                if (LimitedIds.Contains(al))
                    return 1;
                if (SemiLimitedIds.Contains(al))
                    return 2;
                if (UnlimitedIds.Contains(al))
                    return 3;
                return WhitelistOnly ? 0 : 3;
            }
            
        }

        public int GetCredit(int cardId)
        {
            if(!IsCreditBanlist())
                return 0;

            var first = CreditLimits.FirstOrDefault().Key;
            return GetCredit(cardId, first);
        }

        public int GetCredit(int cardId, string creditKey)
        {
            if (!IsCreditBanlist())
                return 0;

            if (CardCredits.ContainsKey(cardId) && CardCredits[cardId].ContainsKey(creditKey))
                return CardCredits[cardId][creditKey];

            return 0;
        }

        public void EnableWhitelistMode()
        {
            if (WhitelistOnly)
                return;
            WhitelistOnly = true;
            Hash ^= 0x0f0f0f0f;
        }

        public void Add(int cardId, int quantity)
        {
            if (quantity < 0 || quantity > 3)
                return;
            switch (quantity)
            {
                case 0:
                    BannedIds.Add(cardId);
                    break;
                case 1:
                    LimitedIds.Add(cardId);
                    break;
                case 2:
                    SemiLimitedIds.Add(cardId);
                    break;
                case 3:
                    UnlimitedIds.Add(cardId);
                    break;
            }
            uint code = (uint)cardId;
            Hash = Hash ^ ((code << 18) | (code >> 14)) ^ ((code << (27 + quantity)) | (code >> (5 - quantity)));
        }

        private bool IsCreditBanlist()
        {
            if(CreditLimits == null || CreditLimits.Count == 0)
                return false;

            if(CardCredits == null || CardCredits.Count == 0)
                return false;

            return true;
        }

        private static uint CreditHash(string s)
        {
            uint h = 2166136261u;
            foreach (char c in s)
            {
                byte b = (byte)c; // 假设字符串为 UTF-8 编码的字节，这里简单取低 8 位
                h ^= b;
                h *= 16777619u;
            }
            return h;
        }

        private static void UpdateHash(ref uint h, uint a, uint b, int c)
        {
            UpdateHash(ref h, a, b, unchecked((uint)c));
        }

        private static void UpdateHash(ref uint h, uint a, uint b, uint c)
        {
            h = h ^ ((a << 18) | (a >> 14)) ^
                     ((b << 9) | (b >> 23)) ^
                     ((c << 27) | (c >> 5));
        }

    }
}
