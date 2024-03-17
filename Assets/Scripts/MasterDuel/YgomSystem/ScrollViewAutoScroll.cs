using UnityEngine;
using UnityEngine.UI;

namespace YgomSystem
{
	public class ScrollViewAutoScroll : MonoBehaviour
	{
		private enum ScrollViewDirection
		{
			Horizontal = 0,
			Vertical = 1
		}

		private enum Step
		{
			BeginIdleStep = 0,
			ScrollStep = 1,
			EndIdleStep = 2
		}

		[SerializeField]
		private float m_ScrollSpeed;

		[SerializeField]
		private float m_WaitTimeBegin;

		[SerializeField]
		private float m_WaitTimeEnd;

		private ScrollViewDirection m_Direction;

		private Coroutine m_UpdateScrollCorutine;

		private ScrollRect m_ScrollRect;

		private RectTransform m_ContentRT;

		private float m_DeltaTime;

		private Step m_Step;

		private void Awake()
		{
		}

		public void ResetScroll()
		{
		}

		private void Update()
		{
		}

		private void BeginIdleProcess()
		{
		}

		private void ScrollProcess()
		{
		}

		private void EndIdleProcess()
		{
		}
	}
}
