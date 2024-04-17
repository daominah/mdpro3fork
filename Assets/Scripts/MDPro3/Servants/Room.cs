using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using MDPro3.YGOSharp;
using MDPro3.UI;
using MDPro3.YGOSharp.OCGWrapper.Enums;
using MDPro3.Net;

namespace MDPro3
{
    public class Room : Servant
    {
        GameObject chatItemMe;
        GameObject chatItemOp;
        GameObject chatItemSystem;

        public RoomPlayer player0;
        public RoomPlayer player1;
        public RoomPlayer player2;
        public RoomPlayer player3;
        List<RoomPlayer> roomPlayers = new List<RoomPlayer>();

        public RectTransform left;
        public RectTransform middle;
        public RectTransform right;
        public Button btnToDuel;
        public Button btnToWatch;
        public Button btnReady;
        public Button btnStart;

        public Text description;
        public ScrollRect chatScroll;
        public InputField chatInput;

        public Button deckBtn;
        public Image deckIcon;
        public Text deckName;
        public RawImage card1;
        public RawImage card2;
        public RawImage card3;

        public uint lfList;
        public byte rule;
        public byte mode;
        public bool noCheckDeck;
        public bool noShuffleDeck;
        public int startLp = 8000;
        public byte startHand;
        public byte drawCount;
        public short timeLimit = 180;
        public int observerCount;
        public int selfType;
        public bool isHost;
        public bool needSide;
        public bool joinWithReconnect;
        public bool sideWaitingObserver;
        public static bool fromSolo;
        public static bool soloLockHand;
        public static bool fromLocalHost;

        public class Player
        {
            public string name = "";
            public bool ready;
        }
        public Player[] players = new Player[32];

        Deck deck;

        public bool duelEnded;


        #region Servant
        public override void Initialize()
        {
            depth = 2;
            haveLine = false;
            returnServant = Program.I().online;
            base.Initialize();
            ChatOff(0);
            deckBtn.GetComponent<ButtonHover>().hoverIn = () => Hover(true);
            deckBtn.GetComponent<ButtonHover>().hoverOut = () => Hover(false);

            var handle = Addressables.LoadAssetAsync<GameObject>("ChatItemMe");
            handle.Completed += (result) =>
            {
                chatItemMe = result.Result;
            };
            handle = Addressables.LoadAssetAsync<GameObject>("ChatItemOp");
            handle.Completed += (result) =>
            {
                chatItemOp = result.Result;
            };
            handle = Addressables.LoadAssetAsync<GameObject>("ChatItemSystem");
            handle.Completed += (result) =>
            {
                chatItemSystem = result.Result;
            };
            chatInput.GetComponent<InputFieldSubmit>().onSubmit.AddListener(OnChat);
            Program.onScreenChanged += OnResize;

            roomPlayers.Add(player0);
            roomPlayers.Add(player1);
            roomPlayers.Add(player2);
            roomPlayers.Add(player3);
        }

        public bool chatOn;
        public bool chatSwitching;
        public void SwitchChat(float moveTime)
        {
            if (chatOn)
                ChatOff(moveTime);
            else
                ChatOn(moveTime);
        }

        public void ChatOn(float moveTime)
        {
            if (chatSwitching)
                return;
            chatSwitching = true;
            chatOn = true;
            OnResize();
            right.gameObject.GetComponent<CanvasGroup>().alpha = 1;
            right.DOAnchorPosX(0, moveTime).OnComplete(() => chatSwitching = false);
        }

        public void ChatOff(float moveTime)
        {
            if (chatSwitching)
                return;
            chatSwitching = true;
            chatOn = false;
            OnResize();
            var width = right.sizeDelta.x;
            right.DOAnchorPosX(width, moveTime).OnComplete(() =>
            {
                right.gameObject.GetComponent<CanvasGroup>().alpha = 0;
                chatSwitching = false;
            });
        }

