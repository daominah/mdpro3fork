using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MDPro3
{
    public class CameraManager : Manager
    {
        //DuelOverlay3D 顶部视角
        //DuelOverlayEffect3D 正视角
        //DuelOverlay2D
        //DuelOverlayEffect2D OverUI
        public Camera cameraMain;
        public Camera camera2D;
        public Camera cameraDuelOverlay3D;
        public Camera cameraDuelOverlayEffect3D;
        public Camera cameraDuelOverlay2D;
        public Camera cameraDuelOverlayEffect2D;
        public Camera cameraUI;
        public Camera cameraUIBlur;
        public Camera cameraRenderTexture;
        public GameObject light;
        public SpriteRenderer black;

        public UniversalRenderPipelineAsset urpAsset;
        public ForwardRendererData forwardRendererData;
        public UniversalRenderPipelineAsset urpAssetForUI;
        public ForwardRendererData forwardRendererDataForUI;

        public override void Initialize()
        {
            base.Initialize();
            urpAsset = Resources.Load<UniversalRenderPipelineAsset>("Settings/URPAsset");
            forwardRendererData = Resources.Load<ForwardRendererData>("Settings/URPAsset_Renderer");
            urpAssetForUI = Resources.Load<UniversalRenderPipelineAsset>("Settings/URPAssetForUI");
            forwardRendererDataForUI = Resources.Load<ForwardRendererData>("Settings/URPAssetForUI_Renderer");

            ShiftTo2D();
            ChangeCameraFOV();
            DuelOverlay2DMinus();
            DuelOverlay3DMinus();
            DuelOverlayEffect2DMinus();
            DuelOverlayEffect3DMinus();
            UIBlurMinus();
            Program.onScreenChanged += ChangeCameraFOV;
        }

        public static void ChangeCameraFOV()
        {
            float aspect = (float)Screen.width * 9 / Screen.height;
            if (aspect > 16)
            {
                Program.I().camera_.cameraMain.fieldOfView = 30 + 16 - aspect;
                Program.I().camera_.cameraDuelOverlay3D.fieldOfView = Program.I().camera_.cameraMain.fieldOfView;
            }
            else
            {
                Program.I().camera_.cameraMain.fieldOfView = 30;
                Program.I().camera_.cameraDuelOverlay3D.fieldOfView = 30;
            }
        }

        public static void ShiftTo2D()
        {
            Program.I().camera_.cameraMain.gameObject.SetActive(false);
            Program.I().camera_.light.SetActive(false);
            Program.I().camera_.camera2D.gameObject.SetActive(true);

            DuelOverlay3DCount = 0;
            DuelOverlay2DCount = 0;
            DuelOverlayEffect3DCount = 0;
            DuelOverlayEffect2DCount = 0;

            DuelOverlay3DMinus();
            DuelOverlayEffect3DMinus();
            DuelOverlay2DMinus();
            DuelOverlayEffect2DMinus();

            QualitySettings.SetQualityLevel(6);
        }
        public static void ShiftTo3D()
        {
            Program.I().camera_.cameraMain.gameObject.SetActive(true);
            Program.I().camera_.light.SetActive(true);
            Program.I().camera_.camera2D.gameObject.SetActive(false);
            QualitySettings.SetQualityLevel(int.Parse(Config.Get("Quality", "0")));
        }
        public static void Overlay3DReset()
        {
            Program.I().camera_.cameraDuelOverlay3D.transform.localPosition = new Vector3(0, 95, -37);
            Program.I().camera_.cameraDuelOverlay3D.transform.localEulerAngles = new Vector3(70, 0, 0);
        }

        public static int DuelOverlay2DCount = 0;
        public static void DuelOverlay2DPlus()
        {
            DuelOverlay2DCount++;
            Program.I().camera_.cameraDuelOverlay2D.gameObject.SetActive(true);
        }
        public static void DuelOverlay2DMinus()
        {
            DuelOverlay2DCount--;
            if (DuelOverlay2DCount < 0)
                DuelOverlay2DCount = 0;
            if (DuelOverlay2DCount == 0)
                Program.I().camera_.cameraDuelOverlay2D.gameObject.SetActive(false);
        }

        public static int DuelOverlayEffect2DCount = 0;
        public static void DuelOverlayEffect2DPlus()
        {
            DuelOverlayEffect2DCount++;
            Program.I().camera_.cameraDuelOverlayEffect2D.gameObject.SetActive(true);
        }
        public static void DuelOverlayEffect2DMinus()
        {
            DuelOverlayEffect2DCount--;
            if (DuelOverlayEffect2DCount < 0)
                DuelOverlayEffect2DCount = 0;
            if (DuelOverlayEffect2DCount == 0)
                Program.I().camera_.cameraDuelOverlayEffect2D.gameObject.SetActive(false);
        }

        public static int DuelOverlay3DCount = 0;
        public static void DuelOverlay3DPlus()
        {
            DuelOverlay3DCount++;
            Program.I().camera_.cameraDuelOverlay3D.gameObject.SetActive(true);
        }
        public static void DuelOverlay3DMinus()
        {
            DuelOverlay3DCount--;
            if (DuelOverlay3DCount < 0)
                DuelOverlay3DCount = 0;
            if (DuelOverlay3DCount == 0)
                Program.I().camera_.cameraDuelOverlay3D.gameObject.SetActive(false);
        }

        public static int DuelOverlayEffect3DCount = 0;
        public static void DuelOverlayEffect3DPlus()
        {
            DuelOverlayEffect3DCount++;
            Program.I().camera_.cameraDuelOverlayEffect3D.gameObject.SetActive(true);
        }
        public static void DuelOverlayEffect3DMinus()
        {
            DuelOverlayEffect3DCount--;
            if (DuelOverlayEffect3DCount < 0)
                DuelOverlayEffect3DCount = 0;
            if (DuelOverlayEffect3DCount == 0)
                Program.I().camera_.cameraDuelOverlayEffect3D.gameObject.SetActive(false);
        }


        public static int uiBlurCount = 0;
        public static void UIBlurPlus()
        {
            uiBlurCount++;
            Program.I().camera_.cameraUIBlur.gameObject.SetActive(true);
            foreach (var feature in Program.I().camera_.forwardRendererData.rendererFeatures)
                if (feature is KawaseBlur)
                    feature.SetActive(true);
            foreach (var feature in Program.I().camera_.forwardRendererDataForUI.rendererFeatures)
                if (feature is KawaseBlur)
                    feature.SetActive(true);
        }
        public static void UIBlurMinus()
        {
            uiBlurCount--;
            if (uiBlurCount < 0)
                uiBlurCount = 0;
            if (uiBlurCount == 0)
            {
                Program.I().camera_.cameraUIBlur.gameObject.SetActive(false);
                foreach (var feature in Program.I().camera_.forwardRendererData.rendererFeatures)
                    if (feature is KawaseBlur)
                        feature.SetActive(false);
                foreach (var feature in Program.I().camera_.forwardRendererDataForUI.rendererFeatures)
                    if (feature is KawaseBlur)
                        feature.SetActive(false);
            }
        }

        public static void BlackInOut(float delay, float inTime, float time, float outTime)
        {
            var sequence = DOTween.Sequence();
            sequence.AppendInterval(delay);
            sequence.Append(Program.I().camera_.black.DOFade(0.75f, inTime));
            sequence.AppendInterval(time);
            sequence.Append(Program.I().camera_.black.DOFade(0, outTime));
        }
        public static void BlackIn(float delay, float inTime)
        {
            var sequence = DOTween.Sequence();
            sequence.AppendInterval(delay);
            sequence.Append(Program.I().camera_.black.DOFade(0.75f, inTime));
        }
        public static void BlackOut(float delay, float outTime)
        {
            var sequence = DOTween.Sequence();
            sequence.AppendInterval(delay);
            sequence.Append(Program.I().camera_.black.DOFade(0f, outTime));
        }

        public static void ShakeCamera(bool heavy = false)
        {
            if (heavy)
            {
                Program.I().camera_.cameraMain.DOShakePosition(0.4f, 5, 100);
            }
            else
            {
                Program.I().camera_.cameraMain.DOShakePosition(0.3f, 1, 100);
            }
        }
    }
}
