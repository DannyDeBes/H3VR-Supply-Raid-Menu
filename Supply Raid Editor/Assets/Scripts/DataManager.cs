using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using SFB;

namespace Supply_Raid_Editor
{
    public class DataManager : MonoBehaviour
    {
        public static DataManager instance;

        [Header("Character")]
        [SerializeField] string characterPath;
        public SR_CharacterPreset character = null;

        [Header("Faction")]
        [SerializeField] string factionPath;

        [Header("Item Categories")]
        [SerializeField] string categoryPath;

        [Header("Mod Folder")]
        [SerializeField] string modPath;
        public string lastCharacterDirectory = "";
        public string lastFactionDirectory = "";
        public string lastItemDirectory = "";

        //Loaded Categories from mod folder
        public List<SR_ItemCategory> loadedCategories = new List<SR_ItemCategory>();



        [Header("Debug")]
        [SerializeField] Text debugLine;
        private string debugLog = "";

        private void Awake()
        {
            instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            modPath = PlayerPrefs.GetString("ModPath", Application.dataPath);

            lastCharacterDirectory = PlayerPrefs.GetString("lastCharacterDirectory");
            lastFactionDirectory = PlayerPrefs.GetString("lastFactionDirectory");
            lastItemDirectory = PlayerPrefs.GetString("lastItemDirectory");
        }

        // Update is called once per frame
        void Update()
        {
        }

        public Sprite LoadSprite(string path)
        {
            Log("Loading External Image at " + path);
            Texture2D tex = null;

            byte[] fileData;

            if (File.Exists(path) && tex == null)
            {
                fileData = File.ReadAllBytes(path);
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
                                         //if (tex.texelSize.x > 256)
                                         //    tex.Resize(256, 256);
            }

            if (tex == null)
            {
                LogError("Texture Not Found at path: " + path);
                return null;
            }
            Sprite NewSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0), 100.0f);

