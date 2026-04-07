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
        }

        public void Start()
        {
            // Starting here, you'll have access to OWML's mod helper.
            ModHelper.Console.WriteLine($"Initializing {nameof(SorryForPartyOwlkinMod)}");

            // Get the New Horizons API and load configs
            NewHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            NewHorizons.LoadConfigs(this);

            new Harmony("TallGirlVanessa.SorryForPartyOwlkin").PatchAll(Assembly.GetExecutingAssembly());

            // Example of accessing game code.
            OnCompleteSceneLoad(OWScene.TitleScreen, OWScene.TitleScreen); // We start on title screen
            LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;

            // Create the new audio entry, and queue it for adding to the library
            CreateCustomAudioEntry("SFPR", "copyrighted/Sorry For Party Rocking.ogg", 0.4f);

            // New Horizons done loading solar system
            UnityEvent<string> systemLoaded = NewHorizons.GetStarSystemLoadedEvent();
            systemLoaded.AddListener(GetNewHorizonsSystemAdditions);
        }

        public void OnCompleteSceneLoad(OWScene previousScene, OWScene newScene)
        {
            ModHelper.Console.WriteLine($"OnCompleteSceneLoad {newScene}", MessageType.Debug);
            if (newScene == OWScene.SolarSystem)
            {
                // Custom enum items are blown away on scene change so we need to recreate it
                CreateCustomAudioType("SFPR");
            }
        }

        public void OnNewHorizonsChangeSystems(string systemName)
        {
            if (systemName != "SolarSystem")
            {
                return;
            }
        }

        public AudioType CreateCustomAudioType(string id)
        {
            // Add custom audio type, if not present
            // Custom enums get blown away on scene change, so we need to do this repeatedly
            var alreadyExists = EnumUtils.TryParse(id, out AudioType parsedType);
            if (alreadyExists)
            {
                return parsedType;
            }
            ModHelper.Console.WriteLine($"Creating new AudioType with name: `{id}`", MessageType.Debug);
            var newAudioType = EnumUtilities.Create<AudioType>(id);
            return newAudioType;
        }

        public void CreateCustomAudioEntry(string id, string path, float libraryVolume)
        {
            // Create new AudioEntry associating the new type with the new clip.
            // Add it to the library, when the library is ready
            // audioEntries persists, so we should only do this once
            var audioType = CreateCustomAudioType(id);
            ModHelper.Console.WriteLine($"Loading new audio clip with AudioType `{id}` at relative path `{path}`", MessageType.Debug);
            AudioClip audioClip = AudioUtilities.LoadAudio(Path.Combine(this.ModHelper.Manifest.ModFolderPath, path));
            NewAudioEntry = new AudioLibrary.AudioEntry(audioType, [audioClip], libraryVolume);
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
            ModHelper.Console.WriteLine("Adding new AudioEntry to AudioLibrary and AudioManager", MessageType.Debug);
            AudioLibrary libraryAsset = Locator.GetAudioManager()._libraryAsset;
            libraryAsset.audioEntries = libraryAsset.audioEntries.AddToArray(NewAudioEntry);
            Locator.GetAudioManager()._audioLibraryDict = libraryAsset.BuildAudioEntryDictionary();
        }

        public void GetNewHorizonsSystemAdditions(string systemName)
        {
            if (systemName != "SolarSystem")
            {
                return;
            }
            ModHelper.Console.WriteLine($"New Horizons customizations complete for system `{systemName}`, time to find our custom AudioSource", MessageType.Debug);
            GameObject newSource = GetNewAudioSource();
            PartyMusicControllerPatch.Initialize(newSource);
        }

        public GameObject GetNewAudioSource()
        {
            ModHelper.Console.WriteLine("Trying to get the GameObject", MessageType.Debug);
            GameObject foundObject = GameObject.Find(
                "DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_1/Ghosts_DreamZone_1/GhostsDirector_PartyHavers/PartyMusicController/PartyMusic_SorryForPartyOwlkin"
            );
            ModHelper.Console.WriteLine($"Got the GameObject! Its name is `{foundObject.name}`", MessageType.Debug);
            ModHelper.Console.WriteLine("Ready to add custom AudioSource to PartyMusicController");
            return foundObject;
        }
    }

}
