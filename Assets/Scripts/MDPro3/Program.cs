using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using MDPro3.YGOSharp;
using Ionic.Zip;
using UnityEngine.EventSystems;
using UnityEngine.AddressableAssets;
using MDPro3.Net;
using System.Threading;
using System.Runtime.InteropServices;

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
        public TimeLineManager timeline_;

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

#if UNITY_EDITOR
        public float timeScaleFloat = 1;

#endif
        #region Initializement

        private static Program instance;
        public static Items items;

        List<Manager> managers = new List<Manager>();
        List<Servant> servants = new List<Servant>();

        #region State
        public static bool Running = true;
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
            ZipManager.Initialize();
            if (!Directory.Exists("Data"))
                Directory.CreateDirectory("Data");
            Config.Initialize("Data/config.conf");
            items.Initialize();
            BanlistManager.Initialize("data/lflist.conf");
            InitializeAllManagers();
            InitializeAllServants();


            //new Thread(Server.Main).Start();
            //new Thread(Client.Main).Start();
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
            foreach (Servant servant in servants)
                servant.Initialize();
        }

        #endregion

        #region MonoBehaviors

        public static string root = "StandaloneWindows64/";
        void Awake()
        {
#if UNITY_ANDROID
            root = "Android/";
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
            timeScale = timeScaleFloat;
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
            GC.Collect();
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
                currentServant.OnReturn();
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
            YgoServer.StopServer();
        }
    }
}
