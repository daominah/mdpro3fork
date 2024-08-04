using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MDPro3.UI
{
    public class SuperScrollViewItemTwoStage : SuperScrollViewItem
    {
        public int distance = 60;
        public Button button;
        public Action action;

        public float startX;
        public int stage = -1;
        public bool refreshed;

        public IEnumerator enumerator;

        private void Start()
        {
            button.onClick.AddListener(OnClick);
        }

        public override void OnClick()
        {
            base.OnClick();
            if (stage == -1)
                ToStage1();
            else
                OnDecide();
        }

        public void ToStage0()
        {
            stage = -1;
            GetComponent<RectTransform>().DOAnchorPosX(startX, 0.2f);
        }
        public void ToStage1()
        {
            stage = 1;
            GetComponent<RectTransform>().DOAnchorPosX(startX + distance, 0.2f);
            foreach (var btn in transform.parent.GetComponentsInChildren<SuperScrollViewItemTwoStage>())
                if (btn != this)
                    btn.ToStage0();
            OnSelected();
        }
        public void OnDecide()
        {
            action?.Invoke();
        }


        public virtual void OnSelected()
        {

        }

        public override void Refresh()
        {
            base.Refresh();
            refreshed = false;
            if(enumerator != null)
                StopCoroutine(enumerator);
            enumerator = RefreshAsync();
            if(gameObject.activeInHierarchy)
                StartCoroutine(enumerator);
        }

        private void OnEnable()
        {
            if(enumerator != null)
                StartCoroutine(enumerator);
        }

        public virtual IEnumerator RefreshAsync()
        {
            yield return null;
            enumerator = null;
            refreshed = true;
        }


        public void Dispose()
        {
            Destroy(gameObject);
            //StartCoroutine(DisposeAsync());
        }

        public IEnumerator DisposeAsync()
        {
            while (!refreshed)
                yield return null;
            Destroy(gameObject);
        }
    }
}
