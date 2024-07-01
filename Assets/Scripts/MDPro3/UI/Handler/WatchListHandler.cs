using MDPro3.Net;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using UnityEngine.UIElements;
using YgomGame.WCS.Portal;

namespace MDPro3.UI
{
    public class WatchListHandler : MonoBehaviour
    {
        public ScrollRect scrollRect;
        public InputField inputSearch;

        SuperScrollView superScrollView;

        List<MyCardRoom> rooms;
        List<string[]> tasks = new List<string[]>();

        private void OnEnable()
        {
            Print();
        }

        private void OnDisable()
        {
            Clear();
        }

        public List<string[]> GetSearchedTasks()
        {
            var returnValue = new List<string[]>();
            foreach (var task in tasks)
                if (task[2].Contains(inputSearch.text) || task[3].Contains(inputSearch.text))
                    returnValue.Add(task);
            return returnValue;
        }

        public void SetRooms(List<MyCardRoom> rooms)
        {
            this.rooms = rooms;
            tasks.Clear();
            foreach (var room in rooms)
            {
                var task = new string[18]
                {
                    room.id,
                    room.title,
                    room.users[0].username,
                    room.users[1].username,
                    room.arena,

                    room.options.lflist.ToString(),
                    room.options.rule.ToString(),
                    room.options.mode.ToString(),
                    room.options.duel_rule.ToString(),
                    room.options.no_check_deck ? "T" : "F",
                    room.options.no_shuffle_deck ? "T" : "F",
                    room.options.start_lp.ToString(),
                    room.options.start_hand.ToString(),
                    room.options.draw_count.ToString(),
                    room.options.time_limit.ToString(),
                    room.options.no_watch ? "T" : "F",
                    room.options.auto_death ? "T" : "F",
                    room.options.replay_mode.ToString()
                };
                tasks.Add(task);
            }
            if(gameObject.activeInHierarchy)
                Print();
        }

        public void CreateRoom(MyCardRoom room)
        {
            var task = new string[18]
            {
                    room.id,
                    room.title,
                    room.users[0].username,
                    room.users[1].username,
                    room.arena,

                    room.options.lflist.ToString(),
                    room.options.rule.ToString(),
                    room.options.mode.ToString(),
                    room.options.duel_rule.ToString(),
                    room.options.no_check_deck ? "T" : "F",
                    room.options.no_shuffle_deck ? "T" : "F",
                    room.options.start_lp.ToString(),
                    room.options.start_hand.ToString(),
                    room.options.draw_count.ToString(),
                    room.options.time_limit.ToString(),
                    room.options.no_watch ? "T" : "F",
                    room.options.auto_death ? "T" : "F",
                    room.options.replay_mode.ToString()
            };
            tasks.Add(task);
            if (gameObject.activeInHierarchy)
                superScrollView.UpdateTasks(GetSearchedTasks());
        }
        public void UpdateRoom(MyCardRoom room)
        {
            var task = new string[18]
            {
                    room.id,
                    room.title,
                    room.users[0].username,
                    room.users[1].username,
                    room.arena,

                    room.options.lflist.ToString(),
                    room.options.rule.ToString(),
                    room.options.mode.ToString(),
                    room.options.duel_rule.ToString(),
                    room.options.no_check_deck ? "T" : "F",
                    room.options.no_shuffle_deck ? "T" : "F",
                    room.options.start_lp.ToString(),
                    room.options.start_hand.ToString(),
                    room.options.draw_count.ToString(),
                    room.options.time_limit.ToString(),
                    room.options.no_watch ? "T" : "F",
                    room.options.auto_death ? "T" : "F",
                    room.options.replay_mode.ToString()
            };
            foreach(var t in tasks)
                if (t[0] == task[0])
                    for (var i = 1; i < task.Length; i++)
                        t[i] = task[i];
            if (gameObject.activeInHierarchy)
                superScrollView.UpdateTasks(GetSearchedTasks());
        }
        public void DeleteRoom(string roomId)
        {
            foreach (var room in rooms)
            {
                if (room.id == roomId)
                {
                    rooms.Remove(room);
                    break;
                }
            }
            if (gameObject.activeInHierarchy)
                superScrollView.UpdateTasks(GetSearchedTasks());
        }

        public void Print()
        {
            if (!gameObject.activeInHierarchy)
                return;
            superScrollView?.Clear();
            var handle = Addressables.LoadAssetAsync<GameObject>("ButtonWatchList");
            handle.Completed += (result) =>
            {

                superScrollView = new SuperScrollView
                (
                    (int)Math.Floor((scrollRect.content.rect.width - 30f) / 300f),
                    300,
                    140,
                    0,
                    0,
                    result.Result,
                    ItemOnListRefresh,
                    scrollRect
                    );
                superScrollView.Print(GetSearchedTasks());
            };
        }

        void ItemOnListRefresh(string[] task, GameObject item)
        {
            var handler = item.GetComponent<SuperScrollViewItemForWatchList>();
            handler.roomId = task[0];
            handler.roomTitile = task[1];
            handler.player0Name = task[2];
            handler.player1Name = task[3];
            handler.arena = task[4];

            handler.options.lflist = int.Parse(task[5]);
            handler.options.rule = int.Parse(task[6]);
            handler.options.mode = int.Parse(task[7]);
            handler.options.duel_rule = int.Parse(task[8]);
            handler.options.no_check_deck = task[9] == "T";
            handler.options.no_shuffle_deck = task[10] == "T";
            handler.options.start_lp = int.Parse(task[11]);
            handler.options.start_hand = int.Parse(task[12]);
            handler.options.draw_count = int.Parse(task[13]);
            handler.options.time_limit = int.Parse(task[14]);
            handler.options.no_watch = task[15] == "T";
            handler.options.auto_death = task[16] == "T";
            handler.options.replay_mode = int.Parse(task[17]);

            handler.Refresh();
        }

        public void Clear()
        {
            superScrollView?.Clear();
        }
    }
}

