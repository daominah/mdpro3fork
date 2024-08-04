using Percy;
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

        bool diyDeck;
        public override void OnSelected()
        {
            base.OnSelected();
            Program.I().solo.superScrollView.selected = id;
            Program.I().solo.description.text = botInfo.desc;
            Program.I().solo.description.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            if(diyDeck)
                Program.I().solo.btnDeck.SetActive(true);
            else
                Program.I().solo.btnDeck.SetActive(false);
        }

        public override void Refresh()
        {
            base.Refresh();
            title.text = botInfo.name;
            diyDeck = botInfo.command.Contains("Lucky");
            action = () =>
            {
                if(Solo.condition == Solo.Condition.ForSolo)
                    Program.I().solo.StartAIForSolo(id, diyDeck);
                else
                    Program.I().solo.StartAIForRoom(id, diyDeck);
            };
        }

        public override IEnumerator RefreshAsync()
        {
            while (TextureManager.container == null)
                yield return null;

            face.texture = TextureManager.container.black.texture;

            var task = TextureManager.LoadArtAsync(botInfo.main0, true);
            while (!task.IsCompleted)
                yield return null;
            face.color = Color.white;
            face.texture = task.Result;

            enumerator = null;
            refreshed = true;
        }
    }
}
