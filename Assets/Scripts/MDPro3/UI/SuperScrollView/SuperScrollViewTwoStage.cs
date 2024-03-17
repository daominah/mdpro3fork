using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MDPro3.UI
{
    public class SuperScrollViewTwoStage : SuperScrollView
    {
        float stageRange;
        public int selected = -1;
        public SuperScrollViewTwoStage
            (
            int numOfEachLine,
            float cellX,
            float cellY,
            float extraForHead,
            float extraForEnd,
            GameObject gameObject,
            Action<string[], GameObject> itemOnListRefresh,
            ScrollRect scrollView,
            float stageRange
            ) : base(numOfEachLine, cellX, cellY, extraForHead, extraForEnd, gameObject, itemOnListRefresh, scrollView)
        {
            this.stageRange = stageRange;
        }

        public override void ItemRefreshPositon(int i)
        {
            if (items[i].gameObject == null)
                return;
            int x = i % numOfEachLine;
            int y = (int)Math.Floor(i / (float)numOfEachLine);
            int stage = 0;
            var handler = items[i].gameObject.GetComponent<SuperScrollViewItemTwoStage>();
            handler.startX = -stageRange;
            if (i == selected)
            {
                stage = 1;
                handler.stage = 1;
            }
            else
            {
                stage = -1;
                handler.stage = -1;
            }
            items[i].gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2
                (
                -((float)numOfEachLine / 2) * cellX + (x + 0.5f) * cellX + stage * stageRange,
                -y * cellY
                );
            handler.id = i;
            itemOnListRefresh(items[i].args, items[i].gameObject);
        }


    }
}
