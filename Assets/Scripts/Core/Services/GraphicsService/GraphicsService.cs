using System;
using System.Linq;
using Core.Graphics;
using UnityEngine;

namespace Core.Services
{
   public class GraphicsService : IGraphicsService
    {
        public event Action<GraphicsSettingsData> OnSettingsApplied;
        
        private readonly SettingsService _settingsService;
        private GraphicsSettingsData _currentSettings;
        
        //public GraphicsService(SettingsService settingsService)
        //{
        //    _settingsService = settingsService;
        //    _currentSettings = GetCurrentSettings();
        //}

        public GraphicsSettingsData GetCurrentSettings()
        {
            return new GraphicsSettingsData
            {
                QualityLevel = QualitySettings.GetQualityLevel(),
                VSync = QualitySettings.vSyncCount > 0,
                Fullscreen = Screen.fullScreen,
                FullscreenMode = Screen.fullScreenMode,
                ResolutionWidth = Screen.currentResolution.width,
                ResolutionHeight = Screen.currentResolution.height,
                RefreshRate = Screen.currentResolution.refreshRate,
                AntiAliasing = QualitySettings.antiAliasing,
                TextureQuality = QualitySettings.globalTextureMipmapLimit,
                ShadowQuality = (int)QualitySettings.shadows,
                TargetFrameRate = Application.targetFrameRate
            };
        }

        public void ApplyGraphicsSettings(GraphicsSettingsData settings)
        {
            if (settings == null)
            {
                Debug.LogWarning("[GraphicsService] Settings is null");
                return;
            }

            try
            {
                // Качество
                SetQualityLevel(settings.QualityLevel);
                
                // VSync
                SetVSync(settings.VSync);
                
                // Разрешение и fullscreen
                SetResolution(settings.ResolutionWidth, settings.ResolutionHeight, settings.Fullscreen);
                
                // Fullscreen mode
                Screen.fullScreenMode = settings.FullscreenMode;
                
                // Anti-aliasing
                SetAntiAliasing(settings.AntiAliasing);
                
                // Текстуры
                QualitySettings.globalTextureMipmapLimit = settings.TextureQuality;
                
                // Тени
                QualitySettings.shadows = (ShadowQuality)settings.ShadowQuality;
                
                // FPS
                Application.targetFrameRate = settings.TargetFrameRate;
                
                // Сохраняем текущие настройки
                _currentSettings = settings;
                
                // Уведомляем подписчиков
                OnSettingsApplied?.Invoke(settings);
                
                Debug.Log($"[GraphicsService] Settings applied: {settings.QualityLevel} quality, {settings.ResolutionWidth}x{settings.ResolutionHeight}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[GraphicsService] Failed to apply settings: {e.Message}");
            }
        }

        public void ApplyPreset(GraphicsQualityPreset preset)
        {
            var settings = new GraphicsSettingsData();
            
            switch (preset)
            {
                case GraphicsQualityPreset.Low:
                    settings.QualityLevel = 0;
                    settings.AntiAliasing = 0;
                    settings.TextureQuality = 2; // Low
                    settings.ShadowQuality = (int)ShadowQuality.HardOnly;
                    break;
                    
                case GraphicsQualityPreset.Medium:
                    settings.QualityLevel = 2;
                    settings.AntiAliasing = 2;
                    settings.TextureQuality = 1; // Medium
                    settings.ShadowQuality = (int)ShadowQuality.All;
                    break;
                    
                case GraphicsQualityPreset.High:
                    settings.QualityLevel = 4;
                    settings.AntiAliasing = 4;
                    settings.TextureQuality = 0; // High
                    settings.ShadowQuality = (int)ShadowQuality.All;
                    break;
                    
                case GraphicsQualityPreset.Ultra:
                    settings.QualityLevel = QualitySettings.names.Length - 1;
                    settings.AntiAliasing = 8;
                    settings.TextureQuality = 0;
                    settings.ShadowQuality = (int)ShadowQuality.All;
                    break;
            }
            
            ApplyGraphicsSettings(settings);
        }

        public Resolution[] GetAvailableResolutions()
        {
            return Screen.resolutions
                .Distinct()
                .OrderByDescending(r => r.width * r.height)
                .ToArray();
        }

        public int[] GetAvailableQualityLevels()
        {
            return Enumerable.Range(0, QualitySettings.names.Length).ToArray();
        }

        public void SetResolution(int width, int height, bool fullscreen)
        {
            // Валидация
            if (width <= 0 || height <= 0)
            {
                Debug.LogWarning($"[GraphicsService] Invalid resolution: {width}x{height}");
                return;
            }

            Screen.SetResolution(width, height, fullscreen);
            Debug.Log($"[GraphicsService] Resolution set to {width}x{height}, Fullscreen: {fullscreen}");
        }

        public void SetQualityLevel(int level)
        {
            if (level < 0 || level >= QualitySettings.names.Length)
            {
                Debug.LogWarning($"[GraphicsService] Invalid quality level: {level}");
                level = Mathf.Clamp(level, 0, QualitySettings.names.Length - 1);
            }

            QualitySettings.SetQualityLevel(level, true);
            Debug.Log($"[GraphicsService] Quality level set to {QualitySettings.names[level]}");
        }

        public void SetVSync(bool enabled)
        {
            QualitySettings.vSyncCount = enabled ? 1 : 0;
            Debug.Log($"[GraphicsService] VSync set to {enabled}");
        }

        public void SetFullscreen(bool fullscreen)
        {
            Screen.fullScreen = fullscreen;
            Debug.Log($"[GraphicsService] Fullscreen set to {fullscreen}");
        }

        public void SetAntiAliasing(int level)
        {
            // Доступные уровни: 0, 2, 4, 8
            var validLevels = new[] { 0, 2, 4, 8 };
            if (!validLevels.Contains(level))
            {
                Debug.LogWarning($"[GraphicsService] Invalid AA level: {level}, using 0");
                level = 0;
            }

            QualitySettings.antiAliasing = level;
            Debug.Log($"[GraphicsService] Anti-aliasing set to {level}x");
        }
    }
}