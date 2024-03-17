using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace MDPro3.UI
{
    public class SuperScrollView
    {
        public int maxShowLines = 0;

        public int numOfEachLine;
        public float cellX;
        public float cellY;
        public float extraForHead;
        public float extraForEnd;

        public GameObject itemObject;
        public Action<string[], GameObject> itemOnListRefresh;
        public List<Item> items = new List<Item>();
        public ScrollRect scrollView;

        public class Item
        {
            public string[] args;
            public GameObject gameObject;
        }

        public SuperScrollView
            (
            int numOfEachLine,
            float cellX,
            float cellY,
            float extraForHead,
            float extraForEnd,
            GameObject itemObject,
            Action<string[], GameObject> itemOnListRefresh,
            ScrollRect scrollView
            )
        {
            this.numOfEachLine = numOfEachLine;
            this.cellX = cellX;
            this.cellY = cellY;
            this.extraForHead = extraForHead;
            this.extraForEnd = extraForEnd;
            this.itemObject = itemObject;
            this.itemOnListRefresh = itemOnListRefresh;
            this.scrollView = scrollView;
            Install();
        }

        public virtual void Install()
        {
            scrollView.verticalScrollbar.onValueChanged.AddListener(OnScrollBarChange);
        }

        public void Clear()
        {
            foreach (var item in items)
                UnityEngine.Object.Destroy(item.gameObject);
            items.Clear();
            scrollView.content.sizeDelta = new Vector2(0, 0);
        }

        public void Print(List<string[]> tasks)
        {
            Clear();
            for (int i = 0; i < tasks.Count; i++)
            {
                var it = new Item();
                it.args = tasks[i];
                it.gameObject = null;
                items.Add(it);
            }
            scrollView.content.sizeDelta = new Vector2(0, (int)Math.Ceiling((float)items.Count / numOfEachLine) * cellY + extraForHead + extraForEnd);
            float viewHeight = scrollView.GetComponent<RectTransform>().rect.height;
            maxShowLines = (int)Math.Ceiling(viewHeight / cellY);
            maxShow = numOfEachLine * (maxShowLines + 1);
            if (maxShow > tasks.Count)
                maxShow = tasks.Count;
            for (int i = 0; i < maxShow; i++)
            {
                CreateItem(i);
            }
            ToTop();
        }

        void CreateItem(int i)
        {
            if (items[i].gameObject == null)
            {
                items[i].gameObject = UnityEngine.Object.Instantiate(itemObject);
                items[i].gameObject.transform.SetParent(scrollView.content, false);
                ItemRefreshPositon(i);
            }
        }

        public virtual void ItemRefreshPositon(int i)
        {
            if (items[i].gameObject == null)
                return;
            int x = i % numOfEachLine;
            int y = (int)Math.Floor(i / (float)numOfEachLine);
            items[i].gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2
                (
                -((float)numOfEachLine / 2) * cellX + (x + 0.5f) * cellX,
                -y * cellY - extraForHead
                );
            var handler = items[i].gameObject.GetComponent<SuperScrollViewItem>();
            handler.id = i;
            itemOnListRefresh(items[i].args, items[i].gameObject);
        }

        public void ToTop()
        {
            scrollView.content.anchoredPosition = new Vector2(0, 0);
            hideLines = 0;
        }

        int maxShow;
        int hideLines = 0;
        public void OnScrollBarChange(float move)
        {
            if (items.Count == 0)
                return;

            int newHideLines = (int)math.floor(math.abs(scrollView.content.anchoredPosition.y) / cellY);
            if (hideLines == newHideLines)
                return;
            hideLines = newHideLines;

            int start = newHideLines * numOfEachLine;
            int end = start + maxShow;
            if (end > items.Count)
            {
                start -= end - items.Count;
                end = items.Count;
            }
            List<GameObject> objects = new List<GameObject>();
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].gameObject == null)
                    continue;
                if (i >= start && i < end)
                {
                }
                else
                {
                    objects.Add(items[i].gameObject);
                    items[i].gameObject = null;
                }
            }
            for (int i = 0; i < items.Count; i++)
            {
                if (i >= start && i < end && items[i].gameObject == null)
                {
                    items[i].gameObject = objects[0];
                    objects.RemoveAt(0);
                    ItemRefreshPositon(i);
                }
            }
        }
    }
}
