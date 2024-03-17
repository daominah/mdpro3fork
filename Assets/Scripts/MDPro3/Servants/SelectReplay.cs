using MDPro3.UI;
using Percy;
using SevenZip.Compression.LZMA;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using GameMessage = MDPro3.YGOSharp.OCGWrapper.Enums.GameMessage;

namespace MDPro3
{
    public class SelectReplay : Servant
    {
        public ScrollRect scrollView;
        public Text description;
        public SuperScrollViewTwoStage superScrollView;
        public GameObject buttons;
        public Button btnPlayer1;
        public Button btnPlayer2;
        public Button btnPlayer3;
        public Button btnPlayer4;
        public Text textSort;
        GameObject item;

        PercyOCG percy;

        public override void Initialize()
        {
            depth = 1;
            haveLine = true;
            returnServant = Program.I().menu;
            base.Initialize();
            buttons.SetActive(false);
            var handle = Addressables.LoadAssetAsync<GameObject>("ButtonTwoStageForReplay");
            handle.Completed += (result) => { item = result.Result; };
        }

        public override void Show(int preDepth)
        {
            base.Show(preDepth);
            Print();
        }

        void SelectZero()
        {
            var item0 = superScrollView.items[0].gameObject.GetComponent<SuperScrollViewItemTwoStageForReplay>();
            item0.ToStage1();
        }

        public void Print(string search = "")
        {
            superScrollView?.Clear();

            if (!Directory.Exists("Replay"))
                Directory.CreateDirectory("Replay");
            var fileInfos = new DirectoryInfo("Replay").GetFiles();
            if (sortByName)
                Array.Sort(fileInfos, Tools.CompareName);
            else
                Array.Sort(fileInfos, Tools.CompareTime);

            List<string[]> tasks = new List<string[]>();
            int count = 0;
            for (int i = 0; i < fileInfos.Length; i++)
            {
                if (fileInfos[i].Name.EndsWith(".yrp3d"))
                {
                    var task = new string[] { count.ToString(), fileInfos[i].Name.Replace(".yrp3d", "") };
                    tasks.Add(task);
                    count++;
                }
                else if (fileInfos[i].Name.EndsWith(".yrp"))
                {
                    var task = new string[] { count.ToString(), fileInfos[i].Name };
                    tasks.Add(task);
                    count++;
                }
            }

            superScrollView = new SuperScrollViewTwoStage
                (
                1,
                700,
                140,
                0,
                -10,
                item,
                ItemOnListRefresh,
                scrollView,
                30
                );
            superScrollView.Print(tasks);
            if (tasks.Count > 0)
                SelectZero();
        }

        void ItemOnListRefresh(string[] task, GameObject item)
        {
            var handler = item.GetComponent<SuperScrollViewItemTwoStageForReplay>();
            handler.id = int.Parse(task[0]);
            handler.replayName = task[1];
            handler.Refresh();
        }

        public void KF_Replay(string name, bool god = false)
        {
            string fileName = "Replay/" + name + (name.EndsWith(".yrp") ? "" : ".yrp3d");
            if (!File.Exists(fileName))
            {
                fileName = fileName.Replace(".yrp3d", ".yrp");
                if (!File.Exists(fileName))
                    return;
            }
            bool yrp3d = fileName.Length > 6 && fileName.ToLower().Substring(fileName.Length - 6, 6) == ".yrp3d";
            try
            {
                if (yrp3d)
                {
                    if (god)
                    {
                        MessageManager.Cast(InterString.Get("您正在观看旧版的回放（上帝视角），不保证稳定性。"));
                        percy?.Dispose();
                        percy = new PercyOCG();
                        var replays = GetYRPBuffer(fileName);
                        var collections = TcpHelper.GetPackages(percy.ygopro.GetYRP3dBuffer(GetYRP(replays[replays.Count - 1])));
                        PushCollection(collections);
                    }
                    else
                    {
                        var collection = TcpHelper.ReadPackagesInRecord(fileName);
                        PushCollection(collection);
                    }
                }
                else
                {
                    MessageManager.Cast(InterString.Get("您正在观看旧版的回放（上帝视角），不保证稳定性。"));
                    percy?.Dispose();
                    percy = new PercyOCG();
                    var collections = TcpHelper.GetPackages(percy.ygopro.GetYRP3dBuffer(GetYRP(File.ReadAllBytes(fileName))));
                    PushCollection(collections);
                }
            }
            catch
            {
                MessageManager.Cast(InterString.Get("回放没有录制完整。"));
            }
        }
        List<byte[]> GetYRPBuffer(string path)
        {
            if (path.EndsWith(".yrp"))
                return new List<byte[]>() { File.ReadAllBytes(path) };
            var returnValue = new List<byte[]>();
            try
            {
                var collection = TcpHelper.ReadPackagesInRecord(path);
                foreach (var item in collection)
                    if (item.Function == (int)GameMessage.sibyl_replay)
                    {
                        var replay = item.Data.reader.ReadToEnd();
                        // TODO: don't include other replays
                        returnValue.Add(replay);
                    }
            }
            catch (Exception e) { Debug.LogError(e); }
            return returnValue;
        }

