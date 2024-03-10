<p align="center">
<img width="400" src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/3bb780b7-ab64-4f02-a9be-9632fcfe1b47">
</p>

<p align="center">
<img alt="Version" src="https://img.shields.io/github/package-json/v/DCFApixels/DragonECS-Unity?color=%23ff4e85&style=for-the-badge">
<img alt="GitHub" src="https://img.shields.io/github/license/DCFApixels/DragonECS-Unity?color=ff4e85&style=for-the-badge">
<a href="https://discord.gg/kqmJjExuCf"><img alt="Discord" src="https://img.shields.io/discord/1111696966208999525?color=%2300b269&label=Discord&logo=Discord&logoColor=%23ffffff&style=for-the-badge"></a>
</p>

# Интеграция с Unity для [DragonECS](https://github.com/DCFApixels/DragonECS)

| Languages: | [Русский](https://github.com/DCFApixels/DragonECS-Unity/blob/main/README-RU.md) | [English(WIP)](https://github.com/DCFApixels/DragonECS-Unity) |
| :--- | :--- | :--- |

Расширение добавит набор инструментов для дебага и связи с движком Unity.

> [!WARNING]
> Проект в стадии разработки. API может меняться.  
> Readme еще не завершен

# Оглавление
- [Установка](#установка)
- [Debug](#debug)
  - [Debug Модуль](#debug-модуль)
  - [Debug Сервис](#debug-сервис)
  - [Визуальная отладка](#визуальная-отладка)
- [Шаблоны](#шаблоны)
- [Связь с GameObject](#связь-с-gameObject)

</br>

# Установка
Семантика версионирования - [Открыть](https://gist.github.com/DCFApixels/e53281d4628b19fe5278f3e77a7da9e8#file-dcfapixels_versioning_ru-md)
## Окружение
Обязательные требования:
+ Зависимость: [DragonECS](https://github.com/DCFApixels/DragonECS)
+ Минимальная версия C# 7.3;
+ Минимальная версия Unity 2021.2.0;

Протестированно:
+ **Unity:** Минимальная версия 2021.2.0;

## Установка для Unity
* ### Unity-модуль
Поддерживается установка в виде Unity-модуля в  при помощи добавления git-URL [в PackageManager](https://docs.unity3d.com/2023.2/Documentation/Manual/upm-ui-giturl.html) или ручного добавления в `Packages/manifest.json`: 
```
https://github.com/DCFApixels/DragonECS-Unity.git
```
* ### В виде иходников
Фреймворк так же может быть добавлен в проект в виде исходников.

</br>

# Debug
## Debug Модуль
Подключение модуля отладки в Unity.
```c#
EcsDefaultWorld _world = new EcsDefaultWorld();
EcsEventWorld _eventWorld = new EcsDefaultWorld();

_pipeline = EcsPipeline.New()
    //...
    // Подключение и инициализация отладки для миров _world и _eventWorld
    .AddUnityDebug(_world, _eventWorld)
    //...
    .BuildAndInit();
```
## Debug Сервис
`UnityDebugService`- реализация [Debug-сервиса для `EcsDebug`](https://github.com/DCFApixels/DragonECS/blob/main/README-RU.md#ecsdebug). В редакторе по умолчанию автоматически инициализируется и связывает `EcsDebug.Print` с консолью юнити, `EcsProfilerMarker` c профайлером и т.д.
```c#
//Ручная активация.
UnityDebugService.Activate();

//Выведет сообщение в консоле Unity.
EcsDebug.Print(); 

var someMarker = new EcsProfilerMarker("SomeMarker");
someMarker.Begin();
//время выполнения этого участка будет отражено в профайлере юнити.
someMarker.End();

//Остановка игрового режима.
EcsDebug.Break();
```
## Визуальная отладка

# Шаблоны

# Связь с GameObject

