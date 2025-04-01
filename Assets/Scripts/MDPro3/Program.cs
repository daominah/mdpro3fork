using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MDPro3.Duel.YGOSharp;
using UnityEngine.EventSystems;
using UnityEngine.AddressableAssets;
using MDPro3.Net;
using MDPro3.UI;
using MDPro3.Servant;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MDPro3
{
    public class Program : MonoBehaviour
    {

        [Header("Public References")]
        public Transform container_3D;
        public Transform container_2D;
        public CardRenderer cardRenderer;

        [Header("Manager")]
        public CameraManager camera_;
        public UIManager ui_;
        public BackgroundManager background_;
        public AudioManager audio_;
        public TextureManager texture_;
        public MessageManager message_;
        public TimelineManager timeline_;

        [Header("Servants")]
        public Servant.MainMenu menu;
        public SoloSelector solo;
        public OnlineServant online;
        public PuzzleSelector puzzle;
        public ReplaySelector replay;
        public CutinViewer cutin;
        public MateViewer mate;
        public DeckSelector deckSelector;
        public SettingServant setting;
        public Appearance appearance;
        public CharacterSelector character;
        public OcgCore ocgcore;
        public RoomServant room;
        public DeckEditor deckEditor;
        public OnlineDeckViewer onlineDeckViewer;
        public DeckBrowser deckBrowser;

        [Header("SidePanels")]

        [HideInInspector]
        public Servant.Servant currentServant;
        [HideInInspector]
        public Servant.Servant currentSubServant;
        [HideInInspector]
        public int depth;

        #region Const
        public static bool Running = true;
        public const string artPath = "Picture/Art/";
        public const string altArtPath = "Picture/Art2/";
        public const string cardPicPath = "Picture/CardGenerated/";
        public const string closeupPath = "Picture/Closeup/";
        public const string dataPath = "Data/";
        public const string localesPath = "Data/locales/";
        public const string configPath = "Data/config.conf";
        public const string lflistPath = "Data/lflist.conf";
        public const string deckPath = "Deck/";
        public const string expansionsPath = "Expansions/";
        public const string puzzlePath = "Puzzle/";
        public const string replayPath = "Replay/";
        public const string diyPath = "Picture/DIY/";
        public const string slash = "/";
        public const string ydkExpansion = ".ydk";
        public const string pngExpansion = ".png";
        public const string jpgExpansion = ".jpg";
        public const string yrpExpansion = ".yrp";
        public const string yrp3dExpansion = ".yrp3d";
        #endregion

        #region Initializement

        public static Program instance;
        public static Items items;

        List<Manager> managers = new List<Manager>();
        List<Servant.Servant> servants = new List<Servant.Servant>();

        void Initialize()
        {
            if (!Directory.Exists(dataPath))
                Directory.CreateDirectory(dataPath);
            Config.Initialize(configPath);

            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            if (items != null)
                InitializeRest();
            else
            {
                var handle = Addressables.LoadAssetAsync<Items>("Items");
                handle.Completed += (result) =>
                {
                    items = result.Result;
                    InitializeRest();
                };
            }
        }

        private void InitializeRest()
        {
            ZipHelper.Initialize();
            items.Initialize();
            BanlistManager.Initialize();
            InitializeAllManagers();
            InitializeAllServants();
            ReadParams();

            var handle = Addressables.InstantiateAsync("CardRenderer");
            handle.Completed += (result) =>
            {
                cardRenderer = result.Result.GetComponent<CardRenderer>();
                cardRenderer.SwitchLanguage();
            };
        }

        public static bool exitOnReturn = false;
        void ReadParams()
        {
            var args = Environment.GetCommandLineArgs();
            //args = new string[2]
            //{
            //    //"-r",
            //    //"TURN023"

            //    //"-s",
            //    //"6ace for win!"

            //    "-d",
            //    "LLÌúÊÞ",

            //    //"-n",
            //    //"³à×ÓÄÎÂä",

            //    //"-h",
            //    //"mygo.superpre.pro",
            //    //"-p",
            //    //"888",
            //    //"-w",
            //    //"M#1008611",
            //    //"-j"
            //};

            string nick = null;
            string host = null;
            string port = null;
            string password = null;
            string deck = null;
            string replay = null;
            string puzzle = null;
            var join = false;
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i].ToLower() == "-n" && args.Length > i + 1)
                {
                    nick = args[++i];
                    Config.Set("DuelPlayerName0", nick);
                    Config.Save();
                }

                if (args[i].ToLower() == "-h" && args.Length > i + 1) 
                    host = args[++i];
                if (args[i].ToLower() == "-p" && args.Length > i + 1) 
                    port = args[++i];
                if (args[i].ToLower() == "-w" && args.Length > i + 1)
                    password = args[++i];
                if (args[i].ToLower() == "-d" && args.Length > i + 1)
                    deck = args[++i];
                if (args[i].ToLower() == "-r" && args.Length > i + 1)
                    replay = args[++i];
                if (args[i].ToLower() == "-s" && args.Length > i + 1)
                    puzzle = args[++i];

                if (args[i].ToLower() == "-j")
                {
                    join = true;
                    Config.SetConfigDeck(deck);
                    Config.Save();
                }
            }

            if (join)
            {
                online.KF_OnlineGame(nick, host, port, password);
                exitOnReturn = true;
            }
            else if (deck != null)
            {
                Config.SetConfigDeck(deck);
                deckEditor.SwitchCondition(DeckEditor.Condition.EditDeck);
                ShiftToServant(deckEditor);
                exitOnReturn = true;
            }
            else if (replay != null)
            {
                exitOnReturn = true;
                this.replay.PlayReplay(replay);
            }
            else if (puzzle != null)
            {
                this.puzzle.StartPuzzle(puzzlePath + puzzle);
                exitOnReturn = true;
            }
        }

        public void InitializeForDataChange()
        {
            ZipHelper.Initialize();
            BanlistManager.Initialize();
            StringHelper.Initialize();
            CardsManager.Initialize();
        }

        private void InitializeAllManagers()
        {
            managers.Add(texture_);
            managers.Add(ui_);
            managers.Add(camera_);
            managers.Add(audio_);
            managers.Add(timeline_);
            managers.Add(background_);
            managers.Add(message_);

            foreach (Manager manager in managers)
                manager.Initialize();
        }
        private void InitializeAllServants()
        {
            servants.Add(setting);
            servants.Add(menu);
            servants.Add(solo);
            servants.Add(online);
            servants.Add(puzzle);
            servants.Add(replay);
            servants.Add(cutin);
            servants.Add(mate);
            servants.Add(deckSelector);
            servants.Add(appearance);
            servants.Add(character);
            servants.Add(ocgcore);
            servants.Add(room);
            servants.Add(deckEditor);
            servants.Add(onlineDeckViewer);
            servants.Add(deckBrowser);
            foreach (Servant.Servant servant in servants)
                servant.Initialize();
        }

        #endregion

        #region MonoBehaviors

        public const string tempFolder = "TempFolder/";
        public const string rootWindows64 = "StandaloneWindows64/";
        public const string rootAndroid = "Android/";
        public static string root = rootWindows64;

        void Awake()
        {
#if UNITY_ANDROID
            root = rootAndroid;
#endif
            instance = this;
            Initialize();
        }

        public float TimeScale
        {
            get 
            { 
                return m_TimeScale;
            }
            set 
            {
                m_TimeScale = value;
                Time.timeScale = value;
            }
        }
        float m_TimeScale = 1f;
