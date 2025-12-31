using Cysharp.Threading.Tasks;
using DG.Tweening;
using MDPro3.Duel.YGOSharp;
using MDPro3.Servant;
using MDPro3.Utility;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.UI;
using YgomSystem.ElementSystem;

namespace MDPro3
{
    public class TextureManager : Manager
    {
        public static TextureManager instance;
        public static TextureContainer container;

        private Material commonShopButtonMat;
        private Material commonShopButtonOverMat;

        public override void Initialize()
        {
            instance = this;
            base.Initialize();
            var handle = Addressables.LoadAssetAsync<TextureContainer>("ScriptableObjects/TextureContainer.asset");
            handle.Completed += (result) =>
            {
                container = result.Result;
            };
            _ = LoadMaterials();
        }

        public static bool loaded;
        private async UniTask LoadMaterials()
        {
            await UniTask.WaitUntil(() => container != null);

            commonShopButtonMat = await ABLoader.LoadMaterialAsync("MasterDuel/Material/GUI_CommonShopButton_N", default);
            SetCommonShopButtonMaterial(commonShopButtonMat);

            commonShopButtonOverMat = await ABLoader.LoadMaterialAsync("MasterDuel/Material/GUI_CommonShopButton_N_Over", default);
            SetCommonShopButtonMaterial(commonShopButtonOverMat);

#if UNITY_ANDROID || UNITY_STANDALONE_LINUX
            var depens = Directory.GetFiles(Program.root + "CrossDuel/Dependency", "*.bundle");
            foreach (var depen in depens)
                await ABLoader.CacheFromFileAsync(Program.root + "CrossDuel/Dependency/" + Path.GetFileName(depen));
#endif
            loaded = true;
        }

        public static async UniTask<Texture2D> LoadPicFromFileAsync(string path)
        {
            if (!File.Exists(path))
                return null;
            string fullPath;
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            fullPath = Path.Combine("file://" + Application.persistentDataPath, path);
#elif UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX
            fullPath = Path.Combine("file://" + Environment.CurrentDirectory, path);
#else
            fullPath = Environment.CurrentDirectory + Program.STRING_SLASH + path;
#endif
            using var request = UnityWebRequestTexture.GetTexture(fullPath);
            await request.SendWebRequest().WithCancellation(Application.exitCancellationToken);

            if (request.result == UnityWebRequest.Result.Success)
                return DownloadHandlerTexture.GetContent(request);
            else
            {
                Debug.LogWarningFormat("Pic File [{0}] not fount.", path);
                return null;
            }
        }

        public async UniTask LoadCardToRawImageWithoutMaterialAsync(RawImage rawImage, int code, bool cache = true)
        {
            rawImage.texture =await CardImageLoader.LoadCardAsync(code, cache, rawImage.destroyCancellationToken);
        }

        public async UniTask LoadCardToRendererWithMaterialAsync(Renderer renderer, int code, bool cache = true)
        {
            var mat = MaterialLoader.GetCardMaterial(code, true);
            mat.mainTexture = await CardImageLoader.LoadCardAsync(code, cache, renderer.GetCancellationTokenOnDestroy());
            if(renderer != null)
                renderer.material = mat;
        }

        public async UniTask LoadDummyCard(ElementObjectManager manager, int code, uint player, bool active = false, 
            Renderer attachRenderer = null, Renderer attachRenderer2 = null)
        {
            if (active)
                manager.gameObject.SetActive(false);
            manager.GetElement<Renderer>("DummyCardModel_back").material = player == 0 ? OcgCore.myProtector : OcgCore.opProtector;

            var renderer = manager.GetElement<Renderer>("DummyCardModel_front");
            renderer.material = MaterialLoader.GetCardMaterial(code, true);
            renderer.material.mainTexture = await CardImageLoader.LoadCardAsync(code, false, manager.destroyCancellationToken);
            if(attachRenderer != null)
                attachRenderer.material.mainTexture = renderer.material.mainTexture;
            if (attachRenderer2 != null)
                attachRenderer2.material.mainTexture = renderer.material.mainTexture;
            if (active)
                manager.gameObject.SetActive(true);
        }

