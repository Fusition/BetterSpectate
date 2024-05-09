using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BetterSpectate.Compatibility
{
    public static class SpectateEnemyCompat
    {
        private static bool? _enabled;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(SpectateEnemy.MyPluginInfo.PLUGIN_GUID);
                }
                return (bool)_enabled;
            }
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void CheckIfSpectatingEnemies()
        {
            if (SpectateEnemy.SpectateEnemiesAPI.IsLoaded && SpectateEnemy.SpectateEnemiesAPI.IsSpectatingEnemies)
            {
                Patches.PlayerControllerB_Patch.SetFirstPersonToggle(false);
                Patches.PlayerControllerB_Patch.SetInputDisabled(true);
            }
        }
    }
}
