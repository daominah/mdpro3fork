using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using YgomSystem.ElementSystem;
using MDPro3.UI.PropertyOverride;

namespace MDPro3.UI
{
    public class PageLegacy : MonoBehaviour
    {

        #region Elements

        private ElementObjectManager m_Manager;
        private ElementObjectManager Manager =>
            m_Manager = m_Manager != null ? m_Manager
            : GetComponent<ElementObjectManager>();

        private const string LABEL_SR_PRESET = "ScrollRectPreset";
        private ScrollRect m_ScrollRectPreset;
        public ScrollRect ScrollRectPreset =>
            m_ScrollRectPreset = m_ScrollRectPreset != null ? m_ScrollRectPreset
            : Manager.GetElement<ScrollRect>(LABEL_SR_PRESET);

        private const string LABEL_IPT_NAME = "InputFieldName";
        private TMP_InputField m_InputName;
        private TMP_InputField InputName =>
            m_InputName = m_InputName != null ? m_InputName
            : Manager.GetElement<TMP_InputField>(LABEL_IPT_NAME);

        private const string LABEL_IPT_HOST = "InputFieldHost";
        private TMP_InputField m_InputHost;
        private TMP_InputField InputHost =>
            m_InputHost = m_InputHost != null ? m_InputHost
            : Manager.GetElement<TMP_InputField>(LABEL_IPT_HOST);

        private const string LABEL_IPT_PORT = "InputFieldPort";
        private TMP_InputField m_InputPort;
        private TMP_InputField InputPort =>
            m_InputPort = m_InputPort != null ? m_InputPort
            : Manager.GetElement<TMP_InputField>(LABEL_IPT_PORT);

        private const string LABEL_IPT_PASSWORD = "InputFieldPassword";
        private TMP_InputField m_InputPassword;
        private TMP_InputField InputPassword =>
            m_InputPassword = m_InputPassword != null ? m_InputPassword
            : Manager.GetElement<TMP_InputField>(LABEL_IPT_PASSWORD);

        private const string LABEL_SBN_SAVE = "ButtonSave";
        private SelectionButton m_ButtonSave;
        private SelectionButton ButtonSave =>
            m_ButtonSave = m_ButtonSave != null ? m_ButtonSave
            : Manager.GetElement<SelectionButton>(LABEL_SBN_SAVE);

        private const string LABEL_SBN_JOIN = "ButtonJoin";
        private SelectionButton m_ButtonJoin;
        private SelectionButton ButtonJoin =>
            m_ButtonJoin = m_ButtonJoin != null ? m_ButtonJoin
            : Manager.GetElement<SelectionButton>(LABEL_SBN_JOIN);

        #endregion

        private struct HostAddress
        {
            public string name;
            public string host;
            public string port;
            public string password;
        }
        private List<HostAddress> addresses = new();
        private const string PATH_ADDRESS_SAVE = "Data/hosts.conf";
        private SuperScrollView hostSuperScrollView;
        private bool addressedLoaded = false;

        private void Awake()
        {
            ResetLegacy();
        }

        private void ResetLegacy()
        {
            InputName.text = Config.Get("DuelPlayerName0", Config.EMPTY_STRING);
            InputHost.text = Config.Get("Host", "s1.ygo233.com");
            InputPort.text = Config.Get("Port", "233");
            InputPassword.text = Config.Get("Password", Config.EMPTY_STRING);
        }

        private void LoadHostAddresses()
        {
            if (!File.Exists(PATH_ADDRESS_SAVE))
                return;
            var txtString = File.ReadAllText(PATH_ADDRESS_SAVE);
            var lines = txtString.Replace("\r", "").Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                var mats = Regex.Split(lines[i], " ");
                var address = new HostAddress();
                if (mats.Length >= 3)
                {
                    address.name = mats[0];
                    address.host = mats[1];
                    address.port = mats[2];
                    address.password = string.Empty;
                    if (mats.Length > 3)
                        address.password = mats[3];
                    addresses.Add(address);
                }
            }

            addressedLoaded = true;
        }

        private void SaveHostAddresses()
        {
            var content = string.Empty;
            foreach (var address in addresses)
            {
                content += address.name + " ";
                content += address.host + " ";
                content += address.port + " ";
                content += address.password + Program.STRING_LINE_BREAK;
            }
            File.WriteAllText(PATH_ADDRESS_SAVE, content);
        }

