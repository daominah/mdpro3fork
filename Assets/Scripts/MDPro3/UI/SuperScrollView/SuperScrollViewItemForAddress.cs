using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MDPro3.UI
{
    public class SuperScrollViewItemForAddress : SuperScrollViewItem
    {
        public string addressName;
        public string addressHost;
        public string addressPort;
        public string addressPassword;

        public override void Refresh()
        {
            base.Refresh();
            transform.GetChild(0).GetComponent<Text>().text = addressName;
        }

        public override void OnClick()
        {
            base.OnClick();
            Program.I().online.inputHost.text = addressHost;
            Program.I().online.OnHostChange(addressHost);
            Program.I().online.inputPort.text = addressPort;
            Program.I().online.OnPortChange(addressPort);
            Program.I().online.inputPassword.text = addressPassword;
            Program.I().online.OnPasswordChange(addressPassword);
        }

        public void OnDelete()
        {
            foreach (var address in Program.I().online.addresses)
            {
                if (address.name == addressName)
                {
                    Program.I().online.addresses.Remove(address);
                    Program.I().online.Save();
                    Program.I().online.Print();
                    break;
                }
            }
        }
        public void OnMoveUp()
        {
            var address = new Online.HostAddress();
            address.name = addressName;
            address.host = addressHost;
            address.port = addressPort;
            address.password = addressPassword;
            Program.I().online.addresses.RemoveAt(id);
            var targetID = id;
            if (id > 0)
                targetID--;
            Program.I().online.addresses.Insert(targetID, address);
            Program.I().online.Save();
            Program.I().online.Print();
        }
        public void OnMoveDown()
        {
            var address = new Online.HostAddress();
            address.name = addressName;
            address.host = addressHost;
            address.port = addressPort;
            address.password = addressPassword;
            Program.I().online.addresses.RemoveAt(id);
            var targetID = id;
            if (id < Program.I().online.addresses.Count)
                targetID++;
            Program.I().online.addresses.Insert(targetID, address);
            Program.I().online.Save();
            Program.I().online.Print();
        }
    }
}