        public override void Show(int preDepth)
        {
            base.Show(preDepth);
            ChatOn(transitionTime);
            Program.I().ocgcore.handler = Handler;
            deckName.text = Config.Get("DeckInUse", "@ui");
            if(File.Exists("Deck/" + deckName.text + ".ydk"))
                deck = new Deck("Deck/" + deckName.text + ".ydk");
            else
            {
                deck = null;
                deckName.text = InterString.Get("请点击此处选择卡组");
            }

            StartCoroutine(RefreshAsync());
        }
        public override void Hide(int preDepth)
        {
            base.Hide(preDepth);
            ChatOff(transitionTime);
        }
        IEnumerator RefreshAsync()
        {
            player0.gameObject.SetActive(false);
            player1.gameObject.SetActive(false);
            player2.gameObject.SetActive(false);
            player3.gameObject.SetActive(false);
            deckIcon.color = Color.clear;
            if(deck != null)
            {
                var ie = TextureManager.LoadItemIcon(deck.Case[0].ToString());
                StartCoroutine(ie);
                while (ie.MoveNext())
                    yield return null;
                deckIcon.color = Color.white;
                deckIcon.sprite = ie.Current;

                while (!Appearance.loaded)
                    yield return null;
            }
            Realize();
            if (deck != null)
            {
                Material pMat = null;
                IEnumerator<Texture2D> ic = null;
                if (deck.Pickup.Count > 0 && deck.Pickup[0] != 0)
                {
                    ic = Program.I().texture_.LoadCardAsync(deck.Pickup[0], true);
                    StartCoroutine(ic);
                    while (ic.MoveNext())
                        yield return null;
                    card1.texture = ic.Current;
                    var mat = TextureManager.GetCardMaterial(deck.Pickup[0]);
                    card1.material = mat;
                }
                else
                {
                    if (pMat == null)
                    {
                        var ip = ABLoader.LoadProtectorMaterial(deck.Protector[0].ToString());
                        while (ip.MoveNext())
                            yield return null;
                        pMat = ip.Current;
                    }
                    card1.texture = null;
                    card1.material = pMat;
                }
                if (deck.Pickup.Count > 1 && deck.Pickup[1] != 0)
                {
                    ic = Program.I().texture_.LoadCardAsync(deck.Pickup[1], true);
                    StartCoroutine(ic);
                    while (ic.MoveNext())
                        yield return null;
                    card2.texture = ic.Current;
                    var mat = TextureManager.GetCardMaterial(deck.Pickup[1]);
                    card2.material = mat;
                }
                else
                {
                    if (pMat == null)
                    {
                        var ip = ABLoader.LoadProtectorMaterial(deck.Protector[0].ToString());
                        while (ip.MoveNext())
                            yield return null;
                        pMat = ip.Current;
                    }
                    card2.texture = null;
                    card2.material = pMat;
                }
                if (deck.Pickup.Count > 2 && deck.Pickup[2] != 0)
                {
                    ic = Program.I().texture_.LoadCardAsync(deck.Pickup[2], true);
                    StartCoroutine(ic);
                    while (ic.MoveNext())
                        yield return null;
                    card3.texture = ic.Current;
                    var mat = TextureManager.GetCardMaterial(deck.Pickup[2]);
                    card3.material = mat;
                }
                else
                {
                    if (pMat == null)
                    {
                        var ip = ABLoader.LoadProtectorMaterial(deck.Protector[0].ToString());
                        while (ip.MoveNext())
                            yield return null;
                        pMat = ip.Current;
                    }
                    card3.texture = null;
                    card3.material = pMat;
                }
            }
        }

        public void Hover(bool hover)
        {
            if (deck == null)
                return;
            card1.GetComponent<Animator>().SetBool("Hover", hover);
            card2.GetComponent<Animator>().SetBool("Hover", hover);
            card3.GetComponent<Animator>().SetBool("Hover", hover);
        }

        void OnResize()
        {
            //left 420 -480
            //right 500 - 600
            var uiWidth = Screen.width * 1080f / Screen.height;
            if (uiWidth >= 1920)
            {
                left.sizeDelta = new Vector2(480, 0);
                right.sizeDelta = new Vector2(600, 0);
            }
            else if (uiWidth >= 1920 - 60)
            {
                left.sizeDelta = new Vector2(480 - (1920 - uiWidth), 0);
                right.sizeDelta = new Vector2(600, 0);
            }
            else if (uiWidth >= 1920 - 60 - 100)
            {
                left.sizeDelta = new Vector2(420, 0);
                right.sizeDelta = new Vector2(600 - (1920 - 60 - uiWidth), 0);
            }
            else
            {
                left.sizeDelta = new Vector2(420, 0);
                right.sizeDelta = new Vector2(500, 0);
            }
            middle.offsetMin = new Vector2(left.sizeDelta.x, 0);
            middle.offsetMax = new Vector2(-right.sizeDelta.x, 0);

            float middleWidth = middle.rect.width;
            btnToDuel.GetComponent<RectTransform>().sizeDelta = new Vector2((middleWidth - 50) / 2f - 50, 80);
            btnToWatch.GetComponent<RectTransform>().sizeDelta = new Vector2((middleWidth - 50) / 2f - 50, 80);
            btnReady.GetComponent<RectTransform>().sizeDelta = new Vector2((middleWidth - 50) / 2f - 50, 80);
            btnStart.GetComponent<RectTransform>().sizeDelta = new Vector2((middleWidth - 50) / 2f - 50, 80);
        }

