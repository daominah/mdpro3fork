using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace YgomSystem.UI
{
	public abstract class Tween : MonoBehaviour
	{
		public enum Easing
		{
			Linear = 0,
			CubicIn = 1,
			CubicOut = 2,
			CubicInOut = 3,
			BackIn = 4,
			BackOut = 5,
			BackInOut = 6,
			BounceIn = 7,
			BounceOut = 8,
			BounceInOut = 9,
			Customize = 10,
			QuartIn = 11,
			QuartOut = 12,
			QuartInOut = 13
		}

		public static DG.Tweening.Ease GetDGTweenEase(Easing ease)
		{
			switch (ease)
			{
                case Easing.Linear: return DG.Tweening.Ease.Linear;
                case Easing.CubicIn: return DG.Tweening.Ease.InCubic;
                case Easing.CubicOut: return DG.Tweening.Ease.OutCubic;
                case Easing.CubicInOut: return DG.Tweening.Ease.InOutCubic;
                case Easing.BackIn: return DG.Tweening.Ease.InBack;
                case Easing.BackOut: return DG.Tweening.Ease.OutBack;
                case Easing.BackInOut: return DG.Tweening.Ease.InOutBack;
                case Easing.BounceIn: return DG.Tweening.Ease.InBounce;
                case Easing.BounceOut: return DG.Tweening.Ease.OutBounce;
                case Easing.BounceInOut: return DG.Tweening.Ease.InOutBounce;
                case Easing.QuartIn: return DG.Tweening.Ease.InQuart;
                case Easing.QuartOut: return DG.Tweening.Ease.OutQuart;
                case Easing.QuartInOut: return DG.Tweening.Ease.InOutQuart;
                default: return DG.Tweening.Ease.Linear;
            }
        }

        public enum Style
		{
			Once = 0,
			Loop = 1,
			PingPong = 2,
			PingPongLoop = 3,
			SyncLoop = 4,
			SyncPingPongLoop = 5
		}

		private static readonly float FRAMERATE_LIMIT;

		public string label;

		[SerializeField]
		public Easing easing;

		[SerializeField]
		public Style style;

		[SecField]
		[SerializeField]
		public float duration;

		[SerializeField]
		[SecField]
		public float setupWait;

		[SecField]
		[SerializeField]
		public float startDelay;

		[SerializeField]
		public bool ignoreTimeScale;

		[SerializeField]
		public UnityEvent onFinished;

		[SerializeField]
		public bool callOnFinishedDestroy;

		[HideInInspector]
		public AnimationCurve curve;

		protected float timeDelta;

		protected float crntTime;

		private bool isCaptured;

		private float setupWaitCount;

		private bool isExecFinished;

		private static float BounceOut(float k)
		{
            if (k < (1 / 2.75f))
                return 7.5625f * k * k;
            else if (k < (2 / 2.75f))
                return 7.5625f * (k -= (1.5f / 2.75f)) * k + 0.75f;
            else if (k < (2.5 / 2.75))
                return 7.5625f * (k -= (2.25f / 2.75f)) * k + 0.9375f;
            else
                return 7.5625f * (k -= (2.625f / 2.75f)) * k + 0.984375f;
        }

		private static float BounceIn(float k)
		{
            return 1 - BounceOut(1 - k);
        }

		public static float EasingValue(float k, Easing e)
		{
            switch (e)
            {
                case Easing.Linear: return k;
                case Easing.CubicIn: return k * k * k;
                case Easing.CubicOut: return 1 - Mathf.Pow(1 - k, 3);
                case Easing.CubicInOut: return k < 0.5 ? 4 * k * k * k : 1 - Mathf.Pow(-2 * k + 2, 3) / 2;
                case Easing.BackIn: return k * k * ((1.70158f + 1) * k - 1.70158f);
                case Easing.BackOut: return 1 + (--k) * k * ((1.70158f + 1) * k + 1.70158f);
                case Easing.BackInOut:
                    float s = 1.70158f * 1.525f;
                    return (k *= 2) < 1 ?
                        0.5f * (k * k * ((s + 1) * k - s)) :
                        0.5f * ((k -= 2) * k * ((s + 1) * k + s) + 2);
                case Easing.BounceIn: return BounceIn(k);
                case Easing.BounceOut: return BounceOut(k);
                case Easing.BounceInOut: return k < 0.5f ? BounceIn(k * 2) * 0.5f : BounceOut(k * 2 - 1) * 0.5f + 0.5f;
                case Easing.QuartIn: return k * k * k * k;
                case Easing.QuartOut: return 1 - Mathf.Pow(1 - k, 4);
                case Easing.QuartInOut: return k < 0.5 ? 8 * k * k * k * k : 1 - Mathf.Pow(-2 * k + 2, 4) / 2;
                default: return k;
            }
        }

		private float GetEasing(float k)
		{
            return easing == Easing.Customize ? curve.Evaluate(k) : EasingValue(k, easing);
        }

		protected virtual void CaptureAwake()
		{
		}

		protected virtual void CaptureFrom()
		{
		}

		protected abstract void OnSetValue(float par);

		private void Awake()
		{
		}

		private void ExecSetup()
		{
		}

		private void ExecPlay(float time, bool forceUpdate = false)
		{
            crntTime = Mathf.Clamp(time, 0, duration);
            float t = duration > 0 ? crntTime / duration : 1f;

            // 处理循环类型
            switch (style)
            {
                case Style.Loop:
                    t %= 1f;
                    break;
                case Style.PingPong:
                    t = Mathf.PingPong(t, 1f);
                    break;
                    // 其他样式处理...
            }

            OnSetValue(GetEasing(t));

            if (crntTime >= duration)
            {
                if (style == Style.Once)
                {
                    isExecFinished = true;
                    onFinished?.Invoke();
                    if (callOnFinishedDestroy) DestroySelf();
                }
                else if (style == Style.Loop)
                {
                    ResetWithTimeDelta();
                }
            }
        }

		private void OnDestroy()
		{
		}

		private void Start()
		{
		}

		private void Update()
		{
            //if (!isExecFinished && setupWaitCount <= 0)
            //{
            //    float delta = ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
            //    timeDelta += delta;
            //    ExecPlay(timeDelta);
            //}

            //if (setupWaitCount > 0)
            //{
            //    setupWaitCount -= ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
            //}
        }

		public void Play()
		{
            if (setupWait > 0)
            {
                setupWaitCount = setupWait;
            }
            isExecFinished = false;
            enabled = true;
        }

		public void Pause()
		{
		}

		public void Stop()
		{
		}

		public void End()
		{
		}

		public void Reset()
		{
		}

		public void ResetWithTimeDelta()
		{
            crntTime = 0;
            timeDelta = 0;
        }

		public void GotoAndPlay(float time)
		{
		}

		public void GotoAndPause(float time)
		{
		}

		public void DestroySelf()
		{
		}

		public void PlayLabel(string _label)
		{
		}

		public bool IsPlaying(string _label = "", bool isActive = false)
		{
			return false;
		}

		public bool IsFinished()
		{
			return false;
		}

		public static void TargetPlayLabel(GameObject target, string _label = "", bool includeChildren = false, bool wakeup = false)
		{
		}

		public static bool TargetIsPlaying(GameObject target, string _label = "", bool includeChildren = false, bool isActive = false)
		{
			return false;
		}

		public static void TargetGotoAndPlayLabel(GameObject target, float time, string _label = "", bool includeChildren = false, bool wakeup = false)
		{
		}

		public static void TargetGotoAndPauseLabel(GameObject target, float time, string _label = "", bool includeChildren = false, bool wakeup = false)
		{
		}

		public static void TargetPauseLabel(GameObject target, string _label = "", bool includeChildren = false)
		{
		}

		public static void TargetStopLabel(GameObject target, string _label = "", bool includeChildren = false, string exlabel = "")
		{
		}

		public static void TargetEndLabel(GameObject target, string _label = "", bool includeChildren = false)
		{
		}

		public static void TargetForwardLabel(GameObject target, float sec, string _label = "", bool includeChildren = false)
		{
		}

		public static void TargetCaptureFrom(GameObject target, string _label = "", bool includeChildren = false, bool force = false)
		{
		}

		public static void TargetCaptureFrom(Tween tween, bool force)
		{
		}

		public static void AllPlayLabel(string label)
		{
		}

		public static void AllStopLabel(string label)
		{
		}

		public static void AllPauseLabel(string label)
		{
		}

		public static void AllEndLabel(string label)
		{
		}

		public static List<Tween> GetTweenTarget(GameObject target, string _label = "", bool includeChildren = false)
		{
            List<Tween> result = new List<Tween>();
            Tween[] tweens = includeChildren ? target.GetComponentsInChildren<Tween>() : target.GetComponents<Tween>();

            foreach (Tween t in tweens)
            {
                if (string.IsNullOrEmpty(_label) || t.label == _label)
                    result.Add(t);
            }
            return result;
        }

		public static List<Tween> GetTweenAll(string label)
		{
			return null;
		}
	}
}
