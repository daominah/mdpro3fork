using ICSharpCode.SharpZipLib.Zip;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using YgomSystem.ElementSystem;
using MDPro3.YGOSharp;
using MDPro3.YGOSharp.OCGWrapper.Enums;
using static MDPro3.EditDeck;

namespace MDPro3
{
    public class TextureManager : Manager
    {

        public static TextureContainer container;
        public static Items Items;

        static Dictionary<int, Texture2D> cachedArts = new Dictionary<int, Texture2D>();
        static Dictionary<int, Texture2D> cachedCards = new Dictionary<int, Texture2D>();
        static Dictionary<int, Texture2D> cachedMasks = new Dictionary<int, Texture2D>();
        static Dictionary<string, Sprite> cachedIcons = new Dictionary<string, Sprite>();

        int cacheMax = 200;

        public static Material cardMatNormal;
        public static Material cardMatShine;
        public static Material cardMatRoyal;
        public static Material cardMatSide;

        public override void Initialize()
        {
            base.Initialize();
            var handle = Addressables.LoadAssetAsync<TextureContainer>("TextureContainer");
            handle.Completed += (result) =>
            {
                container = result.Result;
            };
            StartCoroutine(LoadMaterials());
        }

        IEnumerator LoadMaterials()
        {
            var ie = ABLoader.LoadFromFileAsync("timeline/summon/summonsynchro/summonsynchro01", true);
            StartCoroutine(ie);
            while (ie.MoveNext())
                yield return null;
            var manager = ie.Current.GetComponent<ElementObjectManager>();
            manager.gameObject.SetActive(false);
            Destroy(manager.gameObject);
            manager = manager.GetElement<ElementObjectManager>("SummonSynchroPostSynchro");
            manager = manager.GetElement<ElementObjectManager>("DummyCardSynchro");
            cardMatNormal = Instantiate(manager.GetElement<Renderer>("DummyCardModel_front").material);
            var handle = Addressables.LoadAssetAsync<Material>("MaterialCardModelSide");
            handle.Completed += (result) =>
            {
                cardMatSide = result.Result;
            };

            ie = ABLoader.LoadFromFileAsync("timeline/summon/summonsynchro/summonsynchro01_shinestyle");
            StartCoroutine(ie);
            while (ie.MoveNext())
                yield return null;
            manager = ie.Current.GetComponent<ElementObjectManager>();
            manager.gameObject.SetActive(false);
            Destroy(manager.gameObject);
            manager = manager.GetElement<ElementObjectManager>("SummonSynchroPostSynchro");
            manager = manager.GetElement<ElementObjectManager>("DummyCardSynchro");
            cardMatShine = Instantiate(manager.GetElement<Renderer>("DummyCardModel_front").material);

            ie = ABLoader.LoadFromFileAsync("timeline/summon/summonsynchro/summonsynchro01_royalstyle");
            StartCoroutine(ie);
            while (ie.MoveNext())
                yield return null;
            manager = ie.Current.GetComponent<ElementObjectManager>();
            manager.gameObject.SetActive(false);
            Destroy(manager.gameObject);
            manager = manager.GetElement<ElementObjectManager>("SummonSynchroPostSynchro");
            manager = manager.GetElement<ElementObjectManager>("DummyCardSynchro");
            cardMatRoyal = Instantiate(manager.GetElement<Renderer>("DummyCardModel_front").material);

            cardMatNormal.SetFloat("_FakeBlend", 1);
            cardMatNormal.SetColor("_AmbientColor", new Color(0.0588f, 0.0588f, 0.0588f, 1f));
            cardMatShine.SetFloat("_FakeBlend", 1);
            cardMatRoyal.SetFloat("_FakeBlend", 1);

            cardMatShine.SetVector("_AttributeSize_Pos", new Vector4(9.82f, 13.84f, -3.7f, -5.81f));
            cardMatRoyal.SetVector("_AttributeSize_Pos", new Vector4(9.82f, 13.84f, -3.7f, -5.81f));

            while (container == null)
                yield return null;
            cardMatNormal.SetTexture("_KiraMask", container.cardKiraMask);
            cardMatShine.SetTexture("_KiraMask", container.cardKiraMask);
            cardMatRoyal.SetTexture("_KiraMask", container.cardKiraMask);
            cardMatNormal.enableInstancing = true;
            cardMatShine.enableInstancing = true;
            cardMatRoyal.enableInstancing = true;
#if UNITY_ANDROID
            var depens = Directory.GetFiles(Program.root + "CrossDuel/Dependency", "*.bundle");
            foreach (var depen in depens)
            {
                var cache = ABLoader.CacheFromFileAsync(Program.root + "CrossDuel/Dependency/" + Path.GetFileName(depen));
                StartCoroutine(cache);
                while (cache.MoveNext())
                    yield return null;
            }
#endif
        }