        public void OnReady()
        {
            if (players[selfType] == null)
                return;
            if (players[selfType].ready)
                TcpHelper.CtosMessage_HsNotReady();
            else
            {
                if (File.Exists("Deck/" + Config.Get("DeckInUse", "") + ".ydk"))
                {
                    TcpHelper.CtosMessage_UpdateDeck(new Deck("Deck/" + Config.Get("DeckInUse", "") + ".ydk"));
                    TcpHelper.CtosMessage_HsReady();
                }
                else
                    MessageManager.Cast(InterString.Get("请先选择有效的卡组。"));
            }
        }

        public override void OnExit()
        {
            if (fromSolo)
                returnServant = Program.I().solo;
            else
            {
                returnServant = Program.I().online;
                if (fromLocalHost)
                    YgoServer.StopServer();
            }
            base.OnExit();
            Program.I().ocgcore.CloseConnection();
        }

        #endregion
        public void Handler(byte[] buffer)
        {
            TcpHelper.CtosMessage_Response(buffer);
        }

        public void OnChat(string content)
        {
            if (content == string.Empty)
                return;
            TcpHelper.CtosMessage_Chat(content);
            chatInput.text = string.Empty;
        }

        public void OnSend()
        {
            if (chatInput.text == string.Empty)
                return;
            AudioManager.PlaySE("SE_MENU_DECIDE");
            OnChat(chatInput.text);
        }

        public void OnToDuel()
        {
            TcpHelper.CtosMessage_HsToDuelist();
        }
        public void OnToObserver()
        {
            TcpHelper.CtosMessage_HsToObserver();
        }
        public void OnStart()
        {
            TcpHelper.CtosMessage_HsStart();
        }

        public void OnSelectDeck()
        {
            if (players[selfType] != null && players[selfType].ready)
            {
                MessageManager.Cast(InterString.Get("请先取消准备，再选择卡组。"));
                return;
            }
            SelectDeck.state = SelectDeck.State.ForDuel;
            Program.I().selectDeck.depth = 3;
            Program.I().selectDeck.returnServant = this;
            Program.I().ShiftToServant(Program.I().selectDeck);
        }

        public void OnKick(int player)
        {
            TcpHelper.CtosMessage_HsKick(player);
        }