        private void SetCommonShopButtonMaterial(Material mat)
        {
            mat.SetFloat("_NoiseSize", 500f);
            mat.SetFloat("_NoiseSpeed", 0.5f);
            mat.SetVector("_TilingOffset", new Vector4(1f, 1f, 0f, 0f));
            mat.SetVector("_MainTexMinMax", new Vector4(-0.5f, 1f, -0.5f, 1f));
        }

        public async UniTask SetCommonShopButtonMaterial(Image image, bool hover)
        {
            if (hover)
            {
                await UniTask.WaitUntil(() => commonShopButtonOverMat != null, cancellationToken : image.destroyCancellationToken);
                image.material = commonShopButtonOverMat;
            }
            else
            {
                await UniTask.WaitUntil(() => commonShopButtonMat != null, cancellationToken: image.destroyCancellationToken);
                image.material = commonShopButtonMat;
            }
        }

        #region Closeup

        static Dictionary<int, Texture2D> cachedCloseups = new Dictionary<int, Texture2D>();

        public async UniTask<Texture2D> LoadCloseupAsync(int code, MeshRenderer renderer = null)
        {
            if(renderer != null)
                renderer.gameObject.SetActive(false);
            if (cachedCloseups.TryGetValue(code, out var returenValue))
            {
                if (renderer != null)
                    ResizeCloseup(renderer, returenValue);
                return returenValue;
            }
            if (!Directory.Exists(Program.PATH_CLOSEUP))
                Directory.CreateDirectory(Program.PATH_CLOSEUP);
            var path = Program.PATH_CLOSEUP + code + Program.EXPANSION_PNG;
            if (!File.Exists(path))
                return null;

            returenValue = await LoadPicFromFileAsync(path);

            returenValue.name = "Closeup_" + code;
            if (cachedCloseups.ContainsKey(code))
            {
                Destroy(returenValue);
                returenValue = cachedCloseups[code];
            }
            else
                cachedCloseups.Add(code, returenValue);
            if (renderer != null)
                ResizeCloseup(renderer, returenValue);
            return returenValue;
        }

        void ResizeCloseup(MeshRenderer renderer, Texture2D tex)
        {
            renderer.material.mainTexture = tex;
            var aspect = (float)tex.width / tex.height;
            renderer.transform.localScale = new Vector3 (8f * aspect, 8f, 1f);
            renderer.gameObject.SetActive(true);
            DOTween.To(() => 0f, x =>
            {
                renderer.transform.localScale = new Vector3(x * aspect, x, 1f);
            }, 8f, 0.3f);
        }

        #endregion

        #region Card UI

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
            {
                if(p.controller == 0)
                    return container.locationMyField;
                else
                    return container.locationOpField;
            }
            else if ((p.location & (uint)CardLocation.Search) > 0)
                return container.locationSearch;
            else
                return container.typeNone;
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
            if (data.HasType(CardType.Counter))
                return container.typeCounter;
            else if (data.HasType(CardType.Field))
                return container.typeField;
            else if (data.HasType(CardType.Equip))
                return container.typeEquip;
            else if (data.HasType(CardType.Continuous))
                return container.typeContinuous;
            else if (data.HasType(CardType.QuickPlay))
                return container.typeQuickPlay;
            else if (data.HasType(CardType.Ritual))
                return container.typeRitual;
            else
                return container.typeNone;
        }

        public static Sprite GetCardLevelIcon(Card data)
        {
            if (data.HasType(CardType.Link))
                return container.typeLink;
            else if (data.HasType(CardType.Xyz))
                return container.typeRank;
            else
                return container.typeLevel;
        }

