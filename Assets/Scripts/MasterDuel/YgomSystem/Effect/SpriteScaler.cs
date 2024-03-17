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

		private Vector2 appliedScreenSize;

		public bool isApplyOnUpdate
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public bool useDirectSizeSetting
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public Vector2 directSizeSetting
		{
			get
			{
				return default(Vector2);
			}
			set
			{
			}
		}

		public bool changePosition
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public bool useFixedDepth
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public float fixedDepth
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public Camera viewCamera
		{
			[CompilerGenerated]
			get
			{
				return null;
			}
			[CompilerGenerated]
			private set
			{
			}
		}

		public void SetFitMode(FitMode fitMode)
		{
		}

		public void Setup(Camera view_camera)
		{
		}
        private void Awake()
        {
			Apply();
        }

        public void Apply()
		{
			var widthScale = transform.localScale.x;
			var heightScale = transform.localScale.y;

			if(fitMode == FitMode.FitWidth)
			{
				var x = widthScale * (Screen.width * 9f / (Screen.height * 16f));
				transform.localScale = new Vector2(x, heightScale);
			}
			else if(fitMode == FitMode.FitHeight)
			{
				var y = heightScale * (Screen.height * 16f / (Screen.width * 9f));
				transform.localScale=new Vector2(widthScale, y);
			}
            else if (fitMode == FitMode.FitWidthMaintainAspectRatio)
            {
                var x = widthScale * (Screen.width * 9f / (Screen.height * 16f));
                transform.localScale = new Vector2(x, heightScale * x / widthScale);
            }
            else if (fitMode == FitMode.FitHeightMaintainAspectRatio)
            {
                var y = heightScale * (Screen.height * 16f / (Screen.width * 9f));
                transform.localScale = new Vector2(widthScale * y / heightScale, y);
            }
            else if (fitMode == FitMode.FitWidthHeight)
            {
                var x = heightScale * Screen.width / Screen.height;
                transform.localScale = new Vector2(x * 1.1f, heightScale);
				if (transform.parent.name.StartsWith("Ef04678"))
                    transform.localScale = new Vector2(x * 2f, heightScale * 2f);
            }
        }

        public void Apply(float screenWidth, float screenHeight)
		{
		}

		public void Reapply()
		{
		}

		private void OnDestroy()
		{
		}

		private void Update()
		{
		}
	}
}