        List<GameObject> chatItems = new List<GameObject>();
        public enum PlayerPosition
        {
            Me,
            MyTag,
            Op,
            OpTag,
            WatchMe,
            WatchMyTag,
            WatchOp,
            WatchOpTag,
            Other
        }
        void AddChatItem(int player, string content)
        {
            if (Program.I().ocgcore.isShowed && player < 4)
            {
                if (mode != 2)
                {
                    if (Program.I().ocgcore.isFirst && selfType == 1
                        || !Program.I().ocgcore.isFirst && selfType == 0)
                        player = (player + 1) % 2;
                }
                else
                {
                    if (Program.I().ocgcore.isFirst && selfType > 1
                        || !Program.I().ocgcore.isFirst && selfType < 2)
                        player = (player + 2) % 4;
                }
            }

            var nickName = players[player]?.name;
            GameObject item = null;
            var position = GetPlayerPositon(player);
            switch (position)
            {
                case PlayerPosition.Me:
                    item = Instantiate(chatItemMe);
                    item.transform.GetChild(2).GetComponent<Image>().material = Appearance.duelFrameMat0;
                    item.transform.GetChild(2).GetComponent<Image>().sprite = Appearance.duelFace0;
                    break;
                case PlayerPosition.MyTag:
                    item = Instantiate(chatItemMe);
                    item.transform.GetChild(2).GetComponent<Image>().material = Appearance.duelFrameMat0Tag;
                    item.transform.GetChild(2).GetComponent<Image>().sprite = Appearance.duelFace0Tag;
                    break;
                case PlayerPosition.Op:
                    item = Instantiate(chatItemOp);
                    item.transform.GetChild(2).GetComponent<Image>().material = Appearance.duelFrameMat1;
                    item.transform.GetChild(2).GetComponent<Image>().sprite = Appearance.duelFace1;
                    break;
                case PlayerPosition.OpTag:
                    item = Instantiate(chatItemOp);
                    item.transform.GetChild(2).GetComponent<Image>().material = Appearance.duelFrameMat1Tag;
                    item.transform.GetChild(2).GetComponent<Image>().sprite = Appearance.duelFace1Tag;
                    break;
                case PlayerPosition.WatchMe:
                    item = Instantiate(chatItemMe);
                    item.transform.GetChild(2).GetComponent<Image>().material = Appearance.watchFrameMat0;
                    item.transform.GetChild(2).GetComponent<Image>().sprite = Appearance.watchFace0;
                    break;
                case PlayerPosition.WatchMyTag:
                    item = Instantiate(chatItemMe);
                    item.transform.GetChild(2).GetComponent<Image>().material = Appearance.watchFrameMat0Tag;
                    item.transform.GetChild(2).GetComponent<Image>().sprite = Appearance.watchFace0Tag;
                    break;
                case PlayerPosition.WatchOp:
                    item = Instantiate(chatItemOp);
                    item.transform.GetChild(2).GetComponent<Image>().material = Appearance.watchFrameMat1;
                    item.transform.GetChild(2).GetComponent<Image>().sprite = Appearance.watchFace1;
                    break;
                case PlayerPosition.WatchOpTag:
                    item = Instantiate(chatItemOp);
                    item.transform.GetChild(2).GetComponent<Image>().material = Appearance.watchFrameMat1Tag;
                    item.transform.GetChild(2).GetComponent<Image>().sprite = Appearance.watchFace1Tag;
                    break;
                case PlayerPosition.Other:
                    item = Instantiate(chatItemSystem);
                    break;
            }
            item.transform.GetChild(0).GetComponent<Text>().text = nickName + ":";
            item.transform.GetChild(1).GetComponent<Text>().text = content;
            if(position == PlayerPosition.Other)
            {
                item.transform.GetChild(0).GetComponent<Text>().text = string.Empty;
                item.transform.GetChild(1).GetComponent<Text>().text = string.Empty;
                item.transform.GetChild(2).GetComponent<Text>().text = content;
            }
            item.transform.SetParent(chatScroll.content, false);
            item.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -chatItems.Count * 150);
            chatItems.Add(item);

            chatScroll.content.sizeDelta = new Vector2(0, chatItems.Count * 150);
            DOTween.To(() => chatScroll.verticalScrollbar.value, x => chatScroll.verticalScrollbar.value = x, 0, 0.2f);

            var p = new Package();
            p.Function = (int)GameMessage.sibyl_chat;
            p.Data = new BinaryMaster();
            p.Data.writer.Write(player);
            p.Data.writer.WriteUnicode(content, content.Length + 1);
            TcpHelper.AddRecordLine(p);

            if (Program.I().ocgcore.isShowed)
                Program.I().ocgcore.Chat(player, content);
        }

        public string GetPlayerName(int player)
        {
            string nickName = "";
            switch (player)
            {
                case -1: //local name
                    nickName = Config.Get("DuelPlayerName0", "@ui");
                    break;
                case 0: //from host
                case 1: //from client
                case 2: //host tag
                case 3: //client tag
                    nickName = players[player].name;
                    var configName = GetConfigPlayerName(GetPlayerPositon(player));
                    if (configName.Length > 0)
                        nickName = configName;
                    break;
                case 7: //observer
                    nickName += InterString.Get("观战者");
                    break;
                case 8: //system custom message, no prefix.
                    nickName += "[System]";
                    break;
                case 9: //error message
                    nickName += "[Script error]";
                    break;
                default: //from watcher or unknown
                    nickName += "[---]";
                    break;
            }
            return nickName;
        }

        public string GetConfigPlayerName(PlayerPosition position)
        {
            switch (position)
            {
                case PlayerPosition.Me:
                    return Config.Get("DuelPlayerName0", "@ui");
                case PlayerPosition.MyTag:
                    return Config.Get("DuelPlayerName0Tag", "@ui");
                case PlayerPosition.Op:
                    return Config.Get("DuelPlayerName1", "@ui");
                case PlayerPosition.OpTag:
                    return Config.Get("DuelPlayerName1Tag", "@ui");
                case PlayerPosition.WatchMe:
                    return Config.Get("WatchPlayerName0", "@ui");
                case PlayerPosition.WatchMyTag:
                    return Config.Get("WatchPlayerName0Tag", "@ui");
                case PlayerPosition.WatchOp:
                    return Config.Get("WatchPlayerName1", "@ui");
                case PlayerPosition.WatchOpTag:
                    return Config.Get("WatchPlayerName1Tag", "@ui");
                default:
                    return string.Empty;
            }
        }

