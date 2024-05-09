using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterSpectate.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    public class StartOfRound_Patch
    {
        [HarmonyPatch(typeof(StartOfRound), "ReviveDeadPlayers")]
        [HarmonyPostfix]
        public static void ReviveDeadPlayers_Patch(PlayerControllerB[] ___allPlayerScripts)
        {
            for (int i = 0; i < ___allPlayerScripts.Length; i++)
            {
                if (___allPlayerScripts[i] != GameNetworkManager.Instance.localPlayerController) {
                    ___allPlayerScripts[i].thisPlayerModelArms.enabled = false;
                    ___allPlayerScripts[i].thisPlayerModel.enabled = true;
                    ___allPlayerScripts[i].thisPlayerModelLOD1.enabled = true;
                    ___allPlayerScripts[i].thisPlayerModelLOD2.enabled = true;
                    if (___allPlayerScripts[i].isHoldingObject)
                        ___allPlayerScripts[i].currentlyHeldObjectServer.parentObject = ___allPlayerScripts[i].serverItemHolder.transform;
                }
            }
        }
    }
}
