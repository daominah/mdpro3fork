using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using YgomSystem.ElementSystem;
using YgomSystem.YGomTMPro;
using MDPro3.Duel.YGOSharp;
using MDPro3.UI;
using UnityEngine.EventSystems;
using MDPro3.UI.ServantUI;

namespace MDPro3.Servant
{
    public class CutinViewer : Servant
    {

        public const float CUTIN_PLAY_TIME = 1.6f;

        public static int controller = 0;
        public static List<Card> cards = new();
        public static List<int> codes = new();
        public static List<int> codes2 = new();
        private static DirectoryInfo[] dirInfos;
        private static FileInfo[] fileInfos;
        private static bool playing;
        private bool randomBGMPlayed;
        private readonly List<GameObject> cutins = new();
        [HideInInspector] public SelectionToggle_Cutin lastSelectedCutinItem;

        #region Servant

        public override int Depth => 1;
        protected override bool ShowLine => false;

        public override void Initialize()
        {
            returnServant = Program.instance.menu;
            base.Initialize();
            LoadCutins();
        }

        protected override void ApplyHideArrangement(int nextDepth)
        {
            base.ApplyHideArrangement(nextDepth);
            UserInput.SetMoveRepeatRate(0.1f);

            if (randomBGMPlayed)
            {
                randomBGMPlayed = false;
                AudioManager.PlayBGM(AudioManager.BGM_MENU_MAIN);
            }

            CameraManager.DuelOverlayEffect3DCount = 0;
            CameraManager.DuelOverlayEffect3DMinus();
        }

        protected override void AfterHidingEvent()
        {
            Resources.UnloadUnusedAssets();
        }

        public override void PerFrameFunction()
        {
            if (NeedResponseInput())
            {
                if (UserInput.MouseRightDown || UserInput.WasCancelPressed)
                    OnReturn();

#if UNITY_ANDROID || UNITY_IOS
                if (UserInput.MouseLeftDown)
                    if(autoPlay != null)
                        OnReturn();
#endif

                if (UserInput.WasGamepadButtonWestPressed)
                    GetUI<CutinViewerUI>().FocusOnInputField();
                if (UserInput.WasGamepadButtonNorthPressed)
                    AutoPlay();
            }
        }

        public override void OnReturn()
        {
            if (returnAction != null) return;
            if (inTransition) return;
            AudioManager.PlaySE("SE_MENU_CANCEL");
            if (autoPlay != null)
            {
                StopCoroutine(autoPlay);
                autoPlay = null;
                foreach (var cutin in cutins)
                    Destroy(cutin);
                cutins.Clear();
                UIManager.ShowExitButton(TransitionTime);
                servantUI.CG.alpha = 1;
                servantUI.CG.blocksRaycasts = true;
            }
            else
                OnExit();
        }

        public override void Select(bool forced = false)
        {
            if (!forced && !UserInput.NeedDefaultSelect())
                return;
            lastSelectedCutinItem.GetSelectable().Select();
        }

#endregion

        public void LoadCutins()
        {
            if(dirInfos == null || fileInfos == null)
            {
                var targetFolder = Program.root + "MonsterCutin";
                var targetFolder2 = Program.root + "MonsterCutin2";

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
                targetFolder = Path.Combine(Application.dataPath, Program.root + "MonsterCutin");
                targetFolder2 = Path.Combine(Application.dataPath, Program.root + "MonsterCutin2");
#endif
                if (!Directory.Exists(targetFolder))
                    Directory.CreateDirectory(targetFolder);
                if (!Directory.Exists(targetFolder2))
                    Directory.CreateDirectory(targetFolder2);
                dirInfos = new DirectoryInfo(targetFolder).GetDirectories();
                fileInfos = new DirectoryInfo(targetFolder2).GetFiles();
            }

            cards.Clear();
            codes.Clear();
            codes2.Clear();

            for (int i = 0; i < dirInfos.Length; i++)
            {
                if(int.TryParse(dirInfos[i].Name, out var code))
                {
                    Card card = CardsManager.Get(code);
                    cards.Add(card);
                    codes.Add(card.Id);
                }
            }
            for (int i = 0; i < fileInfos.Length; i++)
            {
                if(int.TryParse(fileInfos[i].Name, out var code))
                {
                    if (!codes.Contains(code))
                    {
                        Card card = CardsManager.Get(code);
                        cards.Add(card);
                        codes2.Add(card.Id);
                    }
                }
            }
            cards.Sort(CardsManager.ComparisonOfCard());

            if (servantUI != null)
                GetUI<CutinViewerUI>().Print();
        }

