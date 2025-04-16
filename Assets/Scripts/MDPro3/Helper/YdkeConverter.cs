using MDPro3.Duel.YGOSharp;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MDPro3
{
    public static class YdkeConverter
    {
        public static string ydkeHeader = "ydke://";

        public static Deck Ydke2Deck(string ydkeString)
        {
            ydkeString = ydkeString.Replace(ydkeHeader, string.Empty);

            var sections = ydkeString.Split('!');
            if(sections.Length < 3)
            {
                //throw new ArgumentException("Invalid YDKE format");
                return null;
            }

            var result = new Deck
            {
                Main = DecodeSection(sections[0]),
                Extra = DecodeSection(sections[1]),
                Side = DecodeSection(sections[2])
            };
            return result;
        }

        private static List<int> DecodeSection(string base64Section)
        {
            if (string.IsNullOrEmpty(base64Section))
                return new List<int>();

            var decodedBytes = Convert.FromBase64String(base64Section);
            var cardIds = new List<int>();

            for (int i = 0; i < decodedBytes.Length; i += 4)
            {
                if (i + 4 > decodedBytes.Length) break;

                var cardId = BitConverter.ToUInt32(decodedBytes, i);
                cardIds.Add((int)cardId);
            }

            return cardIds;
        }

        public static string DeckToYdke(Deck deck)
        {
            deck.Main ??= new();
            deck.Extra ??= new();
            deck.Side ??= new();

            var main = EncodeSection(deck.Main);
            var extra = EncodeSection(deck.Extra);
            var side = EncodeSection(deck.Side);
            return $"{ydkeHeader}{main}!{extra}!{side}";
        }

        private static string EncodeSection(List<int> deck)
        {
            var bytes = new List<byte>();

            foreach (var cardId in deck)
            {
                bytes.Add((byte)(cardId & 0xFF));
                bytes.Add((byte)((cardId >> 8) & 0xFF));
                bytes.Add((byte)((cardId >> 16) & 0xFF));
                bytes.Add((byte)(cardId >> 24));
            }

            return Convert.ToBase64String(bytes.ToArray());
        }
    }
}
