using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using MDPro3.Utility;
using MDPro3.Duel.YGOSharp;
using UnityEngine;
using UnityEngine.AddressableAssets;
using YgomSystem.ElementSystem;

namespace MDPro3
{
    public class MaterialLoader : MonoBehaviour
    {
        private static readonly ConcurrentDictionary<string, Material> _loadedMaterials = new();
        private static readonly ConcurrentDictionary<string, IEnumerator<Material>> _loadingCoroutines = new();

        private static MaterialLoader instance;

        private static Material cardMatNormal;
        private static Material cardMatShine;
        private static Material cardMatShineRD;
        private static Material cardMatRoyal;
        private static Material cardMatRoyalRD;
        private static Material cardMatGold;
        private static Material cardMatGoldRD;
        private static Material cardMatMillennium;
        private static Material cardMatMillenniumRD;
        public static Material cardMatSide;

        private const string CARD_MAT_PATH = "SummonSynchroPostSynchro/DummyCardSynchro/DummyCardModel_front";

        private void Awake()
        {
            instance = this;
            StartCoroutine(LoadMaterials());
        }

        private IEnumerator LoadMaterials()
        {
            while(!TextureManager.loaded)
                yield return null;
            while (TextureManager.container == null)
                yield return null;

            var handle = Addressables.LoadAssetAsync<Material>("MaterialCardModelSide");
            handle.Completed += (result) => { cardMatSide = result.Result; };

            var ie = ABLoader.LoadFromFileAsync
                ("MasterDuel/Timeline/Summon/SummonSynchro/SummonSynchro01");
            while (ie.MoveNext()) yield return null;
            ie.Current.SetActive(false);
            Destroy(ie.Current);
            var manager = ie.Current.GetComponent<ElementObjectManager>();
            cardMatNormal = Instantiate(manager.GetNestedElement<Renderer>
                (CARD_MAT_PATH).material);

            ie = ABLoader.LoadFromFileAsync
                ("MasterDuel/Timeline/Summon/SummonSynchro/SummonSynchro01_ShineStyle");
            while (ie.MoveNext()) yield return null;
            ie.Current.SetActive(false);
            Destroy(ie.Current);
            manager = ie.Current.GetComponent<ElementObjectManager>();
            cardMatShine = Instantiate(manager.GetNestedElement<Renderer>
                (CARD_MAT_PATH).material);

            ie = ABLoader.LoadFromFileAsync
                ("MasterDuel/Timeline/Summon/SummonSynchro/SummonSynchro01_RoyalStyle");
            while (ie.MoveNext()) yield return null;
            ie.Current.SetActive(false);
            Destroy(ie.Current);
            manager = ie.Current.GetComponent<ElementObjectManager>();
            cardMatRoyal = Instantiate(manager.GetNestedElement<Renderer>
                (CARD_MAT_PATH).material);

            cardMatNormal.SetFloat("_FakeBlend", 1);
            cardMatNormal.SetColor("_AmbientColor", new Color(0.0588f, 0.0588f, 0.0588f, 1f));
            cardMatShine.SetFloat("_FakeBlend", 1);
            cardMatRoyal.SetFloat("_FakeBlend", 1);
            cardMatShine.SetVector("_AttributeSize_Pos", new Vector4(9.82f, 13.84f, -3.7f, -5.81f));
            cardMatRoyal.SetVector("_AttributeSize_Pos", new Vector4(9.82f, 13.84f, -3.7f, -5.81f));
            cardMatShine.SetTexture("_KiraMask", TextureManager.container.cardKiraMask);
            cardMatRoyal.SetTexture("_KiraMask", TextureManager.container.cardKiraMask);
            var tempTex = cardMatRoyal.GetTexture("_Texture2DAsset_90c6e35ef4304f289c279037152a03b7_Out_0");
            cardMatNormal.SetTexture("_Texture2DAsset_90c6e35ef4304f289c279037152a03b7_Out_0", tempTex);
            tempTex = cardMatRoyal.GetTexture("_HighlightNormal");
            cardMatRoyal.SetTexture("_Texture2DAsset_3e204bf62e854283be7482d92655b24f_Out_0", tempTex);
            cardMatNormal.enableInstancing = true;
            cardMatShine.enableInstancing = true;
            cardMatRoyal.enableInstancing = true;

            cardMatGold = Instantiate(cardMatRoyal);
            cardMatGold.SetFloat("_CardDistortion01", 1.2f);
            cardMatGold.SetFloat("_Kira01_01Tile", 0.25f);
            cardMatGold.SetFloat("_Kira01_01Power", 3f);
            cardMatGold.SetColor("_KiraColor02", new Color(0.5f, 0.5f, 0f, 0f));
            cardMatGold.SetColor("_CubemapColor", new Color(0.7f, 0.7f, 0f, 0f));

            cardMatMillennium = Instantiate(cardMatRoyal);
            cardMatMillennium.SetTexture("_HighlightNormal"
                , TextureManager.container.CardKiraNormal03_Millennium);
            cardMatMillennium.SetTexture("_Texture2DAsset_3e204bf62e854283be7482d92655b24f_Out_0"
                , TextureManager.container.CardKiraNormal03_Millennium);
            cardMatMillennium.SetColor("_CubemapColor", new Color(0.898f, 0.3245f, 0.7723f, 0f));
            cardMatMillennium.SetColor("_KiraColor02", new Color(0.3099f, 0.1633f, 0.2753f, 0f));
            cardMatMillennium.SetFloat("_Kira01_01Tile", 0.25f);
            cardMatMillennium.SetFloat("_Kira01_02Tile", 0f);
            cardMatMillennium.SetFloat("_RanbowPower", 0.5f);

            cardMatShineRD = Instantiate(cardMatShine);
            MaterialToRD(cardMatShineRD);
            cardMatRoyalRD = Instantiate(cardMatRoyal);
            MaterialToRD(cardMatRoyalRD);
            cardMatGoldRD = Instantiate(cardMatGold);
            MaterialToRD(cardMatGoldRD);
            cardMatMillenniumRD = Instantiate(cardMatMillennium);
            MaterialToRD(cardMatMillenniumRD);
        }

