using HarmonyLib;
using System;
using System.Reflection;
using OWML.Common;
using SorryForPartyOwlkin;

[HarmonyPatch]
public class PartyMusicControllerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PartyMusicController), nameof(PartyMusicController.Start))]
    [HarmonyPatch(typeof(PartyMusicController), nameof(PartyMusicController.FadeIn))]
    [HarmonyPatch(typeof(PartyMusicController), nameof(PartyMusicController.FadeOut))]
    [HarmonyPatch(typeof(PartyMusicController), nameof(PartyMusicController.StaggerStop))]
    public static void PartyMusicController_MethodLogger(MethodBase __originalMethod, object[] __args)
    {
        SorryForPartyOwlkinMod.Instance.ModHelper.Console.WriteLine(
            $"PartyMusicController method: `{__originalMethod.Name}` called with args `{__args}`",
            MessageType.Success
        );
    }
}