        public IEnumerator LoadDummyCard(ElementObjectManager manager, int code, bool active = false)
        {
            if(active)
                manager.gameObject.SetActive(false);
            var ie = LoadCardAsync(code, true);
            while (ie.MoveNext())
                yield return null;
            var mat = GetCardMaterial(code);
            manager.GetElement<Renderer>("DummyCardModel_side").material = cardMatSide;
            manager.GetElement<Renderer>("DummyCardModel_front").material = mat;
            manager.GetElement<Renderer>("DummyCardModel_front").material.mainTexture = ie.Current;
            if (active)
                manager.gameObject.SetActive(true);
        }

        public static IEnumerator<Texture2D> LoadFromFileAsync(string path)
        {
            if (!File.Exists(path))
            {
                //Debug.Log("Œ¥’“µΩÕº∆¨£∫" + path);
                yield break;
            }
            string fullPath;
#if !UNITY_EDITOR && UNITY_ANDROID
        fullPath = "file://" + Application.persistentDataPath + Program.slash + path;
#else
            fullPath = System.Environment.CurrentDirectory + Program.slash + path;
#endif
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(fullPath);
            request.SendWebRequest();
            while (request.result != UnityWebRequest.Result.Success)
                yield return null;
            yield return DownloadHandlerTexture.GetContent(request);
            request.Dispose();
        }

        bool loadingArt;
        public IEnumerator<Texture2D> LoadArtAsync(int code, bool cache = false)
        {
            if (cachedArts.TryGetValue(code, out var returenValue))
            {
                yield return returenValue;
                yield break;
            }
            //while(loadingArt)
            //    yield return null;
            //loadingArt = true;
            if (!Directory.Exists(Program.artPath))
                Directory.CreateDirectory(Program.artPath);
            if (!Directory.Exists(Program.altArtPath))
                Directory.CreateDirectory(Program.altArtPath);
            var path = Program.altArtPath + Program.slash + code;
            if (File.Exists(path + ".jpg"))
                path += ".jpg";
            else if (File.Exists(path + ".png"))
                path += ".png";
            else if (File.Exists(Program.artPath + Program.slash + code.ToString() + ".jpg"))
                path = Program.artPath + Program.slash + code.ToString() + ".jpg";
            else
            {
                foreach (var zip in ZipHelper.zips)
                {
                    if (zip.Name.ToLower().EndsWith("script.zip"))
                        continue;
                    foreach (var file in zip.EntryFileNames)
                    {
                        foreach (var extName in new[] { ".png", ".jpg" })
                        {
                            var picPath = $"art/{code}{extName}";
                            if (file.ToLower() == picPath)
                            {
                                returenValue = new Texture2D(0, 0);
                                MemoryStream stream = new MemoryStream();
                                var entry = zip[picPath];
                                entry.Extract(stream);
                                returenValue.LoadImage(stream.ToArray());
                            }
                        }
                    }
                }
                if(returenValue == null)
                {
                    foreach (var zip in ZipHelper.zips)
                    {
                        if (zip.Name.ToLower().EndsWith("script.zip"))
                            continue;
                        foreach (var file in zip.EntryFileNames)
                        {
                            foreach (var extName in new[] { ".png", ".jpg" })
                            {
                                var picPath = $"pics/{code}{extName}";
                                if (file.ToLower() == picPath)
                                {
                                    returenValue = new Texture2D(0, 0);
                                    MemoryStream stream = new MemoryStream();
                                    var entry = zip[picPath];
                                    entry.Extract(stream);
                                    returenValue.LoadImage(stream.ToArray());
                                    var card = CardsManager.Get(code);
                                    if(code >= 120000000 && code < 130000000)
                                    {
                                        if ((card.Type & (uint)CardType.Monster) > 0)
                                            returenValue = GetArtFromRushDuelMonsterCard(returenValue);
                                        else
                                            returenValue = GetArtFromRushDuelSpellCard(returenValue);
                                    }
                                    else if ((card.Type & (uint)CardType.Pendulum) > 0)
                                        returenValue = GetArtFromPendulumCard(returenValue);
                                    else
                                        returenValue = GetArtFromCard(returenValue);
                                }
                            }
                        }
                    }
                }
            }
            if (returenValue == null)
            {
                IEnumerator ie = LoadFromFileAsync(path);
                StartCoroutine(ie);
                while (ie.MoveNext())
                    yield return null;
                returenValue = ie.Current as Texture2D;
            }
            if (returenValue == null)
                yield return container.unknownArt.texture;
            else
            {
                returenValue.name = "Art_" + code;
                if (cache)
                {
                    if (cachedArts.ContainsKey(code))
                    {
                        Destroy(returenValue);
                        returenValue = cachedArts[code];
                    }
                    else
                        cachedArts.Add(code, returenValue);
                }
                yield return returenValue;
            }
        }

