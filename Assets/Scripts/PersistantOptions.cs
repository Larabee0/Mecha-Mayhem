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
        public SettingsChanged OnUserSettingsChangedData;

        private void Awake()
        {
            userSettingsPath = Path.Combine(Application.persistentDataPath, "userSettings.xml");
            instance = this;
            userSettings = new UserSettingsSaveData();

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
    }

    public class UserSettingsSaveData
    {
        public float player1Sens = 1;
        public float player2Sens = 1;
        public float player3Sens = 1;
        public float player4Sens = 1;

        public int gimmickDelay = 30;

        public int roundCount = 3;

        public int screenIndex;

        public float GetPlayerSense(Controller playerNum) => playerNum switch
        {
            Controller.One => player1Sens,
            Controller.Two => player2Sens,
            Controller.Three => player3Sens,
            Controller.Four => player4Sens,
            _ => 1,
        };
    }
}