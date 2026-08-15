using HarmonyLib;
using RecRoom.Core.Combat;
using UnityEngine;

namespace RecRoomPSVR2.Patches
{
    [HarmonyPatch]
    public static class BulletPatch
    {
        [HarmonyPatch(typeof(Bullet), nameof(Bullet.KNMMJKDHJLM))]
        [HarmonyPostfix]
        public static void HitPostfix(Bullet __instance, Player EDIBICHIJJH, Player.GAHEJKDCLLE EAMJHCGHCIE, GHMDIDJLFMC KNLMJBAKCGF)
        {
            Debug.Log("PLEASWE WOPTRK");

            if (EDIBICHIJJH == Player.MDMMDPEKICF && EAMJHCGHCIE == Player.GAHEJKDCLLE.Head)
            {
                Debug.Log("headshot maybve idpsfp;lk");
                Plugin.HeadshotHMDFeedback();
            }
        }
    }
}