        private void ItemOnListRefresh(string[] task, GameObject item)
        {
            var handler = item.GetComponent<SelectionToggle_Address>();
            handler.addressName = task[0];
            handler.addressHost = task[1];
            handler.addressPort = task[2];
            handler.addressPassword = task[3];
            handler.Refresh();
        }

        private void AddAddress(string name)
        {
            var address = new HostAddress
            {
                name = name,
                host = InputHost.text,
                port = InputPort.text,
                password = InputPassword.text
            };
            foreach (var add in addresses)
                if (add.name == name)
                {
                    addresses.Remove(add);
                    break;
                }

            addresses.Add(address);
            SaveHostAddresses();
            PrintAddresses();
        }

        public void PrintAddresses(string search = "")
        {
            if (!addressedLoaded)
                LoadHostAddresses();

            hostSuperScrollView?.Clear();

            var tasks = new List<string[]>();
            foreach (var address in addresses)
            {
                if (address.name.Contains(search))
                {
                    string[] task = new string[] { address.name, address.host, address.port, address.password };
                    tasks.Add(task);
                }
            }

            var handle = Addressables.LoadAssetAsync<GameObject>("UI/ItemAddress.prefab");
            handle.Completed += (result) =>
            {
                var itemWidth = PropertyOverrider.NeedMobileLayout() ? 460f : 360f;
                var itemHeight = PropertyOverrider.NeedMobileLayout() ? 80f : 40f;

                hostSuperScrollView = new SuperScrollView(
                    1,
                    itemWidth,
                    itemHeight,
                    0,
                    0,
                    result.Result,
                    ItemOnListRefresh,
                    ScrollRectPreset);
                hostSuperScrollView.Print(tasks);
                if (hostSuperScrollView.items.Count > 0)
                {
                    Program.instance.online.lastSelectedAddressItem
                        = hostSuperScrollView.items[0].gameObject.GetComponent<SelectionToggle_Address>();
                }
            };
        }

        public void SetHost(string host, string port, string passwd)
        {
            InputHost.text = host;
            InputPort.text = port;
            InputPassword.text = passwd;
            OnNameChange(InputName.text);
            OnHostChange(host);
            OnPortChange(port);
            OnPasswordChange(passwd);
        }

        public void DeleteAddress(string hostName)
        {
            foreach (var address in addresses)
                if (address.name == hostName)
                    addresses.Remove(address);
            SaveHostAddresses();
            PrintAddresses();
        }

        public void AddressMoveUp(string hostName)
        {
            int index = -1;
            for (int i = 0; i < addresses.Count; i++)
                if (addresses[i].name == hostName)
                {
                    index = i;
                    break;
                }
            if (index < 0)
            {
                Debug.LogError("Did not find target host.");
                return;
            }
            if (index == 0)
                return;

            var host = addresses[index];
            addresses.RemoveAt(index);
            index--;
            addresses.Insert(index, host);
            SaveHostAddresses();
            PrintAddresses();
        }

        public void OnNameChange(string name)
        {
            Config.Set("DuelPlayerName0", name == "" ? Config.EMPTY_STRING : name);
            Config.Save();
        }

        public void OnHostChange(string host)
        {
            Config.Set("Host", host);
            Config.Save();
        }

        public void OnPortChange(string port)
        {

            Config.Set("Port", port);
            Config.Save();
        }

        public void OnPasswordChange(string password)
        {
            Config.Set("Password", password == "" ? Config.EMPTY_STRING : password);
            Config.Save();
        }

        public void OnPresetSave()
        {
            var selections = new List<string>()
            {
                InterString.Get("请输入预设名称"),
                string.Empty
            };
            UIManager.ShowPopupInput(selections, AddAddress, null, TmpInputValidation.ValidationType.NoSpace);
        }

        public void OnJoin()
        {
            Program.instance.online.KF_OnlineGame(InputName.text, InputHost.text, InputPort.text, InputPassword.text);
        }

        public void SelectLastAddressItem()
        {
            if (Program.instance.online.lastSelectedAddressItem != null)
            {
                UserInput.NextSelectionIsAxis = true;
                Program.instance.online.lastSelectedAddressItem.GetSelectable().Select();
            }
        }

        public void SelectDefault()
        {
            ButtonJoin.GetSelectable().Select();
        }

    }
}