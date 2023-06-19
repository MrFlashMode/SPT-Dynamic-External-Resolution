﻿using System.Collections.Generic;
using System.Reflection;
using Aki.Reflection.Patching;
using DynamicExternalResolution.Configs;
using Comfort.Common;
using EFT;
using EFT.CameraControl;
using UnityEngine;
using EFT.Settings.Graphics;

namespace DynamicExternalResolution
{
    class Patcher
    {
        public static void PatchAll()
        {
            new PatchManager().RunPatches();
        }

        public static void UnpatchAll()
        {
            new PatchManager().RunUnpatches();
        }
    }

    public class PatchManager
    {

        public PatchManager()
        {
            this._patches = new List<ModulePatch>
            {
                new DynamicExternalResolutionPatches.AmandsGraphicsOpticSightPatch(),
                new DynamicExternalResolutionPatches.OpticSightOnEnablePath(),
                new DynamicExternalResolutionPatches.OpticSightOnDisablePath(),
                new DynamicExternalResolutionPatches.ClientFirearmControllerChangeAimingModePath(),
            };
        }

        public void RunPatches()
        {
            foreach (ModulePatch patch in this._patches)
            {
                patch.Enable();
            }
        }

        public void RunUnpatches()
        {
            foreach (ModulePatch patch in this._patches)
            {
                patch.Disable();
            }
        }

        private readonly List<ModulePatch> _patches;
    }

    public static class DynamicExternalResolutionPatches
    {
        private static void SetResolutionAim()
        {
            bool DLSSEnabled = Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.DLSSEnabled;
            bool FSREnabled = Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.FSREnabled;
            bool FSR2Enabled = Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.FSR2Enabled;

            float defaultSuperSamplingFactor = Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.SuperSamplingFactor;
            float configSuperSamplingFactor = DynamicExternalResolutionConfig.SuperSampling.Value;

            EAntialiasingMode defaultAAMode = Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.AntiAliasing;

            EDLSSMode defaultDLSSMode = Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.DLSSMode;
            EDLSSMode configDLSSMode = DynamicExternalResolutionConfig.DLSSMode.Value;

            EFSRMode defaultFSRMode = Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.FSRMode;
            EFSRMode configFSRMode = DynamicExternalResolutionConfig.FSRMode.Value;

            EFSR2Mode defaultFSR2Mode = Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.FSR2Mode;
            EFSR2Mode configFSR2Mode = DynamicExternalResolutionConfig.FSR2Mode.Value;

            if (!FSREnabled && !FSR2Enabled && (configSuperSamplingFactor < defaultSuperSamplingFactor))
            {
                SetSuperSampling(1f - configSuperSamplingFactor);
            }
            else if (DLSSEnabled && (configDLSSMode != defaultDLSSMode))
            {
                SetAntiAliasing(defaultAAMode, configDLSSMode, defaultFSR2Mode);
            }
            else if (FSREnabled && (configFSRMode != defaultFSRMode))
            {
                SetFSR(configFSRMode);
            }
            else if (FSR2Enabled && (configFSR2Mode != defaultFSR2Mode))
            {
                SetFSR2(configFSR2Mode);
            }
        }

        private static void SetResolutionDefault()
        {
            bool DLSSEnabled = Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.DLSSEnabled;
            bool FSREnabled = Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.FSREnabled;
            bool FSR2Enabled = Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.FSR2Enabled;

            float defaultSuperSamplingFactor = Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.SuperSamplingFactor;
            EAntialiasingMode defaultAAMode = Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.AntiAliasing;
            EDLSSMode defaultDLSSMode = Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.DLSSMode;
            EFSRMode defaultFSRMode = Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.FSRMode;
            EFSR2Mode defaultFSR2Mode = Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.FSR2Mode;

            if (!FSREnabled && !FSR2Enabled)
            {
                SetSuperSampling(defaultSuperSamplingFactor);
            }
            else if (DLSSEnabled)
            {
                SetAntiAliasing(defaultAAMode, defaultDLSSMode, defaultFSR2Mode);
            }
            else if (FSREnabled)
            {
                SetFSR(defaultFSRMode);
            }
            else if (FSR2Enabled)
            {
                SetFSR2(defaultFSR2Mode);
            }
        }

