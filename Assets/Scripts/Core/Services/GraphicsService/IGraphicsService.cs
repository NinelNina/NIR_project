using System;
using Core.Graphics;
using UnityEngine;

namespace Core.Services
{
    public interface IGraphicsService {
        event Action<GraphicsSettingsData> OnSettingsApplied;
        
        GraphicsSettingsData GetCurrentSettings();
        void ApplyGraphicsSettings(GraphicsSettingsData settings);
        void ApplyPreset(GraphicsQualityPreset preset);
        
        Resolution[] GetAvailableResolutions();
        int[] GetAvailableQualityLevels();
        
        void SetResolution(int width, int height, bool fullscreen);
        void SetQualityLevel(int level);
        void SetVSync(bool enabled);
        void SetFullscreen(bool fullscreen);
        void SetAntiAliasing(int level);
    }
}