        int getCount;
        public static bool loadingCard;
        public IEnumerator<Texture2D> LoadCardAsync(int code, bool cache = false)
        {
            if (cachedCards.TryGetValue(code, out var returnValue))
            {
                if (returnValue != null)
                {
                    yield return returnValue;
                    yield break;
                }
                else
                    cachedCards.Remove(code);
            }
            while (container == null)
                yield return null;

            var data = CardsManager.Get(code);
            if (data.Id == 0)
            {
                yield return container.unknownCard.texture;
                yield break;
            }

            //while (loadingCard)
            //    yield return null;
            //loadingCard = true;

            IEnumerator ie = LoadArtAsync(code);
            StartCoroutine(ie);
            while (ie.MoveNext())
                yield return null;
            if (ie.Current == null)
            {
                yield return container.unknownCard.texture;
                yield break;
            }
            returnValue = ie.Current as Texture2D;

            if (cachedCards.TryGetValue(code, out var card))
            {
                yield return card;
                yield break;
            }
            else
            {
                RenderTexture.active = Program.I().cardRenderer.renderTexture;
                Program.I().cardRenderer.RenderCard(code, returnValue);
                Program.I().camera_.cameraRenderTexture.Render();
                returnValue = new Texture2D(RenderTexture.active.width, RenderTexture.active.height, TextureFormat.RGB24, false);
                returnValue.ReadPixels(new Rect(0, 0, RenderTexture.active.width, RenderTexture.active.height), 0, 0);
                returnValue.Apply();
                returnValue.name = "Card_" + code;
            }
            if (cache)
                cachedCards.Add(code, returnValue);

            getCount++;
            if (getCount > cacheMax)
            {
                getCount = 0;
                Program.I().UnloadUnusedAssets();
            }
            //loadingCard = false;
            yield return returnValue;
        }
        public static void ClearCache()
        {
            foreach (var card in cachedCards.Values)
                Destroy(card);
            cachedCards.Clear();
            foreach (var mask in cachedMasks.Values)
                Destroy(mask);
            cachedMasks.Clear();
        }
        public Texture2D GetNameMask(int code, bool cache = false)
        {
            if (cachedMasks.ContainsKey(code))
                return cachedMasks[code];
            Texture2D returnValue;
            RenderTexture.active = Program.I().cardRenderer.renderTexture;
            Program.I().cardRenderer.RenderName(code);
            Program.I().camera_.cameraRenderTexture.Render();
            returnValue = new Texture2D(RenderTexture.active.width, 203, TextureFormat.RGBA32, false);
            var rect = new Rect(0, Program.I().cardRenderer.renderTexture.height - 203, Program.I().cardRenderer.renderTexture.width, 203);
            if (SystemInfo.graphicsUVStartsAtTop)
                rect = new Rect(0, 0, Program.I().cardRenderer.renderTexture.width, 203); returnValue.ReadPixels(rect, 0, 0);
            returnValue.Apply();
            returnValue.wrapMode = TextureWrapMode.Clamp;
            if (cache)
                cachedMasks.Add(code, returnValue);
            return returnValue;
        }
        public static Material GetCardMaterial(int code, bool cache = false)
        {
            var rarity = GetRarity(code);

            Material mat = null;
            bool needChange = false;
            switch (rarity)
            {
                case CardRarity.Normal:
                    mat = Instantiate(cardMatNormal);
                    break;
                case CardRarity.Shine:
                    mat = Instantiate(cardMatShine);
                    needChange = true;
                    break;
                case CardRarity.Royal:
                    mat = Instantiate(cardMatRoyal);
                    needChange = true;
                    break;
            }
            if (needChange)
            {
                var data = CardsManager.Get(code);
                if ((data.Type & (uint)CardType.Spell) > 0)
                    mat.SetFloat("_AttributeTile", 7);
                else if ((data.Type & (uint)CardType.Trap) > 0)
                    mat.SetFloat("_AttributeTile", 8);
                else if ((data.Attribute & (uint)CardAttribute.Light) > 0)
                    mat.SetFloat("_AttributeTile", 0);
                else if ((data.Attribute & (uint)CardAttribute.Dark) > 0)
                    mat.SetFloat("_AttributeTile", 1);
                else if ((data.Attribute & (uint)CardAttribute.Water) > 0)
                    mat.SetFloat("_AttributeTile", 2);
                else if ((data.Attribute & (uint)CardAttribute.Fire) > 0)
                    mat.SetFloat("_AttributeTile", 3);
                else if ((data.Attribute & (uint)CardAttribute.Earth) > 0)
                    mat.SetFloat("_AttributeTile", 4);
                else if ((data.Attribute & (uint)CardAttribute.Wind) > 0)
                    mat.SetFloat("_AttributeTile", 5);
                else if ((data.Attribute & (uint)CardAttribute.Divine) > 0)
                    mat.SetFloat("_AttributeTile", 6);
                var mask = Program.I().texture_.GetNameMask(code, cache);
                mat.SetTexture("_MonsterNameTex", mask);
                if ((data.Type & (uint)CardType.Link) > 0)
                {
                    mat.SetTexture("_FrameMask", container.cardFrameMaskLink);
                    mat.SetTexture("_KiraMask", container.cardKiraMaskLink);
                    mat.SetTexture("_MainNormal", container.cardNormalLink);
                }
                else if ((data.Type & (uint)CardType.Pendulum) > 0)
                {
                    mat.SetTexture("_FrameMask", container.cardFrameMaskPendulum);
                    mat.SetTexture("_KiraMask", container.cardKiraMaskPendulum);
                    mat.SetTexture("_MainNormal", container.cardNormalPendulum);
                }
            }

            return mat;
        }
        public static Sprite GetCardLocationIcon(GPS p)
        {
            if ((p.location & (uint)CardLocation.Hand) > 0)
                return container.locationHand;
            else if ((p.location & (uint)CardLocation.Deck) > 0)
                return container.locationDeck;
            else if ((p.location & (uint)CardLocation.Extra) > 0)
                return container.locationExtra;
            else if ((p.location & (uint)CardLocation.Grave) > 0)
                return container.locationGrave;
            else if ((p.location & (uint)CardLocation.Removed) > 0)
                return container.locationRemoved;
            else if ((p.location & (uint)CardLocation.Overlay) > 0)
                return container.locationOverlay;
            else if ((p.location & (uint)CardLocation.Onfield) > 0)
                return container.locationField;
            else if ((p.location & (uint)CardLocation.Search) > 0)
                return container.locationSearch;
            else
                return container.typeNone;
        }
        public static Sprite GetCardAttributeIcon(int attribute)
        {
            if ((attribute & (uint)CardAttribute.Light) > 0)
                return container.attributeLight;
            else if ((attribute & (uint)CardAttribute.Dark) > 0)
                return container.attributeDark;
            else if ((attribute & (uint)CardAttribute.Water) > 0)
                return container.attributeWater;
            else if ((attribute & (uint)CardAttribute.Fire) > 0)
                return container.attributeFire;
            else if ((attribute & (uint)CardAttribute.Earth) > 0)
                return container.attributeEarth;
            else if ((attribute & (uint)CardAttribute.Wind) > 0)
                return container.attributeWind;
            else
                return container.attributeDivine;
        }
        public static Sprite GetCardRaceIcon(int race)
        {
            if ((race & (uint)CardRace.Warrior) > 0)
                return container.raceWarrior;
            else if ((race & (uint)CardRace.SpellCaster) > 0)
                return container.raceSpellCaster;
            else if ((race & (uint)CardRace.Fairy) > 0)
                return container.raceFairy;
            else if ((race & (uint)CardRace.Fiend) > 0)
                return container.raceFiend;
            else if ((race & (uint)CardRace.Zombie) > 0)
                return container.raceZombie;
            else if ((race & (uint)CardRace.Machine) > 0)
                return container.raceMachine;
            else if ((race & (uint)CardRace.Aqua) > 0)
                return container.raceAqua;
            else if ((race & (uint)CardRace.Pyro) > 0)
                return container.racePyro;
            else if ((race & (uint)CardRace.Rock) > 0)
                return container.raceRock;
            else if ((race & (uint)CardRace.WindBeast) > 0)
                return container.raceWindBeast;
            else if ((race & (uint)CardRace.Plant) > 0)
                return container.racePlant;
            else if ((race & (uint)CardRace.Insect) > 0)
                return container.raceInsect;
            else if ((race & (uint)CardRace.Thunder) > 0)
                return container.raceThunder;
            else if ((race & (uint)CardRace.Dragon) > 0)
                return container.raceDragon;
            else if ((race & (uint)CardRace.Beast) > 0)
                return container.raceBeast;
            else if ((race & (uint)CardRace.BeastWarrior) > 0)
                return container.raceBeastWarrior;
            else if ((race & (uint)CardRace.Dinosaur) > 0)
                return container.raceDinosaur;
            else if ((race & (uint)CardRace.Fish) > 0)
                return container.raceFish;
            else if ((race & (uint)CardRace.SeaSerpent) > 0)
                return container.raceSeaSerpent;
            else if ((race & (uint)CardRace.Reptile) > 0)
                return container.raceReptile;
            else if ((race & (uint)CardRace.Psycho) > 0)
                return container.racePsycho;
            else if ((race & (uint)CardRace.DivineBeast) > 0)
                return container.raceDivineBeast;
            else if ((race & (uint)CardRace.CreatorGod) > 0)
                return container.raceCreatorGod;
            else if ((race & (uint)CardRace.Wyrm) > 0)
                return container.raceWyrm;
            else if ((race & (uint)CardRace.Cyberse) > 0)
                return container.raceCyberse;
            else if ((race & (uint)CardRace.Illustion) > 0)
                return container.raceIllustion;
            else
                return container.typeNone;
        }
        public static Sprite GetSpellTrapTypeIcon(Card data)
        {
            if ((data.Type & (uint)CardType.Counter) > 0)
                return container.typeCounter;
            else if ((data.Type & (uint)CardType.Field) > 0)
                return container.typeField;
            else if ((data.Type & (uint)CardType.Equip) > 0)
                return container.typeEquip;
            else if ((data.Type & (uint)CardType.Continuous) > 0)
                return container.typeContinuous;
            else if ((data.Type & (uint)CardType.QuickPlay) > 0)
                return container.typeQuickPlay;
            else if ((data.Type & (uint)CardType.Ritual) > 0)
                return container.typeRitual;
            else
                return container.typeNone;
        }
        public static Sprite GetCardLevelIcon(Card data)
        {
            if ((data.Type & (uint)CardType.Link) > 0)
                return container.typeLink;
            else if ((data.Type & (uint)CardType.Xyz) > 0)
                return container.typeRank;
            else
                return container.typeLevel;
        }

