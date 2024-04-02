using System;
using System.Collections.Generic;
using System.IO;
using MDPro3.YGOSharp.OCGWrapper.Enums;

namespace MDPro3.YGOSharp
{
    public class Deck
    {
        public List<int> Main { get; private set; }
        public List<int> Extra { get; private set; }
        public List<int> Side { get; private set; }
        public List<int> Pickup { get; private set; }
        public List<int> Protector { get; private set; }
        public List<int> Case { get; private set; }
        public List<int> Field { get; private set; }
        public List<int> Grave { get; private set; }
        public List<int> Stand { get; private set; }
        public List<int> Mate { get; private set; }

        public Deck()
        {
            Main = new List<int>();
            Extra = new List<int>();
            Side = new List<int>();
            Pickup = new List<int>();
            Protector = new List<int>();
            Case = new List<int>();
            Field = new List<int>();
            Grave = new List<int>();
            Stand = new List<int>();
            Mate = new List<int>();
        }

        public Deck(string path)
        {
            Main = new List<int>();
            Extra = new List<int>();
            Side = new List<int>();
            Pickup = new List<int>();
            Protector = new List<int>();
            Case = new List<int>();
            Field = new List<int>();
            Grave = new List<int>();
            Stand = new List<int>();
            Mate = new List<int>();
            try
            {
                string text = File.ReadAllText(path);
                string st = text.Replace("\r", "");
                string[] lines = st.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                int flag = -1;
                foreach (string line in lines)
                {
                    if (line == "#main")
                        flag = 1;
                    else if (line == "#extra")
                        flag = 2;
                    else if (line == "!side")
                        flag = 3;
                    else if (line == "#pickup")
                        flag = 4;
                    else if (line == "#protector")
                        flag = 5;
                    else if (line == "#case")
                        flag = 6;
                    else if (line == "#field")
                        flag = 7;
                    else if (line == "#grave")
                        flag = 8;
                    else if (line == "#stand")
                        flag = 9;
                    else if (line == "#mate")
                        flag = 10;
                    else
                    {
                        int code = 0;
                        try
                        {
                            code = Int32.Parse(line.Replace("#", ""));
                        }
                        catch (Exception)
                        {

                        }
                        if (code > 100)
                        {
                            switch (flag)
                            {
                                case 1:
                                    Main.Add(code);
                                    break;
                                case 2:
                                    Extra.Add(code);
                                    break;
                                case 3:
                                    Side.Add(code);
                                    break;
                                case 4:
                                    Pickup.Add(code);
                                    break;
                                case 5:
                                    Protector.Add(code);
                                    break;
                                case 6:
                                    Case.Add(code); 
                                    break;
                                case 7:
                                    Field.Add(code); 
                                    break;
                                case 8:
                                    Grave.Add(code); 
                                    break;
                                case 9:
                                    Stand.Add(code); 
                                    break;
                                case 10:
                                    Mate.Add(code); 
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
            }
            if (Protector.Count == 0)
                Protector.Add(1070001);
            if (Case.Count == 0)
                Case.Add(1080001);
            if(Field.Count == 0)
                Field.Add(1090001);
            if(Grave.Count == 0)
                Grave.Add(1100001);
            if(Stand.Count == 0)
                Stand.Add(1110001);
            if (Mate.Count == 0)
                Mate.Add(1000001);
        }

        public Deck(List<int> main, List<int> extra, List<int> side)
        {
            Main = main;
            Extra = extra;
            Side = side;
            Pickup = new List<int>();
            Protector = new List<int>();
            Case = new List<int>();
            Field = new List<int>();
            Grave = new List<int>();
            Stand = new List<int>();
            Mate = new List<int>();
            Protector.Add(1070001);
            Case.Add(1080001);
            Field.Add(1090001);
            Grave.Add(1100001);
            Stand.Add(1110001);
            Mate.Add(1000001);
        }

        public int Check(Banlist ban, bool ocg, bool tcg)
        {
            if (Main.Count < 40 ||
                Main.Count > 60 ||
                Extra.Count > 15 ||
                Side.Count > 15)
                return 1;

            Dictionary<int, int> cards = new Dictionary<int, int>();

            List<int>[] stacks = { Main, Extra, Side };
            foreach (List<int> stack in stacks)
            {
                foreach (int id in stack)
                {
                    Card card = CardsManager.Get(id);
                    AddToCards(cards, card);
                    if (!ocg && card.Ot == 1 || !tcg && card.Ot == 2)
                        return id;
                    if (card.HasType(CardType.Token))
                        return id;
                }
            }

            if (ban == null)
                return 0;

            foreach (var pair in cards)
            {
                int max = ban.GetQuantity(pair.Key);
                if (pair.Value > max)
                    return pair.Key;
            }
            return 0;
        }

        public int GetCardCount(int code)
        {
            int al = 0;
            try
            {
                al = CardsManager.Get(code).Alias;
            }
            catch (Exception)
            {
            }
            int returnValue = 0;
            return returnValue;
        }

        public bool Check(Deck deck)
        {
            if (deck.Main.Count != Main.Count || deck.Extra.Count != Extra.Count)
                return false;

            Dictionary<int, int> cards = new Dictionary<int, int>();
            Dictionary<int, int> ncards = new Dictionary<int, int>();
            List<int>[] stacks = { Main, Extra, Side };
            foreach (IList<int> stack in stacks)
            {
                foreach (int id in stack)
                {
                    if (!cards.ContainsKey(id))
                        cards.Add(id, 1);
                    else
                        cards[id]++;
                }
            }
            stacks = new[] { deck.Main, deck.Extra, deck.Side };
            foreach (var stack in stacks)
            {
                foreach (int id in stack)
                {
                    if (!ncards.ContainsKey(id))
                        ncards.Add(id, 1);
                    else
                        ncards[id]++;
                }
            }
            foreach (var pair in cards)
            {
                if (!ncards.ContainsKey(pair.Key))
                    return false;
                if (ncards[pair.Key] != pair.Value)
                    return false;
            }
            return true;
        }

        private static void AddToCards(Dictionary<int, int> cards, Card card)
        {
            int id = card.Id;
            if (card.Alias != 0)
                id = card.Alias;
            if (cards.ContainsKey(id))
                cards[id]++;
            else
                cards.Add(id, 1);
        }
    }
}