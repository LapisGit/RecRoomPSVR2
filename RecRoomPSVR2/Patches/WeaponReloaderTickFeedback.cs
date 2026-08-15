using HarmonyLib;
using RecRoom.Tools.Weapons;
using UnityEngine;

namespace RecRoomPSVR2.Patches
{
    [HarmonyPatch]
    public class WeaponReloaderTickFeedback
    {
        [HarmonyPatch(typeof(WeaponReloader), "ICJOIHKKABO")]
        [HarmonyPostfix]
        public static void TickPostfix(WeaponReloader __instance)
        {
            if (__instance._tool.BHNPNHMILIF == Player.MDMMDPEKICF)
            {
                Plugin.TickFeedback(__instance._tool);   
            }
        }
    }
}