using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using MDPro3.Utility;
using MDPro3.Duel.YGOSharp;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MDPro3
{
    public class MaterialLoader : MonoBehaviour
    {

        private static MaterialLoader instance;
        private void Awake()
        {
            instance = this;
            StartCoroutine(LoadCardMaterials());
        }

        #region Card Materials

        private static Material cardMatNormalUI;
        private static Material cardMatShineUI;
        private static Material cardMatShineRDUI;
        private static Material cardMatRoyalUI;
        private static Material cardMatRoyalRDUI;
        private static Material cardMatGoldUI;
        private static Material cardMatGoldRDUI;
        private static Material cardMatMillenniumUI;
        private static Material cardMatMillenniumRDUI;

        private static Material cardMatNormal3D;
        private static Material cardMatShine3D;
        private static Material cardMatShineRD3D;
        private static Material cardMatRoyal3D;
        private static Material cardMatRoyalRD3D;
        private static Material cardMatGold3D;
        private static Material cardMatGoldRD3D;
        private static Material cardMatMillennium3D;
        private static Material cardMatMillenniumRD3D;

        public static Material cardMatSide;

        private const string cardMatMaskName = "_Texture2DAsset_90c6e35ef4304f289c279037152a03b7_Out_0_Texture2D";

        private IEnumerator LoadCardMaterials()
        {
            while (!TextureManager.loaded)
                yield return null;
            while (TextureManager.container == null)
                yield return null;

            var handle = Addressables.LoadAssetAsync<Material>("MaterialCardModelSide");
            handle.Completed += (result) => { cardMatSide = result.Result; };

            var matLoad = Addressables.LoadAssetAsync<Material>("NormalStyleUI");
            while (!matLoad.IsDone)
                yield return null;
            cardMatNormalUI = matLoad.Result;
            var shaderLoad = LoadShaderByNameAsync("Shader Graphs_NormalStyleUI");
            while (shaderLoad.MoveNext())
                yield return null;
            cardMatNormalUI.shader = shaderLoad.Current;

            matLoad = Addressables.LoadAssetAsync<Material>("ShineStyleUI");
            while (!matLoad.IsDone)
                yield return null;
            cardMatShineUI = matLoad.Result;
            shaderLoad = LoadShaderByNameAsync("Shader Graphs_ShineStyleUI");
            while (shaderLoad.MoveNext())
                yield return null;
            cardMatShineUI.shader = shaderLoad.Current;

            matLoad = Addressables.LoadAssetAsync<Material>("RoyalStyleUI");
            while (!matLoad.IsDone)
                yield return null;
            cardMatRoyalUI = matLoad.Result;
            shaderLoad = LoadShaderByNameAsync("Shader Graphs_RoyalStyleUI");
            while (shaderLoad.MoveNext())
                yield return null;
            cardMatRoyalUI.shader = shaderLoad.Current;

            cardMatGoldUI = Instantiate(cardMatRoyalUI);
            cardMatGoldUI.SetFloat("_CardDistortion01", 1.2f);
            cardMatGoldUI.SetFloat("_Kira01_01Tile", 0.25f);
            cardMatGoldUI.SetFloat("_Kira01_01Power", 3f);
            cardMatGoldUI.SetColor("_KiraColor02", new Color(0.5f, 0.5f, 0f, 0f));
            cardMatGoldUI.SetColor("_CubemapColor", new Color(0.7f, 0.7f, 0f, 0f));

            cardMatMillenniumUI = Instantiate(cardMatRoyalUI);
            cardMatMillenniumUI.SetTexture("_HighlightNormal"
                , TextureManager.container.CardKiraNormal03_Millennium);
            cardMatMillenniumUI.SetColor("_CubemapColor", new Color(0.898f, 0.3245f, 0.7723f, 0f));
            cardMatMillenniumUI.SetColor("_KiraColor02", new Color(0.3099f, 0.1633f, 0.2753f, 0f));
            cardMatMillenniumUI.SetFloat("_Kira01_01Tile", 0.25f);
            cardMatMillenniumUI.SetFloat("_Kira01_02Tile", 0f);
            cardMatMillenniumUI.SetFloat("_RanbowPower", 0.5f);

            cardMatShineRDUI = Instantiate(cardMatShineUI);
            MaterialToRD(cardMatShineRDUI);
            cardMatRoyalRDUI = Instantiate(cardMatRoyalUI);
            MaterialToRD(cardMatRoyalRDUI);
            cardMatGoldRDUI = Instantiate(cardMatGoldUI);
            MaterialToRD(cardMatGoldRDUI);
            cardMatMillenniumRDUI = Instantiate(cardMatMillenniumUI);
            MaterialToRD(cardMatMillenniumRDUI);

            matLoad = Addressables.LoadAssetAsync<Material>("NormalStyle3D");
            while (!matLoad.IsDone)
                yield return null;
            cardMatNormal3D = matLoad.Result;
            shaderLoad = LoadShaderByNameAsync("Shader Graphs_NormalStyle3D");
            while (shaderLoad.MoveNext())
                yield return null;
            cardMatNormal3D.shader = shaderLoad.Current;

            matLoad = Addressables.LoadAssetAsync<Material>("ShineStyle3D");
            while (!matLoad.IsDone)
                yield return null;
            cardMatShine3D = matLoad.Result;
            shaderLoad = LoadShaderByNameAsync("Shader Graphs_ShineStyle3D");
            while (shaderLoad.MoveNext())
                yield return null;
            cardMatShine3D.shader = shaderLoad.Current;

            matLoad = Addressables.LoadAssetAsync<Material>("RoyalStyle3D");
            while (!matLoad.IsDone)
                yield return null;
            cardMatRoyal3D = matLoad.Result;
            shaderLoad = LoadShaderByNameAsync("Shader Graphs_RoyalStyle3D");
            while (shaderLoad.MoveNext())
                yield return null;
            cardMatRoyal3D.shader = shaderLoad.Current;

            cardMatGold3D = Instantiate(cardMatRoyal3D);
            cardMatGold3D.SetFloat("_CardDistortion01", 1.2f);
            cardMatGold3D.SetFloat("_Kira01_01Tile", 0.25f);
            cardMatGold3D.SetFloat("_Kira01_01Power", 3f);
            cardMatGold3D.SetColor("_KiraColor02", new Color(0.5f, 0.5f, 0f, 0f));
            cardMatGold3D.SetColor("_CubemapColor", new Color(0.7f, 0.7f, 0f, 0f));

            cardMatMillennium3D = Instantiate(cardMatRoyal3D);
            cardMatMillennium3D.SetTexture("_HighlightNormal"
                , TextureManager.container.CardKiraNormal03_Millennium);
            cardMatMillennium3D.SetColor("_CubemapColor", new Color(0.898f, 0.3245f, 0.7723f, 0f));
            cardMatMillennium3D.SetColor("_KiraColor02", new Color(0.3099f, 0.1633f, 0.2753f, 0f));
            cardMatMillennium3D.SetFloat("_Kira01_01Tile", 0.25f);
            cardMatMillennium3D.SetFloat("_Kira01_02Tile", 0f);
            cardMatMillennium3D.SetFloat("_RanbowPower", 0.5f);

            cardMatShineRD3D = Instantiate(cardMatShine3D);
            MaterialToRD(cardMatShineRD3D);
            cardMatRoyalRD3D = Instantiate(cardMatRoyal3D);
            MaterialToRD(cardMatRoyalRD3D);
            cardMatGoldRD3D = Instantiate(cardMatGold3D);
            MaterialToRD(cardMatGoldRD3D);
            cardMatMillenniumRD3D = Instantiate(cardMatMillennium3D);
            MaterialToRD(cardMatMillenniumRD3D);

        }

        private void MaterialToRD(Material material)
        {
            material.SetTexture("_FrameMask", TextureManager.container.rd_Mask);
            material.SetTexture("_KiraMask", TextureManager.container.rd_KiraMask);
            material.SetTexture("_MainNormal", TextureManager.container.rd_CardNormal);
            material.SetTexture("_AttributeTex", TextureManager.container.rd_CardAttributeSet);
            material.SetVector("_AttributeSize_Pos", new Vector4(8.31f, 12.26f, -3.19f, -5.13f));
        }

        private static Color GetMillenniumFrameColor(Card data)
        {
            Color color;
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
                return new Color(0.8f, 0.8f, 0.8f, 1f);
            else if ((data.Attribute & (uint)CardAttribute.Wind) > 0)
                return new Color(0f, 1f, 0f, 1f);
            else
                return new Color(1f, 1f, 0f, 1f);
        }

        public static async Task<Material> LoadCardMaterialAsync(int code, bool use3D = false)
        {
            Material mat = null;

            if (code < 0)
            {
                mat = Instantiate(use3D ? cardMatNormal3D : cardMatNormalUI);
                mat.SetTexture(cardMatMaskName, TextureManager.container.CardMask001);
                return mat;
            }

            bool rushDuel = CardRenderer.NeedRushDuelStyle(code);
            var rarity = CardRarity.GetRarity(code);

            bool needSet = true;
            switch (rarity)
            {
                case CardRarity.Rarity.Normal:
                    mat = Instantiate(use3D ? cardMatNormal3D : cardMatNormalUI);
                    needSet = false;
                    break;
                case CardRarity.Rarity.Shine:
                    mat = Instantiate(rushDuel ? use3D ? cardMatShineRD3D : cardMatShineRDUI : use3D ? cardMatShine3D : cardMatShineUI);
                    break;
                case CardRarity.Rarity.Royal:
                    mat = Instantiate(rushDuel ? use3D ? cardMatRoyalRD3D : cardMatRoyalRDUI : use3D ? cardMatRoyal3D : cardMatRoyalUI);
                    break;
                case CardRarity.Rarity.Gold:
                    mat = Instantiate(rushDuel ? use3D ? cardMatGoldRD3D : cardMatGoldRDUI : use3D ? cardMatGold3D : cardMatGoldUI);
                    break;
                case CardRarity.Rarity.Millennium:
                    mat = Instantiate(rushDuel ? use3D ? cardMatMillenniumRD3D : cardMatMillenniumRDUI : use3D ? cardMatMillennium3D : cardMatMillenniumUI);
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

            mat.SetTexture(cardMatMaskName, TextureManager.container.CardMask001);

            return mat;
        }

        #endregion

        #region Load Material

        private static readonly ConcurrentDictionary<string, Material> _loadedMaterials = new();
        private static readonly ConcurrentDictionary<string, IEnumerator<Material>> _loadMaterialCoroutines = new();

        private IEnumerator<Material> LoadMaterialCoroutine(string materialName)
        {
            var loadOperation = ABLoader.LoadMaterialAsync("MasterDuel/Material/" + materialName);
            while (loadOperation.MoveNext())
                yield return null;

            _loadedMaterials.TryAdd(materialName, loadOperation.Current);
            _loadMaterialCoroutines.TryRemove(materialName, out _);
            yield return loadOperation.Current;
        }

        public static IEnumerator<Material> LoadMaterialByNameAsync(string materialName)
        {
            if (_loadedMaterials.TryGetValue(materialName, out var material))
            {
                yield return material;
                yield break;
            }

            if (_loadMaterialCoroutines.TryGetValue(materialName, out var loading))
            {
                while (loading.MoveNext())
                    yield return null;
                yield return loading.Current;
            }
            else
            {
                var coroutine = instance.LoadMaterialCoroutine(materialName);
                if (_loadMaterialCoroutines.TryAdd(materialName, coroutine))
                {
                    while (coroutine.MoveNext())
                        yield return null;
                    yield return coroutine.Current;
                }
                else
                {
                    instance.StopCoroutine(coroutine);
                    while (_loadMaterialCoroutines[materialName].MoveNext())
                        yield return null;
                    yield return _loadMaterialCoroutines[materialName].Current;
                }
            }
        }

        #endregion

        #region Load Shader

        private static readonly ConcurrentDictionary<string, Shader> _loadedShaders = new();
        private static readonly ConcurrentDictionary<string, IEnumerator<Shader>> _loadShaderCoroutines = new();

        private IEnumerator<Shader> LoadShaderCoroutine(string shaderName)
        {
            var loadOperation = ABLoader.LoadShaderAsync("MasterDuel/Shader/" + shaderName);
            while (loadOperation.MoveNext())
                yield return null;
            _loadedShaders.TryAdd(shaderName, loadOperation.Current);
            _loadShaderCoroutines.TryRemove(shaderName, out _);
            yield return loadOperation.Current;
        }

        public static IEnumerator<Shader> LoadShaderByNameAsync(string shaderName)
        {
            if (_loadedShaders.TryGetValue(shaderName, out var shader))
            {
                yield return shader;
                yield break;
            }

            if (_loadShaderCoroutines.TryGetValue(shaderName, out var loading))
            {
                while (loading.MoveNext())
                    yield return null;
                yield return loading.Current;
            }
            else
            {
                var coroutine = instance.LoadShaderCoroutine(shaderName);
                if (_loadShaderCoroutines.TryAdd(shaderName, coroutine))
                {
                    while (coroutine.MoveNext())
                        yield return null;
                    yield return coroutine.Current;
                }
                else
                {
                    instance.StopCoroutine(coroutine);
                    while (_loadShaderCoroutines[shaderName].MoveNext())
                        yield return null;
                    yield return _loadShaderCoroutines[shaderName].Current;
                }
            }
        }

        #endregion

    }
}