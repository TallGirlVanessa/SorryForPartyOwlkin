using HarmonyLib;
using System;
using System.Reflection;
using OWML.Common;
using SorryForPartyOwlkin;
using UnityEngine;
using OWML.ModHelper;
using System.Linq;
using System.Collections.Generic;

[HarmonyPatch]
public class PartyMusicControllerPatch
{
    private static OWAudioSource OWAudioSource_PartyOwlkin;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PartyMusicController), nameof(PartyMusicController.Start))]
    [HarmonyPatch(typeof(PartyMusicController), nameof(PartyMusicController.FadeIn))]
    [HarmonyPatch(typeof(PartyMusicController), nameof(PartyMusicController.FadeOut))]
    [HarmonyPatch(typeof(PartyMusicController), nameof(PartyMusicController.StaggerStop))]
    public static void PartyMusicController_MethodLogger(PartyMusicController __instance, MethodBase __originalMethod, object[] __args)
    {
        var allArgs = StringFromArgs(__args);
        SorryForPartyOwlkinMod.Instance.ModHelper.Console.WriteLine(
            $"PartyMusicController method: `{__originalMethod.Name}` just called, with args `{allArgs}`.",
            MessageType.Success
        );
    }

    public static void Initialize(GameObject partyMusic_SorryForPartyOwlkin)
    {
        OWAudioSource_PartyOwlkin = partyMusic_SorryForPartyOwlkin.GetComponent<OWAudioSource>();
        SorryForPartyOwlkinMod.Instance.ModHelper.Console.WriteLine(
            $"Got OWAudioSource: `{OWAudioSource_PartyOwlkin}`",
            MessageType.Success
        );
    }

    private static string StringFromArgs(object[] args)
    {
        var argStrings = args.Select(x => x.ToString());
        var allArgs = String.Join(", ", argStrings);
        return allArgs;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PartyMusicController), nameof(PartyMusicController.Start))]
    public static void PartyMusicController_Start()
    {
        // I tried adding our new OWAudioSource to __instance._instrumentSources
        // But everything in there needs to be in the AudioLibrary, and our custom
        // AudioClip is not. So we just patch all the relevant methods and duplicate what
        // PartyMusicController does while it loops through the base game sources.
        OWAudioSource_PartyOwlkin.SetLocalVolume(0f);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PartyMusicController), nameof(PartyMusicController.FadeIn))]
    public static void PartyMusicController_FadeIn(float duration)
    {
        OWAudioSource_PartyOwlkin.Stop();
        // The library for the 4 base game sources uses 0.4 for the volume
        // See AudioLibrary.audioEntries indices 664, 665, 666, 667
        OWAudioSource_PartyOwlkin.FadeIn(duration, false, false, 0.4f);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PartyMusicController), nameof(PartyMusicController.FadeOut))]
    public static void PartyMusicController_FadeOut(float duration)
    {
        OWAudioSource_PartyOwlkin.FadeOut(duration, OWAudioSource.FadeOutCompleteAction.STOP, 0f);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PartyMusicController), nameof(PartyMusicController.Update))]
    public static void PartyMusicController_Update(PartyMusicController __instance)
    {
        if (!OWAudioSource_PartyOwlkin.isPlaying)
        {
            __instance.enabled = false;
        } else if (!OWAudioSource_PartyOwlkin.IsFadingOut() && Time.time >= __instance._stopTime)
        {
            OWAudioSource_PartyOwlkin.FadeOut(0.5f, OWAudioSource.FadeOutCompleteAction.STOP, 0f);
        }
    }
}
