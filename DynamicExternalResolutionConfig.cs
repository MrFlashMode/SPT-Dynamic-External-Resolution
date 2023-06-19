using BepInEx.Configuration;
using EFT.Settings.Graphics;

#pragma warning disable 0169, 0414, 0649

namespace DynamicExternalResolution.Configs
{
    internal class DynamicExternalResolutionConfig
    {
        //public class DLSSModeQuality
        //{
        //    public static EDLSSMode Quality;            // 0.9
        //    public static EDLSSMode Balanced;           // 0.75
        //    public static EDLSSMode Performance;        // 0.5
        //    public static EDLSSMode UltraPerformance;   // 0.35
        //}

        public class FSRModeQuality
        {
            public static EFSRMode UltraQuality;        // 0.77
            public static EFSRMode Quality;             // 0.66
            public static EFSRMode Balanced;            // 0.59
            public static EFSRMode Performance;         // 0.5
        }

        public class FSR2ModeQuality
        {
            public static EFSR2Mode Quality;            // 0.66
            public static EFSR2Mode Balanced;           // 0.58
            public static EFSR2Mode Performance;        // 0.5
            public static EFSR2Mode UltraPerformance;   // 0.33
        }

        public static ConfigEntry<bool> EnableMod { get; set; }
        public static ConfigEntry<float> SuperSampling { get; set; }

        //public static ConfigEntry<EDLSSMode> DLSSMode { get; set; }
        public static ConfigEntry<EFSRMode> FSRMode { get; set; }
        public static ConfigEntry<EFSR2Mode> FSR2Mode { get; set; }

        public static void Init(ConfigFile Config)
        {
            string scaling = "Settings";

            EnableMod = Config.Bind(scaling, "Enable Mod", true, new ConfigDescription("Enable/Disable reducing the resolution of the external rendering, when aiming through the telescopic sight.", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 5 }));
            SuperSampling = Config.Bind(scaling, "Sampling Downgrade", 0.5f, new ConfigDescription("(Only works when FSR 1.0/FSR 2.2/DLSS is Disable) \nPercentage of how much the external rendering will go down, when aiming through the telescopic sight. Default value 50%.", new AcceptableValueRange<float>(0f, 1f), new ConfigurationManagerAttributes { ShowRangeAsPercent = true, Order = 4 }));
            //DLSSMode = Config.Bind(scaling, "DLSS Scaling Mode", EDLSSMode.Performance, new ConfigDescription("(Only works when DLSS is Enable) \nImage scaling mode when using DLSS, which will change external rendering to this level when you aiming through the telescopic scope. Default is Performance Mode (50% resolution downgrade).", null, new ConfigurationManagerAttributes { Order = 3 }));
            FSRMode = Config.Bind(scaling, "FSR Scaling Mode", EFSRMode.Performance, new ConfigDescription("(Only works when FSR 1.0 is Enable) \nImage scaling mode when using FSR, which will change external rendering to this level when you aiming through the telescopic scope. Default is Performance Mode (50% resolution downgrade).", null, new ConfigurationManagerAttributes { Order = 2 }));
            FSR2Mode = Config.Bind(scaling, "FSR2 Scaling Mode", EFSR2Mode.Performance, new ConfigDescription("(Only works when FSR 2.2 is Enable) \nImage scaling mode when using FSR2, which will change external rendering to this level when you aiming through the telescopic scope. Default is Performance Mode (50% resolution downgrade).", null, new ConfigurationManagerAttributes { Order = 1 }));
        }
    }
}