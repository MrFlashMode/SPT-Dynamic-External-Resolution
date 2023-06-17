using BepInEx;
using DynamicExternalResolution.Configs;
using EFT;

namespace DynamicExternalResolution
{
    [BepInPlugin("com.DynamicExternalResolution", "Dynamic External Resolution", "1.0")]
    public class DynamicExternalResolution : BaseUnityPlugin
    {
        static Player _localPlayer = null;
        
        static CameraClass _camera = null;
        
        public static Player getPlayetInstance()
        {
            if (_localPlayer != null)
            {
                return _localPlayer;
            }
            
            _localPlayer = FindObjectOfType<Player>();
            return _localPlayer;
        }
        
        public static CameraClass getCameraInstance()
        {
            if (_camera != null)
            {
                return _camera;
            }
            
            _camera = CameraClass.Instance;
            return _camera;
        }
        
        private void Awake()
        {
            DynamicExternalResolutionConfig.Init(Config);
            Patcher.PatchAll();
            Logger.LogInfo($"Plugin Dynamic External Resolution is loaded!");
        }
        
        private void OnDestroy()
        {
            Patcher.UnpatchAll();
            Logger.LogInfo($"Plugin DynamicExternalResolution is unloaded!");
        }
    }
}
