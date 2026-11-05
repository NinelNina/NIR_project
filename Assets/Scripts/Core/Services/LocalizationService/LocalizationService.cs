using UnityEngine.Localization.Settings;

namespace Core.Services
{
    public class LocalizationService : ILocalizationService
    {
        public void ApplyLanguage(GameSettingsData settings)
        {
            var locale = LocalizationSettings.AvailableLocales.Locales
                .Find(l => l.Identifier.Code == settings.LanguageCode);
            
            if (locale != null)
            {
                LocalizationSettings.SelectedLocale = locale;
            }
        }
    }
}