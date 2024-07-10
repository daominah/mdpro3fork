using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MDPro3.YGOSharp;
using UnityEngine.EventSystems;
using UnityEngine.AddressableAssets;
using MDPro3.Net;

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
        public NewsManager news_;

        [Header("Servants")]
        public Menu menu;
        public Solo solo;
        public Online online;
        public SelectPuzzle puzzle;
        public SelectReplay replay;
        public MonsterCutin cutin;
        public MateView mate;
        public SelectDeck selectDeck;
        public Setting setting;
        public Appearance appearance;
        public OcgCore ocgcore;
        public Room room;
        public EditDeck editDeck;
        public OnlineDeckViewer onlineDeckViewer;

        #region Initializement

        private static Program instance;
        public static Items items;

        List<Manager> managers = new List<Manager>();
        List<Servant> servants = new List<Servant>();

        #region State
        public static bool Running = true;
        public const string artPath = "Picture/Art/";
        public const string altArtPath = "Picture/Art2/";
        public const string cardPicPath = "Picture/CardGenerated";
        public const string closeupPath = "Picture/Closeup";
        public const string dataPath = "Data";
        public const string localesPath = "Data/locales/";
        public const string configPath = "Data/config.conf";
        public const string lflistPath = "Data/lflist.conf";
        public const string deckPath = "Deck/";
        public const string expansionsPath = "Expansions";
        public const string puzzlePath = "Puzzle";
        public const string replayPath = "Replay";
        public const string diyPath = "Picture/DIY/";
        public const string slash = "/";
        public const string ydkExpansion = ".ydk";
        #endregion

        public static Program I()
        {
            return instance;
        }
        void Initialize()
        {
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

        void InitializeRest()
        {
            ZipHelper.Initialize();
            if (!Directory.Exists(dataPath))
                Directory.CreateDirectory(dataPath);
            Config.Initialize(configPath);
            items.Initialize();
            BanlistManager.Initialize();
            InitializeAllManagers();
            InitializeAllServants();
            ReadParams();
        }

        public static bool exitOnReturn = false;
        void ReadParams()
        {
            var args = Environment.GetCommandLineArgs();
            //args = new string[11]
            //{
            //    //"-r",
            //    //"TURN023"

            //    //"-s",
            //    //"6ace for win!"

            //    "-d",
            //    "LLÌúÊÞ",

            //    "-n",
            //    "³à×ÓÄÎÂä",

            //    "-h",
            //    "mygo.superpre.pro",
            //    "-p",
            //    "888",
            //    "-w",
            //    "M#1008611",
            //    "-j"
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
                    Config.Set("DeckInUse", deck);
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
                Config.Set("DeckInUse", deck);
                editDeck.SwitchCondition(EditDeck.Condition.EditDeck);
                StartCoroutine(ShiftToServantAsync(editDeck));
                exitOnReturn = true;
            }
            else if (replay != null)
            {
                this.replay.KF_Replay(replay);
                exitOnReturn = true;
            }
            else if (puzzle != null)
            {
                this.puzzle.StartPuzzle(puzzle);
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
            servants.Add(selectDeck);
            servants.Add(appearance);
            servants.Add(ocgcore);
            servants.Add(room);
            servants.Add(editDeck);
            servants.Add(onlineDeckViewer);
            foreach (Servant servant in servants)
                servant.Initialize();
        }

        #endregion

        #region MonoBehaviors

        public static string tempFolder = "TempFolder";
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

        static int preWidth;
        static int preHeight;
        public static GameObject hoverObject;
        public float timeScale
        {
            get 
            { 
                return m_timeScale;
            }
            set 
            {
                m_timeScale = value;
                Time.timeScale = value;
            }
        }
        float m_timeScale = 1f;
#if UNITY_EDITOR
        public float timeScaleForEdit = 1;
#endif

        public static bool InputGetMouse0;
        public static bool InputGetMouse0Down;
        public static bool InputGetMouse0Up;
        public static bool InputGetMouse1;
        public static bool InputGetMouse1Down;
        public static bool InputGetMouse1Up;
        public static float pressingTime;

        void Update()
        {
            InputGetMouse0 = Input.GetMouseButton(0);
            InputGetMouse0Down = Input.GetMouseButtonDown(0);
            InputGetMouse0Up = Input.GetMouseButtonUp(0);
            InputGetMouse1 = Input.GetMouseButton(1);
            InputGetMouse1Down = Input.GetMouseButtonDown(1);
            InputGetMouse1Up = Input.GetMouseButtonUp(1);

            if (InputGetMouse0Down)
                pressingTime = 0;
            else if (InputGetMouse0)
                pressingTime += Time.deltaTime;
            else if (InputGetMouse0Up)
                pressingTime = 0;

            hoverObject = null;
            if (camera_.cameraMain.gameObject.activeInHierarchy
                && !(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                )
            {
                Ray ray = camera_.cameraMain.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                    hoverObject = hit.collider.gameObject;
            }


            if (Screen.width != preWidth || Screen.height != preHeight)
                OnResize();


            TcpHelper.PerFrameFunction();
            foreach (Manager manager in managers) manager.PerFrameFunction();
            foreach (Servant servant in servants) servant.PerFrameFunction();

#if UNITY_EDITOR
            timeScale = timeScaleForEdit;
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
            //GC.Collect();
            gc = null;
        }

        public delegate void OnScreenChanged();
        public static OnScreenChanged onScreenChanged;
        static void OnResize()
        {
            preWidth = Screen.width;
            preHeight = Screen.height;
            onScreenChanged.Invoke();
        }


        public static bool noAccess = false;

        #endregion

        #region Tools
        public static int TimePassed()
        {
            return (int)(Time.time * 1000f);
        }

        [HideInInspector]
        public Servant currentServant;
        [HideInInspector]
        public Servant currentSubServant;
        [HideInInspector]
        public int depth;
        public void ShiftToServant(Servant servant)
        {
            currentServant = servant;
            foreach (var ser in servants)
                if (ser != servant)
                    ser.Hide(servant.depth);
            foreach (var ser in servants)
                if (ser == servant)
                    ser.Show(depth);
            depth = servant.depth;
        }
        IEnumerator ShiftToServantAsync(Servant servant)
        {
            while(!servant.initialized)
                yield return null;
            ShiftToServant(servant);
        }
        public void ShowSubServant(Servant servant)
        {
            if (currentSubServant == null)
            {
                servant.Show(0);
                currentSubServant = servant;
            }
            else
            {
                currentSubServant.Hide(servant.depth);
                servant.Show(currentSubServant.depth);
                currentSubServant = servant;
            }
        }

        public void ExitCurrentServant()
        {
            if (currentSubServant != null)
                currentSubServant.OnReturn();
            else
            {
                if(currentServant == null)
                {
                    foreach(var servant in  servants)
                        if (servant.isShowed)
                        {
                            currentServant = servant;
                            break;
                        }
                }
                if (currentServant == null)
                {
                    foreach (var servant in servants)
                        if (servant.isShowed)
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

        private void OnApplicationQuit()
        {
            Running = false;
            Config.Save();
            ClearCache();
            YgoServer.StopServer();
            ZipHelper.Dispose();
            try
            {
                TcpHelper.tcpClient.Close();
            }
            catch { }
            TcpHelper.tcpClient = null;

            MyCard.CloseAthleticWatchListWebSocket();
        }

        void ClearCache()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject cacheDir = currentActivity.Call<AndroidJavaObject>("getCacheDir");
            string cachePath = cacheDir.Call<string>("getAbsolutePath");
            ClearDirectoryRecursively(new DirectoryInfo(cachePath));
#else
            if (Directory.Exists(tempFolder))
                Directory.Delete(tempFolder, true);
#endif
        }

        void ClearDirectoryRecursively(DirectoryInfo directory)
        {
            foreach(var file in directory.GetFiles())
                file.Delete();
            foreach(var subDir in directory.GetDirectories())
            {
                ClearDirectoryRecursively(subDir);
                subDir.Delete();
            }
        }
    }
}