        PlayerPosition GetPlayerPositon(int player)
        {
            PlayerPosition position;
            if (player < 4)
            {
                if (mode < 2)
                {
                    if (selfType != 7)
                    {
                        if (selfType == player)
                            position = PlayerPosition.Me;
                        else
                            position = PlayerPosition.Op;
                    }
                    else
                    {
                        if (player == 0)
                            position = PlayerPosition.WatchMe;
                        else
                            position = PlayerPosition.WatchOp;
                    }
                }
                else
                {
                    if (selfType != 7)
                    {
                        if (selfType == player)
                            position = PlayerPosition.Me;
                        else if ((selfType == 0 && player == 1)
                        || (selfType == 1 && player == 0)
                        || (selfType == 2 && player == 3)
                        || (selfType == 3 && player == 2))
                            position = PlayerPosition.MyTag;
                        else if (player == 0 || player == 2)
                            position = PlayerPosition.Op;
                        else
                            position = PlayerPosition.OpTag;
                    }
                    else
                    {
                        if (player == 0)
                            position = PlayerPosition.WatchMe;
                        else if (player == 1)
                            position = PlayerPosition.WatchMyTag;
                        else if (player == 2)
                            position = PlayerPosition.WatchOp;
                        else
                            position = PlayerPosition.WatchOpTag;
                    }
                }
            }
            else
                position = PlayerPosition.Other;
            return position;
        }

        void Realize()
        {
            var description = "";
            if (fromLocalHost)
            {
                foreach(var ip in Tools.GetLocalIPv4())
                    description += InterString.Get("本机地址：") + ip + "\r\n";
                description += InterString.Get("端口：") + "7911\r\n";
            }
            description += StringHelper.GetUnsafe(1244 + mode) + "\r\n";//模式
            description += StringHelper.GetUnsafe(1259 + Program.I().ocgcore.MasterRule) + "\r\n";//规则
            description += StringHelper.GetUnsafe(1225) + StringHelper.GetUnsafe(1481 + rule) + "\r\n";//卡片允许：
            description += StringHelper.GetUnsafe(1226) + BanlistManager.GetName(lfList) + "\r\n";//禁限卡表
            description += StringHelper.GetUnsafe(1231) + startLp + "\r\n";//初始基本分：
            description += StringHelper.GetUnsafe(1232) + startHand + "\r\n";//初始手卡数：
            description += StringHelper.GetUnsafe(1233) + drawCount + "\r\n";//每回合抽卡：
            description += StringHelper.GetUnsafe(1237) + timeLimit + "\r\n";//每回合时间：
            description += StringHelper.GetUnsafe(1253) + observerCount + "\r\n";//当前观战人数：
            if (noCheckDeck) description += StringHelper.GetUnsafe(1229) + "\r\n";//不检查卡组
            if (noShuffleDeck) description += StringHelper.GetUnsafe(1230);//不洗切卡组
            this.description.text = description;
            OnResize();

            if (!Appearance.loaded)
                return;

            for (int i = 0; i < 4; i++)
            {
                if (players[i] == null)
                    roomPlayers[i].gameObject.SetActive(false);
                else
                {
                    roomPlayers[i].gameObject.SetActive(true);
                    roomPlayers[i].playerName.text = players[i].name;
                    if (players[i].ready)
                        roomPlayers[i].readyIcon.SetActive(true);
                    else
                        roomPlayers[i].readyIcon.SetActive(false);
                    if (selfType == i)
                        roomPlayers[i].playerName.color = Color.cyan;
                    else
                        roomPlayers[i].playerName.color = Color.white;

                    var position = GetPlayerPositon(i);
                    switch (position)
                    {
                        case PlayerPosition.Me:
                            roomPlayers[i].frame.material = Appearance.duelFrameMat0;
                            roomPlayers[i].frame.sprite = Appearance.duelFace0;
                            break;
                        case PlayerPosition.MyTag:
                            roomPlayers[i].frame.material = Appearance.duelFrameMat0Tag;
                            roomPlayers[i].frame.sprite = Appearance.duelFace0Tag;
                            break;
                        case PlayerPosition.Op:
                            roomPlayers[i].frame.material = Appearance.duelFrameMat1;
                            roomPlayers[i].frame.sprite = Appearance.duelFace1;
                            break;
                        case PlayerPosition.OpTag:
                            roomPlayers[i].frame.material = Appearance.duelFrameMat1Tag;
                            roomPlayers[i].frame.sprite = Appearance.duelFace1Tag;
                            break;
                        case PlayerPosition.WatchMe:
                            roomPlayers[i].frame.material = Appearance.watchFrameMat0;
                            roomPlayers[i].frame.sprite = Appearance.watchFace0;
                            break;
                        case PlayerPosition.WatchMyTag:
                            roomPlayers[i].frame.material = Appearance.watchFrameMat0Tag;
                            roomPlayers[i].frame.sprite = Appearance.watchFace0Tag;
                            break;
                        case PlayerPosition.WatchOp:
                            roomPlayers[i].frame.material = Appearance.watchFrameMat1;
                            roomPlayers[i].frame.sprite = Appearance.watchFace1;
                            break;
                        case PlayerPosition.WatchOpTag:
                            roomPlayers[i].frame.material = Appearance.watchFrameMat1Tag;
                            roomPlayers[i].frame.sprite = Appearance.watchFace1Tag;
                            break;
                    }
                }
            }
            if (isHost)
            {
                btnStart.gameObject.SetActive(true);
                roomPlayers[0].button.SetActive(true);
                roomPlayers[1].button.SetActive(true);
                roomPlayers[2].button.SetActive(true);
                roomPlayers[3].button.SetActive(true);
            }
            else
            {
                btnStart.gameObject.SetActive(false);
                roomPlayers[0].button.SetActive(false);
                roomPlayers[1].button.SetActive(false);
                roomPlayers[2].button.SetActive(false);
                roomPlayers[3].button.SetActive(false);
            }
            if (selfType == 7)
                btnReady.gameObject.SetActive(false);
            else
                btnReady.gameObject.SetActive(true);
        }

