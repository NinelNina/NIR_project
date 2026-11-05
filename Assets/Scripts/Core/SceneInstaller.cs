using System;
using Core.Services;
using Core.Utility;
using Reflex.Core;
using Reflex.Enums;
using Resolution = Reflex.Enums.Resolution;
using UnityEngine;

public class SceneInstaller : MonoBehaviour, IInstaller
{

    public void InstallBindings(ContainerBuilder builder)
    {
        //TODO: Reflex обновился. Разобрать с Resolution: может быть Lazy и Eager
        builder.RegisterType(typeof(GameStateService), Lifetime.Singleton, Resolution.Eager);
        builder.RegisterType(typeof(GameplayUtilities), Lifetime.Singleton, Resolution.Lazy);

        // Регистрируем состояния
        // TODO: добавить остальные состояния
        builder.RegisterValue(new BootState(), new[] { typeof(IGameState) });
        builder.RegisterValue(new MainMenuState(), new[] { typeof(IGameState) });
        builder.RegisterValue(new GameplayState(), new[] { typeof(IGameState) });

        //Регистрируем сервисы
        builder.RegisterType(typeof(SettingsService), Lifetime.Scoped, Resolution.Eager);
        builder.RegisterType(typeof(AudioService), Lifetime.Singleton, Resolution.Eager);
        builder.RegisterType(typeof(AudioLibrary), Lifetime.Singleton, Resolution.Eager);
        builder.RegisterType(typeof(GraphicsService), Lifetime.Singleton, Resolution.Eager);
        builder.RegisterType(typeof(LocalizationService), Lifetime.Singleton, Resolution.Eager);
        builder.RegisterType(typeof(SceneService), Lifetime.Singleton, Resolution.Eager);
    }
}