using BepInEx;
using GameNetcodeStuff;
using HarmonyLib;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BetterSpectate.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    public class PlayerControllerB_Patch
    {
        /*
         * Zoom related variables
         */
        private static bool isZoomEnabled;
        private static float zoomDistance = 1.4f;
        private static float zoomSpeed = 0.4f;
        private static float defaultZoomDistance;

        /*
         * First Person related variables
         */
        private static bool isFirstPersonEnabled;
        private static InputAction firstPersonSpectateAction;
        private static bool firstPersonSpectateToggle = false;
        private static bool isInFirstPerson = false;
        private static bool inputDisabledForCompat = false;

        /*
         * Patch Methods
         */
        [HarmonyPatch(typeof(PlayerControllerB), "LateUpdate")]
        [HarmonyPrefix]
        public static void LateUpdate_Patch(PlayerControllerB __instance)
        {
            if (__instance == GameNetworkManager.Instance.localPlayerController && __instance.spectatedPlayerScript != null)
            {
                if (__instance.spectatedPlayerScript.isHoldingObject) // Corrects item position for first person every game tick. NOTE: Redo this more efficiently later
                    __instance.spectatedPlayerScript.currentlyHeldObjectServer.parentObject = firstPersonSpectateToggle ? __instance.spectatedPlayerScript.localItemHolder.transform : __instance.spectatedPlayerScript.serverItemHolder.transform;
                if (UnityInput.Current.mouseScrollDelta.y != 0f && isZoomEnabled)
                {
                    __instance.thisPlayerModel.enabled = false;
                    SetZoomDistance(SpectateUtils.ZoomClamp(zoomDistance + UnityInput.Current.mouseScrollDelta.y / -120f * zoomSpeed));
                }
                if (Compatibility.SpectateEnemyCompat.enabled)
                    Compatibility.SpectateEnemyCompat.CheckIfSpectatingEnemies();
                if (!inputDisabledForCompat && firstPersonSpectateAction.WasPressedThisFrame())
                {
                    SwitchPerspective(__instance);
                }
            }
        }
        [HarmonyPatch(typeof(PlayerControllerB), "RaycastSpectateCameraAroundPivot")]
        [HarmonyPrefix]
        public static bool RaycastSpectateCameraAroundPivot_Patch(PlayerControllerB __instance, RaycastHit ___hit, int ___walkableSurfacesNoPlayersMask)
        {
            if (__instance.spectatedPlayerScript != null)
            {
                Transform spectatedVisorTransform = __instance.spectatedPlayerScript.visorCamera.transform;

                if (isFirstPersonEnabled && firstPersonSpectateToggle) //Spectator camera placement logic
                {
                    __instance.playersManager.spectateCamera.transform.position = spectatedVisorTransform.position;
                    __instance.playersManager.spectateCamera.transform.rotation = spectatedVisorTransform.rotation;
                    isInFirstPerson = true;
                }
                else if (isZoomEnabled)
                {
                    RaycastCameraToZoomDistance(__instance, ___hit, ___walkableSurfacesNoPlayersMask);
                }
                return false;
            }
            return true;
        }
        [HarmonyPatch(typeof(PlayerControllerB), "SpectateNextPlayer")]
        [HarmonyPrefix]
        public static bool SpectateNextPlayer_Patch(PlayerControllerB __instance, RaycastHit ___hit, int ___walkableSurfacesNoPlayersMask)
        {
            int num = 0;
            if (__instance.spectatedPlayerScript != null)
            {
                num = (int)__instance.spectatedPlayerScript.playerClientId;

                if (__instance == GameNetworkManager.Instance.localPlayerController) //Resets the currently spectated person's model visibility
                {
                    SetModelVisibilityForThirdPerson(__instance.spectatedPlayerScript);
                }
            }
            for (int i = 0; i < __instance.playersManager.allPlayerScripts.Length; i++) //Iterates through all players and finds the first available one to spectate
            {
                num = (num + 1) % __instance.playersManager.allPlayerScripts.Length;
                if (!__instance.playersManager.allPlayerScripts[num].isPlayerDead && __instance.playersManager.allPlayerScripts[num].isPlayerControlled && __instance.playersManager.allPlayerScripts[num] != __instance)
                {
                    __instance.spectatedPlayerScript = __instance.playersManager.allPlayerScripts[num];
                    if (__instance == GameNetworkManager.Instance.localPlayerController)
                    {
                        if (!firstPersonSpectateToggle)
                        {
                            BetterSpectateBase.fusLogSource.LogInfo("Model visibility adjusted for dead player in third person");
                            SetModelVisibilityForThirdPerson(__instance.spectatedPlayerScript);
                        }
                        else
                        {
                            BetterSpectateBase.fusLogSource.LogInfo("Model visibility adjusted for dead player in first person");
                            SetModelVisibilityForFirstPerson(__instance.spectatedPlayerScript);
                        }
                    }
                    __instance.SetSpectatedPlayerEffects();
                    return false;
                }
            }
            if (__instance.deadBody != null && __instance.deadBody.gameObject.activeSelf)
            {
                __instance.spectateCameraPivot.position = __instance.deadBody.bodyParts[0].position;
                RaycastSpectateCameraAroundPivot_Patch(__instance, ___hit, ___walkableSurfacesNoPlayersMask);
            }
            StartOfRound.Instance.SetPlayerSafeInShip();
            return false;
        }

        /*
         * First Person Related Methods
         */
        public static void InitializeFirstPersonSpectateInputAction(string binding)
        {
            firstPersonSpectateAction = new InputAction("FirstPersonSpectatePressed", (InputActionType)0, binding);
            firstPersonSpectateAction.Enable();
        }
        public static bool IsPlayerInFirstPerson()
        {
            return isInFirstPerson;
        }
        public static bool GetZoomEnabled()
        {
            return isZoomEnabled;
        }
        public static void SetZoomEnabled(bool enabled)
        {
            isZoomEnabled = true;
        }
        public static bool GetFirstPersonEnabled()
        {
            return isFirstPersonEnabled;
        }
        public static void SetFirstPersonEnabled(bool enabled)
        {
            isFirstPersonEnabled = true;
        }
        public static void SwitchPerspective(PlayerControllerB controller)
        {
            firstPersonSpectateToggle = !firstPersonSpectateToggle;

            if (!firstPersonSpectateToggle) //Spectate View Toggle Functionality
            {
                BetterSpectateBase.fusLogSource.LogInfo("Player Toggled to Third Person");
                SetModelVisibilityForThirdPerson(controller.spectatedPlayerScript);
                controller.spectateCameraPivot.transform.rotation = controller.spectatedPlayerScript.visorCamera.transform.rotation;
                zoomDistance = defaultZoomDistance;
            }
            else
            {
                BetterSpectateBase.fusLogSource.LogInfo("Player Toggled to First Person");
                SetModelVisibilityForFirstPerson(controller.spectatedPlayerScript);
            }
        }
        public static void SetFirstPersonToggle(bool value)
        {
            firstPersonSpectateToggle = value;
        }
        public static void SetModelVisibilityForFirstPerson(PlayerControllerB controller)
        {
            controller.thisPlayerModelArms.enabled = true;
            controller.thisPlayerModel.enabled = false;
            controller.thisPlayerModelLOD1.enabled = false;
            controller.thisPlayerModelLOD2.enabled = false;
        }
        public static void SetModelVisibilityForThirdPerson(PlayerControllerB controller)
        {
            controller.thisPlayerModelArms.enabled = false;
            controller.thisPlayerModel.enabled = true;
            controller.thisPlayerModelLOD1.enabled = true;
            controller.thisPlayerModelLOD2.enabled = true;
        }

        /*
         * Camera Zoom Methods
         */
        public static float GetZoomDistance ()
        {
            return zoomDistance;
        }
        public static void SetZoomDistance(float value)
        {
            zoomDistance = value;
        }
        public static void SetDefaultZoomDistance(float value)
        {
            defaultZoomDistance = value;
        }
        public static float GetZoomSpeed()
        {
            return zoomSpeed;
        }
        public static void SetZoomSpeed(float value)
        {
            zoomSpeed = value;
        }
        public static void SetInputDisabled(bool value)
        {
            inputDisabledForCompat = value;
        }
        private static void RaycastCameraToZoomDistance (PlayerControllerB controller, RaycastHit hit, int walkableSurfacesNoPlayersMask)
        {
            Ray interactRay = new Ray(controller.spectateCameraPivot.position, -controller.spectateCameraPivot.forward);
            if (Physics.Raycast(interactRay, out hit, zoomDistance, walkableSurfacesNoPlayersMask, QueryTriggerInteraction.Ignore))
            {
                controller.playersManager.spectateCamera.transform.position = interactRay.GetPoint(hit.distance - 0.25f);
            }
            else
            {
                controller.playersManager.spectateCamera.transform.position = interactRay.GetPoint(zoomDistance - 0.1f);
            }
            controller.playersManager.spectateCamera.transform.LookAt(controller.spectateCameraPivot);
        }
    }
    static class SpectateUtils
    {
        /*
         * ZoomClamp Parameters
         */
        private static float maxZoomDistance;
        private static float minZoomDistance;
        public static float ZoomClamp (this float value)
        {
            if (maxZoomDistance > minZoomDistance)
            {
                if (value > maxZoomDistance) return maxZoomDistance;
                else if (value < minZoomDistance) return minZoomDistance;
                else return value;
            }
            else return PlayerControllerB_Patch.GetZoomDistance();
        }
        public static float GetMaxZoom()
        {
            return maxZoomDistance;
        }
        public static void SetMaxZoom(float value)
        {
            maxZoomDistance = value;
        }
        public static float GetMinZoom()
        {
            return minZoomDistance;
        }
        public static void SetMinZoom(float value)
        {
            minZoomDistance = value;
        }
    }
}
