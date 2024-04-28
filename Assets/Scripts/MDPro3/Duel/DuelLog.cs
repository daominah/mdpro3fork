using DG.Tweening;
using MDPro3;
using UnityEngine;
using UnityEngine.UI;

public class DuelLog : MonoBehaviour
{
    public static Color myColor = Color.blue;
    public static Color opColor = Color.red;
    public static Color myArrowColor = new Color(0f, 0.5f, 1f, 1f);
    public static Color opArrowColor = new Color(1f, 0.2f, 0.2f, 1f);
    public static Color myChainColor = new Color(0.2f, 0.6f, 1f, 1f);
    public static Color opChainColor = new Color(1f, 0.2f, 0.2f, 1f);
    public static Color damageColor = Color.red;
    public static Color recoverColor = new Color(0, 0.7f, 1f, 1f);

    public RectTransform baseRect;
    public ScrollRect scrollRect;
    public bool showing;
    public void Show()
    {
        showing = true;
        AudioManager.PlaySE("SE_LOG_OPEN");
        baseRect.DOAnchorPosX(-20f, 0.2f);
        scrollRect.verticalScrollbar.value = 0f;
    }

    public void Hide(bool silent = false)
    {
        showing = false;
        baseRect.DOAnchorPosX(400f, 0.2f);

        if (!silent)
            AudioManager.PlaySE("SE_LOG_CLOSE");
    }

    float fullHeight;
    public void AddLog(GameObject item, bool indent = false)
    {
        var rect = item.GetComponent<RectTransform>();
        var height = rect.rect.height;
        rect.SetParent(scrollRect.content, false);
        rect.sizeDelta = new Vector2(0, height);
        rect.anchoredPosition = new Vector2(0, -fullHeight);
        fullHeight += height;
        scrollRect.content.sizeDelta = new Vector2(0, fullHeight);

        if (indent || Program.I().ocgcore.chainSolving > 0 && rect.GetChild(1).name == "Image Side")
        {
            rect.GetChild(0).gameObject.SetActive(false);
            rect.GetChild(1).gameObject.SetActive(false);
            rect.offsetMin = new Vector2(50f, rect.offsetMin.y);
            rect.offsetMax = new Vector2(0f, rect.offsetMax.y);
        }
        if(!showing && fullHeight > scrollRect.viewport.rect.height)
            item.SetActive(false);
    }

    public void ClearLog()
    {
        for(int i = 0; i < scrollRect.content.childCount; i++)
            Destroy(scrollRect.content.GetChild(i).gameObject);
        fullHeight = 0;
    }

    private void Start()
    {
        scrollRect.verticalScrollbar.onValueChanged.AddListener(Refresh);
    }

    void Refresh(float value)
    {
        if (!showing)
            return;
        var visibleRect = GetVisibleRect();
        int stage = 0;
        bool visible = false;
        if(value > 0.5f)
        {
            for (int i = 0; i < scrollRect.content.childCount; i++)
            {
                var childRect = scrollRect.content.GetChild(i) as RectTransform;
                if (stage < 2)
                {
                    var isVisible = IsRectVisible(childRect, visibleRect);
                    if (visible != isVisible)
                    {
                        visible = isVisible;
                        stage++;
                    }
                    childRect.gameObject.SetActive(isVisible);
                }
                else
                    childRect.gameObject.SetActive(false);
            }
        }
        else
        {
            for (int i = scrollRect.content.childCount - 1; i >= 0; i--)
            {
                var childRect = scrollRect.content.GetChild(i) as RectTransform;
                if (stage < 2)
                {
                    var isVisible = IsRectVisible(childRect, visibleRect);
                    if (visible != isVisible)
                    {
                        visible = isVisible;
                        stage++;
                    }
                    childRect.gameObject.SetActive(isVisible);
                }
                else
                    childRect.gameObject.SetActive(false);
            }
        }
    }

    Rect GetVisibleRect()
    {
        Rect viewportRect = scrollRect.viewport.rect;

        float top = -scrollRect.content.anchoredPosition.y;
        float bottom = top - viewportRect.height;

        Rect visibleRect = new Rect(0f, bottom, viewportRect.width, viewportRect.height);
        return visibleRect;
    }

    bool IsRectVisible(RectTransform rectTransform, Rect visibleRect)
    {
        float top = rectTransform.anchoredPosition.y;
        float bottom = top - rectTransform.rect.height;
        return top > visibleRect.yMin && bottom < visibleRect.yMax;
    }

}