        private static void SetSuperSampling(float value)
        {
            CameraClass camera = DynamicExternalResolution.getCameraInstance();
            if (camera != null)
            {
                camera.SetSuperSampling(Mathf.Clamp(value, 0.01f, 1f));
            }
        }

        private static void SetAntiAliasing(EAntialiasingMode quality, EDLSSMode value, EFSR2Mode fsr2Mode)
        {
            CameraClass camera = DynamicExternalResolution.getCameraInstance();
            if (camera != null)
            {
                camera.SetAntiAliasing(quality, value, fsr2Mode);
            }
        }

        private static void SetFSR(EFSRMode value)
        {
            CameraClass camera = DynamicExternalResolution.getCameraInstance();
            if (camera != null)
            {
                camera.SetFSR(value);
            }
        }

        private static void SetFSR2(EFSR2Mode value)
        {
            CameraClass camera = DynamicExternalResolution.getCameraInstance();
            if (camera != null)
            {
                camera.SetFSR2(value);
            }
        }

        public class OpticSightOnEnablePath : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return typeof(OpticSight).GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            [PatchPostfix]
            private static void PatchPostfix()
            {
                if (DynamicExternalResolutionConfig.EnableMod.Value)
                {
                    Player localPlayer = DynamicExternalResolution.getPlayerInstance();
                    if (localPlayer != null && localPlayer.ProceduralWeaponAnimation != null && localPlayer.ProceduralWeaponAnimation.IsAiming && localPlayer.ProceduralWeaponAnimation.CurrentAimingMod != null && localPlayer.ProceduralWeaponAnimation.CurrentScope != null)
                    {
                        if (localPlayer.ProceduralWeaponAnimation.CurrentScope.IsOptic)
                        {
                            SetResolutionAim();
                        }
                    }
                }
                else
                {
                    return;
                }
            }
        }

        public class AmandsGraphicsOpticSightPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return typeof(OpticSight).GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            [PatchPostfix]
            private static void PatchPostFix()
            {
                if (DynamicExternalResolutionConfig.EnableMod.Value)
                {
                    Player localPlayer = DynamicExternalResolution.getPlayerInstance();
                    if (localPlayer != null && localPlayer.ProceduralWeaponAnimation != null && localPlayer.ProceduralWeaponAnimation.IsAiming && localPlayer.ProceduralWeaponAnimation.CurrentAimingMod != null && localPlayer.ProceduralWeaponAnimation.CurrentScope != null)
                    {
                        if (localPlayer.ProceduralWeaponAnimation.CurrentScope.IsOptic)
                        {
                            SetResolutionAim();
                        }
                    }
                }
                else
                {
                    return;
                }
            }
        }

        public class OpticSightOnDisablePath : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return typeof(OpticSight).GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            [PatchPostfix]
            private static void PatchPostfix()
            {
                Player localPlayer = DynamicExternalResolution.getPlayerInstance();

                if (localPlayer != null && localPlayer.ProceduralWeaponAnimation != null)
                {
                    SetResolutionDefault();
                }
            }
        }

        public class ClientFirearmControllerChangeAimingModePath : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return typeof(Player.FirearmController).GetMethod("ChangeAimingMode", BindingFlags.Instance | BindingFlags.Public);
            }

            [PatchPostfix]
            private static void PatchPostfix()
            {
                Player localPlayer = DynamicExternalResolution.getPlayerInstance();

                if (localPlayer != null && localPlayer.ProceduralWeaponAnimation != null && localPlayer.ProceduralWeaponAnimation.IsAiming && localPlayer.ProceduralWeaponAnimation.CurrentAimingMod != null && localPlayer.ProceduralWeaponAnimation.CurrentScope != null)
                {
                    if (localPlayer.ProceduralWeaponAnimation.CurrentScope.IsOptic)
                    {
                        SetResolutionAim();
                    }
                    else
                    {
                        SetResolutionDefault();
                    }
                }
            }
        }
    }
}
