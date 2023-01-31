using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using WiimoteApi;
using RedButton.Core.WiimoteSupport;

namespace RedButton.Core
{
    public partial class ControlArbiter : MonoBehaviour
    {
        public static Vector3 Wiimote1PointerPos
        {
            set
            {
                if (Instance.wiimotePointer1 != null)
                {
                    Instance.wiimotePointer1.transform.position = new Vector3(value.x, Screen.height - value.y);
                }
            }
        }
        public static Vector3 Wiimote2PointerPos
        {
            set
            {
                if (Instance.wiimotePointer2 != null)
                {
                    Instance.wiimotePointer2.transform.position = new Vector3(value.x, Screen.height - value.y);
                }
            }
        }
        public static Vector3 Wiimote3PointerPos
        {
            set
            {
                if (Instance.wiimotePointer3 != null)
                {
                    Instance.wiimotePointer3.transform.position = new Vector3(value.x, Screen.height - value.y);
                }
            }
        }
        public static Vector3 Wiimote4PointerPos
        {
            set
            {
                if (Instance.wiimotePointer4 != null)
                {
                    Instance.wiimotePointer4.transform.position = new Vector3(value.x, Screen.height - value.y);
                }
            }
        }
        public static bool Wiimote1PointerEnable
        {
            set
            {
                if(Instance.wiimotePointer1 != null)
                {
                    Instance.wiimotePointer1.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }
        }
        public static bool Wiimote2PointerEnable
        {
            set
            {
                if (Instance.wiimotePointer2 != null)
                {
                    Instance.wiimotePointer2.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }
        }
        public static bool Wiimote3PointerEnable
        {
            set
            {
                if (Instance.wiimotePointer3 != null)
                {
                    Instance.wiimotePointer3.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }
        }
        public static bool Wiimote4PointerEnable
        {
            set
            {
                if (Instance.wiimotePointer4 != null)
                {
                    Instance.wiimotePointer4.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }
        }

        [Header("Wiimote Support")]
        [SerializeField] private UIDocument wiimoteOverlay;
        private VisualElement wiimotePointer1;
        private VisualElement wiimotePointer2;
        private VisualElement wiimotePointer3;
        private VisualElement wiimotePointer4;

        private List<Wiimote> connectedWiimotes = new();
        private Dictionary<string,Wiimote> wiimotePaths = new();
        private Dictionary<string, WiimoteDevice> ISwiimotePaths = new();
        private Dictionary<Wiimote, WiimoteDevice> wiimoteToDevice = new();
        public static Action<string> wiimoteAdded;
        public static Action<string> wiimoteRemoved;

        private void PollWiimotes()
        {
            WiimoteManager.FindWiimotes();
            if (WiimoteManager.HasWiimote())
            {
                connectedWiimotes = new(WiimoteManager.Wiimotes);
                // init wiimotes
                connectedWiimotes.ForEach(wiimote =>
                {
                    string path = wiimote.hidapi_path;
                    UpdateWiimote(wiimote);
                    wiimotePaths.Add(path,wiimote);
                    wiimoteAdded?.Invoke(path);
                });
            }
        }

        private void WiimoteUISetup()
        {
            if (wiimoteOverlay.rootVisualElement == null)
            {
                Debug.Log("wiimote overlay root is null");
            }
            wiimotePointer1 = wiimoteOverlay.rootVisualElement.Q("WiiCusor1");
            wiimotePointer2 = wiimoteOverlay.rootVisualElement.Q("WiiCusor2");
            wiimotePointer3 = wiimoteOverlay.rootVisualElement.Q("WiiCusor3");
            wiimotePointer4 = wiimoteOverlay.rootVisualElement.Q("WiiCusor4");
            Wiimote1PointerEnable = false;
            Wiimote2PointerEnable = false;
            Wiimote3PointerEnable = false;
            Wiimote4PointerEnable = false;
        }

        private void UpdateWiimote(Wiimote remote)
        {
            int ret;
            do
            {
                ret = remote.ReadWiimoteData();
            } while (ret > 0);
        }

        private void Update()
        {
            if(ISwiimotePaths.Count != WiimoteDevice.all.Count)
            {
                for (int i = 0; i < WiimoteDevice.all.Count; i++)
                {
                    WiimoteDevice wiimote = WiimoteDevice.all[i];
                    if (ISwiimotePaths.ContainsValue(wiimote)||wiimoteToDevice.ContainsValue(wiimote))
                    {
                        // assume we already have registered a wiimote to this device.
                        continue;
                    }
                    KeyValuePair<string, Wiimote> res = wiimotePaths.Where(pair => !ISwiimotePaths.ContainsKey(pair.Key) && !wiimoteToDevice.ContainsKey(pair.Value)).First();
                    ISwiimotePaths.Add(res.Key, wiimote);
                    wiimoteToDevice.Add(res.Value, wiimote);
                    wiimote.RegisterWiimote(res.Value);
                }
            }

            if ((wiimotePaths.Count != WiimoteManager.Wiimotes.Count && WiimoteManager.HasWiimote()) || WiimoteManager.FindWiimotes())
            {
                for (int i = 0; i < WiimoteManager.Wiimotes.Count; i++)
                {
                    Wiimote wiimote = WiimoteManager.Wiimotes[i];
                    string path = wiimote.hidapi_path;
                    if (!wiimotePaths.ContainsKey(path))
                    {
                        wiimotePaths.Add(path, wiimote);
                        UpdateWiimote(wiimote);
                        wiimoteAdded?.Invoke(path);
                    }
                }
            }
        }

        private void OnApplicationQuit()
        {
            connectedWiimotes.Clear();
            Debug.Log("Cleaning up Wiimote bluetooth data");
            lock (WiimoteManager.Wiimotes)
            {
                for (int i = 0; i < WiimoteManager.Wiimotes.Count; i++)
                {
                    Wiimote remote = WiimoteManager.Wiimotes[i];
                    wiimoteRemoved?.Invoke(remote.hidapi_path);
                    WiimoteManager.Cleanup(remote);
                }
            }
        }
    }
}