        public static Sprite GetCardCounterIcon(int counter)
        {
            return counter switch
            {
                0x1 => container.counterMagic,
                0x1002 => container.counterWedge,
                0x3 => container.counterBushido,
                0x4 => container.counterPsycho,
                0x5 => container.counterShine,
                0x6 => container.counterGem,
                0x8 => container.counterDeformer,
                0x1009 => container.counterVenom,
                0xA => container.counterGenex,
                0xC => container.counterThunder,
                0xD => container.counterGreed,
                0x100E => container.counterAlien,
                0xF => container.counterWorm,
                0x10 => container.counterBF,
                0x11 => container.counterHyper,
                0x12 => container.counterKarakuri,
                0x13 => container.counterChaos,
                0x1015 => container.counterIce,
                0x16 => container.counterStone,
                0x17 => container.counterDonguri,
                0x18 => container.counterFlower,
                0x1019 => container.counterFog,
                0x1A => container.counterDouble,
                0x1B => container.counterClock,
                0x1C => container.counterD,
                0x1D => container.counterJunk,
                0x1E => container.counterGate,
                0x20 => container.counterPlant,
                0x1021 => container.counterGuard2,
                0x22 => container.counterDragonic,
                0x23 => container.counterOcean,
                0x1024 => container.counterString,
                0x25 => container.counterChronicle,
                0x2B => container.counterDestiny,
                0x2C => container.counterOrbital,
                0x2E => container.counterShark,
                0x2F => container.counterPumpkin,
                0x30 => container.counterKattobing,
                0x31 => container.counterHopeSlash,
                0x32 => container.counterBalloon,
                0x33 => container.counterYosen,
                0x35 => container.counterSound,
                0x36 => container.counterEM,
                0x37 => container.counterKaiju,
                0x1038 => container.counterHoukai,
                0x1039 => container.counterZushin,
                0x1041 => container.counterPredator,
                0x43 => container.counterDefect,
                0x1045 => container.counterScales,
                0x1049 => container.counterPolice,
                0x4A => container.counterAthlete,
                0x4B => container.counterBarrel,
                0x4C => container.counterSummon,
                0x104D => container.counterSignal,
                0x104F => container.counterVenemy,
                0x56 => container.counterFireStar,
                0x57 => container.counterPhantasm,
                0x59 => container.counterOtoshidama,
                0x105C => container.counterBurn,
                0x5E => container.counterOunokagi,
                0x5F => container.counterPiece,
                0x1063 => container.counterIllusion,
                0x64 => container.counterGG,
                0x1065 => container.counterRabbit,
                0x6A => container.counterKyoumei,
                0x102A => container.counterGardna,
                _ => container.counterNormal,
            };
        }

        #endregion

        #region Public Static Functions

        public static Texture2D ResizeTexture2D(Texture2D texture, int newWidth, int newHeight)
        {
            var returnValue = new Texture2D(newWidth, newHeight);
            var resizePixels = ResizePixelsBilinear(texture.GetPixels(), texture.width, texture.height, newWidth, newHeight);
            returnValue.SetPixels(resizePixels);
            returnValue.Apply();
            Destroy(texture);
            return returnValue;
        }