        public void SelectLastCutinItem()
        {
            UserInput.NextSelectionIsAxis = true;
            Select();
        }

        public static bool HasCutin(int code)
        {
            if (OcgCore.condition == OcgCore.Condition.Duel
                && !Config.GetBool("DuelCutin", true))
                return false;
            if (OcgCore.condition == OcgCore.Condition.Watch
                && !Config.GetBool("WatchCutin", true))
                return false;
            if (OcgCore.condition == OcgCore.Condition.Replay
                && !Config.GetBool("ReplayCutin", true))
                return false;
            code = AliasCode(code);
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

        private static int AliasCode(int code)
        {
            if (code == 89631142 || code == 89631148)//青眼白龙
                return 89631141;
            if (code == 89943725)//新宇侠
                return 89943723;
            if (code == 46986424 || code == 46986426)//黑魔术师
                return 46986417;
            if (code == 74677425)//真红眼黑龙
                return 74677424;
            if (code == 44508096)//星尘龙
                return 44508094;
            if (code == 84013240)//霍普
                return 84013237;
            if (code == 16178684)//异色眼
                return 16178681;
            if (code == 5043013)//防火龙
                return 5043010;
            return code;
        }

        public static void Play(int code, int controller, bool isDiy = false, GameObject cutin = null)
        {
            if (playing) 
                return;
            playing = true;
            if (Program.instance.ocgcore.showing)
                AudioManager.PlayBgmKeyCard();
            DOTween.To(v => { }, 0, 0, CUTIN_PLAY_TIME).OnComplete(() =>
            {
                playing = false;
            });
            code = AliasCode(code);
            Card card = CardsManager.Get(code);

            GameObject loader = null;
            bool diy = false;
            if(cutin == null)
            {
                if (codes.Contains(code))
                    loader = ABLoader.LoadFromFolder("MonsterCutin/" + code, "Spine" + code);
                else
                {
                    loader = ABLoader.LoadFromFile("MonsterCutin2/" + code);
                    diy = true;
                }
            }
            else
            {
                loader = cutin;
                diy = isDiy;
            }

            loader.transform.SetParent(Program.instance.container_2D, false);
            Destroy(loader, CUTIN_PLAY_TIME);

            if (!diy)
            {
                loader.transform.GetChild(0).localPosition = Vector3.zero;
                loader.transform.GetChild(0).GetComponent<PlayableDirector>().time = 0;
            }

            //BackEffects
            GameObject back;
            if ((card.Attribute & (uint)CardAttribute.Dark) > 0)//125
                back = ABLoader.LoadFromFile("MasterDuel/Timeline/Summon/SummonMonster/04BackEff/SummonMonster_Bgdak_S2", true);
            else if ((card.Attribute & (uint)CardAttribute.Light) > 0)//100
                back = ABLoader.LoadFromFile("MasterDuel/Timeline/Summon/SummonMonster/04BackEff/SummonMonster_Bglit_S2", true);
            else if ((card.Attribute & (uint)CardAttribute.Earth) > 0)//56
                back = ABLoader.LoadFromFile("MasterDuel/Timeline/Summon/SummonMonster/04BackEff/SummonMonster_Bgeah_S2", true);
            else if ((card.Attribute & (uint)CardAttribute.Water) > 0)//35
                back = ABLoader.LoadFromFile("MasterDuel/Timeline/Summon/SummonMonster/04BackEff/SummonMonster_Bgwtr_S2", true);
            else if ((card.Attribute & (uint)CardAttribute.Fire) > 0)//31
                back = ABLoader.LoadFromFile("MasterDuel/Timeline/Summon/SummonMonster/04BackEff/SummonMonster_Bgfie_S2", true);
            else if ((card.Attribute & (uint)CardAttribute.Wind) > 0)//25
                back = ABLoader.LoadFromFile("MasterDuel/Timeline/Summon/SummonMonster/04BackEff/SummonMonster_Bgwid_S2", true);
            else//4
                back = ABLoader.LoadFromFile("MasterDuel/Timeline/Summon/SummonMonster/04BackEff/SummonMonster_Bgdve_S2", true);
            back.transform.SetParent(Program.instance.container_2D, false);
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
            Destroy(back, CUTIN_PLAY_TIME);

            //Name Bar
            GameObject nameBar;
            if (controller == 0)
                nameBar = ABLoader.LoadFromFile("MasterDuel/Timeline/Summon/SummonMonster/01Text/SummonMonster_Name_near", true);
            else
                nameBar = ABLoader.LoadFromFile("MasterDuel/Timeline/Summon/SummonMonster/01Text/SummonMonster_Name_far", true);

            nameBar.transform.SetParent(Program.instance.container_2D, false);
            var manager = nameBar.GetComponent<ElementObjectManager>();
            var tmp = manager.GetElement<ExtendedTextMeshPro>("Monster_Name_TMP");
            tmp.font = Program.instance.ui_.tmpFont;
            tmp.text = card.Name;
            var para = "ATK " + card.GetAttackString();
            if (!card.HasType(CardType.Link))
            {
                para += " DEF " + card.GetDefenseString();
                Destroy(manager.GetElement("Icon_LINK"));
            }
            else
            {
                Destroy(manager.GetElement("Icon_Level"));
                Destroy(manager.GetElement("Icon_Level_Odd"));
                Destroy(manager.GetElement("Icon_Rank"));
                Destroy(manager.GetElement("Icon_Rank_Odd"));
                switch (card.GetLinkCount())
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
            if (!card.HasType(CardType.Xyz))
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
            if (!card.HasType(CardType.Link))
                for (int i = card.Level + 1; i < 14; i++)
                    Destroy(subManager.GetElement("Icon" + i));
            manager.GetElement<TextMesh>("Monster_Para").text = para;
            Destroy(nameBar, CUTIN_PLAY_TIME);

            //front Effect
            var frontEffect = ABLoader.LoadFromFile("MasterDuel/Timeline/Summon/SummonMonster/02FrontEff/SummonMonster_Thunder_power", true);
            frontEffect.transform.SetParent(Program.instance.container_2D, false);
            Destroy(frontEffect, CUTIN_PLAY_TIME);
        }

        Coroutine autoPlay;
        public void AutoPlay()
        {
            if (autoPlay != null) 
                return;
            autoPlay = StartCoroutine(AutoPlayAsync());
        }

        private IEnumerator AutoPlayAsync()
        {
            while (playing)
                yield return null;
            if(!showing)
                yield break;

            AudioManager.PlayRandomKeyCardBGM();
            randomBGMPlayed = true;
            servantUI.CG.alpha = 0f;
            servantUI.CG.blocksRaycasts = false;
            UIManager.HideExitButton(TransitionTime);
            int count = 0;
            foreach (var card in cards)
            {
                IEnumerator<GameObject> ie;
                bool diy = false;
                if (codes.Contains(card.Id))
                    ie = ABLoader.LoadFromFolderAsync("MonsterCutin/" + card.Id, "Spine" + card.Id, false, true);
                else
                {
                    ie = ABLoader.LoadFromFileAsync("MonsterCutin2/" + card.Id, false, true);
                    diy = true;
                }
                while (ie.MoveNext())
                    yield return null;
                ie.Current.SetActive(false);
                cutins.Add(ie.Current);
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
            servantUI.CG.alpha = 1f;
            servantUI.CG.blocksRaycasts = true;
            UIManager.ShowExitButton(TransitionTime);
            autoPlay = null;
        }

    }
}
