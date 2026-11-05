using System;
using UnityEngine;

namespace Core.Graphics
{
    [Serializable]
    public class GraphicsSettingsData
    {
        public int QualityLevel = 3;
        public bool VSync = true;
        public bool Fullscreen = true;
        public FullScreenMode FullscreenMode = FullScreenMode.FullScreenWindow;
        public int ResolutionWidth = 1920;
        public int ResolutionHeight = 1080;
        public int RefreshRate = 60;
        public int AntiAliasing = 2;
        public int TextureQuality = 0;
        public int ShadowQuality = 2;
        public int TargetFrameRate = -1;
    }

    public enum GraphicsQualityPreset
    {
        Low,
        Medium,
        High,
        Ultra
    }
}
   
