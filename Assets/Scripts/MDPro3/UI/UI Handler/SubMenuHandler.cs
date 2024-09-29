using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace MDPro3.UI
{
    public class SubMenuHandler : UIHandler
    {
        [Header("SubMenu Handler")]

        public ScrollRect scrollRect;
        private float width = 700f;


        public override void Initialize()
        {
            base.Initialize();
            width = window.rect.width;
            window.anchoredPosition = new Vector2(width, 0f);
        }

        public override void PerframeFunction()
        {
            base.PerframeFunction();
            if (!showing) return;
            if (inTransition) return;
            if (Program.returnClicked)
            {
                Hide();
                AudioManager.PlaySE("SE_MENU_SLIDE_02");
            }
        }

        public void Show(List<string> menus, List<Action> actions)
        {
            Show();
            AudioManager.PlaySE("SE_MENU_SLIDE_01");
            Clear();
            var height = -30f;
            for (int i = 0; i < menus.Count; i++)
            {
                var text = menus[i];
                var action = actions[i];
                var currentHeight = height;
                if (action == null)
                {
                    var handle = Addressables.InstantiateAsync("SubMenuTitle");
                    handle.Completed += (result) =>
                    {
                        var rect = result.Result.GetComponent<RectTransform>();
                        rect.SetParent(scrollRect.content, false);
                        rect.anchoredPosition = new Vector2(0f, currentHeight);
                        rect.GetComponent<Text>().text = text;
                    };
                }
                else
                {
                    var handle = Addressables.InstantiateAsync("SubMenuButton");
                    handle.Completed += (result) =>
                    {
                        var rect = result.Result.GetComponent<RectTransform>();
                        rect.SetParent(scrollRect.content, false);
                        rect.anchoredPosition = new Vector2(0f, currentHeight);
                        rect.GetChild(0).GetComponent<Button>().onClick.AddListener(() => { action.Invoke(); });
                        rect.GetChild(0).GetComponent<Button>().onClick.AddListener(Hide);
                        rect.GetChild(0).GetChild(0).GetComponent<Text>().text = text;
                    };
                }
                if (action == null)
                {
                    height -= 80f;
                }
                else
                {
                    height -= 90f;
                }

                DOTween.To(v => { }, 0, 0, transitionTime).OnComplete(() =>
                {
                    inTransition = false;
                });

            }

            height -= 30f;
            scrollRect.content.sizeDelta = new Vector2(0, -height);
            scrollRect.verticalScrollbar.value = 1f;
        }

        public override void Show()
        {
            base.Show();
            window.anchoredPosition = new Vector2(width, 0f);
            window.DOAnchorPosX(0f, transitionTime);
        }
        public override void Hide()
        {
            base.Hide();
            window.DOAnchorPosX(width, transitionTime);
            DOTween.To(v => { }, 0, 0, transitionTime).OnComplete(() =>
            {
                Clear();
            });
        }

        private void Clear()
        {
            scrollRect.content.DestroyAllChildren();
        }
    }
}
