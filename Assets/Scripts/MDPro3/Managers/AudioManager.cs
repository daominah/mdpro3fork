using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MDPro3
{
    public class AudioManager : Manager
    {
        public AudioSource seR;
        public AudioSource bgmR;
        public AudioSource voiceR;

        static AudioSource se;
        static AudioSource bgm;
        static AudioSource voice;
        public override void Initialize()
        {
            base.Initialize();
            se = seR;
            bgm = bgmR;
            voice = voiceR;
            AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;

            PlayBGM("BGM_MENU_01");
        }

        struct LastSE
        {
            public float time;
            public string seName;
        }

        static LastSE lastSE = new LastSE();
        public static string nextMuteSE;
        public static void PlaySE(string path, float volumeScale = 1)
        {
            if (string.IsNullOrEmpty(path))
                return;
            if (path == nextMuteSE)
            {
                nextMuteSE = string.Empty;
                return;
            }
            if (lastSE.time > 0)
                if (lastSE.seName == path && Time.time - lastSE.time < 0.1f)
                    return;
            lastSE.time = Time.time;
            lastSE.seName = path;

            var handle = Addressables.LoadAssetAsync<AudioClip>(path);
            handle.Completed += (result) =>
            {
                se.PlayOneShot(result.Result, volumeScale);
            };
        }

        public static IEnumerator PlaySEGroup(List<string> ses, float volumeScale = 1)
        {
            foreach (string s in ses)
            {
                var handle = Addressables.LoadAssetAsync<AudioClip>(s);
                while (!handle.IsDone)
                    yield return null;
                se.PlayOneShot(handle.Result, volumeScale);
                yield return new WaitForSeconds(handle.Result.length * 0.5f);
            }
        }

        public void PlayShuffleSE()
        {
            List<string> ses = new List<string>()
        {
            "SE_CARD_MOVE_01",
            "SE_CARD_MOVE_02",
            "SE_CARD_MOVE_03",
            "SE_CARD_MOVE_04"
        };
            StartCoroutine(PlaySEGroup(ses));
        }

        public static void PlaySEClip(AudioClip clip, float volumeScale = 1)
        {
            se.PlayOneShot(clip, volumeScale);
        }
        public static void ResetSESource()
        {
            se.gameObject.SetActive(false);
            se.gameObject.SetActive(true);
        }

        static int bgmState = 0;

        static int GetFieldID(string fieldName)
        {
            return int.Parse(fieldName.Substring(4, 3));
        }

        static List<int> commonBgms = new List<int>()
    {
        1,
        2,
        3,
        4,
        6,
        7,
        8,
        10,
        11,
        12,
        13,
        15,
        16
    };
        static int currentBgmID = 1;
        static List<int> GetBgmByField(int fieldID)
        {
            var returnValue = new List<int>();
            if (fieldID == 7)
                returnValue.Add(5);
            else if (fieldID == 16 || fieldID == 17)
                returnValue.Add(9);
            else if (fieldID == 22)
                returnValue.Add(14);
            else
                return commonBgms;
            return returnValue;
        }

        public static void PlayBGMNormal(string filedName)
        {
            bgmState = 0;
            var fieldID = GetFieldID(filedName);
            if (fieldID == 8)
                PlayBGM("BGM_DUEL_EX_02_NORMAL");
            else if (fieldID == 13)
                PlayBGM("BGM_DUEL_EX_03_NORMAL");
            else
            {
                var list = GetBgmByField(fieldID);
                currentBgmID = list[Random.Range(0, list.Count)];
                PlayBGM("BGM_DUEL_NORMAL_" + currentBgmID.ToString("D2"));
            }
        }
        public static void PlayBGMKeyCard(string filedName)
        {
            if (bgmState > 0)
                return;
            bgmState = 1;
            var fieldID = GetFieldID(filedName);
            if (fieldID == 8)
                PlayBGM("BGM_DUEL_EX_02_KEYCARD");
            else if (fieldID == 13)
                PlayBGM("BGM_DUEL_EX_03_KEYCARD");
            else
                PlayBGM("BGM_DUEL_KEYCARD_" + currentBgmID.ToString("D2"));
        }
        public static void PlayBGMClimax(string filedName)
        {
            if (bgmState == 2)
                return;
            bgmState = 2;
            var fieldID = GetFieldID(filedName);
            if (fieldID == 8)
                PlayBGM("BGM_DUEL_EX_02_CLIMAX");
            else if (fieldID == 13)
                PlayBGM("BGM_DUEL_EX_03_CLIMAX");
            else
                PlayBGM("BGM_DUEL_CLIMAX_" + currentBgmID.ToString("D2"));
        }

        static float currentBGMScale = 1f;
        public static void PlayBGM(string path, float volumeScale = 1f)
        {
            currentBGMScale = volumeScale;
            var handle = Addressables.LoadAssetAsync<AudioClip>(path);
            handle.Completed += (result) =>
            {
                var volume = Program.I().setting.bgmVol.value * currentBGMScale;
                DOTween.To(() => volume, x => bgm.volume = x, 0, 0.2f).OnComplete(() =>
                {
                    SetCurrentBGM(path, result.Result.length);
                    bgm.volume = volume;
                    bgm.clip = result.Result;
                    bgm.time = 0;
                    bgm.Play();
                });
            };
        }

        public static void StopBGM()
        {
            var volume = bgm.volume;
            DOTween.To(() => volume, x => bgm.volume = x, 0, 0.5f).OnComplete(() =>
            {
                bgm.Stop();
                bgm.volume = volume;
            });
        }
        public static void PlayRandomKeyCardBGM()
        {
            var bgm = "BGM_DUEL_KEYCARD_" + commonBgms[Random.Range(0, commonBgms.Count)].ToString("D2");
            PlayBGM(bgm);
        }

        static string currentBGM = string.Empty;
        static float loopStart = 0;
        static float loopEnd = 10;
        static void SetCurrentBGM(string bgm, float bgmLength)
        {
            currentBGM = bgm;
            bool found = false;
            foreach (var loop in loops)
            {
                if (loop.name == currentBGM)
                {
                    found = true;
                    loopStart = loop.startTime;
                    loopEnd = loop.endTime;
                    break;
                }
            }
            if (!found)
            {
                loopStart = 0;
                loopEnd = bgmLength - 1;
            }
        }

        private void Update()
        {
            if (bgm == null)
                return;
            if (bgm.time > loopEnd)
                bgm.time = loopStart;
            if (Input.GetKeyDown(KeyCode.T))
                bgm.time = loopEnd - 5;
        }

        public static void PlayVoice(string path)
        {

        }

        public static void SetSeVol(float vol)
        {
            se.volume = vol;
        }
        public static void SetBGMVol(float vol)
        {
            bgm.volume = vol * currentBGMScale;
        }
        public static void SetVoiceVol(float vol)
        {
            voice.volume = vol;
        }

        void OnAudioConfigurationChanged(bool deviceWasChanged)
        {
#if !UNITY_EDITOR && UNITY_ANDROID
        if (deviceWasChanged)
        {
            AudioConfiguration config = AudioSettings.GetConfiguration();
            AudioSettings.Reset(config);
        }
        bgm.Play();
#endif
        }

        public struct BGMLoop
        {
            public string name;
            public float startTime;
            public float endTime;
        }

        readonly static List<BGMLoop> loops = new List<BGMLoop>
    {
        new BGMLoop{name = "BGM_MENU_01", startTime = 12.433f, endTime = 120 + 31.100f },
        new BGMLoop{name = "BGM_MENU_02", startTime = 15.687f, endTime = 120 + 2.354f },
        new BGMLoop{name = "BGM_DUEL_NORMAL_01", startTime = 9.600f, endTime = 60 + 55.200f },
        new BGMLoop{name = "BGM_DUEL_NORMAL_02", startTime = 16.500f, endTime = 60 + 48.500f },
        new BGMLoop{name = "BGM_DUEL_NORMAL_03", startTime = 5.727f, endTime = 120 + 11.444f },
        new BGMLoop{name = "BGM_DUEL_NORMAL_04", startTime = 13.518f, endTime = 60 + 57.300f },
        new BGMLoop{name = "BGM_DUEL_NORMAL_05", startTime = 11.208f, endTime = 120 + 22.875f },
        new BGMLoop{name = "BGM_DUEL_NORMAL_06", startTime = 9.527f, endTime = 60 + 41.906f },
        new BGMLoop{name = "BGM_DUEL_NORMAL_07", startTime = 17.456f, endTime = 120 +9.247f },
        new BGMLoop{name = "BGM_DUEL_NORMAL_08", startTime = 18.400f, endTime = 120 + 12.400f },
        new BGMLoop{name = "BGM_DUEL_NORMAL_09", startTime = 6.200f, endTime = 60 +51.400f },
        new BGMLoop{name = "BGM_DUEL_NORMAL_10", startTime = 9.989f, endTime = 60 + 51.636f },
        new BGMLoop{name = "BGM_DUEL_NORMAL_11", startTime = 2.378f, endTime = 60 +29.650f },
        new BGMLoop{name = "BGM_DUEL_NORMAL_12", startTime = 7.500f, endTime = 60 +47.800f },
        new BGMLoop{name = "BGM_DUEL_NORMAL_13", startTime = 7.433f, endTime = 60 + 54.741f },
        new BGMLoop{name = "BGM_DUEL_NORMAL_14", startTime = 5.538f, endTime = 60 +34.142f },
        new BGMLoop{name = "BGM_DUEL_NORMAL_15", startTime = 8.455f, endTime = 60 +34.855f },
        new BGMLoop{name = "BGM_DUEL_NORMAL_16", startTime = 14.440f, endTime = 60 + 44.440f },
        new BGMLoop{name = "BGM_DUEL_KEYCARD_01", startTime = 11.744f, endTime = 60 + 49.390f },
        new BGMLoop{name = "BGM_DUEL_KEYCARD_02", startTime = 10.500f, endTime = 60 + 46.500f },
        new BGMLoop{name = "BGM_DUEL_KEYCARD_03", startTime = 13.697f, endTime = 60 + 38.150f },
        new BGMLoop{name = "BGM_DUEL_KEYCARD_04", startTime = 7.032f, endTime = 60 + 49.888f },
        new BGMLoop{name = "BGM_DUEL_KEYCARD_05", startTime = 12.495f, endTime = 60 +23.079f },
        new BGMLoop{name = "BGM_DUEL_KEYCARD_06", startTime = 11.400f, endTime = 60 + 38.400f },
        new BGMLoop{name = "BGM_DUEL_KEYCARD_07", startTime = 6.518f, endTime = 60 + 24.928f },
        new BGMLoop{name = "BGM_DUEL_KEYCARD_08", startTime = 13.783f, endTime = 60 + 57.727f },
        new BGMLoop{name = "BGM_DUEL_KEYCARD_09", startTime = 3.800f, endTime = 60 + 20.300f },
        new BGMLoop{name = "BGM_DUEL_KEYCARD_10", startTime = 17.599f, endTime = 60 + 40.508f },
        new BGMLoop{name = "BGM_DUEL_KEYCARD_11", startTime = 11.738f, endTime = 60 + 57.104f },
        new BGMLoop{name = "BGM_DUEL_KEYCARD_12", startTime = 13.630f, endTime = 60 + 45.684f },
        new BGMLoop{name = "BGM_DUEL_KEYCARD_13", startTime = 18.519f, endTime = 60 + 55.734f },
        new BGMLoop{name = "BGM_DUEL_KEYCARD_14", startTime = 2.269f, endTime = 60 + 35.830f },
        new BGMLoop{name = "BGM_DUEL_KEYCARD_15", startTime = 11.369f, endTime = 60 + 41.369f },
        new BGMLoop{name = "BGM_DUEL_KEYCARD_16", startTime = 6.348f, endTime = 60 + 36.151f },
        new BGMLoop{name = "BGM_DUEL_CLIMAX_01", startTime = 6.300f, endTime = 60 + 37.800f },
        new BGMLoop{name = "BGM_DUEL_CLIMAX_02", startTime = 12.883f, endTime = 60 + 53.958f },
        new BGMLoop{name = "BGM_DUEL_CLIMAX_03", startTime = 12.579f, endTime = 120 + 7.444f },
        new BGMLoop{name = "BGM_DUEL_CLIMAX_04", startTime = 3.325f, endTime = 60 + 31.047f },
        new BGMLoop{name = "BGM_DUEL_CLIMAX_05", startTime = 5.424f, endTime = 60 + 37.188f },
        new BGMLoop{name = "BGM_DUEL_CLIMAX_06", startTime = 5.896f, endTime = 60 + 26.184f },
        new BGMLoop{name = "BGM_DUEL_CLIMAX_07", startTime = 11.500f, endTime = 60 + 31.500f },
        new BGMLoop{name = "BGM_DUEL_CLIMAX_08", startTime = 15.547f, endTime = 60 + 48.505f },
        new BGMLoop{name = "BGM_DUEL_CLIMAX_09", startTime = 6.300f, endTime = 60 + 28.800f },
        new BGMLoop{name = "BGM_DUEL_CLIMAX_10", startTime = 2.500f, endTime = 60 + 34.500f },
        new BGMLoop{name = "BGM_DUEL_CLIMAX_11", startTime = 13.223f, endTime = 60 + 43.955f },
        new BGMLoop{name = "BGM_DUEL_CLIMAX_12", startTime = 6.448f, endTime = 60 + 34.252f },
        new BGMLoop{name = "BGM_DUEL_CLIMAX_13", startTime = 5.637f, endTime = 60 + 50.429f },
        new BGMLoop{name = "BGM_DUEL_CLIMAX_14", startTime = 12.169f, endTime = 60 + 48.165f },
        new BGMLoop{name = "BGM_DUEL_CLIMAX_15", startTime = 7.056f, endTime = 60 + 39.847f },
        new BGMLoop{name = "BGM_DUEL_CLIMAX_16", startTime = 9.606f, endTime = 60 + 28.067f },
        new BGMLoop{name = "BGM_DUEL_EX_01", startTime = 21.014f, endTime = 60 + 57.026f },
        new BGMLoop{name = "BGM_DUEL_EX_02_NORMAL", startTime = 2.466f, endTime = 60 + 47.193f },
        new BGMLoop{name = "BGM_DUEL_EX_02_KEYCARD", startTime = 6.941f, endTime = 60 + 46.766f },
        new BGMLoop{name = "BGM_DUEL_EX_02_CLIMAX", startTime = 2.346f, endTime = 60 + 43.210f },
        new BGMLoop{name = "BGM_DUEL_EX_03_NORMAL", startTime = 11.478f, endTime = 60 + 56.473f },
        new BGMLoop{name = "BGM_DUEL_EX_03_KEYCARD", startTime = 12.463f, endTime = 60 + 46.098f },
        new BGMLoop{name = "BGM_DUEL_EX_03_CLIMAX", startTime = 1.815f, endTime = 120 + 8.792f },
        new BGMLoop{name = "BGM_OUT_TUTORIAL_2", startTime = 7.480f, endTime = 60 + 22.480f },
    };
    }
}
