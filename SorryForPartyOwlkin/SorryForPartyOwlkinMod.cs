using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using NewHorizons.Utility.OWML;
using NewHorizons.Utility.Files;
using OWML.Utils;
using NewHorizons.Handlers;
using System.IO;

namespace SorryForPartyOwlkin
{
    public class SorryForPartyOwlkinMod : ModBehaviour
    {
        public static SorryForPartyOwlkinMod Instance;
        public INewHorizons NewHorizons;
        public static AudioLibrary.AudioEntry NewAudioEntry;
        public static bool EntryAdded = false;

        public void Awake()
        {
            Instance = this;
            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.

            // Extra NH logging
            NHLogger.UpdateLogLevel(NHLogger.LogType.Verbose);

        }

        public void Start()
        {
            // Starting here, you'll have access to OWML's mod helper.
            ModHelper.Console.WriteLine($"My mod {nameof(SorryForPartyOwlkinMod)} is loaded!", MessageType.Success);

            // Get the New Horizons API and load configs
            NewHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            NewHorizons.LoadConfigs(this);

            new Harmony("TallGirlVanessa.SorryForPartyOwlkin").PatchAll(Assembly.GetExecutingAssembly());

            // Example of accessing game code.
            OnCompleteSceneLoad(OWScene.TitleScreen, OWScene.TitleScreen); // We start on title screen
            LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;

            // New Horizons done loading title screen
            // UnityEvent<string, int> titleLoaded = NewHorizons.GetTitleScreenLoadedEvent();
            // titleLoaded.AddListener(OnNewHorizonsTitleScreenLoaded);

            // New Horizons switching systems
            // UnityEvent<string> changeSystem = NewHorizons.GetChangeStarSystemEvent();
            // changeSystem.AddListener(OnNewHorizonsChangeSystems);
            // Add custom audio type when the audio library is ready
            // Delay.RunWhen(
            //     () => Locator.GetAudioManager()?._libraryAsset != null && Locator.GetAudioManager()?._audioLibraryDict != null,
            //     AddCustomAudioType
            // );
            // Create the new audio entry, and queue it for adding to the library
            CreateCustomAudioEntry();

            // New Horizons done loading solar system
            UnityEvent<string> systemLoaded = NewHorizons.GetStarSystemLoadedEvent();
            systemLoaded.AddListener(GetNewHorizonsSystemAdditions);
        }

        public void OnCompleteSceneLoad(OWScene previousScene, OWScene newScene)
        {
            ModHelper.Console.WriteLine($"OnCompleteSceneLoad {newScene}", MessageType.Success);
            if (newScene == OWScene.SolarSystem)
            {
                // Custom enum items are blown away on scene change so we need to recreate it
                CreateCustomAudioType();
            }

        }

        public void OnNewHorizonsChangeSystems(string systemName)
        {
            if (systemName != "SolarSystem")
            {
                return;
            }
        }

        // public void AddCustomAudioType()
        // {
        //     // Add custom audio type
        //     var audioClip = AudioUtilities.LoadAudio("C:/Users/vlmph/AppData/Roaming/OuterWildsModManager/OWML/Mods/TallGirlVanessa.SorryForPartyOwlkin/copyrighted/Sorry For Party Rocking.ogg");
        //     ModHelper.Console.WriteLine($"New audio clip loaded: {audioClip}", MessageType.Success);
        //     AudioType audioType = AudioTypeHandler.AddCustomAudioType("SFPR", [audioClip]);
        //     ModHelper.Console.WriteLine($"New audio type created: {audioType}", MessageType.Success);
        // }
        public AudioType CreateCustomAudioType()
        {
            // Add custom audio type
            ModHelper.Console.WriteLine($"Creating new audio type", MessageType.Success);
            var result = EnumUtils.TryParse("SFPR", out AudioType parsedType);
            ModHelper.Console.WriteLine($"Audio type parse test. Result: `{result}` Output: `{parsedType}`", MessageType.Success);
            var audioType = EnumUtilities.Create<AudioType>("SFPR");
            ModHelper.Console.WriteLine($"New audio type created: {audioType}", MessageType.Success);
            result = EnumUtils.TryParse("SFPR", out parsedType);
            ModHelper.Console.WriteLine($"Audio type parse test. Result: `{result}` Output: `{parsedType}`", MessageType.Success);
            return audioType;
        }

        public void CreateCustomAudioEntry()
        {
            var audioType = CreateCustomAudioType();
            AudioClip audioClip = AudioUtilities.LoadAudio(Path.Combine(this.ModHelper.Manifest.ModFolderPath, "copyrighted/Sorry For Party Rocking.ogg"));
            ModHelper.Console.WriteLine($"New audio clip loaded: {audioClip}", MessageType.Success);
            NewAudioEntry = new AudioLibrary.AudioEntry(audioType, [audioClip], 0.4f);
            ModHelper.Console.WriteLine($"New audio entry created: {NewAudioEntry}", MessageType.Success);
            Delay.RunWhen(
                () => Locator.GetAudioManager()?._libraryAsset != null && Locator.GetAudioManager()?._audioLibraryDict != null,
                AddEntryToLibrary
            );
        }

        public void AddEntryToLibrary()
        {
            // Adapted from NewHorizons.Handlers.AudioTypeHelper
            // Except we're a little more evil, we persist our addition to audioEntries
            // This is required to prevent AudioTypeHandler from blowing away our changes
            AudioLibrary libraryAsset = Locator.GetAudioManager()._libraryAsset;
            ModHelper.Console.WriteLine($"Old audioEntries length: {libraryAsset.audioEntries.Length}", MessageType.Success);
            libraryAsset.audioEntries = libraryAsset.audioEntries.AddToArray(NewAudioEntry);
            Locator.GetAudioManager()._audioLibraryDict = libraryAsset.BuildAudioEntryDictionary();
            ModHelper.Console.WriteLine($"New audioEntries length: {libraryAsset.audioEntries.Length}", MessageType.Success);
            var result = EnumUtils.TryParse("SFPR", out AudioType parsedType);
            ModHelper.Console.WriteLine($"Audio type parse test. Result: `{result}` Output: `{parsedType}`", MessageType.Success);
        }

        public void GetNewHorizonsSystemAdditions(string systemName)
        {
            if (systemName != "SolarSystem")
            {
                return;
            }
            ModHelper.Console.WriteLine($"New Horizons customizations complete for system `{systemName}`", MessageType.Success);
            GameObject newSource = GetNewAudioSource();
            PartyMusicControllerPatch.Initialize(newSource);
        }

        public GameObject GetNewAudioSource()
        {
            ModHelper.Console.WriteLine("Trying to get the GameObject", MessageType.Info);
            GameObject foundObject = GameObject.Find(
                "DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_1/Ghosts_DreamZone_1/GhostsDirector_PartyHavers/PartyMusicController/PartyMusic_SorryForPartyOwlkin"
            );
            ModHelper.Console.WriteLine($"Got the GameObject! Its name is `{foundObject.name}`", MessageType.Success);
            return foundObject;
        }
    }

}
