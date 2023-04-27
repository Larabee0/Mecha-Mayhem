using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using WiimoteApi;
using RedButton.Core.WiimoteSupport;
using UnityEngine.InputSystem.UI;

namespace RedButton.Core
{
    /// <summary>
    /// ControlArbiter.Wiimotes
    /// this part of hte control arbiter is reliable but should be imporved to make the static properties more robust.
    /// This part of hte control arbiter is resonsible for talking to the WiimoteAPI.WiimoteManager to get wiimotes,
    /// and send them to the Input System so they can be used in game.
    /// </summary>
    public partial class ControlArbiter : MonoBehaviour
    {
        public static Vector3 Wiimote1PointerPos
        {
            set
            {
                if (Instance.wiimotePointer1 != null)
                {
                    //Instance.wiimotePointer1.transform.position = new Vector3(value.x, Screen.height - value.y);
                    //Vector3 point = Camera.main.ScreenToWorldPoint();
                    Instance.wiimoteSinglePointer.position = new Vector3(value.x, value.y, 0);
                }
            }
        }
        public static Vector3 Wiimote2PointerPos
        {
            set
            {
                if (Instance.wiimotePointer2 != null)
                {
                    //Instance.wiimotePointer2.transform.position = new Vector3(value.x, Screen.height - value.y);
                    Instance.wiimoteSinglePointer.position = new Vector3(value.x, value.y, 0);
                }
            }
        }
        public static Vector3 Wiimote3PointerPos
        {
            set
            {
                if (Instance.wiimotePointer3 != null)
                {
                    //Instance.wiimotePointer3.transform.position = new Vector3(value.x, Screen.height - value.y);
                    Instance.wiimoteSinglePointer.position = new Vector3(value.x, value.y, 0);
                }
            }
        }
        public static Vector3 Wiimote4PointerPos
        {
            set
            {
                if (Instance.wiimotePointer4 != null)
                {
                    //Instance.wiimotePointer4.transform.position = new Vector3(value.x, Screen.height - value.y);
                    Instance.wiimoteSinglePointer.position = new Vector3(value.x, value.y, 0);
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

        public static Action<string> wiimoteAdded;
        public static Action<string> wiimoteRemoved;

        [Header("Wiimote Support")]
        // Wiimote pointer UI for multiple wiimote support
        [SerializeField] private UIDocument wiimoteOverlay;
        private VisualElement wiimotePointer1;
        private VisualElement wiimotePointer2;
        private VisualElement wiimotePointer3;
        private VisualElement wiimotePointer4;

        [SerializeField] private RectTransform wiimoteSinglePointer;
        [SerializeField] private VirtualMouseInput wiimoteVirtualMouse;

        // these strcutures are used to safely add and remove wiimotes from the Input System via the Static actions above.
        private List<Wiimote> Connected_WiimoteAPI_Wiimotes = new(); // all conneted WiimoteAPI.Wiimotes
        private Dictionary<string,Wiimote> HID_Paths_To_WiimoteAPI_Wiimote = new(); // WiimoteAPI.Wiimote HID Paths to WiimoteAPI.Wiimote Instances
        private Dictionary<string, WiimoteDevice> IS_Paths_To_IS_Wiimote = new(); // InputSystem WiimoteDevice Paths to WiimoteDevice Instances
        private Dictionary<Wiimote, WiimoteDevice> WiimoteAPI_Wiimote_To_IS_Wiimote = new(); // WiimoteAPI.Wiimote to WiimoteDevice Instances

        /// <summary>
        /// Called from Awake.
        /// Used to get the initially connected wiimotes and fill up the wiimotePaths Dictionary
        /// </summary>
        private void PollWiimotes()
        {
            WiimoteManager.FindWiimotes();
            if (WiimoteManager.HasWiimote())
            {
                Connected_WiimoteAPI_Wiimotes = new(WiimoteManager.Wiimotes);
                // init IS wiimotes
                Connected_WiimoteAPI_Wiimotes.ForEach(wiimoteAPI_Wiimote =>
                {
                    string HID_Path = wiimoteAPI_Wiimote.hidapi_path;
                    UpdateWiimote(wiimoteAPI_Wiimote);
                    HID_Paths_To_WiimoteAPI_Wiimote.Add(HID_Path,wiimoteAPI_Wiimote);
                    wiimoteAdded?.Invoke(HID_Path);
                });
            }
        }

        /// <summary>
        /// Called from Start. Initilises the UI for the Wiimotes
        /// </summary>
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

        /// <summary>
        /// Created based off the WiimoteAPI documentation.
        /// The wiimote class recieves data in a queue, which must be read through to update the class.
        /// This simply reads through all the data currently in the queue until the quite is empty.
        /// </summary>
        /// <param name="remote">Target WiimoteAPI.Wiimote to update the state of</param>
        private void UpdateWiimote(Wiimote remote)
        {
            int ret;
            do
            {
                ret = remote.ReadWiimoteData();
            } while (ret > 0);
        }

        /// <summary>
        /// This assigns WiimoteAPI.Wiimotes to InputSystem WiimoteDevices.
        /// This also checks for WiimoteAPI device changes (connected/disconnecte)
        /// THIS DOES NOT YET HANDLE WIIMOTES DISCONNECTING DURING RUNTIME.
        /// </summary>
        private void Update()
        {
            // Assigning WiimoteAPI.Wiimotes to InputSystem WiimoteDevices.
            if (IS_Paths_To_IS_Wiimote.Count != WiimoteDevice.all.Count)
            {
                for (int i = 0; i < WiimoteDevice.all.Count; i++)
                {
                    WiimoteDevice wiimote = WiimoteDevice.all[i];
                    if (IS_Paths_To_IS_Wiimote.ContainsValue(wiimote)||WiimoteAPI_Wiimote_To_IS_Wiimote.ContainsValue(wiimote))
                    {
                        // assume we already have registered a wiimote to this device.
                        continue;
                    }
                    KeyValuePair<string, Wiimote> res = HID_Paths_To_WiimoteAPI_Wiimote.Where(pair => !IS_Paths_To_IS_Wiimote.ContainsKey(pair.Key) && !WiimoteAPI_Wiimote_To_IS_Wiimote.ContainsKey(pair.Value)).First();
                    IS_Paths_To_IS_Wiimote.Add(res.Key, wiimote);
                    WiimoteAPI_Wiimote_To_IS_Wiimote.Add(res.Value, wiimote);
                    wiimote.RegisterWiimote(res.Value);
                }
            }

            //Checking for WiimoteAPI device changes, firing events accordingly
            if ((HID_Paths_To_WiimoteAPI_Wiimote.Count != WiimoteManager.Wiimotes.Count && WiimoteManager.HasWiimote()) || WiimoteManager.FindWiimotes())
            {
                for (int i = 0; i < WiimoteManager.Wiimotes.Count; i++)
                {
                    Wiimote wiimote = WiimoteManager.Wiimotes[i];
                    string path = wiimote.hidapi_path;
                    if (!HID_Paths_To_WiimoteAPI_Wiimote.ContainsKey(path))
                    {
                        HID_Paths_To_WiimoteAPI_Wiimote.Add(path, wiimote);
                        UpdateWiimote(wiimote);
                        wiimoteAdded?.Invoke(path);
                    }
                }
            }
        }

        /// <summary>
        /// when the application exits we need to clean up the wiimote bluetooth data.
        /// </summary>
        private void OnApplicationQuit()
        {
            Connected_WiimoteAPI_Wiimotes.Clear();
            Debug.Log("Cleaning up Wiimote bluetooth data");
            Connected_WiimoteAPI_Wiimotes = new(WiimoteManager.Wiimotes);
            for (int i = 0; i < Connected_WiimoteAPI_Wiimotes.Count; i++)
            {
                Wiimote remote = Connected_WiimoteAPI_Wiimotes[i];
                wiimoteRemoved?.Invoke(remote.hidapi_path);
                WiimoteManager.Cleanup(remote);
            }
            Connected_WiimoteAPI_Wiimotes.Clear();
        }
    }
}