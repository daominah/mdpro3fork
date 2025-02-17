using MDPro3.Duel.YGOSharp;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YgomSystem.ElementSystem;

namespace MDPro3.UI
{
    public class UIWidget : MonoBehaviour
    {
        #region Elements

        private const string LABEL_RT_ROOT = "Root";
        private RectTransform m_Root;
        protected RectTransform Root =>
            m_Root = m_Root != null ? m_Root 
            : Manager.GetElement<RectTransform>(LABEL_RT_ROOT);

        private const string LABEL_RT_WINDOW = "Window";
        private RectTransform m_Window;
        protected RectTransform Window =>
            m_Window = m_Window != null ? m_Window 
            : Manager.GetElement<RectTransform>(LABEL_RT_WINDOW);

        protected const string LABEL_CG_BG = "BG";
        private CanvasGroup m_BG;
        protected CanvasGroup BG =>
            m_BG = m_BG != null ? m_BG
            : Manager.GetElement<CanvasGroup>(LABEL_CG_BG);

        private const string LABEL_CG_BUTTONGROUP = "ButtonGroup";
        private CanvasGroup m_ButtonGroup;
        protected CanvasGroup ButtonGroup =>
            m_ButtonGroup = m_ButtonGroup != null ? m_ButtonGroup
            : Manager.GetElement<CanvasGroup>(LABEL_CG_BUTTONGROUP);

        #endregion

        private ElementObjectManager _manager;
        protected ElementObjectManager Manager
        {
            get 
            { 
                if (_manager == null)
                    _manager = GetComponent<ElementObjectManager>();
                return _manager; 
            }
        }

        private CanvasGroup _cg;
        protected CanvasGroup Cg
        {
            get
            {
                if (_cg == null)
                    _cg = GetComponent<CanvasGroup>();
                return _cg;
            }
        }

        protected virtual void Awake()
        {

        }

        #region Input Response
        [HideInInspector] public bool responseInput;
        public void SetResponse(bool response)
        {
            responseInput = response;
        }

        protected Selectable defaultSelectable;
        public virtual void SelectDefaultSelectable()
        {
            if(defaultSelectable != null)
                defaultSelectable.Select();
        }

        #endregion
    }
}