            return NewSprite;
        }


        public void OnLoadDialogue(JSONTypeEnum loadType)
        {
            string[] paths;


            string modPath = "";

            switch (loadType)
            {
                case JSONTypeEnum.Character:
                    modPath = lastCharacterDirectory;
                    break;
                case JSONTypeEnum.Faction:
                    modPath = lastFactionDirectory;
                    break;
                case JSONTypeEnum.ItemCategory:
                    modPath = lastItemDirectory;
                    break;
            }

            if (modPath == "")
                modPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/r2modmanPlus-local/H3VR/profiles/";

            if (!Directory.Exists(modPath))
                modPath = "";

            switch (loadType)
            {
                case JSONTypeEnum.Faction:
                    paths = StandaloneFileBrowser.OpenFilePanel("SR_Faction", modPath, "json", false);
                    if (paths.Length > 0 && !paths[0].Contains("SR_Faction"))
                    {
                        LogError("Not a valid Faction, check the file name  - " + paths[0]);
                        return;
                    }
                    break;

                case JSONTypeEnum.ItemCategory:
                    paths = StandaloneFileBrowser.OpenFilePanel("SR_IC", modPath, "json", false);
                    if (paths.Length > 0 && !paths[0].Contains("SR_IC"))
                    {
                        LogError("Not a valid Item Category, check the file name  - " + paths[0]);
                        return;
                    }
                    break;

                case JSONTypeEnum.Character:
                default:
                    paths = StandaloneFileBrowser.OpenFilePanel("SR_Character", modPath, "json", false);
                    if (paths.Length > 0 && !paths[0].Contains("SR_Character"))
                    {
                        LogError("Not a valid Character Preset, check the file name  - " + paths[0]);
                        return;
                    }
                    break;
            }

            if (paths.Length > 0)
            {
                switch (loadType)
                {
                    case JSONTypeEnum.Character:
                        lastCharacterDirectory = paths[0];
                        PlayerPrefs.SetString("lastCharacterDirectory", lastCharacterDirectory);
                        break;
                    case JSONTypeEnum.Faction:
                        lastFactionDirectory = paths[0];
                        PlayerPrefs.SetString("lastFactionDirectory", lastFactionDirectory);
                        break;
                    case JSONTypeEnum.ItemCategory:
                        lastItemDirectory = paths[0];
                        PlayerPrefs.SetString("lastItemDirectory", lastItemDirectory);
                        break;
                }

                StartCoroutine(OutputRoutine(new Uri(paths[0]).AbsoluteUri, loadType));
            }
            else
                Log("json loading canceled");
        }

        private IEnumerator OutputRoutine(string url, JSONTypeEnum loadType)
        {
            var loader = new WWW(url);
            yield return loader;

            switch (loadType)
            {
                case JSONTypeEnum.Character:
                    LoadCharacter(loader.text);
                    yield return null;
                    MenuManager.instance.characterLoaded = true;
                    MenuManager.instance.OpenCharacterPanel();
                    MenuManager.instance.RefreshCharacter();

                    url = url.Remove(url.Length - 4) + "png";
                    url = url.Remove(0, 8);
                    MenuManager.instance.characterThumbnail.sprite = LoadSprite(url);
                    break;
                case JSONTypeEnum.Faction:
                    //DataManager.instance.LoadFaction(url);
                    break;
                case JSONTypeEnum.ItemCategory:
                    //DataManager.instance.LoadItemCategory(url);
                    break;
                default:
                    break;
            }
        }

        public void OnSaveDialogue(JSONTypeEnum saveType, string json, string saveName)
        {
            string path;
            switch (saveType)
            {
                case JSONTypeEnum.Faction:
                    path = StandaloneFileBrowser.SaveFilePanel(
                        "Save Faction", 
                        lastFactionDirectory, 
                        "SR_Faction_" + saveName, 
                        "json");
                    break;
                case JSONTypeEnum.ItemCategory:
                    path = StandaloneFileBrowser.SaveFilePanel(
                        "Save Item Category", 
                        lastCharacterDirectory, 
                        "SR_IC_" + saveName, 
                        "json");
                    break;

                case JSONTypeEnum.Character:
                default:
                    path = StandaloneFileBrowser.SaveFilePanel(
                        "Save Character", 
                        lastCharacterDirectory, 
                        "SR_Character_" + saveName, 
                        "json");
                    break;
            }

            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllText(path, json);
                Log("Saved file " + path);
            }
        }

            /*
            public static List<SR_ItemCategory> LoadItemCategories()
            {
                List<string> directories = GetItemCategoriesDirectory();

                if (directories.Count == 0)
                {
                    Debug.LogError("No Item Categories were found!");
                    return null;
                }

                List<SR_ItemCategory> items = new List<SR_ItemCategory>();

                //Load up each of our categories
                for (int i = 0; i < directories.Count; i++)
                {
                    SR_ItemCategory category;

                    //Load each Category via the Directory
                    using (StreamReader streamReader = new StreamReader(directories[i]))
                    {
                        string json = streamReader.ReadToEnd();

                        try
                        {
                            category = JsonUtility.FromJson<SR_ItemCategory>(json);
                        }
                        catch (Exception ex)
                        {
                            Debug.Log(ex.Message);
                            return null;
                        }

                        //Add to our item category pool
                        items.Add(category);
                        string newDirectory = directories[i];
                        newDirectory = newDirectory.Remove(newDirectory.Length - 4) + "png";
                        category.SetupThumbnailPath(newDirectory);

                        Debug.Log("Supply Raid: Loaded Item Category " + category.name);
                    }
                }
                return items;
            }

            public static List<SR_SosigFaction> LoadFactions()
            {
                List<string> directories = GetFactionDirectory();

                if (directories.Count == 0)
                {
                    Debug.LogError("No Factions were found!");
                    return null;
                }

                List<SR_SosigFaction> factions = new List<SR_SosigFaction>();

                //Load up each of our categories
                for (int i = 0; i < directories.Count; i++)
                {
                    SR_SosigFaction faction;

                    //Load each Category via the Directory
                    using (StreamReader streamReader = new StreamReader(directories[i]))
                    {
                        string json = streamReader.ReadToEnd();

                        try
                        {
                            faction = JsonUtility.FromJson<SR_SosigFaction>(json);
                        }
                        catch (Exception ex)
                        {
                            Debug.Log(ex.Message);
                            return null;
                        }

                        //Add to our item category pool
                        factions.Add(faction);
                        string newDirectory = directories[i];
                        newDirectory = newDirectory.Remove(newDirectory.Length - 4) + "png";
                        faction.SetupThumbnailPath(newDirectory);

                        Debug.Log("Supply Raid: Loaded Faction " + faction.name);
                    }
                }
                return factions;
            }
            */
            public SR_CharacterPreset LoadCharacter(string json)
        {
            //Log("Loading Character from " + path);
            // Load each Category via the Directory
            //using (StreamReader streamReader = new StreamReader(path))
            //{
                //string json = streamReader.ReadToEnd();

                try
                {
                    character = JsonUtility.FromJson<SR_CharacterPreset>(json);
                }
                catch (Exception ex)
                {
                    DataManager.Log(ex.Message);
                    return null;
                }

            //Add to our item category pool
            //string newDirectory = path;
            //newDirectory = newDirectory.Remove(newDirectory.Length - 4) + "png";
            if (character != null)
            {
                Log("Loaded Character " + character.name);
            }
            //}

            return character;
        }

        public List<SR_CharacterPreset> LoadCharacters()
        {
            List<string> directories = DataManager.instance.GetCharactersDirectory();

            if (directories.Count == 0)
            {
                DataManager.LogError("No Characters were found!");
                return null;
            }

            List<SR_CharacterPreset> characters = new List<SR_CharacterPreset>();

            //Load up each of our categories
            for (int i = 0; i < directories.Count; i++)
            {
                SR_CharacterPreset character;

                //Load each Category via the Directory
                using (StreamReader streamReader = new StreamReader(directories[i]))
                {
                    string json = streamReader.ReadToEnd();

                    try
                    {
                        character = JsonUtility.FromJson<SR_CharacterPreset>(json);
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex.Message);
                        return null;
                    }

                    //Add to our item category pool
                    characters.Add(character);
                    string newDirectory = directories[i];
                    newDirectory = newDirectory.Remove(newDirectory.Length - 4) + "png";

                    DataManager.Log("Supply Raid: Loaded Character " + character.name);

                }
            }
            return characters;
        }

        public static void LogError(string text)
        {
            instance.debugLog += "\n" + text;
            instance.debugLine.text = text;
            instance.debugLine.color = Color.red;
            Debug.LogError(text);
        }

        public static void Log(string text)
        {
            instance.debugLog += "\n" + text;
            instance.debugLine.text = text;
            instance.debugLine.color = Color.white;
            Debug.Log(text);
        }

        public List<string> GetCharactersDirectory()
        {
            return Directory.GetFiles(modPath, "SR_Character*.json", SearchOption.AllDirectories).ToList();
        }

        public List<string> GetFactionDirectory()
        {
            return Directory.GetFiles(modPath, "SR_Faction*.json", SearchOption.AllDirectories).ToList();
        }

        public List<string> GetItemCategoriesDirectory()
        {
            return Directory.GetFiles(modPath, "SR_IC*.json", SearchOption.AllDirectories).ToList();
        }
    }
    public enum JSONTypeEnum
    {
        Character = 0,
        Faction = 1,
        ItemCategory = 2,
    }
}