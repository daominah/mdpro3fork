using MDPro3;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace YgomSystem.Effect
{
	public class SpriteScaler : MonoBehaviour
	{
		public enum FitMode
		{
			None = 0,
			FitWidth = 1,
			FitHeight = 2,
			FitWidthMaintainAspectRatio = 3,
			FitHeightMaintainAspectRatio = 4,
			FitWidthHeight = 5,
			FitHighestResolutionMaintainAspectRatio = 6,
			FitLowestResolutionMaintainAspectRatio = 7
		}

		[SerializeField]
		private bool applyOnUpdate;

		[SerializeField]
		private FitMode fitMode;

		[SerializeField]
		public Vector3 offsetScale;

		[SerializeField]
		private bool _useDirectSizeSetting;

		[SerializeField]
		private Vector2 _directSizeSetting;

		[SerializeField]
		private bool _changePosition;

		[SerializeField]
		private bool _isUseFixedDepth;

		[SerializeField]
		private float _fixedDepth;

		private SpriteRenderer targetSprite;

		private SpriteMask targetMask;

		private bool isApplied;

		private bool applyOnCustomSize;

        private bool m_ScaleYtoZ;

        private Vector2 appliedScreenSize;

		public bool isApplyOnUpdate
		{
			get
			{
				return applyOnUpdate;
			}
			set
			{
				applyOnUpdate = value;
			}
		}

		public bool useDirectSizeSetting
		{
			get
			{
				return _useDirectSizeSetting;
			}
			set
			{
			}
		}

		public Vector2 directSizeSetting
		{
			get
			{
				return _directSizeSetting;
			}
			set
			{
			}
		}

		public bool changePosition
		{
			get
			{
				return _changePosition;
			}
			set
			{
			}
		}

		public bool useFixedDepth
		{
			get
			{
				return _isUseFixedDepth;
			}
			set
			{
			}
		}

		public float fixedDepth
		{
			get
			{
				return _fixedDepth;
			}
			set
			{
			}
		}

        public Camera viewCamera { get; private set; }

        public bool scaleYtoZ
        {
            get
            {
                return m_ScaleYtoZ;
            }
            set
            {
            }
        }

        public void SetFitMode(FitMode fitMode)
		{
			this.fitMode = fitMode;
		}

        public FitMode GetFitMode()
        {
            return fitMode;
        }

        public Sprite TryGetTargetSprite()
        {
			if(targetSprite != null)
				return targetSprite.sprite;
            return null;
        }

        private void OnEnable()
        {
			StartCoroutine(SetupAsync());
        }

		private IEnumerator SetupAsync()
		{
            if (targetSprite == null) targetSprite = GetComponent<SpriteRenderer>();
            var originalColor = targetSprite.color;
            targetSprite.color = Color.clear;

            // Wait for the next frame to ensure script changes are ready.
            yield return null;

            targetSprite.color = originalColor;

            Camera viewCamera = gameObject.layer switch
            {
                3 => Program.instance.camera_.camera2D,
                16 => Program.instance.camera_.cameraDuelOverlay3D,
                17 => Program.instance.camera_.cameraDuelOverlayEffect3D,
                18 => Program.instance.camera_.cameraDuelOverlay2D,
                19 => Program.instance.camera_.cameraDuelOverlayEffect2D,
                _ => Program.instance.camera_.cameraMain,
            };

            Setup(viewCamera);
        }

        public void Setup(Camera view_camera)
		{
			viewCamera = view_camera;
			Apply();
		}

        public void Apply()
		{
            if (viewCamera == null)
                return;

            if (_changePosition)
                transform.position = viewCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, _fixedDepth));

            //float depth = _isUseFixedDepth ? _fixedDepth : viewCamera.WorldToScreenPoint(transform.position).z;
            float depth = viewCamera.WorldToScreenPoint(transform.position).z;

            float screenWidth;
            float screenHeight;

            if (viewCamera.orthographic)
            {
                Vector3 bottomLeft = viewCamera.ViewportToWorldPoint(new Vector3(0, 0, depth));
                Vector3 topRight = viewCamera.ViewportToWorldPoint(new Vector3(1, 1, depth));
                screenWidth = topRight.x - bottomLeft.x;
                screenHeight = topRight.y - bottomLeft.y;
            }
            else
            {
                float halfFOV = viewCamera.fieldOfView * 0.5f * Mathf.Deg2Rad;
                screenHeight = 2f * depth * Mathf.Tan(halfFOV);
                screenWidth = screenHeight * viewCamera.aspect;
            }
            Apply(screenWidth, screenHeight);
        }

        public void Apply(float screenWidth, float screenHeight)
		{
            if (isApplied && appliedScreenSize.x == screenWidth && appliedScreenSize.y == screenHeight)
                return;

            if (targetSprite == null) targetSprite = GetComponent<SpriteRenderer>();
            if (targetMask == null) targetMask = GetComponent<SpriteMask>();

            Sprite sprite = TryGetTargetSprite();
            if (sprite == null) return;

            Vector2 spriteSize = sprite.rect.size / sprite.pixelsPerUnit;
            Vector2 targetSize = Vector2.zero;

            if (_useDirectSizeSetting)
            {
                targetSize = _directSizeSetting;
            }
            else
            {
                switch (fitMode)
                {
                    case FitMode.None:
                        targetSize = spriteSize;
                        break;
                    case FitMode.FitWidth:
                        targetSize = new Vector2(screenWidth, spriteSize.y);
                        break;
                    case FitMode.FitHeight:
                        targetSize = new Vector2(spriteSize.x, screenHeight);
                        break;
                    case FitMode.FitWidthMaintainAspectRatio:
                        float widthScale = screenWidth / spriteSize.x;
                        targetSize = spriteSize * widthScale;
                        break;
                    case FitMode.FitHeightMaintainAspectRatio:
                        float heightScale = screenHeight / spriteSize.y;
                        targetSize = spriteSize * heightScale;
                        break;
                    case FitMode.FitWidthHeight:
                        targetSize = new Vector2(screenWidth, screenHeight);
                        break;
                    case FitMode.FitHighestResolutionMaintainAspectRatio:
                        float maxScale = Mathf.Max(screenWidth / spriteSize.x, screenHeight / spriteSize.y);
                        targetSize = spriteSize * maxScale;
                        break;
                    case FitMode.FitLowestResolutionMaintainAspectRatio:
                        float minScale = Mathf.Min(screenWidth / spriteSize.x, screenHeight / spriteSize.y);
                        targetSize = spriteSize * minScale;
                        break;
                    default:
                        targetSize = spriteSize;
                        break;
                }
            }

            Vector3 newScale = new(
                targetSize.x / spriteSize.x,
                targetSize.y / spriteSize.y,
                1f
            );

			newScale += offsetScale;

            if (m_ScaleYtoZ)
                newScale.z = newScale.y;

            transform.localScale = newScale;

            isApplied = true;
            appliedScreenSize = new Vector2(screenWidth, screenHeight);

            Debug.Log($"SpriteScaler applied: {gameObject.name}, Camera: {viewCamera}, Mode: {fitMode}, Scale: {transform.localScale}, Screen Size: {appliedScreenSize}");
        }

		public void Reapply()
		{
            isApplied = false;
            Apply();
        }

		private void OnDestroy()
		{
		}

		private void Update()
		{
            if (applyOnUpdate && viewCamera != null)
                Apply();
        }
	}
}