        void ShowOcgCore()
        {
            if (Program.I().ocgcore.isShowed)
                return;
            if (mode != 2)
            {
                if (selfType == 7)
                {
                    Program.I().ocgcore.name_0 = GetPlayerName(0);
                    Program.I().ocgcore.name_1 = GetPlayerName(1);
                }
                else
                {
                    Program.I().ocgcore.name_0 = GetPlayerName(selfType);
                    Program.I().ocgcore.name_1 = GetPlayerName(1 - selfType);
                }
                Program.I().ocgcore.name_0_c = Program.I().ocgcore.name_0;
                Program.I().ocgcore.name_1_c = Program.I().ocgcore.name_1;
                Program.I().ocgcore.name_0_tag = "---";
                Program.I().ocgcore.name_1_tag = "---";
            }
            else
            {
                if (selfType == 7)
                {
                    Program.I().ocgcore.name_0 = GetPlayerName(0);
                    Program.I().ocgcore.name_0_tag = GetPlayerName(1);
                    Program.I().ocgcore.name_1 = GetPlayerName(2);
                    Program.I().ocgcore.name_1_tag = GetPlayerName(3);
                }
                else
                {
                    int op = 0;
                    int opTag = 0;
                    switch (selfType)
                    {
                        case 0:
                        case 1:
                            op = 2;
                            opTag = 3;
                            break;
                        case 2:
                        case 3:
                            op = 0;
                            opTag = 1;
                            break;
                    }
                    Program.I().ocgcore.name_0 = GetPlayerName((selfType == 0 || selfType == 2) ? selfType : selfType - 1);
                    Program.I().ocgcore.name_0_tag = GetPlayerName((selfType == 0 || selfType == 2) ? selfType + 1 : selfType);
                    Program.I().ocgcore.name_1 = GetPlayerName(op);
                    Program.I().ocgcore.name_1_tag = GetPlayerName(opTag);
                }
            }
            Program.I().ocgcore.timeLimit = timeLimit;
            Program.I().ocgcore.lpLimit = startLp;
            if(fromSolo)
                Program.I().ocgcore.returnServant = Program.I().solo;
            else
                Program.I().ocgcore.returnServant = Program.I().online;
            if (selfType == 7)
                Program.I().ocgcore.condition = OcgCore.Condition.Watch;
            else
                Program.I().ocgcore.condition = OcgCore.Condition.Duel;
            Program.I().ocgcore.inAi = false;
            Program.I().ShiftToServant(Program.I().ocgcore);
        }

