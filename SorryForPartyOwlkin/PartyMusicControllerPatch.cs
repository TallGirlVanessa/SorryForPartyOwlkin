using HarmonyLib;
using System;
using System.Reflection;
using OWML.Common;
using SorryForPartyOwlkin;
using UnityEngine;
using System.Linq;
using OWML.Utils;

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
            MessageType.Debug
        );
        CheckPartyMusicControllerFields(__instance);
    }

    public static void Initialize(GameObject partyMusic_SorryForPartyOwlkin)
    {
        OWAudioSource_PartyOwlkin = partyMusic_SorryForPartyOwlkin.GetComponent<OWAudioSource>();
        SorryForPartyOwlkinMod.Instance.ModHelper.Console.WriteLine(
            $"Got OWAudioSource: `{OWAudioSource_PartyOwlkin}`",
            MessageType.Debug
        );
    }

	[HarmonyPrefix]
    [HarmonyPatch(typeof(PartyMusicController), nameof(PartyMusicController.Start))]
    public static void PartyMusicController_Start(PartyMusicController __instance)
    {
        __instance._instrumentSources = [OWAudioSource_PartyOwlkin];
        __instance._stopDelays = [0f];
        SorryForPartyOwlkinMod.Instance.ModHelper.Console.WriteLine(
            "PartyMusicController sources replaced with custom OWAudioSource");
        CheckPartyMusicControllerFields(__instance);

    }

    private static string StringFromArgs(object[] args)
    {
        var argStrings = args.Select(x => x.ToString());
        var allArgs = String.Join(", ", argStrings);
        return allArgs;
    }
    private static void CheckPartyMusicControllerFields(PartyMusicController partyMusicController)
    {
        var sourceNames = partyMusicController._instrumentSources.Select(source => source.name);
        var sourceNameString = String.Join(", ", sourceNames);
        var delaysString = String.Join(", ", partyMusicController._stopDelays);
        SorryForPartyOwlkinMod.Instance.ModHelper.Console.WriteLine(
            "PartyMusicController fields check:\n" +
            $"Sources: `{sourceNameString}`\n" +
            $"Stop delays: `{delaysString}`",
            MessageType.Debug
        );
    }
}
