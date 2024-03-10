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
Выполнена в виде специальных объектов-мониторов в которых отображается состояние разных аспектов фреймворка. Найти эти мониторы можно в Play Mode в разделе `DontDestroyOnLoad`. Расширение содержит 4 объекта-монитора: 
* `PipelineMonitor` - показывает состояние `EcsPipeline`.
* `PipelineProcessMonitor` - отображает в виде матрицы процессы и сситемы.
* `WorldMonitor` - показывает состояние `EcsWorld`. на каждый казанный мир создается отдельный монитор.
* `EntityMonitor` - показывает состояние сущности мира. На кажду сущность в мире создается отдельынй монитор, все мониторы сущностей помещаются в монитор мира.

![image](https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/2d3c842f-c0c9-44f7-b35b-4f879a08f267)

### Pipeline
Пример `PipelineMonitor`:

![image](https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/3682fd0f-f47a-40ed-9d4c-cbad5d512e5d)

Пример `PipelineProcessMonitor`:

![image](https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/c1a7960a-d65a-4f22-9597-fd863ff2b00c)

### World

Пример `WorldMonitor`:

![image](https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/7b6455fc-9211-425c-b0b8-288077e61543)

### Entity

Пример `EntityMonitor`:

![image](https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/509ff472-05b5-4fd8-a0e6-739d7fa81ab1)

# Шаблоны
Шаблоны - это настраиваемые наборы компонентов которые можно применить к сущностям. Шаблоны должны реализовавыть интерфейс  `ITemplateNode`. 
```c#
ITemplateNode someSamplate = /*...*/;
//...
foreach (var e in _world.Where(out Aspect a))
{
    // Применение шаблона сущности.
    someSamplate.Apply(e, _world.id);
}
```
```c#
// Применение шаблона сразу при создании сущности.
int e = _world.NewEntity(someSamplate);
```
По умолчанию расширение содержит 2 вида шаблонов: `ScriptableEntityTemplate` и `MonoEntityTemplate`. 
## ScriptableEntityTemplate
Хранится как отдельынй ассет. Наследуется от `ScriptableObject`. </br>
Дейсвия чтобы создать `ScriptableEntityTemplate` ассет: 

![image](https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/8362e2d8-b83a-4dfc-91fd-38993746012f)

Пример:

![image](https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/26379ee5-cadd-4838-a3b6-5b46771012c1)

## MonoEntityTemplate
Крепится к GameObject. Наследуется от `MonoBehaviour`. </br>
Дейсвия чтобы добавить `MonoEntityTemplate` на GameObject:

![image](https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/07a43cb7-96e5-440c-965d-2970803df330)

Пример:

![image](https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/7f6b722e-6f98-4d13-b2cd-5d576a3610bd)

# Связь с GameObject

