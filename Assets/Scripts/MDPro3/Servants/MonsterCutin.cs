using DG.Tweening;
using JetBrains.Annotations;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Playables;
using UnityEngine.UI;
using YgomSystem.ElementSystem;
using YgomSystem.YGomTMPro;
using MDPro3.YGOSharp;
using MDPro3.YGOSharp.OCGWrapper.Enums;
using MDPro3.UI;

namespace MDPro3
{
    public class MonsterCutin : Servant
    {
        public ScrollRect scrollView;
        public InputField inputField;
        public static List<Card> cards = new List<Card>();
        public static int controller = 0;

        static DirectoryInfo[] dirInfos;
        static FileInfo[] fileInfos;
        List<string[]> tasks = new List<string[]>();
        SuperScrollView superScrollView;

        public override void Initialize()
        {
            depth = 1;
            haveLine = false;
            returnServant = Program.I().menu;
            base.Initialize();
            if (!Directory.Exists(Program.root + "Monstercutin"))
                Directory.CreateDirectory(Program.root + "Monstercutin");
            dirInfos = new DirectoryInfo(Program.root + "Monstercutin").GetDirectories();
            if (!Directory.Exists(Program.root + "Monstercutin2"))
                Directory.CreateDirectory(Program.root + "Monstercutin2");
            fileInfos = new DirectoryInfo(Program.root + "Monstercutin2").GetFiles();
            inputField.onEndEdit.AddListener(Print);
            Load();
        }

        public void Load()
        {
            cards.Clear();
            for (int i = 0; i < dirInfos.Length; i++)
            {
                Card card = CardsManager.Get(int.Parse(dirInfos[i].Name));
                cards.Add(card);
            }
            for (int i = 0; i < fileInfos.Length; i++)
            {
                Card card = CardsManager.Get(int.Parse(fileInfos[i].Name));
                if (!cards.Contains(card))
                    cards.Add(card);
            }
            cards.Sort(CardsManager.ComparisonOfCard());
            Print();
        }

        public void Print(string search = "")
        {
            superScrollView?.Clear();
            tasks.Clear();
            foreach (var card in cards)
            {
                if (card.Name.Contains(search))
                {
                    string code = card.Id.ToString();
                    string cardName = card.Name;
                    string[] task = new string[] { code, cardName };
                    tasks.Add(task);
                }
            }
            var handle = Addressables.LoadAssetAsync<GameObject>("ButtonMonsterCutin");
            handle.Completed += (result) =>
            {
                superScrollView = new SuperScrollView
                    (
                    1,
                    360,
                    40,
                    0,
                    0,
                    result.Result,
                    ItemOnListRefresh,
                    scrollView
                    );
                superScrollView.Print(tasks);
            };
        }

        public override void OnExit()
        {
            base.OnExit();
            if (randomBGMPlayed)
            {
                randomBGMPlayed = false;
                AudioManager.PlayBGM("BGM_MENU_01");
            }

            CameraManager.DuelOverlayEffect3DCount = 0;
            CameraManager.DuelOverlayEffect3DMinus();
            DOTween.To(v => { }, 0, 0, transitionTime).OnComplete(() =>
            {
                Resources.UnloadUnusedAssets();
                GC.Collect();
            });
        }

        public override void OnReturn()
        {
            if (returnAction != null) return;
            if (inTransition) return;
            AudioManager.PlaySE("SE_MENU_CANCEL");
            if (cg.alpha == 0)
            {
                StopCoroutine(autoPlay);
                autoPlay = null;
                UIManager.ShowExitButton(transitionTime);
                cg.alpha = 1;
                cg.blocksRaycasts = true;
            }
            else
                OnExit();
        }

        public static bool HasCutin(int code)
        {
            bool cutinEffect = true;
            if (Program.I().ocgcore.condition == OcgCore.Condition.Duel
                && Config.Get("DuelCutin", "1") == "0")
                cutinEffect = false;
            if (Program.I().ocgcore.condition == OcgCore.Condition.Watch
                && Config.Get("WatchCutin", "1") == "0")
                cutinEffect = false;
            if (Program.I().ocgcore.condition == OcgCore.Condition.Replay
                && Config.Get("ReplayCutin", "1") == "0")
                cutinEffect = false;
            if (!cutinEffect)
                return false;

            bool returnValue = false;
            foreach (var card in cards)
            {
                if (card.Id == code)
                {
                    returnValue = true;
                    break;
                }
            }
            return returnValue;
        }