        private void MaterialToRD(Material material)
        {
            material.SetTexture("_FrameMask", TextureManager.container.rd_Mask);
            material.SetTexture("_KiraMask", TextureManager.container.rd_KiraMask);
            material.SetTexture("_MainNormal", TextureManager.container.rd_CardNormal);
            material.SetTexture("_AttributeTex", TextureManager.container.rd_CardAttributeSet);
            material.SetVector("_AttributeSize_Pos", new Vector4(8.31f, 12.26f, -3.19f, -5.13f));
        }

        private IEnumerator<Material> LoadMaterialCoroutine(string materialName)
        {
            var loadOperation = ABLoader.LoadMaterialAsync("MasterDuel/Material/" + materialName);
            while (loadOperation.MoveNext())
                yield return null;

            _loadedMaterials.TryAdd(materialName, loadOperation.Current);
            _loadingCoroutines.TryRemove(materialName, out _);
            yield return loadOperation.Current;
        }

        private static Color GetMillenniumFrameColor(Card data)
        {
            var color = new Color(0.3099f, 0.1633f, 0.2753f, 0f);
            if (data.HasType(CardType.Pendulum))
                color = new Color(0.3099f, 0.1633f, 0.2753f, 0f);
            else if (data.HasType(CardType.Spell))
                color = new Color(0f, 0.8867f, 1f, 0f);
            else if (data.HasType(CardType.Trap))
                color = new Color(1f, 0f, 1f, 0f);
            else if (data.HasType(CardType.Normal))
                color = new Color(1f, 0.6f, 0f, 0f);
            else if (data.HasType(CardType.Fusion))
                color = new Color(1f, 0f, 1f, 0f);
            else if (data.HasType(CardType.Ritual))
                color = new Color(0f, 0.2f, 1f, 0f);
            else if (data.HasType(CardType.Synchro))
                color = new Color(0.4f, 0.4f, 0.4f, 0f);
            else if (data.HasType(CardType.Xyz))
                color = new Color(0.1f, 0.1f, 0.1f, 0f);
            else if (data.HasType(CardType.Link))
                color = new Color(0f, 0.4f, 1f, 0f);
            else
                color = new Color(1f, 0.2357f, 0f, 0f);
            return color;
        }