        public static Sprite GetCardCounterIcon(int counter)
        {
            switch (counter)
            {
                case 0x1:
                    return container.counterMagic;
                case 0x1002:
                    return container.counterWedge;
                case 0x3:
                    return container.counterBushido;
                case 0x4:
                    return container.counterPsycho;
                case 0x5:
                    return container.counterShine;
                case 0x6:
                    return container.counterGem;
                case 0x8:
                    return container.counterDeformer;
                case 0x1009:
                    return container.counterVenom;
                case 0xA:
                    return container.counterGenex;
                case 0xC:
                    return container.counterThunder;
                case 0xD:
                    return container.counterGreed;
                case 0x100E:
                    return container.counterAlien;
                case 0xF:
                    return container.counterWorm;
                case 0x10:
                    return container.counterBF;
                case 0x11:
                    return container.counterHyper;
                case 0x12:
                    return container.counterKarakuri;
                case 0x13:
                    return container.counterChaos;
                case 0x1015:
                    return container.counterIce;
                case 0x16:
                    return container.counterStone;
                case 0x17:
                    return container.counterDonguri;
                case 0x18:
                    return container.counterFlower;
                case 0x1019:
                    return container.counterFog;
                case 0x1A:
                    return container.counterDouble;
                case 0x1B:
                    return container.counterClock;
                case 0x1C:
                    return container.counterD;
                case 0x1D:
                    return container.counterJunk;
                case 0x1E:
                    return container.counterGate;
                case 0x20:
                    return container.counterPlant;
                case 0x1021:
                    return container.counterGuard2;
                case 0x22:
                    return container.counterDragonic;
                case 0x23:
                    return container.counterOcean;
                case 0x1024:
                    return container.counterString;
                case 0x25:
                    return container.counterChronicle;
                case 0x2B:
                    return container.counterDestiny;
                case 0x2C:
                    return container.counterOrbital;
                case 0x2E:
                    return container.counterShark;
                case 0x2F:
                    return container.counterPumpkin;
                case 0x30:
                    return container.counterKattobing;
                case 0x31:
                    return container.counterHopeSlash;
                case 0x32:
                    return container.counterBalloon;
                case 0x33:
                    return container.counterYosen;
                case 0x35:
                    return container.counterSound;
                case 0x36:
                    return container.counterEM;
                case 0x37:
                    return container.counterKaiju;
                case 0x1038:
                    return container.counterHoukai;
                case 0x1039:
                    return container.counterZushin;
                case 0x1041:
                    return container.counterPredator;
                case 0x43:
                    return container.counterDefect;
                case 0x1045:
                    return container.counterScales;
                case 0x1049:
                    return container.counterPolice;
                case 0x4A:
                    return container.counterAthlete;
                case 0x4B:
                    return container.counterBarrel;
                case 0x4C:
                    return container.counterSummon;
                case 0x104D:
                    return container.counterSignal;
                case 0x104F:
                    return container.counterVenemy;
                case 0x56:
                    return container.counterFireStar;
                case 0x57:
                    return container.counterPhantasm;
                case 0x59:
                    return container.counterOtoshidama;
                case 0x105C:
                    return container.counterBurn;
                case 0x5E:
                    return container.counterOunokagi;
                case 0x5F:
                    return container.counterPiece;
                case 0x1063:
                    return container.counterIllusion;
                case 0x64:
                    return container.counterGG;
                case 0x1065:
                    return container.counterRabbit;

                case 0x6A:
                    return container.counterKyoumei;

                case 0x102A:
                    return container.counterGardna;

                default:
                    return container.counterNormal;
            }
        }

