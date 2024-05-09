using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BetterSpectate.Patches;

namespace BetterSpectate
{
    public class BetterSpectateAPI
    {
        public bool isModLoaded => BetterSpectateBase.instance != null;
        public bool isFirstPersonSpectateEnabled => PlayerControllerB_Patch.GetFirstPersonEnabled();
        public bool isZoomEnabled => PlayerControllerB_Patch.GetZoomEnabled();
        public bool isFirstPersonSpectating => PlayerControllerB_Patch.IsPlayerInFirstPerson();
        public float zoomDistance => PlayerControllerB_Patch.GetZoomDistance();
        public float zoomSpeed => PlayerControllerB_Patch.GetZoomSpeed();
        public float maxZoom => SpectateUtils.GetMaxZoom();
        public float minZoom => SpectateUtils.GetMinZoom();
    }
}
