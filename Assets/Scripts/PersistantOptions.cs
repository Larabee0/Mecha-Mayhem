using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace RedButton.Core
{
    public class PersistantOptions : MonoBehaviour
    {
        public UserSettingsSaveData userSettings;

        public static bool UseLocal = false;
        public static PersistantOptions instance;

        private string userSettingsPath;

        public delegate void SettingsChanged();
        public SettingsChanged OnUserSettingsChanged;

        [SerializeField] private GameObject RelayNetworkManager;
        [SerializeField] private GameObject PTPNetworkManager;

        private void Awake()
        {
            PrepareScene();
            userSettingsPath = Path.Combine(Application.persistentDataPath, "userSettings.xml");
            instance = this;
            userSettings = new UserSettingsSaveData();
        }

        private void Start()
        {
            if (File.Exists(userSettingsPath))
            {
                XmlSerializer reader = new(typeof(UserSettingsSaveData));
                StreamReader file = new(userSettingsPath);
                userSettings = (UserSettingsSaveData)reader.Deserialize(file);
                file.Close();
                if (userSettings == null)
                {
                    Debug.LogWarning("Read User Settings file, but failed to deserialize it!");
                    userSettings = new UserSettingsSaveData();
                }
            }
        }

        private void OnDestroy()
        {
            userSettings ??= new UserSettingsSaveData();
            XmlSerializer writer = new(typeof(UserSettingsSaveData));
            FileStream file = File.Create(userSettingsPath);
            writer.Serialize(file, userSettings);
            file.Close();
        }

        private void PrepareScene()
        {
            if (UseLocal)
            {
                Instantiate(PTPNetworkManager);
            }
            else
            {
                Instantiate(RelayNetworkManager);
            }
        }
    }

    public class UserSettingsSaveData
    {
        public float player1Sens;
        public float player2Sens;
        public float player3Sens;
        public float player4Sens;

        public float gimmickDelay;

        public int roundCount;
    }
}