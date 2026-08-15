using HarmonyLib;
using RecRoom.Core.Combat;
using UnityEngine;

namespace RecRoomPSVR2.Patches
{
    [HarmonyPatch]
    public static class PlayerImpactPatch
    {
        [HarmonyPatch(typeof(Bullet.CEFAOBHGLKL), nameof(Bullet.CEFAOBHGLKL.Invoke))]
        [HarmonyPostfix]
        public static void Postfix(Bullet EDPPMBPBAAC, Player EDIBICHIJJH, Player.GAHEJKDCLLE EAMJHCGHCIE)
        {
            if (!Plugin.hmdRumble.Value)
            {
                return;
            }
            
            if (EDIBICHIJJH == Player.MDMMDPEKICF && EAMJHCGHCIE == Player.GAHEJKDCLLE.Head)
            {
                Plugin.HeadshotHMDFeedback();
            }
        }
    }
}