using DG.Tweening;
using MDPro3;
using MDPro3.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class OnlineDeckViewer : Servant
{
    public ScrollRect scrollRect;
    public InputField searchDeckName;
    public InputField searchAuthorName;
    public ButtonSwitchForDeckPickup btnPickup;
    public Text textCount;

    SuperScrollView superScrollView;
    OnlineDeck.OnlineDeckData[] decks;
    public List<SuperScrollViewItemForOnlineDeckSelect> items = new List<SuperScrollViewItemForOnlineDeckSelect>();

    public override void Initialize()
    {
        haveLine = true;
        depth = 5;
        returnServant = Program.I().selectDeck;
        base.Initialize();
    }

    public override void ApplyShowArrangement(int preDepth)
    {
        base.ApplyShowArrangement(preDepth);
        RefreshList();
    }

    public override void ApplyHideArrangement(int preDepth)
    {
        base.ApplyHideArrangement(preDepth);
        DOTween.To(v => { }, 0, 0, transitionTime * 0.9f).OnComplete(() =>
        {
            btnPickup.OnSwitchOff();
            if (superScrollView != null)
                foreach (var item in superScrollView.items)
                    item.gameObject.GetComponent<SuperScrollViewItemForOnlineDeckSelect>().Dispose();
            Clear();
        });
    }

    void RefreshList()
    {
        Clear();
        btnPickup.OnSwitchOff();
        StartCoroutine(RefreshAsync());
    }

    IEnumerator RefreshAsync()
    {
        var task = OnlineDeck.FetchSimpleDeckList(10000, searchDeckName.text, searchAuthorName.text);
        yield return new WaitUntil(() => task.IsCompleted);

        if(task.Status == System.Threading.Tasks.TaskStatus.RanToCompletion)
        {
            decks = task.Result;
            if(decks == null)
            {
                MessageManager.Cast(InterString.Get("网络异常，获取在线卡组列表失败。"));
                yield break;
            }
            textCount.text = decks.Length.ToString();
            Print();
        }
        else
            MessageManager.Cast(InterString.Get("网络异常，获取在线卡组列表失败。"));
    }

    void Print()
    {
        if(superScrollView != null)
        {
            superScrollView.Clear();
        }

        var defau = 1f;
#if UNITY_ANDROID
        defau = 1.5f;
#endif
        var scale = Config.GetFloat("UIScale", defau);

        var handle = Addressables.LoadAssetAsync<GameObject>("OnlineDeckOnSelect");
        handle.Completed += (result) =>
        {
            superScrollView = new SuperScrollView
            (
            (int)Math.Floor(scrollRect.content.rect.width / (260 * scale)),
            260 * scale,
            260 * scale,
            0,
            128,
            result.Result,
            ItemOnListRefresh,
            scrollRect
            );
            List<string[]> tasks = new List<string[]>();
            foreach (var deck in decks)
            {
                //if (!deck.deckName.ToLower().Contains(searchDeckName.text.ToLower()))
                //    continue;
                //if (!deck.deckContributor.ToLower().Contains(searchAuthorName.text.ToLower()))
                //    continue;

                var task = new string[10]
                {
                    deck.deckName,
                    deck.deckContributor,
                    deck.deckId,
                    deck.deckCase == 0 ? "1080001" : deck.deckCase.ToString(),
                    deck.deckCoverCard1.ToString(),
                    deck.deckCoverCard2.ToString(),
                    deck.deckCoverCard3.ToString(),
                    deck.deckProtector == 0 ? "1070001" : deck.deckProtector.ToString(),
                    deck.deckLike.ToString(),
                    deck.lastDate
                };
                tasks.Add(task);
            }
            superScrollView.Print(tasks);
        };

    }

    void ItemOnListRefresh(string[] task, GameObject item)
    {
        var handler = item.GetComponent<SuperScrollViewItemForOnlineDeckSelect>();
        handler.deckName = task[0];
        handler.authorName = task[1];
        handler.deckId = task[2];
        handler.deckCase = int.Parse(task[3]);
        handler.card1 = int.Parse(task[4]);
        handler.card2 = int.Parse(task[5]);
        handler.card3 = int.Parse(task[6]);
        handler.protector = task[7];
        handler.like = int.Parse(task[8]);
        handler.lastDate = task[9];
        handler.Refresh();
    }

    void Clear()
    {
        decks = null;
        items.Clear();
    }


    public bool hoverOn
    {
        get { return m_hoverOn; }
        set
        {
            m_hoverOn = value;
            DeckHover();
        }
    }
    private bool m_hoverOn = false;
    public void DeckHover()
    {
        foreach (var item in items)
            item.Hover(m_hoverOn);
    }


    public void OnSearchSubmit(string value)
    {
        RefreshList();
    }
}