        public static IEnumerator<Sprite> LoadItemIcon(string id)
        {
            if (cachedIcons.ContainsKey(id))
            {
                yield return cachedIcons[id];
                yield break;
            }
            var handle = Addressables.LoadAssetAsync<Sprite>(id);
            while (!handle.IsDone)
                yield return null;
            Sprite returnValue;
            if (cachedIcons.ContainsKey(id))
            {
                returnValue = cachedIcons[id];
                Addressables.Release(handle);
            }
            else
            {
                returnValue = handle.Result;
                cachedIcons.Add(id, handle.Result);
            }
            yield return returnValue;
        }

        public static Texture2D GetArtFromCard(Texture2D cardPic)
        {
            var startX = Mathf.CeilToInt(cardPic.width * 0.13f);
            var startY = Mathf.CeilToInt(cardPic.height * 0.3f);
            var width = Mathf.CeilToInt(cardPic.width * 0.87f);
            var height = Mathf.CeilToInt(cardPic.height * 0.81f);
            return GetCroppingTex(cardPic, startX, startY, width, height);
        }
        public static Texture2D GetArtFromPendulumCard(Texture2D cardPic)
        {
            var startX = Mathf.CeilToInt(cardPic.width * 0.067f);
            var startY = Mathf.CeilToInt(cardPic.height * 0.38f);
            var width = Mathf.CeilToInt(cardPic.width * 0.933f);
            var height = Mathf.CeilToInt(cardPic.height * 0.81f);
            return GetCroppingTex(cardPic, startX, startY, width, height);
        }
        public static Texture2D GetArtFromRushDuelMonsterCard(Texture2D cardPic)
        {
            var startX = Mathf.CeilToInt(cardPic.width * 0.067f);
            var startY = Mathf.CeilToInt(cardPic.height * 0.29f);
            var width = Mathf.CeilToInt(cardPic.width * 0.933f);
            var height = Mathf.CeilToInt(cardPic.height * 0.90f);
            return GetCroppingTex(cardPic, startX, startY, width, height);
        }
        public static Texture2D GetArtFromRushDuelSpellCard(Texture2D cardPic)
        {
            var startX = Mathf.CeilToInt(cardPic.width * 0.067f);
            var startY = Mathf.CeilToInt(cardPic.height * 0.29f);
            var width = Mathf.CeilToInt(cardPic.width * 0.933f);
            var height = Mathf.CeilToInt(cardPic.height * 0.90f);
            return GetCroppingTex(cardPic, startX, startY, width, height);
        }

        public static Texture2D GetCroppingTex(Texture2D texture, int startX, int startY, int width, int height)
        {
            var returnValue = new Texture2D(width - startX, height - startY);
            var pix = new Color[returnValue.width * returnValue.height];
            var index = 0;
            for (var y = startY; y < height; y++)
                for (var x = startX; x < width; x++)
                    pix[index++] = texture.GetPixel(x, y);
            returnValue.SetPixels(pix);
            returnValue.Apply();
            return returnValue;
        }

        public static Sprite GetChainNumSprite(int num)
        {
            switch (num)
            {
                case 0:
                    return container.chainNumSet0;
                case 1:
                    return container.chainNumSet1;
                case 2:
                    return container.chainNumSet2;
                case 3:
                    return container.chainNumSet3;
                case 4:
                    return container.chainNumSet4;
                case 5:
                    return container.chainNumSet5;
                case 6:
                    return container.chainNumSet6;
                case 7:
                    return container.chainNumSet7;
                case 8:
                    return container.chainNumSet8;
                case 9:
                    return container.chainNumSet9;
                default:
                    return container.chainNumSet0;
            }
        }
    }
}
