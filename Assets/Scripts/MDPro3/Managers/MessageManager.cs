using DG.Tweening;
using KonamiCommonIAB;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace MDPro3
{
    public class MessageManager : Manager
    {
        public GameObject messageItem;
        public GameObject messageCard;

        static MessageManager instance;
        static List<GameObject> items = new List<GameObject>();
        static readonly float transitionTime = 0.3f;
        static readonly float existTime = 3f;
        public override void Initialize()
        {
            base.Initialize();
            instance = this;
            var handle = Addressables.LoadAssetAsync<GameObject>("MessageItem");
            handle.Completed += (result) =>
            {
                messageItem = result.Result;
            };
            handle = Addressables.LoadAssetAsync<GameObject>("MessageCard");
            handle.Completed += (result) =>
            {
                messageCard = result.Result;
            };
        }

        public void CastCard(int code)
        {
            CameraManager.UIBlurPlus();
            var item = Instantiate(Program.I().message_.messageCard);
            Program.I().ocgcore.allGameObjects.Add(item);
            item.transform.SetParent(instance.transform, false);
            StartCoroutine(RefreshAsync(item, code));
        }

        IEnumerator RefreshAsync(GameObject item, int code)
        {
            var ie = Program.I().texture_.LoadCardAsync(code);
            while (ie.MoveNext())
                yield return null;
            var mat = TextureManager.GetCardMaterial(code);
            item.GetComponent<RawImage>().material = mat;
            item.GetComponent<RawImage>().texture = ie.Current;
            var rect = item.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(200, -160);
            rect.DOAnchorPosX(-50f, transitionTime);
            DOTween.To(v => { }, 0, 0, transitionTime + existTime).OnComplete(() =>
            {
                rect.DOAnchorPosX(200f, transitionTime);
            });
            DOTween.To(v => { }, 0, 0, existTime + transitionTime * 2).OnComplete(() =>
            {
                Destroy(item);
                CameraManager.UIBlurMinus();
            });
        }

        public static void Cast(string message)
        {
            if (items.Count > 10)
                return;

            CameraManager.UIBlurPlus();
            var item = Instantiate(Program.I().message_.messageItem);
            item.transform.SetParent(instance.transform, false);
            item.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = message;
            RectTransform rect = item.GetComponent<RectTransform>();
            int id = items.Count;
            items.Add(item);
            rect.anchoredPosition = new Vector2(900, -160 - id * 120);
            rect.DOAnchorPosX(-10f, transitionTime);
            DOTween.To(v => { }, 0, 0, existTime + id).OnComplete(() =>
            {
                rect.DOAnchorPosX(900f, transitionTime);
            });
            DOTween.To(v => { }, 0, 0, existTime + transitionTime + id).OnComplete(() =>
            {
                items.Remove(item);
                Destroy(item);
                MoveUp();
                CameraManager.UIBlurMinus();
            });
        }

        static void MoveUp()
        {
            foreach (var item in items)
            {
                var rect = item.GetComponent<RectTransform>();
                rect.DOAnchorPosY(rect.anchoredPosition.y + 120f, transitionTime);
            }
        }
    }
}