        static bool playing;
        public static void Play(int code, int controller, bool isDiy = false, GameObject cutin = null)
        {
            if (playing) 
                return;
            playing = true;
            if (Program.I().ocgcore.isShowed)
                AudioManager.PlayBGMKeyCard(Program.I().ocgcore.field1Manager.name);
            DOTween.To(v => { }, 0, 0, 1.6f).OnComplete(() =>
            {
                playing = false;
            });
            code = AliasCode(code);
            Card card = CardsManager.Get(code);

            GameObject loader = null;
            bool diy = false;
            if(cutin == null)
            {
                if (Directory.Exists(Program.root + "Monstercutin/" + code))
                    loader = ABLoader.LoadFromFolder("Monstercutin/" + code, "Spine" + code);
                else
                {
                    loader = ABLoader.LoadFromFile("Monstercutin2/" + code);
                    diy = true;
                }
            }
            else
            {
                loader = cutin;
                diy = isDiy;
            }

            loader.transform.SetParent(Program.I().container_2D, false);
            Destroy(loader, 1.6f);

            if (diy)
            {

            }
            else
            {
                loader.transform.GetChild(0).localPosition = Vector3.zero;
            }

            //BackEffects
            GameObject back;
            if ((card.Attribute & (uint)CardAttribute.Dark) > 0)//125
                back = ABLoader.LoadFromFile("timeline/summon/summonmonster/04backeff/summonmonster_bgdak_s2", true);
            else if ((card.Attribute & (uint)CardAttribute.Light) > 0)//100
                back = ABLoader.LoadFromFile("timeline/summon/summonmonster/04backeff/summonmonster_bglit_s2", true);
            else if ((card.Attribute & (uint)CardAttribute.Earth) > 0)//56
                back = ABLoader.LoadFromFile("timeline/summon/summonmonster/04backeff/summonmonster_bgeah_s2", true);
            else if ((card.Attribute & (uint)CardAttribute.Water) > 0)//35
                back = ABLoader.LoadFromFile("timeline/summon/summonmonster/04backeff/summonmonster_bgwtr_s2", true);
            else if ((card.Attribute & (uint)CardAttribute.Fire) > 0)//31
                back = ABLoader.LoadFromFile("timeline/summon/summonmonster/04backeff/summonmonster_bgfie_s2", true);
            else if ((card.Attribute & (uint)CardAttribute.Wind) > 0)//25
                back = ABLoader.LoadFromFile("timeline/summon/summonmonster/04backeff/summonmonster_bgwid_s2", true);
            else//4
                back = ABLoader.LoadFromFile("timeline/summon/summonmonster/04backeff/summonmonster_bgdve_s2", true);
            back.transform.SetParent(Program.I().container_2D, false);
            Transform eff_flame = back.transform.Find("Eff_Flame");
            eff_flame.localScale = new Vector3(2.76f, 1.55f, 1f);
            eff_flame.gameObject.AddComponent<AutoScaleOnce>();
            Transform eff_bg00 = back.transform.Find("Eff_Bg00");
            eff_bg00.localScale = new Vector3(250f, 25f, 1f);
            Transform flame_re = back.transform.Find("flame_re");
            if (flame_re == null)
                flame_re = back.transform.Find("Eff_group/flame_re");
            if (flame_re == null)
                flame_re = back.transform.Find("Eff_Flame01_re");
            flame_re.gameObject.AddComponent<AutoScaleOnce>();
            Destroy(back, 1.6f);

            //Name Bar
            GameObject nameBar;
            if (controller == 0)
                nameBar = ABLoader.LoadFromFile("timeline/summon/summonmonster/01text/summonmonster_name_near", true);
            else
                nameBar = ABLoader.LoadFromFile("timeline/summon/summonmonster/01text/summonmonster_name_far", true);

            nameBar.transform.SetParent(Program.I().container_2D, false);
            var manager = nameBar.GetComponent<ElementObjectManager>();
            var tmp = manager.GetElement<ExtendedTextMeshPro>("Monster_Name_TMP");
            tmp.font = Program.I().ui_.tmpFont;
            tmp.text = card.Name;
            var para = "ATK " + (card.Attack == -2 ? "?" : card.Attack.ToString());
            if ((card.Type & (uint)CardType.Link) == 0)
            {
                para += " DEF " + (card.Defense == -2 ? "?" : card.Defense.ToString());
                Destroy(manager.GetElement("Icon_LINK"));
            }
            else
            {
                Destroy(manager.GetElement("Icon_Level"));
                Destroy(manager.GetElement("Icon_Level_Odd"));
                Destroy(manager.GetElement("Icon_Rank"));
                Destroy(manager.GetElement("Icon_Rank_Odd"));
                switch (CardDescription.GetCardLinkCount(card))
                {
                    case 2:
                        manager.GetElement<ElementObjectManager>("Icon_LINK").
                            GetElement<SpriteRenderer>("LINK1").sprite = TextureManager.container.link2;
                        break;
                    case 3:
                        manager.GetElement<ElementObjectManager>("Icon_LINK").
                            GetElement<SpriteRenderer>("LINK1").sprite = TextureManager.container.link3;
                        break;
                    case 4:
                        manager.GetElement<ElementObjectManager>("Icon_LINK").
                            GetElement<SpriteRenderer>("LINK1").sprite = TextureManager.container.link4;
                        break;
                    case 5:
                        manager.GetElement<ElementObjectManager>("Icon_LINK").
                            GetElement<SpriteRenderer>("LINK1").sprite = TextureManager.container.link5;
                        break;
                    case 6:
                        manager.GetElement<ElementObjectManager>("Icon_LINK").
                            GetElement<SpriteRenderer>("LINK1").sprite = TextureManager.container.link6;
                        break;
                }
            }

            ElementObjectManager subManager;
            if ((card.Type & (uint)CardType.Xyz) == 0)
            {
                Destroy(manager.GetElement("Icon_Rank"));
                Destroy(manager.GetElement("Icon_Rank_Odd"));
                if (card.Level % 2 == 0)
                {
                    subManager = manager.GetElement<ElementObjectManager>("Icon_Level");
                    Destroy(manager.GetElement("Icon_Level_Odd"));
                }
                else
                {
                    subManager = manager.GetElement<ElementObjectManager>("Icon_Level_Odd");
                    Destroy(manager.GetElement("Icon_Level"));
                }
            }
            else
            {
                Destroy(manager.GetElement("Icon_Level"));
                Destroy(manager.GetElement("Icon_Level_Odd"));
                if (card.Level % 2 == 0)
                {
                    subManager = manager.GetElement<ElementObjectManager>("Icon_Rank");
                    Destroy(manager.GetElement("Icon_Rank_Odd"));
                }
                else
                {
                    subManager = manager.GetElement<ElementObjectManager>("Icon_Rank_Odd");
                    Destroy(manager.GetElement("Icon_Rank"));
                }
            }
            if ((card.Type & (uint)CardType.Link) == 0)
                for (int i = card.Level + 1; i < 14; i++)
                    Destroy(subManager.GetElement("Icon" + i));
            manager.GetElement<TextMesh>("Monster_Para").text = para;
            Destroy(nameBar, 1.6f);

            //front Effect
            var frontEffect = ABLoader.LoadFromFile("timeline/summon/summonmonster/02fronteff/summonmonster_thunder_power", true);
            frontEffect.transform.SetParent(Program.I().container_2D, false);
            Destroy(frontEffect, 1.6f);
        }