        #region STOC
        public void StocMessage_GameMsg(BinaryReader r)
        {
            ShowOcgCore();
            var p = new Package();
            p.Function = r.ReadByte();
            p.Data = new BinaryMaster(r.ReadToEnd());
            Program.I().ocgcore.AddPackage(p);
        }

        public void StocMessage_ErrorMsg(BinaryReader r)
        {
            int msg = r.ReadByte();
            r.ReadByte();
            r.ReadByte();
            r.ReadByte();
            var code = r.ReadInt32();
            switch (msg)
            {
                case 1:
                    switch (code)
                    {
                        case 0:
                            MessageManager.Cast(StringHelper.GetUnsafe(1403));
                            break;
                        case 1:
                            MessageManager.Cast(StringHelper.GetUnsafe(1404));
                            break;
                        case 2:
                            MessageManager.Cast(StringHelper.GetUnsafe(1405));
                            break;
                    }
                    break;
                case 2:
                    var flag = code >> 28;
                    code = code & 0xFFFFFFF;
                    var cardName = CardsManager.Get(code).Name;
                    List<string> tasks = new List<string>() { StringHelper.GetUnsafe(1406) };
                    var task = "";
                    switch (flag)
                    {
                        case 1:
                            task = StringHelper.GetUnsafe(1407);//「%ls」的数量不符合当前禁限卡表设定。
                            var replace = new Regex("%ls");
                            task = replace.Replace(task, cardName);
                            break;
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                            task = StringHelper.GetUnsafe(1411 + flag);
                            replace = new Regex("%ls");
                            task = replace.Replace(task, cardName);
                            break;
                        case 6:
                        case 7:
                        case 8:
                        case 9:
                            task = StringHelper.GetUnsafe(1411 + flag);
                            replace = new Regex("%ls");
                            var target = "";
                            if (flag == 6)
                                target = deck.Main.Count.ToString();
                            else if (flag == 7)
                                target = deck.Extra.Count.ToString();
                            else if (flag == 8)
                                target = deck.Side.Count.ToString();
                            task = replace.Replace(task, target);
                            break;
                        default:
                            task = StringHelper.GetUnsafe(1406);
                            break;
                    }
                    task = task.Replace("(%d)", "");
                    tasks.Add(task);
                    UIManager.ShowPopupConfirm(tasks);
                    break;
                case 3:
                    tasks = new List<string>()
                {
                    StringHelper.GetUnsafe(1408),
                    StringHelper.GetUnsafe(1410),
                };
                    UIManager.ShowPopupConfirm(tasks);
                    break;
                case 4:
                    Debug.Log("Room Error: 4");
                    break;
            }
        }
        public void StocMessage_SelectHand(BinaryReader r)
        {
            //DOTween.To(() => cg.alpha, x => cg.alpha = x, 0, transitionTime);
            if (soloLockHand || Config.Get("AutoRPS", "0") == "0")
            {
                var handle = Addressables.InstantiateAsync("PopupRockPaperScissors");
                handle.Completed += (result) =>
                {
                    result.Result.transform.SetParent(Program.I().ui_.popup, false);
                    var popupRPS = result.Result.GetComponent<PopupRockPaperScissors>();
                    popupRPS.selections = new List<string> { InterString.Get("猜拳") };
                    popupRPS.Show();
                };
            }
            else
                TcpHelper.CtosMessage_HandResult(UnityEngine.Random.Range(1, 4));
        }

        public void StocMessage_SelectTp(BinaryReader r)
        {
            List<string> selections = new List<string>
            {
                Program.I().currentServant == Program.I().room ?
                InterString.Get("猜拳获胜") :
                InterString.Get("选择先后手"),
                InterString.Get("选择是否由我方先手？"),
                InterString.Get("先攻"),
                InterString.Get("后攻")
            };
            UIManager.ShowPopupYesOrNo(selections, () => { GoFirst(true); }, () => { GoFirst(false); });
        }
        void GoFirst(bool first)
        {
            TcpHelper.CtosMessage_TpResult(first);
        }
        public void StocMessage_HandResult(BinaryReader r)
        {
            if (selfType == 7)
                return;

            int meResult = r.ReadByte();
            int opResult = r.ReadByte();
            if (meResult == opResult)
                MessageManager.Cast(InterString.Get("猜拳平局。"));
            else if (meResult == 1 && opResult == 2
                || meResult == 2 && opResult == 3
                || meResult == 3 && opResult == 1)
                MessageManager.Cast(InterString.Get("猜拳落败。"));
        }
        public void StocMessage_TpResult(BinaryReader r)
        {
        }