#if UNITY_EDITOR
        public float timeScaleForEditor = 1;
#endif

        void Update()
        {
            TcpHelper.PerFrameFunction();
            foreach (Manager manager in managers) 
                manager.PerFrameFunction();
            foreach (Servant.Servant servant in servants) 
                servant.PerFrameFunction();

#if UNITY_EDITOR
            TimeScale = timeScaleForEditor;
#endif
        }

        public void UnloadUnusedAssets()
        {
            if (gc == null)
            {
                gc = UnloadUnusedAssetsAsync();
                StartCoroutine(gc);
            }
        }
        IEnumerator gc;
        IEnumerator UnloadUnusedAssetsAsync()
        {
            var unload = Resources.UnloadUnusedAssets();
            while (!unload.isDone)
                yield return null;
            gc = null;
        }

        public static bool noAccess = false;

        #endregion

        #region Tools

        public static int TimePassed()
        {
            return (int)(Time.time * 1000f);
        }

        public void ShiftToServant(Servant.Servant servant)
        {
            currentServant = servant;
            foreach (var ser in servants)
                if (ser != servant)
                    ser.Hide(servant.Depth);
            foreach (var ser in servants)
                if (ser == servant)
                    ser.Show(depth);
            depth = servant.Depth;
        }

        public void ShowSubServant(Servant.Servant servant)
        {
            if (currentSubServant == null)
            {
                servant.Show(0);
                currentSubServant = servant;
            }
            else
            {
                currentSubServant.Hide(servant.Depth);
                servant.Show(currentSubServant.Depth);
                currentSubServant = servant;
            }
        }

        public void ExitCurrentServant()
        {
            if (currentSubServant != null)
                currentSubServant.OnReturn();
            else
            {
                if (currentServant == null)
                {
                    foreach (var servant in servants)
                        if (servant.showing)
                        {
                            currentServant = servant;
                            break;
                        }
                    if(currentServant == null)
                        currentServant = online;
                }
                currentServant.OnReturn();
            }
        }

        public void ExitDuel()
        {
            currentSubServant.OnReturn();
            currentServant.OnReturn();
        }

        #endregion

        #region System

        private void OnApplicationQuit()
        {
            Running = false;
            Config.Save();
            ClearCache();
            YgoServer.StopServer();
            ZipHelper.Dispose();
            try
            { TcpHelper.tcpClient.Close(); }
            catch { }
            TcpHelper.tcpClient = null;
            MyCard.CloseAthleticWatchListWebSocket();
        }

        private void ClearCache()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject cacheDir = currentActivity.Call<AndroidJavaObject>("getCacheDir");
            string cachePath = cacheDir.Call<string>("getAbsolutePath");
            Tools.ClearDirectoryRecursively(new DirectoryInfo(cachePath));
#else
            if (Directory.Exists(tempFolder))
                Directory.Delete(tempFolder, true);
#endif
        }

        public static void GameQuit()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion
    }
}