        public static Color[] ResizePixelsNearest(Color[] originalPixels, int originalWidth, int originalHeight, int newWidth, int newHeight)
        {
            Color[] newPixels = new Color[newWidth * newHeight];

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    int origX = (int)((float)x / newWidth * originalWidth);
                    int origY = (int)((float)y / newHeight * originalHeight);

                    newPixels[y * newWidth + x] = originalPixels[origY * originalWidth + origX];
                }
            }

            return newPixels;
        }

        public static Color BilinearInterpolation(Color c1, Color c2, Color c3, Color c4, float u, float v)
        {
            // Perform linear interpolation in the horizontal direction.
            Color c12 = new Color(
                c1.r * (1 - u) + c2.r * u,
                c1.g * (1 - u) + c2.g * u,
                c1.b * (1 - u) + c2.b * u,
                c1.a * (1 - u) + c2.a * u);

            Color c34 = new Color(
                c3.r * (1 - u) + c4.r * u,
                c3.g * (1 - u) + c4.g * u,
                c3.b * (1 - u) + c4.b * u,
                c3.a * (1 - u) + c4.a * u);

            // Then perform linear interpolation in the vertical direction.
            return new Color(
                c12.r * (1 - v) + c34.r * v,
                c12.g * (1 - v) + c34.g * v,
                c12.b * (1 - v) + c34.b * v,
                c12.a * (1 - v) + c34.a * v);
        }

        public static Color[] ResizePixelsBilinear(Color[] originalPixels, int originalWidth, int originalHeight, int newWidth, int newHeight)
        {
            Color[] newPixels = new Color[newWidth * newHeight];

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    float origX = ((float)x / newWidth) * originalWidth;
                    float origY = ((float)y / newHeight) * originalHeight;

                    int floorX = (int)Math.Floor(origX);
                    int floorY = (int)Math.Floor(origY);
                    int ceilX = Math.Min(floorX + 1, originalWidth - 1); // Ensure not to go out of bounds
                    int ceilY = Math.Min(floorY + 1, originalHeight - 1); // Ensure not to go out of bounds

                    if (floorX == ceilX || floorY == ceilY)
                    {
                        // Avoid division by zero and handle edge cases.
                        newPixels[y * newWidth + x] = originalPixels[floorY * originalWidth + floorX];
                        continue;
                    }

                    Color c1 = originalPixels[floorY * originalWidth + floorX];
                    Color c2 = originalPixels[floorY * originalWidth + ceilX];
                    Color c3 = originalPixels[ceilY * originalWidth + floorX];
                    Color c4 = originalPixels[ceilY * originalWidth + ceilX];

                    float u = origX - floorX;
                    float v = origY - floorY;

                    newPixels[y * newWidth + x] = BilinearInterpolation(c1, c2, c3, c4, u, v);
                }
            }

            return newPixels;
        }

        public static Color BicubicInterpolation(Color c00, Color c01, Color c02, Color c03,
                                                Color c10, Color c11, Color c12, Color c13,
                                                Color c20, Color c21, Color c22, Color c23,
                                                Color c30, Color c31, Color c32, Color c33,
                                                float u, float v)
        {
            // Implement Catmull-Rom spline kernel.
            float b = -0.5f;
            float c = 1.5f;
            float d = -1.5f;
            float e = 1.0f;
            float f = -0.5f;
            float g = 0.5f;
            float h = -0.5f;

            // Construct the cubic basis matrix.
            float[] m = new float[] { b, c, d, e, f, g, h, 0.0f };
            float[] uMat = new float[] { u * u * u, u * u, u, 1.0f };
            float[] vMat = new float[] { v * v * v, v * v, v, 1.0f };

            // Interpolate horizontally.
            Color c0 = new Color(
                Clamp(uMat[0] * m[0] * c00.r + uMat[1] * m[1] * c00.r + uMat[2] * m[2] * c00.r + uMat[3] * m[3] * c00.r, 0, 1),
                Clamp(uMat[0] * m[0] * c00.g + uMat[1] * m[1] * c00.g + uMat[2] * m[2] * c00.g + uMat[3] * m[3] * c00.g, 0, 1),
                Clamp(uMat[0] * m[0] * c00.b + uMat[1] * m[1] * c00.b + uMat[2] * m[2] * c00.b + uMat[3] * m[3] * c00.b, 0, 1),
                Clamp(uMat[0] * m[0] * c00.a + uMat[1] * m[1] * c00.a + uMat[2] * m[2] * c00.a + uMat[3] * m[3] * c00.a, 0, 1));

            Color c1 = new Color(
                Clamp(uMat[0] * m[0] * c10.r + uMat[1] * m[1] * c10.r + uMat[2] * m[2] * c10.r + uMat[3] * m[3] * c10.r, 0, 1),
                Clamp(uMat[0] * m[0] * c10.g + uMat[1] * m[1] * c10.g + uMat[2] * m[2] * c10.g + uMat[3] * m[3] * c10.g, 0, 1),
                Clamp(uMat[0] * m[0] * c10.b + uMat[1] * m[1] * c10.b + uMat[2] * m[2] * c10.b + uMat[3] * m[3] * c10.b, 0, 1),
                Clamp(uMat[0] * m[0] * c10.a + uMat[1] * m[1] * c10.a + uMat[2] * m[2] * c10.a + uMat[3] * m[3] * c10.a, 0, 1));

            Color c2 = new Color(
                Clamp(uMat[0] * m[0] * c20.r + uMat[1] * m[1] * c20.r + uMat[2] * m[2] * c20.r + uMat[3] * m[3] * c20.r, 0, 1),
                Clamp(uMat[0] * m[0] * c20.g + uMat[1] * m[1] * c20.g + uMat[2] * m[2] * c20.g + uMat[3] * m[3] * c20.g, 0, 1),
                Clamp(uMat[0] * m[0] * c20.b + uMat[1] * m[1] * c20.b + uMat[2] * m[2] * c20.b + uMat[3] * m[3] * c20.b, 0, 1),
                Clamp(uMat[0] * m[0] * c20.a + uMat[1] * m[1] * c20.a + uMat[2] * m[2] * c20.a + uMat[3] * m[3] * c20.a, 0, 1));

            Color c3 = new Color(
                Clamp(uMat[0] * m[0] * c30.r + uMat[1] * m[1] * c30.r + uMat[2] * m[2] * c30.r + uMat[3] * m[3] * c30.r, 0, 1),
                Clamp(uMat[0] * m[0] * c30.g + uMat[1] * m[1] * c30.g + uMat[2] * m[2] * c30.g + uMat[3] * m[3] * c30.g, 0, 1),
                Clamp(uMat[0] * m[0] * c30.b + uMat[1] * m[1] * c30.b + uMat[2] * m[2] * c30.b + uMat[3] * m[3] * c30.b, 0, 1),
                Clamp(uMat[0] * m[0] * c30.a + uMat[1] * m[1] * c30.a + uMat[2] * m[2] * c30.a + uMat[3] * m[3] * c30.a, 0, 1));

            // Interpolate vertically.
            return new Color(
                Clamp(vMat[0] * m[0] * c0.r + vMat[1] * m[1] * c0.r + vMat[2] * m[2] * c0.r + vMat[3] * m[3] * c0.r, 0, 1),
                Clamp(vMat[0] * m[0] * c0.g + vMat[1] * m[1] * c0.g + vMat[2] * m[2] * c0.g + vMat[3] * m[3] * c0.g, 0, 1),
                Clamp(vMat[0] * m[0] * c0.b + vMat[1] * m[1] * c0.b + vMat[2] * m[2] * c0.b + vMat[3] * m[3] * c0.b, 0, 1),
                Clamp(vMat[0] * m[0] * c0.a + vMat[1] * m[1] * c0.a + vMat[2] * m[2] * c0.a + vMat[3] * m[3] * c0.a, 0, 1));
        }

        public static float Clamp(float value, float min, float max)
        {
            return value < min ? min : (value > max ? max : value);
        }

        public static Color[] ResizePixelsBicubic(Color[] originalPixels, int originalWidth, int originalHeight, int newWidth, int newHeight)
        {
            Color[] newPixels = new Color[newWidth * newHeight];

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    float origX = ((float)x / newWidth) * originalWidth;
                    float origY = ((float)y / newHeight) * originalHeight;

                    int floorX = (int)Math.Floor(origX);
                    int floorY = (int)Math.Floor(origY);
                    int ceilX = Math.Min(floorX + 3, originalWidth - 1); // Ensure not to go out of bounds
                    int ceilY = Math.Min(floorY + 3, originalHeight - 1); // Ensure not to go out of bounds

                    if (floorX >= ceilX - 1 || floorY >= ceilY - 1)
                    {
                        newPixels[y * newWidth + x] = originalPixels[floorY * originalWidth + floorX];
                        continue;
                    }

                    // Fetch the 4x4 neighborhood around the pixel.
                    Color[,] colors = new Color[4, 4];
                    for (int row = 0; row < 4; row++)
                    {
                        for (int col = 0; col < 4; col++)
                        {
                            colors[row, col] = originalPixels[(floorY + row) * originalWidth + floorX + col];
                        }
                    }

                    // Pass the 4x4 neighborhood to the bicubic interpolation function.
                    float u = origX - floorX;
                    float v = origY - floorY;

                    newPixels[y * newWidth + x] = BicubicInterpolation(colors[0, 0], colors[0, 1], colors[0, 2], colors[0, 3],
                                                                       colors[1, 0], colors[1, 1], colors[1, 2], colors[1, 3],
                                                                       colors[2, 0], colors[2, 1], colors[2, 2], colors[2, 3],
                                                                       colors[3, 0], colors[3, 1], colors[3, 2], colors[3, 3],
                                                                       u, v);
                }
            }

            return newPixels;
        }

        public static Sprite Texture2Sprite(Texture2D texture)
        {
            if (texture == null)
                return null;
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            return sprite;
        }

        public static void ReplaceTransparentPixelsWithColor(Texture2D texture, Color replacementColor)
        {
            var pixels = texture.GetPixels32();

            for(int i = 0; i < pixels.Length; i++)
                if (pixels[i].a == 0)
                    pixels[i] = replacementColor;

            texture.SetPixels32(pixels);
            texture.Apply();
        }

        /// <summary>
        /// Creates a new Texture2D with the original texture centered and allows for vertical and horizontal offsets.
        /// </summary>
        /// <param name="originalTexture">The original Texture2D.</param>
        /// <param name="newSize">The size of the new Texture2D.</param>
        /// <param name="offsetX">Horizontal offset in pixels.</param>
        /// <param name="offsetY">Vertical offset in pixels.</param>
        /// <returns>A new Texture2D with the original texture centered and offset.</returns>
        public static Texture2D CreateCenteredTexture(Texture2D originalTexture, int newSize, int offsetX, int offsetY)
        {
            if (originalTexture == null)
                throw new System.ArgumentNullException("originalTexture", "Original texture cannot be null.");

            Texture2D newTexture = new(newSize, newSize, originalTexture.format, false);

            for (int y = 0; y < newSize; y++)
                for (int x = 0; x < newSize; x++)
                    newTexture.SetPixel(x, y, Color.clear);
            newTexture.Apply();

            int centerX = newSize / 2;
            int centerY = newSize / 2;

            int startX = centerX - originalTexture.width / 2 + offsetX;
            int startY = centerY - originalTexture.height / 2 + offsetY;

            for (int y = 0; y < originalTexture.height; y++)
            {
                for (int x = 0; x < originalTexture.width; x++)
                {
                    Color pixelColor = originalTexture.GetPixel(x, y);
                    int newX = startX + x;
                    int newY = startY + y;

                    if (newX >= 0 && newX < newSize && newY >= 0 && newY < newSize)
                        newTexture.SetPixel(newX, newY, pixelColor);
                }
            }

            newTexture.Apply();

            return newTexture;
        }

        public static void ChangeProfileFrameMaterialWrapMode(Material mat)
        {
#if !UNITY_ANDROID
            return;
#endif

            if (mat == null)
                return;

            for(int i = 0; i < mat.shader.GetPropertyCount(); i++)
            {
                if(mat.shader.GetPropertyType(i) == UnityEngine.Rendering.ShaderPropertyType.Texture)
                {
                    var propName = mat.shader.GetPropertyName(i);
                    if (propName != "_ProfileFrameTex" && propName != "_MainTex")
                    {
                        var tex = mat.GetTexture(propName);
                        if (tex != null)
                            tex.wrapMode = TextureWrapMode.Repeat;
                    }
                }
            }
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

        #endregion
    }
}
