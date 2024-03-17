using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MDPro3.UI 
{
    public class PopupRockPaperScissors : Popup
    {
        [Header("Popup Rock Paper Scissors Reference")]
        public Button rock;
        public Button paper;
        public Button scissors;

        private void Start()
        {
            rock.onClick.AddListener(() => { TcpHelper.CtosMessage_HandResult(2); Hide(); });
            paper.onClick.AddListener(() => { TcpHelper.CtosMessage_HandResult(3); Hide(); });
            scissors.onClick.AddListener(() => { TcpHelper.CtosMessage_HandResult(1); Hide(); });
            StartCoroutine(LoadAsync());
        }

        IEnumerator LoadAsync()
        {
            var ie = TextureManager.LoadFromFileAsync("DIY/Rock.png");
            StartCoroutine(ie);
            while (ie.MoveNext())
                yield return null;
            rock.GetComponent<RawImage>().texture = ie.Current;
            rock.GetComponent<RawImage>().color = Color.white;

            ie = TextureManager.LoadFromFileAsync("DIY/Paper.png");
            StartCoroutine(ie);
            while (ie.MoveNext())
                yield return null;
            paper.GetComponent<RawImage>().texture = ie.Current;
            paper.GetComponent<RawImage>().color = Color.white;

            ie = TextureManager.LoadFromFileAsync("DIY/Scissors.png");
            StartCoroutine(ie);
            while (ie.MoveNext())
                yield return null;
            scissors.GetComponent<RawImage>().texture = ie.Current;
            scissors.GetComponent<RawImage>().color = Color.white;
        }
    }
}