        YRP GetYRP(byte[] buffer)
        {
            var returnValue = new YRP();
            try
            {
                var reader = new BinaryReader(new MemoryStream(buffer));
                returnValue.ID = reader.ReadInt32();
                returnValue.Version = reader.ReadInt32();
                returnValue.Flag = reader.ReadInt32();
                returnValue.Seed = reader.ReadUInt32();
                returnValue.DataSize = reader.ReadInt32();
                returnValue.Hash = reader.ReadInt32();
                returnValue.Props = reader.ReadBytes(8);
                var raw = reader.ReadToEnd();
                if ((returnValue.Flag & 0x1) > 0)
                {
                    var lzma = new Decoder();
                    lzma.SetDecoderProperties(returnValue.Props);
                    var decompressed = new MemoryStream();
                    lzma.Code(new MemoryStream(raw), decompressed, raw.LongLength, returnValue.DataSize, null);
                    raw = decompressed.ToArray();
                }

                reader = new BinaryReader(new MemoryStream(raw));
                if ((returnValue.Flag & 0x2) > 0)
                {
                    Program.I().room.mode = 2;
                    returnValue.playerData.Add(new YRP.PlayerData());
                    returnValue.playerData.Add(new YRP.PlayerData());
                    returnValue.playerData.Add(new YRP.PlayerData());
                    returnValue.playerData.Add(new YRP.PlayerData());
                    returnValue.playerData[0].name = reader.ReadUnicode(20);
                    returnValue.playerData[1].name = reader.ReadUnicode(20);
                    returnValue.playerData[2].name = reader.ReadUnicode(20);
                    returnValue.playerData[3].name = reader.ReadUnicode(20);
                    returnValue.StartLp = reader.ReadInt32();
                    returnValue.StartHand = reader.ReadInt32();
                    returnValue.DrawCount = reader.ReadInt32();
                    returnValue.opt = reader.ReadInt32();
                    Program.I().ocgcore.MasterRule = returnValue.opt >> 16;
                    for (var i = 0; i < 4; i++)
                    {
                        var count = reader.ReadInt32();
                        for (var i2 = 0; i2 < count; i2++) returnValue.playerData[i].main.Add(reader.ReadInt32());
                        count = reader.ReadInt32();
                        for (var i2 = 0; i2 < count; i2++) returnValue.playerData[i].extra.Add(reader.ReadInt32());
                    }
                }
                else
                {
                    returnValue.playerData.Add(new YRP.PlayerData());
                    returnValue.playerData.Add(new YRP.PlayerData());
                    returnValue.playerData[0].name = reader.ReadUnicode(20);
                    returnValue.playerData[1].name = reader.ReadUnicode(20);
                    returnValue.StartLp = reader.ReadInt32();
                    returnValue.StartHand = reader.ReadInt32();
                    returnValue.DrawCount = reader.ReadInt32();
                    returnValue.opt = reader.ReadInt32();
                    Program.I().ocgcore.MasterRule = returnValue.opt >> 16;
                    for (var i = 0; i < 2; i++)
                    {
                        var count = reader.ReadInt32();
                        for (var i2 = 0; i2 < count; i2++) returnValue.playerData[i].main.Add(reader.ReadInt32());
                        count = reader.ReadInt32();
                        for (var i2 = 0; i2 < count; i2++) returnValue.playerData[i].extra.Add(reader.ReadInt32());
                    }
                }
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                    returnValue.gameData.Add(reader.ReadBytes(reader.ReadByte()));
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            return returnValue;
        }

        Dictionary<string, YRP> cachedYRPs = new Dictionary<string, YRP>();
        public YRP CacheYRP(string replay)
        {
            if (cachedYRPs.ContainsKey(replay))
                return cachedYRPs[replay];
            YRP yrp;
            if (File.Exists("Replay/" + replay))
                yrp = GetYRP(File.ReadAllBytes("Replay/" + replay));
            else
            {
                var buffer = GetYRPBuffer("Replay/" + replay + ".yrp3d");
                if (buffer.Count == 0)
                    yrp = null;
                else
                    yrp = GetYRP(buffer[0]);
            }
            if (yrp != null)
                cachedYRPs.Add(replay, yrp);
            return yrp;
        }

        void PushCollection(List<Package> collection)
        {
            Program.I().ocgcore.returnServant = Program.I().replay;
            Program.I().ocgcore.handler = a => { };
            Program.I().ocgcore.name_0 = Config.Get("ReplayPlayerName0", "@ui");
            Program.I().ocgcore.name_0_tag = Config.Get("ReplayPlayerName0Tag", "@ui");
            Program.I().ocgcore.name_0_c = Program.I().ocgcore.name_0;
            Program.I().ocgcore.name_1 = Config.Get("ReplayPlayerName1", "@ui");
            Program.I().ocgcore.name_1_tag = Config.Get("ReplayPlayerName1Tag", "@ui");
            Program.I().ocgcore.name_1_c = Program.I().ocgcore.name_1;
            Program.I().ocgcore.timeLimit = 240;
            Program.I().ocgcore.lpLimit = 8000;
            Program.I().ocgcore.isFirst = true;
            //Program.I().ocgcore.inAI = false;
            Program.I().ocgcore.condition = OcgCore.Condition.Replay;
            Program.I().ShiftToServant(Program.I().ocgcore);
            Program.I().ocgcore.FlushPackages(collection);
        }


        public void OnRename()
        {
            var selections = new List<string>()
        {
            InterString.Get("请输入新的回放名称"),
            superScrollView.items[superScrollView.selected].args[1].Replace(".yrp", "")
        };
            UIManager.ShowPopupInput(selections, ReplayRename, null, InputValidation.ValidationType.Path);
        }

        void ReplayRename(string newName)
        {
            string replay = superScrollView.items[superScrollView.selected].args[1];
            if (replay.EndsWith(".yrp"))
                File.Move("Replay/" + replay, "Replay/" + newName + ".yrp");
            else
                File.Move("Replay/" + replay + ".yrp3d", "Replay/" + newName + ".yrp3d");
            Print();
        }

        public void OnPlay()
        {
            Program.I().replay.KF_Replay(superScrollView.items[superScrollView.selected].args[1]);
        }
        public void OnGod()
        {
            Program.I().replay.KF_Replay(superScrollView.items[superScrollView.selected].args[1], true);
        }
        public void OnDelete()
        {
            var replay = superScrollView.items[superScrollView.selected].args[1];
            if (File.Exists("Replay/" + replay))
                File.Delete("Replay/" + replay);
            else
                File.Delete("Replay/" + replay + ".yrp3d");
            MessageManager.Cast(InterString.Get("已删除回放「[?]」。", replay));
            Print();
        }

        bool sortByName = true;
        public void OnSort()
        {
            sortByName = !sortByName;
            if (sortByName)
                textSort.text = InterString.Get("名称排序");
            else
                textSort.text = InterString.Get("时间排序");

            Print();
        }
        public void OnDeck(int player)
        {
            var replay = superScrollView.items[superScrollView.selected].args[1];
            var yrp = cachedYRPs[replay];
            replay = replay.Replace(".yrp", "");

            var value = "#created by mdpro3\r\n#main\r\n";
            for (int i = 0; i < yrp.playerData[player].main.Count; i++)
                value += yrp.playerData[player].main[i] + "\r\n";
            value += "#extra\r\n";
            for (int i = 0; i < yrp.playerData[player].extra.Count; i++)
                value += yrp.playerData[player].extra[i] + "\r\n";
            int count = 0;
            bool saved = false;
            while (!saved)
            {
                if (File.Exists("Deck/" + replay + "_" + yrp.playerData[player].name + "_" + count + ".ydk"))
                    count++;
                else
                {
                    File.WriteAllText("Deck/" + replay + "_" + yrp.playerData[player].name + "_" + count + ".ydk", value);
                    Config.Set("DeckInUse", replay + "_" + yrp.playerData[player].name + "_" + count);
                    saved = true;
                }
            }

            Program.I().ShiftToServant(Program.I().editDeck);
            Program.I().editDeck.returnServant = Program.I().replay;

        }
    }
}
