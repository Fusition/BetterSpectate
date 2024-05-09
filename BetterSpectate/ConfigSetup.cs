using BepInEx;
using BepInEx.Configuration;
using BetterSpectate.Patches;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace BetterSpectate
{
    public class ConfigSetup
    {
        private const string CONFIG_FILE_NAME = "Fusition.BetterSpectate";
        private static ConfigFile config;

        //Config initialization
        ConfigEntry<bool> isZoomEnabled;
        ConfigEntry<float> maxZoomDistance;
        ConfigEntry<float> minZoomDistance;
        ConfigEntry<float> defaultZoomDistance;
        ConfigEntry<float> zoomSpeed;
        ConfigEntry<bool> isFirstPersonEnabled;
        ConfigEntry<bool> isFirstPersonDefault;
        ConfigEntry<string> firstPersonKeybind;

        public static void Initialize()
        {
            string path = Path.Combine(Paths.ConfigPath, CONFIG_FILE_NAME + ".cfg");
            config = new ConfigFile(path, true);
            //Config Entry Initialization
            ConfigEntry<bool> isZoomEnabled = config.Bind<bool>("Zoom Settings", "Third Person Zoom Enabled", true, "Allows spectators to zoom in and out.");
            ConfigEntry<float> maxZoomDistance = config.Bind<float>("Zoom Settings", "Max Zoom Distance", 15f, "Furthest distance a spectator can zoom out.");
            ConfigEntry<float> minZoomDistance = config.Bind<float>("Zoom Settings", "Min Zoom Distance", 1f, "Closest distance a spectator can zoom in.");
            ConfigEntry<float> defaultZoomDistance = config.Bind<float>("Zoom Settings", "Default Zoom Distance", 1.4f, "Distance a spectator is set upon death.");
            ConfigEntry<float> zoomSpeed = config.Bind<float>("Zoom Settings", "Zoom Speed", 0.4f, "Speed that scrolling will zoom in or out.");
            ConfigEntry<bool> isFirstPersonEnabled = config.Bind<bool>("General Settings", "First Person Spectate Enabled", true, "Allows spectators to enter a first person view.");
            ConfigEntry<bool> isFirstPersonDefault = config.Bind<bool>("General Settings", "Default to First Person", true, "Defaults players to a first person view on death.");
            ConfigEntry<string> firstPersonKeybind = config.Bind<string>("General Settings", "First Person Keybind", "P", "Sets the keybind to switch between perspectives.");
            
            //Copy values from config entries to where they are used
            PlayerControllerB_Patch.SetFirstPersonEnabled(isFirstPersonEnabled.Value);
            PlayerControllerB_Patch.SetFirstPersonToggle(isFirstPersonDefault.Value);
            PlayerControllerB_Patch.InitializeFirstPersonSpectateInputAction("<Keyboard>/" + firstPersonKeybind.Value);
            PlayerControllerB_Patch.SetZoomEnabled(isZoomEnabled.Value);
            PlayerControllerB_Patch.SetZoomDistance(defaultZoomDistance.Value);
            PlayerControllerB_Patch.SetDefaultZoomDistance(defaultZoomDistance.Value);
            PlayerControllerB_Patch.SetZoomSpeed(zoomSpeed.Value);
            SpectateUtils.SetMaxZoom(maxZoomDistance.Value);
            SpectateUtils.SetMinZoom(minZoomDistance.Value);
        }
    }
}