        public void StocMessage_ChangeSide(BinaryReader r)
        {
            Program.I().ocgcore.returnServant = Program.I().editDeck;
            needSide = true;
            if (Program.I().ocgcore.condition != OcgCore.Condition.Duel || joinWithReconnect)
                Program.I().ocgcore.OnDuelResultConfirmed();
        }
        public void StocMessage_WaitingSide(BinaryReader r)
        {
            sideWaitingObserver = true;
            MessageManager.Cast(InterString.Get("请耐心等待双方玩家更换副卡组。"));
        }
        public void StocMessage_DeckCount(BinaryReader r)
        {

        }
        public void StocMessage_CreateGame(BinaryReader r)
        {

        }
        public void StocMessage_JoinGame(BinaryReader r)
        {
            lfList = r.ReadUInt32();
            rule = r.ReadByte();
            mode = r.ReadByte();
            Program.I().ocgcore.MasterRule = r.ReadChar();
            noCheckDeck = r.ReadBoolean();
            noShuffleDeck = r.ReadBoolean();
            r.ReadByte();
            r.ReadByte();
            r.ReadByte();
            startLp = r.ReadInt32();
            startHand = r.ReadByte();
            drawCount = r.ReadByte();
            timeLimit = r.ReadInt16();

            for (int i = 0; i < 4; i++)
                players[i] = null;
            Program.I().ShiftToServant(Program.I().room);
        }
        public void StocMessage_TypeChange(BinaryReader r)
        {
            int type = r.ReadByte();
            selfType = type & 0xF;
            isHost = ((type >> 4) & 0xF) != 0;
            if (selfType < 4 && players[selfType] != null)
                players[selfType].ready = false;
            Realize();
        }
        public void StocMessage_LeaveGame(BinaryReader r)
        {

        }
        public void StocMessage_DuelStart(BinaryReader r)
        {
            needSide = false;
            joinWithReconnect = true;
            if (Program.I().editDeck.isShowed)
            {
                Program.I().editDeck.Hide(0);
                MessageManager.Cast("更换副卡组成功，请等待对手更换副卡组。");
            }

            if (isShowed)
                Hide(0);
        }
        public void StocMessage_DuelEnd(BinaryReader r)
        {
            duelEnded = true;
            Program.I().ocgcore.ForceMSquit();
        }
        public void StocMessage_Replay(BinaryReader r)
        {
            var data = r.ReadToEnd();
            var p = new Package();
            p.Function = (int)GameMessage.sibyl_replay;
            p.Data.writer.Write(data);
            TcpHelper.AddRecordLine(p);
        }
        public void StocMessage_Chat(BinaryReader r)
        {

            int player = r.ReadInt16();
            var length = r.BaseStream.Length - 3;
            var content = r.ReadUnicode((int)length);
            //Debug.Log("StocMessage_Chat: " + player + "-" + content);

            AddChatItem(player, content);
        }
        public void StocMessage_HsPlayerEnter(BinaryReader r)
        {
            AudioManager.PlaySE("SE_ROOM_SITDOWN");
            var name = r.ReadUnicode(20);
            var pos = r.ReadByte() & 3;
            var player = new Player();
            player.name = name;
            player.ready = false;
            players[pos] = player;
            Realize();
        }
        public void StocMessage_HsPlayerChange(BinaryReader r)
        {
            int status = r.ReadByte();
            var pos = (status >> 4) & 0xF;
            var state = status & 0xF;
            if (pos < 4)
            {
                if (state < 8)
                {
                    players[state] = players[pos];
                    players[pos] = null;
                }
                if (state == 0x9)
                    players[pos].ready = true;
                if (state == 0xA)
                    players[pos].ready = false;
                if (state == 0xB)
                    players[pos] = null;
                if (state == 0x8)
                {
                    players[pos] = null;
                    observerCount++;
                }
                Realize();
            }
        }

        public void StocMessage_HsWatchChange(BinaryReader r)
        {
            observerCount = r.ReadUInt16();
            Realize();
        }



        #endregion
    }
}
