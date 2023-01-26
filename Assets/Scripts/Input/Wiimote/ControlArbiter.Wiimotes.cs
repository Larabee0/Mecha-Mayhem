using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WiimoteApi;

namespace RedButton.Core
{
    public partial class ControlArbiter : MonoBehaviour
    {
        [SerializeField] private List<Wiimote> connectedWiimotes = new();
        private Dictionary<string,Wiimote> wiimotePaths = new();
        public static Action<string> wiimoteAdded;
        public static Action<string> wiimoteRemoved;
        private List<WiimoteDevice> ISwiimotes = new();
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
            if(ISwiimotes.Count != WiimoteDevice.all.Count)
            {
                ISwiimotes.Clear();
                for (int i = 0; i < WiimoteDevice.all.Count; i++)
                {
                    WiimoteDevice wiimote = WiimoteDevice.all[i];
                    
                    ISwiimotes.Add(wiimote);
                    ISwiimotes[^1].RegisterWiimote(wiimotePaths[wiimote.description.product]);
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