        IEnumerator autoPlay;
        public void AutoPlay()
        {
            if (autoPlay != null) return;
            autoPlay = AutoPlayAsync();
            StartCoroutine(autoPlay);
        }
        bool randomBGMPlayed;
        IEnumerator AutoPlayAsync()
        {
            while (playing)
                yield return null;
            AudioManager.PlayRandomKeyCardBGM();
            randomBGMPlayed = true;
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            UIManager.HideExitButton(transitionTime);
            int count = 0;
            foreach (var card in cards)
            {
                IEnumerator<GameObject> ie;
                bool diy = false;
                if (Directory.Exists(Program.root + "Monstercutin/" + card.Id))
                    ie = ABLoader.LoadFromFolderAsync("MonsterCutin/" + card.Id, "Spine" + card.Id, false, true);
                else
                {
                    ie = ABLoader.LoadFromFileAsync("MonsterCutin2/" + card.Id, false, true);
                    diy = true;
                }
                StartCoroutine(ie);
                while (ie.MoveNext())
                    yield return null;
                ie.Current.SetActive(false);
                while (playing)
                    yield return null;
                ie.Current.SetActive(true);
                Play(card.Id, 0, diy, ie.Current);
                count++;
                if (count % 20 == 0)
                {
                    var unload =  Resources.UnloadUnusedAssets();
                    while (!unload.isDone)
                        yield return null;
                }
            }
            cg.alpha = 1f;
            cg.blocksRaycasts = true;
            UIManager.ShowExitButton(transitionTime);
            autoPlay = null;
        }

        void ItemOnListRefresh(string[] task, GameObject item)
        {
            SuperScrollViewItemForCutin handler = item.GetComponent<SuperScrollViewItemForCutin>();
            handler.code = int.Parse(task[0]);
            handler.cardName = task[1];
            handler.Refresh();
        }

        static int AliasCode(int code)
        {
            if (code == 89631142 || code == 89631148)//ÇàÑÛ°×Áú
                return 89631141;
            if (code == 89943725)//ÐÂÓîÏÀ
                return 89943723;
            if (code == 46986424 || code == 46986426)//ºÚÄ§ÊõÊ¦
                return 46986417;
            if (code == 74677425)//ÕæºìÑÛºÚÁú
                return 74677424;
            return code;
        }
    }
}