        private static Color GetMillenniumNameColor(Card data)
        {
            if (data.HasType(CardType.Spell))
                return new Color(0f, 1f, 1f, 1f);
            else if (data.HasType(CardType.Trap))
                return new Color(1f, 0f, 0.5f, 1f);
            else if ((data.Attribute & (uint)CardAttribute.Light) > 0)
                return new Color(1f, 1f, 0f, 1f);
            else if ((data.Attribute & (uint)CardAttribute.Divine) > 0)
                return new Color(1f, 1f, 0f, 1f);
            else if ((data.Attribute & (uint)CardAttribute.Dark) > 0)
                return new Color(1f, 0f, 1f, 1f);
            else if ((data.Attribute & (uint)CardAttribute.Water) > 0)
                return new Color(0f, 1f, 1f, 1f);
            else if ((data.Attribute & (uint)CardAttribute.Fire) > 0)
                return new Color(1f, 0f, 0f, 1f);
            else if ((data.Attribute & (uint)CardAttribute.Earth) > 0)
                return new Color(0.2f, 0.2f, 0.2f, 1f);
            else if ((data.Attribute & (uint)CardAttribute.Wind) > 0)
                return new Color(0f, 1f, 0f, 1f);
            else
                return new Color(1f, 1f, 0f, 1f);
        }

        public static IEnumerator<Material> LoadMaterialByNameAsync(string materialName)
        {
            if (_loadedMaterials.TryGetValue(materialName, out var material))
            {
                yield return material;
                yield break;
            }

            if (_loadingCoroutines.TryGetValue(materialName, out var loading))
            {
                while (loading.MoveNext())
                    yield return null;
                yield return loading.Current;
            }
            else
            {
                var coroutine = instance.LoadMaterialCoroutine(materialName);
                if(_loadingCoroutines.TryAdd(materialName, coroutine))
                {
                    while (coroutine.MoveNext())
                        yield return null;
                    yield return coroutine.Current;
                }
                else
                {
                    instance.StopCoroutine(coroutine);
                    while (_loadingCoroutines[materialName].MoveNext())
                        yield return null;
                    yield return _loadingCoroutines[materialName].Current;
                }
            }
        }

        public static async Task<Material> LoadCardMaterialAsync(int code)
        {
            if(code < 0)
                return Instantiate(cardMatNormal);

            bool rushDuel = CardRenderer.NeedRushDuelStyle(code);
            var rarity = CardRarity.GetRarity(code);

            Material mat = null;
            bool needSet = true;
            switch (rarity)
            {
                case CardRarity.Rarity.Normal:
                    mat = Instantiate(cardMatNormal);
                    needSet = false;
                    break;
                case CardRarity.Rarity.Shine:
                    mat = Instantiate(rushDuel ? cardMatShineRD : cardMatShine);
                    break;
                case CardRarity.Rarity.Royal:
                    mat = Instantiate(rushDuel ? cardMatRoyalRD : cardMatRoyal);
                    break;
                case CardRarity.Rarity.Gold:
                    mat = Instantiate(rushDuel ? cardMatGoldRD : cardMatGold);
                    break;
                case CardRarity.Rarity.Millennium:
                    mat = Instantiate(rushDuel ? cardMatMillenniumRD : cardMatMillennium);
                    break;
            }

            if (needSet)
            {
                var data = CardsManager.Get(code);
                if (data.HasType(CardType.Spell))
                    mat.SetFloat("_AttributeTile", 7);
                else if (data.HasType(CardType.Trap))
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

                var nameTask = CardImageLoader.LoadCardNameAsync(code);
                await TaskUtility.WaitUntil(() => nameTask.IsCompleted);
                mat.SetTexture("_MonsterNameTex", nameTask.Result);

                if (rushDuel)
                {
                    if (data.HasType(CardType.Pendulum))
                        mat.SetTexture("_KiraMask", TextureManager.container.rd_KiraMaskPendulum);
                }
                else
                {
                    if (data.HasType(CardType.Link))
                    {
                        mat.SetTexture("_FrameMask", TextureManager.container.cardFrameMaskLink);
                        mat.SetTexture("_KiraMask", TextureManager.container.cardKiraMaskLink);
                        mat.SetTexture("_MainNormal", TextureManager.container.cardNormalLink);
                        if (rarity == CardRarity.Rarity.Shine)
                            mat.SetFloat("_LinkOn_Off", 1f);
                    }
                    else if (data.HasType(CardType.Pendulum))
                    {
                        mat.SetTexture("_FrameMask", TextureManager.container.cardFrameMaskPendulum);
                        mat.SetTexture("_KiraMask", TextureManager.container.cardKiraMaskPendulum);
                        mat.SetTexture("_MainNormal", TextureManager.container.cardNormalPendulum);
                    }
                }

                if (rarity == CardRarity.Rarity.Millennium)
                {
                    mat.SetColor("_KiraColor02", GetMillenniumFrameColor(data));
                    mat.SetColor("_CubemapColor", GetMillenniumNameColor(data));
                }
            }

            return mat;
        }

    }
}
