using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MDPro3.UI
{
    public class SuperScrollViewItemTwoStageForSolo : SuperScrollViewItemTwoStage
    {
        public Text title;
        public RawImage face;
        public Solo.BotInfo botInfo;

        public bool refreshed;
        IEnumerator enumerator;
        public override void OnSelected()
        {
            base.OnSelected();
            Program.I().solo.superScrollView.selected = id;
            Program.I().solo.description.text = botInfo.desc;
            Program.I().solo.description.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            if(id == 4)
                Program.I().solo.btnDeck.SetActive(true);
            else
                Program.I().solo.btnDeck.SetActive(false);
        }

        public override void Refresh()
        {
            base.Refresh();
            refreshed = false;
            title.text = botInfo.name;
            if (enumerator != null)
                StopCoroutine(enumerator);
            enumerator = RefreshFace();
            StartCoroutine(enumerator);
            action = () =>
            {
                Program.I().solo.StartAI(id);
            };
        }

        IEnumerator RefreshFace()
        {
            while (TextureManager.container == null)
                yield return null;
            face.texture = TextureManager.container.black.texture;
            IEnumerator ie = Program.I().texture_.LoadArtAsync(botInfo.main0, true);
            StartCoroutine(ie);
            while (ie.MoveNext())
                yield return null;
            face.texture = ie.Current as Texture2D;
            face.color = Color.white;
            enumerator = null;
            refreshed = true;
        }
    }
}
