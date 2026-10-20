using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CodeMonkey.Toolkit.TSaveSystem {

    /// <summary>
    /// ** Save System **
    /// 
    /// Save your game data easily
    /// Just construct a SaveObject class (see Demo, make it [Serializable])
    /// Then call Save(saveObject);
    /// 
    /// Your data will be converted to JSON and saved to a file.
    /// 
    /// To Load just call LoadMostRecent(); or Load(fileName);
    /// Then process your SaveObject (see Demo)
    /// 
    /// This is a very versatile system, you can store any data you want
    /// as long as it's serializable
    /// 
    /// Also see the SaveFileScreenshot tool if you want to include
    /// a screenshot/image with your save data
    /// </summary>
    public static class SaveSystem {


        private const string SAVE_EXTENSION = "sav";
        private static readonly string DEFAULT_SAVE_FOLDER = Application.dataPath + "/Saves/";


        private static bool isInit = false;



        private static void Init() {
            if (!isInit) {
                isInit = true;
                // Test if Save Folder exists
                if (!Directory.Exists(DEFAULT_SAVE_FOLDER)) {
                    // Create Save Folder
                    Directory.CreateDirectory(DEFAULT_SAVE_FOLDER);
                }
            }
        }

        private static string GetFullPath(string saveFileName, string saveFolder = null) {
            string fullPath;
            if (saveFolder != null) {
                fullPath = saveFolder + saveFileName + "." + SAVE_EXTENSION;
            } else {
                fullPath = DEFAULT_SAVE_FOLDER + saveFileName + "." + SAVE_EXTENSION;
            }
            return fullPath;
        }

        private static void Save(string fileName, string saveString, bool overwrite, string saveFolder = null) {
            Init();
            if (!Directory.Exists(saveFolder)) {
                Debug.LogError("PATH DOES NOT EXIST! Make sure you create the Save folder - " + saveFolder);
                return;
            }
            string saveFileName = fileName;
            if (!overwrite) {
                // Make sure the Save Number is unique so it doesnt overwrite a previous save file
                int saveNumber = 1;
                while (File.Exists(GetFullPath(saveFileName, saveFolder))) {
                    saveNumber++;
                    saveFileName = fileName + "_" + saveNumber;
                }
                // saveFileName is unique
            }
            File.WriteAllText(GetFullPath(saveFileName, saveFolder), saveString);
        }

        private static string Load(string fileName, string saveFolder = null) {
            Init();
            if (File.Exists(GetFullPath(fileName, saveFolder))) {
                string saveString = File.ReadAllText(GetFullPath(fileName, saveFolder));
                return saveString;
            } else {
                return null;
            }
        }

        private static string LoadMostRecentFile(string saveFolder = null) {
            Init();

            // Get all save files
            FileInfo[] saveFileArray = GetSaveFileArray(saveFolder);
            // Cycle through all save files and identify the most recent one
            FileInfo mostRecentFile = null;
            foreach (FileInfo fileInfo in saveFileArray) {
                if (mostRecentFile == null) {
                    mostRecentFile = fileInfo;
                } else {
                    if (fileInfo.LastWriteTime > mostRecentFile.LastWriteTime) {
                        mostRecentFile = fileInfo;
                    }
                }
            }

            // If theres a save file, load it, if not return null
            if (mostRecentFile != null) {
                string saveString = File.ReadAllText(mostRecentFile.FullName);
                return saveString;
            } else {
                return null;
            }
        }

        public static FileInfo[] GetSaveFileArray(string saveFolder = null) {
            Init();
            DirectoryInfo directoryInfo;
            if (saveFolder != null) {
                directoryInfo = new DirectoryInfo(saveFolder);
            } else {
                directoryInfo = new DirectoryInfo(DEFAULT_SAVE_FOLDER);
            }
            // Get all save files
            if (!directoryInfo.Exists) {
                Debug.LogError("PATH DOES NOT EXIST! Make sure you create the Save folder - " + saveFolder);
            }
            return directoryInfo.GetFiles("*." + SAVE_EXTENSION);
        }

        public static void Save(object saveObject, string saveFolder = null) {
            Save("defaultSave", saveObject, false, saveFolder);
        }

        public static void Save(string fileName, object saveObject, bool overwrite, string saveFolder = null) {
            Init();
            string json = JsonUtility.ToJson(saveObject);
            Save(fileName, json, overwrite, saveFolder);
        }

        public static TSaveObject LoadMostRecent<TSaveObject>(string saveFolder = null) {
            Init();
            string saveString = LoadMostRecentFile(saveFolder);
            if (saveString != null) {
                TSaveObject saveObject = JsonUtility.FromJson<TSaveObject>(saveString);
                return saveObject;
            } else {
                return default(TSaveObject);
            }
        }

        public static TSaveObject Load<TSaveObject>(string fileName, string saveFolder = null) {
            Init();
            string saveString = Load(fileName, saveFolder);
            if (saveString != null) {
                TSaveObject saveObject = JsonUtility.FromJson<TSaveObject>(saveString);
                return saveObject;
            } else {
                return default(TSaveObject);
            }
        }

    }

}