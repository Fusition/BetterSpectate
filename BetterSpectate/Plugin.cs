using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BetterSpectate.Patches;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

namespace BetterSpectate
{
    [BepInPlugin(MOD_GUID, MOD_NAME, MOD_VERS)]
    [BepInDependency(SpectateEnemy.MyPluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class BetterSpectateBase : BaseUnityPlugin
    {
        public const string MOD_GUID = "Fusition.BetterSpectate";
        public const string MOD_NAME = "BetterSpectate";
        public const string MOD_VERS = "1.0.0.0";

        private readonly Harmony harmony = new Harmony(MOD_GUID);
        public static BetterSpectateBase instance;

        internal static ManualLogSource fusLogSource;
        
        void Awake ()
        {
            if (instance == null)
            {
                instance = this;
            }


            this.gameObject.hideFlags = HideFlags.HideAndDontSave;

            fusLogSource = BepInEx.Logging.Logger.CreateLogSource(BetterSpectateBase.MOD_GUID);

            fusLogSource.LogInfo(Compatibility.SpectateEnemyCompat.enabled);
            ConfigSetup.Initialize();

            harmony.PatchAll(typeof(Patches.PlayerControllerB_Patch));
            harmony.PatchAll(typeof(Patches.StartOfRound_Patch));
        }
    }
}
