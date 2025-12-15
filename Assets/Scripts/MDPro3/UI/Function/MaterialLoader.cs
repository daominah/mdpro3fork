using Cysharp.Threading.Tasks;
using MDPro3.Duel.YGOSharp;
using MDPro3.Utility;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MDPro3
{
    public static class MaterialLoader
    {

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


        [RuntimeInitializeOnLoadMethod]
        public static async UniTask LoadCardMaterials()
        {
            await UniTask.WaitUntil(() => TextureManager.loaded && TextureManager.container != null);

            cardMatNormalUI = ABLoader.LoadMasterDuelMaterial("NormalStyleUI");
            cardMatShineUI = ABLoader.LoadMasterDuelMaterial("ShineStyleUI");
            cardMatRoyalUI = ABLoader.LoadMasterDuelMaterial("RoyalStyleUI");

            cardMatGoldUI = UnityEngine.Object.Instantiate(cardMatRoyalUI);
            cardMatGoldUI.SetFloat("_CardDistortion01", 1.2f);
            cardMatGoldUI.SetFloat("_Kira01_01Tile", 0.25f);
            cardMatGoldUI.SetFloat("_Kira01_01Power", 3f);
            cardMatGoldUI.SetColor("_KiraColor02", new Color(0.5f, 0.5f, 0f, 0f));
            cardMatGoldUI.SetColor("_CubemapColor", new Color(0.7f, 0.7f, 0f, 0f));

            cardMatMillenniumUI = UnityEngine.Object.Instantiate(cardMatRoyalUI);
            cardMatMillenniumUI.SetTexture("_HighlightNormal"
                , TextureManager.container.CardKiraNormal03_Millennium);
            cardMatMillenniumUI.SetColor("_CubemapColor", new Color(0.898f, 0.3245f, 0.7723f, 0f));
            cardMatMillenniumUI.SetColor("_KiraColor02", new Color(0.3099f, 0.1633f, 0.2753f, 0f));
            cardMatMillenniumUI.SetFloat("_Kira01_01Tile", 0.25f);
            cardMatMillenniumUI.SetFloat("_Kira01_02Tile", 0f);
            cardMatMillenniumUI.SetFloat("_RanbowPower", 0.5f);

            cardMatShineRDUI = UnityEngine.Object.Instantiate(cardMatShineUI);
            MaterialToRD(cardMatShineRDUI);
            cardMatRoyalRDUI = UnityEngine.Object.Instantiate(cardMatRoyalUI);
            MaterialToRD(cardMatRoyalRDUI);
            cardMatGoldRDUI = UnityEngine.Object.Instantiate(cardMatGoldUI);
            MaterialToRD(cardMatGoldRDUI);
            cardMatMillenniumRDUI = UnityEngine.Object.Instantiate(cardMatMillenniumUI);
            MaterialToRD(cardMatMillenniumRDUI);

            cardMatNormal3D = ABLoader.LoadMasterDuelMaterial("NormalStyle3D");
            cardMatShine3D = ABLoader.LoadMasterDuelMaterial("ShineStyle3D");
            cardMatRoyal3D = ABLoader.LoadMasterDuelMaterial("RoyalStyle3D");

            cardMatGold3D = UnityEngine.Object.Instantiate(cardMatRoyal3D);
            cardMatGold3D.SetFloat("_CardDistortion01", 1.2f);
            cardMatGold3D.SetFloat("_Kira01_01Tile", 0.25f);
            cardMatGold3D.SetFloat("_Kira01_01Power", 3f);
            cardMatGold3D.SetColor("_KiraColor02", new Color(0.5f, 0.5f, 0f, 0f));
            cardMatGold3D.SetColor("_CubemapColor", new Color(0.7f, 0.7f, 0f, 0f));

            cardMatMillennium3D = UnityEngine.Object.Instantiate(cardMatRoyal3D);
            cardMatMillennium3D.SetTexture("_HighlightNormal"
                , TextureManager.container.CardKiraNormal03_Millennium);
            cardMatMillennium3D.SetColor("_CubemapColor", new Color(0.898f, 0.3245f, 0.7723f, 0f));
            cardMatMillennium3D.SetColor("_KiraColor02", new Color(0.3099f, 0.1633f, 0.2753f, 0f));
            cardMatMillennium3D.SetFloat("_Kira01_01Tile", 0.25f);
            cardMatMillennium3D.SetFloat("_Kira01_02Tile", 0f);
            cardMatMillennium3D.SetFloat("_RanbowPower", 0.5f);

            cardMatShineRD3D = UnityEngine.Object.Instantiate(cardMatShine3D);
            MaterialToRD(cardMatShineRD3D);
            cardMatRoyalRD3D = UnityEngine.Object.Instantiate(cardMatRoyal3D);
            MaterialToRD(cardMatRoyalRD3D);
            cardMatGoldRD3D = UnityEngine.Object.Instantiate(cardMatGold3D);
            MaterialToRD(cardMatGoldRD3D);
            cardMatMillenniumRD3D = UnityEngine.Object.Instantiate(cardMatMillennium3D);
            MaterialToRD(cardMatMillenniumRD3D);
        }

        private static void MaterialToRD(Material material)
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

        public static Material GetCardMaterial(int code, bool use3D = false)
        {
            Material mat = null;

            if (code < 0)
            {
                mat = UnityEngine.Object.Instantiate(use3D ? cardMatNormal3D : cardMatNormalUI);
                return mat;
            }

            bool rushDuel = CardRenderer.NeedRushDuelStyle(code);
            var rarity = CardRarity.GetRarity(code);

            bool needSet = true;
            switch (rarity)
            {
                case CardRarity.Rarity.Normal:
                    mat = UnityEngine.Object.Instantiate(use3D ? cardMatNormal3D : cardMatNormalUI);
                    needSet = false;
                    break;
                case CardRarity.Rarity.Shine:
                    mat = UnityEngine.Object.Instantiate(rushDuel ? use3D ? cardMatShineRD3D : cardMatShineRDUI : use3D ? cardMatShine3D : cardMatShineUI);
                    break;
                case CardRarity.Rarity.Royal:
                    mat = UnityEngine.Object.Instantiate(rushDuel ? use3D ? cardMatRoyalRD3D : cardMatRoyalRDUI : use3D ? cardMatRoyal3D : cardMatRoyalUI);
                    break;
                case CardRarity.Rarity.Gold:
                    mat = UnityEngine.Object.Instantiate(rushDuel ? use3D ? cardMatGoldRD3D : cardMatGoldRDUI : use3D ? cardMatGold3D : cardMatGoldUI);
                    break;
                case CardRarity.Rarity.Millennium:
                    mat = UnityEngine.Object.Instantiate(rushDuel ? use3D ? cardMatMillenniumRD3D : cardMatMillenniumRDUI : use3D ? cardMatMillennium3D : cardMatMillenniumUI);
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

                var nameTask = CardImageLoader.LoadCardName(code);
                mat.SetTexture("_MonsterNameTex", nameTask);

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

                    if (Language.AttributeNeedRuby())
                        mat.SetVector("_AttributeSize_Pos", new Vector4(9.85f, 13.96f, -3.7f, -5.81f));
                }

                if (rarity == CardRarity.Rarity.Millennium)
                {
                    mat.SetColor("_KiraColor02", GetMillenniumFrameColor(data));
                    mat.SetColor("_CubemapColor", GetMillenniumNameColor(data));
                }
            }

            return mat;
        }

        #endregion

        #region Load Material

        private static readonly ConcurrentDictionary<string, Material> _loadedMaterials = new();
        private static readonly ConcurrentDictionary<string, Task<Material>> _loadMaterialTasks = new();

        private static async Task<Material> LoadMaterialAsync(string materialName, CancellationToken token)
        {
            var mat = await ABLoader.LoadMaterialAsync("MasterDuel/Material/" + materialName, token);
            _loadedMaterials.TryAdd(materialName, mat);
            _loadMaterialTasks.TryRemove(materialName, out _);
            return mat;
        }

        public static async UniTask<Material> LoadMaterialByNameAsync(string materialName)
        {
            if (_loadedMaterials.TryGetValue(materialName, out var material))
            {
                return material;
            }

            if (_loadMaterialTasks.TryGetValue(materialName, out var task))
            {
                return await task;
            }
            else
            {
                using var cts = new CancellationTokenSource();
                task = LoadMaterialAsync(materialName, cts.Token);
                if (_loadMaterialTasks.TryAdd(materialName, task))
                {
                    return await task;
                }
                else
                {
                    cts.Cancel();
                    return await _loadMaterialTasks[materialName];
                }
            }
        }

        #endregion

        #region Load Shader

        private static readonly ConcurrentDictionary<string, Shader> _loadedShaders = new();
        private static readonly ConcurrentDictionary<string, UniTask<Shader>> _loadShaderTasks = new();

        private static async UniTask<Shader> LoadShaderAsync(string shaderName, CancellationToken token)
        {
            var shader = await ABLoader.LoadShaderAsync("MasterDuel/Shader/" + shaderName, token);
            _loadedShaders.TryAdd(shaderName, shader);
            _loadShaderTasks.TryRemove(shaderName, out _);
            return shader;
        }

        public static async UniTask<Shader> LoadShaderByNameAsync(string shaderName)
        {
            if (_loadedShaders.TryGetValue(shaderName, out var shader))
                return shader;

            if (_loadShaderTasks.TryGetValue(shaderName, out var task))
            {
                return await task;
            }
            else
            {
                using var cts = new CancellationTokenSource();
                task = LoadShaderAsync(shaderName, cts.Token);
                if (_loadShaderTasks.TryAdd(shaderName, task))
                {
                    return await task;
                }
                else
                {
                    cts.Cancel();
                    return await _loadShaderTasks[shaderName];
                }
            }
        }

        #endregion

    }
}