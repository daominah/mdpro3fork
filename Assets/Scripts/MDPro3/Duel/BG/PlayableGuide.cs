using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MDPro3.Duel
{
    public class PlayableGuide : MonoBehaviour
    {
        private const string LABEL_TRIGGER_APPEAR = "Apper";
        private const string LABEL_TRIGGER_CHANGE = "Change"; // With Highlight
        private const string LABEL_TRIGGER_CHANGE2 = "Change2"; // No Highlight
        private const string LABEL_TRIGGER_NOTICE = "Notice";
        private const string LABEL_TRIGGER_OUT = "Out";
        private const string LABEL_TRIGGER_END = "End";

        private string field0Name;
        private string field1Name;
        private Animator animator0;
        private Animator animator1;
        private Coroutine coroutine0;
        private Coroutine coroutine1;
        private bool guide0Showing;
        private bool guide1Showing;
        private float guide0ShowLasted;
        private float guide1ShowLasted;

        private readonly float playTime = 1f;
        private readonly float noticeTime = 10f;

        private float lumiHeight = 0.11f;

        public bool loaded;

        private void Awake()
        {
            transform.SetParent(Program.instance.container_3D, false);
        }

        private void Update()
        {
            if (!loaded) return;

            if (guide0Showing)
                guide0ShowLasted += Time.unscaledDeltaTime;
            else
                guide0ShowLasted = 0f;

            if(guide1Showing)
                guide1ShowLasted += Time.unscaledDeltaTime;
            else
                guide1ShowLasted = 0f;

            if (guide0ShowLasted > noticeTime && coroutine0 == null)
            {
                Notice(true);
                guide0ShowLasted = 0f;
            }

            if(guide1ShowLasted > noticeTime && coroutine1 == null)
            {
                Notice(false);
                guide1ShowLasted = 0f;
            }
        }

        public void SetHeight(float height)
        {
            if (loaded)
            {
                animator0.transform.GetChild(2).localPosition = new Vector3(0f, height, 0);
                animator1.transform.GetChild(2).localPosition = new Vector3(0f, height, 0);
            }
            else
            {
                lumiHeight = height;
            }
        }

        public void Load(string field0Name, string field1Name)
        {
            this.field0Name = field0Name;
            this.field1Name = field1Name;
            loaded = false;
            StartCoroutine(LoadAsync());
        }

        private IEnumerator LoadAsync()
        {
            IEnumerator<GameObject> ie;
            if (field0Name.StartsWith("Mat_013"))
                ie = ABLoader.LoadFromFileAsync("MasterDuel/BG/Timer/PlayableGuide_c001_near_Mat13", true);
            else
                ie = ABLoader.LoadFromFileAsync("MasterDuel/BG/Timer/PlayableGuide_c001_near", true);
            while (ie.MoveNext())
                yield return null;

            animator0 = ie.Current.GetComponent<Animator>();
            animator0.transform.SetParent(transform, false);
            animator0.SetTrigger(LABEL_TRIGGER_OUT);

            if (field1Name.StartsWith("Mat_013"))
                ie = ABLoader.LoadFromFileAsync("MasterDuel/BG/Timer/PlayableGuide_c001_far_Mat13", true);
            else
                ie = ABLoader.LoadFromFileAsync("MasterDuel/BG/Timer/PlayableGuide_c001_far", true);
            while (ie.MoveNext())
                yield return null;

            animator1 = ie.Current.GetComponent<Animator>();
            animator1.transform.SetParent(transform, false);
            animator1.SetTrigger(LABEL_TRIGGER_OUT);

            animator0.transform.GetChild(2).localPosition = new Vector3(0f, lumiHeight, 0);
            animator1.transform.GetChild(2).localPosition = new Vector3(0f, lumiHeight, 0);
            loaded = true;
        }

        public void Set(bool me)
        {
            if (me)
            {
                if (!guide0Showing)
                {
                    if(coroutine0 != null)
                        guide0Showing = true;
                    else
                        coroutine0 = StartCoroutine(PlayAnimator0Async(true));
                }

                if (guide1Showing)
                {
                    if (coroutine1 != null)
                        guide1Showing = false;
                    else
                        coroutine1 = StartCoroutine(PlayAnimator1Async(false));
                }
            }
            else
            {
                if (guide0Showing)
                {
                    if(coroutine0 != null)
                        guide0Showing = false;
                    else
                        coroutine0 = StartCoroutine(PlayAnimator0Async(false));
                }

                if (!guide1Showing)
                {
                    if (coroutine1 != null)
                        guide1Showing = true;
                    else
                        coroutine1 = StartCoroutine(PlayAnimator1Async(true));
                }
            }
        }

        public void End()
        {
            animator0.SetTrigger(LABEL_TRIGGER_END);
            animator1.SetTrigger(LABEL_TRIGGER_END);
        }

        private void Notice(bool me)
        {
            if (me)
            {
                animator0.SetTrigger(LABEL_TRIGGER_NOTICE);
            }
            else
            {
                animator1.SetTrigger(LABEL_TRIGGER_NOTICE);
            }
        }

        private IEnumerator PlayAnimator0Async(bool show)
        {
            guide0Showing = show;
            if (show)
                animator0.SetTrigger(LABEL_TRIGGER_CHANGE);
            else
                animator0.SetTrigger(LABEL_TRIGGER_OUT);

            yield return new WaitForSeconds(playTime);
            if(guide0Showing != show)
                coroutine0 = StartCoroutine(PlayAnimator0Async(guide0Showing));
            else
                coroutine0 = null;
        }

        private IEnumerator PlayAnimator1Async(bool show) 
        {
            guide1Showing = show;
            if (show)
                animator1.SetTrigger(LABEL_TRIGGER_CHANGE);
            else
                animator1.SetTrigger(LABEL_TRIGGER_OUT);

            yield return new WaitForSeconds(playTime);
            if (guide1Showing != show)
                coroutine1 = StartCoroutine(PlayAnimator1Async(guide1Showing));
            else
                coroutine1 = null;
        }

    }
}