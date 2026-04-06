using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace SorryForPartyOwlkin
{
    public class SorryForPartyOwlkinMod : ModBehaviour
    {
        public static SorryForPartyOwlkinMod Instance;
        public INewHorizons NewHorizons;

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
            ModHelper.Console.WriteLine($"My mod {nameof(SorryForPartyOwlkinMod)} is loaded!", MessageType.Success);

            // Get the New Horizons API and load configs
            NewHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            NewHorizons.LoadConfigs(this);

            new Harmony("TallGirlVanessa.SorryForPartyOwlkin").PatchAll(Assembly.GetExecutingAssembly());

            // Example of accessing game code.
            OnCompleteSceneLoad(OWScene.TitleScreen, OWScene.TitleScreen); // We start on title screen
            LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;

            // Vanessa's code follows
            UnityEvent<string> systemLoaded = NewHorizons.GetStarSystemLoadedEvent();
            systemLoaded.AddListener(GetNewHorizonsSystemAdditions);
        }

        public void OnCompleteSceneLoad(OWScene previousScene, OWScene newScene)
        {
            if (newScene != OWScene.SolarSystem) return;
            ModHelper.Console.WriteLine("Loaded into solar system!", MessageType.Success);
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
