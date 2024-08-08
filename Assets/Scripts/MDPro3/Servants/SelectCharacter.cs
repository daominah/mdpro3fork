using DG.Tweening;
using KonamiCommonIAB;
using MDPro3;
using MDPro3.UI;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using YgomSystem;

public class SelectCharacter : Servant
{
    public Text title;
    public ButtonList defaultSeries;
    public ButtonList defaultPlayer;

    public Text hoverName;
    public TextMeshProUGUI detailName;
    public TextMeshProUGUI detailDescription;
    public Image detailImage;
    public ScrollRect scrollRect;
    public ButtonListManager buttonListManager;

    public Characters characters;
    GameObject characterItem;
    public static string player = "0";
    string currentSerial = "00";
    List<GameObject> targetItems = new List<GameObject>();

    static List<GameObject> dm = new List<GameObject>();
    static List<GameObject> gx = new List<GameObject>();
    static List<GameObject> _5ds = new List<GameObject>();
    static List<GameObject> dsod = new List<GameObject>();
    static List<GameObject> zexal = new List<GameObject>();
    static List<GameObject> arcv = new List<GameObject>();
    static List<GameObject> vrains = new List<GameObject>();
    static List<GameObject> sevens = new List<GameObject>();
    static List<GameObject> npc = new List<GameObject>();

    Dictionary<string, List<GameObject>> pools = new Dictionary<string, List<GameObject>>()
    {
        { "00", dm},
        { "01", gx},
        { "02", _5ds},
        { "03", dsod},
        { "04", zexal},
        { "05", arcv},
        { "06", vrains},
        { "07", sevens},
        { "08", npc},
    };

    public enum Condition
    {
        Duel,
        Watch,
        Replay
    }
    public Condition condition = Condition.Duel;
    public void SwitchCondition(Condition condition)
    {
        this.condition = condition;
        switch (condition)
        {
            case Condition.Duel:
                depth = 3;
                title.text = InterString.Get("决斗角色");
                break;
            case Condition.Watch:
                depth = 3;
                title.text = InterString.Get("观战角色");
                break;
            case Condition.Replay:
                depth = 3;
                title.text = InterString.Get("回放角色");
                break;
        }
    }

    public override void Initialize()
    {
        depth = 3;
        haveLine = false;
        subBlackAlpha = 0.9f;

        base.Initialize();

        var handle = Addressables.LoadAssetAsync<Characters>("Characters");
        handle.Completed += (result) =>
        {
            characters = result.Result;
            LoadCharacters();
            Program.I().setting.RefreshCharacterName();
        };

        var handle2 = Addressables.LoadAssetAsync<GameObject>("CharacterItem");
        handle2.Completed += (result) =>
        {
            characterItem = result.Result;
        };

        Program.onScreenChanged += RefreshItemsPosition;
    }
    public void LoadCharacters()
    {
        characters.Initialize();
        characters.ChangeLanguage(Config.Get("Language", "zh-CN"));
        //foreach (var pool in pools)
        //    foreach (var c in pool.Value)
        //        c.GetComponent<CharacterItem>().Load();
    }

    public override void OnExit()
    {
        if (Program.I().currentSubServant == this)
            Program.I().ShowSubServant(Program.I().setting);
        else
            Program.I().ShiftToServant(Program.I().setting);
    }

    public void SwitchPlayer(string player)
    {
        SelectCharacter.player = player;
        if (!isShowed)
            return;

        var configCharacter = Config.Get(condition + "Character" + player, VoiceHelper.defaultCharacter);
        var configSeries = characters.GetCharacterSeries(configCharacter);
        buttonListManager.GetButtonListByName(configSeries[..2]).SelectThis();
    }

    public void ShowCharacters(string serial)
    {
        currentSerial = serial;

        if(characters == null || characterItem == null) 
            return;

        foreach (var pool in pools)
        {
            if (pool.Key != currentSerial)
            {
                foreach (var character in pool.Value)
                    character.GetComponent<CharacterItem>().Hide();
            }
            else
                targetItems = pool.Value;
        }

        if(targetItems.Count == 0)
        {
            var targetCharacters = characters.GetSeriesCharacters(currentSerial);
            int count = 0;
            for(int i = 0; i < targetCharacters.Count; i++)
            {
                if (targetCharacters[i].notReady)
                    continue;
                var item = Instantiate(characterItem);
                var mono = item.GetComponent<CharacterItem>();
                mono.id = count;
                mono.characterID = targetCharacters[i].id;
                mono.Load();
                item.transform.SetParent(scrollRect.content, false);
                targetItems.Add(item);
                count++;
            }
        }

        foreach(var item in targetItems)
        {
            item.SetActive(true);
            var mono = item.GetComponent<CharacterItem>();
            mono.Show();
            var config = Config.Get(condition + "Character" + player, VoiceHelper.defaultCharacter);
            if (mono.characterID == config)
                mono.SelectThis();
        }
        RefreshItemsPosition();
        scrollRect.content.anchoredPosition = Vector2.zero;
    }

    void RefreshItemsPosition()
    {
        var numOfEachLine = (int)(scrollRect.content.rect.width / 130);
        if (numOfEachLine < 1)
            numOfEachLine = 1;

        foreach (var item in targetItems)
        {
            var mono = item.GetComponent<CharacterItem>();
            item.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                (mono.id % numOfEachLine) * 130,
                -(int)Math.Floor(mono.id / (float)numOfEachLine) * 150);
        }
        int lines = (int)Math.Ceiling(targetItems.Count / (float)numOfEachLine);
        scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, 150 * lines);
    }

    public override void Show(int preDepth)
    {
        base.Show(preDepth);
        defaultPlayer.SelectThis();
    }

    public override void ApplyHideArrangement(int preDepth)
    {
        base.ApplyHideArrangement(preDepth);

        DOTween.To(v => { }, 0, 0, transitionTime).OnComplete(() =>
        {
            foreach(var pool in pools)
                foreach(var c in pool.Value)
                    Destroy(c);
            foreach (var pool in pools)
                pool.Value.Clear();
        });
    }

    public void SetHoverText(string hoverText)
    {
        hoverName.text = hoverText;
    }
}
