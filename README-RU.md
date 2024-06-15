<p align="center">
<img width="400" src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/3bb780b7-ab64-4f02-a9be-9632fcfe1b47">
</p>

<p align="center">
<img alt="Version" src="https://img.shields.io/github/package-json/v/DCFApixels/DragonECS-Unity?color=%23ff4e85&style=for-the-badge">
<img alt="GitHub" src="https://img.shields.io/github/license/DCFApixels/DragonECS-Unity?color=ff4e85&style=for-the-badge">
<a href="https://discord.gg/kqmJjExuCf"><img alt="Discord" src="https://img.shields.io/badge/Discord-JOIN-00b269?logo=discord&logoColor=%23ffffff&style=for-the-badge"></a>
<a href="http://qm.qq.com/cgi-bin/qm/qr?_wv=1027&k=IbDcH43vhfArb30luGMP1TMXB3GCHzxm&authKey=s%2FJfqvv46PswFq68irnGhkLrMR6y9tf%2FUn2mogYizSOGiS%2BmB%2B8Ar9I%2Fnr%2Bs4oS%2B&noverify=0&group_code=949562781"><img alt="QQ" src="https://img.shields.io/badge/QQ-JOIN-00b269?logo=tencentqq&logoColor=%23ffffff&style=for-the-badge"></a>
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
- [Связь с GameObject](#связь-с-gameobject)
- [World Provider](#world-provider)
- [FixedUpdate LateUpdate ](#fixedupdate-lateupdate)
- [Документация проекта](#документация-проекта)
- [FAQ](#faq)

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
Выполнена в виде специальных объектов-мониторов в которых отображается состояние разных аспектов фреймворка. Найти эти мониторы можно в Play Mode в разделе `DontDestroyOnLoad`. 

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/54e3f6d1-13c4-4226-a983-c672a29d33bb">   
</p>

-----

* ### `PipelineMonitor`
Показывает состояние `EcsPipeline`. Системы отображаются в порядке их выполнения.

<p align="center">
<img width="270px" src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/3682fd0f-f47a-40ed-9d4c-cbad5d512e5d">   
</p>

-----

* ### `PipelineProcessMonitor` 
Отображает в виде матрицы процессы и сситемы. Системы отображабтся в порядке их выполнения. Точка в пересечении системы и процесса означает что эта система является частью этого процесса.

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/c1a7960a-d65a-4f22-9597-fd863ff2b00c">   
</p>

-----

* ### `WorldMonitor` 
Показывает состояние `EcsWorld`. на каждый казанный мир создается отдельный монитор.

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/7b6455fc-9211-425c-b0b8-288077e61543">   
</p>

-----

* ### `EntityMonitor`
Показывает состояние сущности мира. На кажду сущность в мире создается отдельынй монитор. Все мониторы сущностей помещаются в монитор мира.

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/509ff472-05b5-4fd8-a0e6-739d7fa81ab1">   
</p>

-----

</br>

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
По умолчанию расширение содержит 2 вида шаблонов: `ScriptableEntityTemplate`, `MonoEntityTemplate`. 

## ScriptableEntityTemplate
Хранится как отдельынй ассет. Наследуется от `ScriptableObject`.
Дейсвия чтобы создать `ScriptableEntityTemplate` ассет: 

<details>
<summary>Создать ассет: Asset > Create > DragonECS > ScriptableEntityTemplate.</summary>

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/8362e2d8-b83a-4dfc-91fd-38993746012f">   
</p>

</details>

Чтобы добавить компонент в меню `Add Component` Нужен [Шаблон компонента](#шаблон-компонента). Пример:

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/26379ee5-cadd-4838-a3b6-5b46771012c1">   
</p>

-----

## MonoEntityTemplate
Крепится к GameObject. Наследуется от `MonoBehaviour`. 

<details>
<summary>Повесить компонент: Add Component > DragonECS > MonoEntityTemplate.</summary>

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/07a43cb7-96e5-440c-965d-2970803df330">   
</p>

</details>

Чтобы добавить компонент в меню `Add Component` Нужен [Шаблон компонента](#шаблон-компонента). Пример:

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/7f6b722e-6f98-4d13-b2cd-5d576a3610bd">   
</p>

-----

## Шаблон компонента
Чтобы компонент попал в меню `Add Component` нужно реализовать шаблон компонента. Шаблоны компонента это типы реализующие `IComponentTemplate`. 

* Упрощенная реализация:
```c#
// Обязательно добавить [Serializable] к типу компонента.
[Serializable]
struct SomeComponent : IEcsComponent { /* ... */ }
class SomeComponentTemplate : ComponentTemplate<SomeComponent> { }
```

```c#
// Тоже самое но для компонентов-тегов.
[Serializable]
struct SomeTagComponent : IEcsTagComponent { }
class SomeTagComponentTemplate : TagComponentTemplate<SomeComponent> { }
```

<details>
<summary>* Полная реализация:</summary>

```c#
[Serializable] 
struct SomeComponent : IEcsComponent { /* ... */ }
class SomeComponentTemplate : IComponentTemplate
{
    [SerializeField]
    protected SomeComponent component;
    public Type Type { get { return typeof(SomeComponent); } }
    public void Apply(int worldID, int entityID)
    {
        EcsWorld.GetPoolInstance<EcsPool<SomeComponent>>(worldID).TryAddOrGet(entityID) = component;
    }
    public object GetRaw() { return component; }
    public void SetRaw(object raw) { component = (SomeComponent)raw; }
    public void OnGizmos(Transform transform, IComponentTemplate.GizmosMode mode) { /*...*/ }
    public void OnValidate(UnityEngine.Object obj) { /*...*/ }
}
```

</details>

В раскрывающемся при нажатии `Add Component` меню выбора компонента поддерживается иерархическое группирование. Производится группирование на основе мета-атрибута `[MetaGroup]`.

Компоненты в инспектрре по умолчанию отображаются окрашенными в случайный цвет сгенерированный на основе имени компонента, выбрать другой режим окраски можно в настройках фреймворка. Задать конкретный цвет можно при помощи мета-атрибута `[MetaColor]`.

Если редактор смог автоматически определить связанный с компонентом скрипт, то слева от крестика удаления компонента будет иконка файла. Клик по иконке выделит файл скрипта в папке проекта, двойной клик откроет скрип для редактирования. Связанный файл ищется по сопоставлению имени типа и имени файла скрипта. 

Если у компонента есть мета-атрибут `[MetaDescription]`, то слева от крестика удаления компонента будет иконка подсказки, при наведении курсора покажется информация из `[MetaDescription]`.

</br>

При необходимости создания кастомного шаблона, шаблоны компонентов поддерживают отображение вне стандартных `MonoEntityTemplate` и `ScriptableEntityTemplate`.
```c#
// ComponentTemplateReference добавляет кнопку выбора доступной реализации IComponentTempalte
// и отображает шаблон компонента аналогично компонентам в MonoEntityTemplate или ScriptableEntityTemplate.
[SerializeReference, ComponentTemplateReference]
private IComponentTempalte _someComponent1;

// Обертка над IComponentTempalte, которая работает аналогично примеру с атрибутом ComponentTemplateReference.
private ComponentTemplateProperty _someComponent2;

// Все это работает и для массивов.
[SerializeReference, ComponentTemplateReference]
private IComponentTempalte[] _components;
```

</br>

# Связь с GameObject
Связываются сущности и GameObject-ы с помощью коннектов. Коннекты со стороны GameObject - `EcsEntityConnect`, со стороны сущности - `GameObjectConnect`. `EcsEntityConnect` - управляющий коннект, `GameObjectConnect` - создается/удаляется автоматически.
```c#
EcsEntityConnect connect = /*...*/;
entlong entity = _world.NewEntityLong();

// Связывание сущности с GameObject.
// Автоматически добавляется GameObjectConnect в сущность.
// Автоматическки применяются шаблоны.
connect.ConnectWith(entity);

// Или создать без применения шаблнов.
connect.ConnectWith(entity, false);

// Отвязать.
// Автоматически удалится GameObjectConnect.
connect.Disconnect();
```
<details>
<summary>Повесить компонент: Add Component > DragonECS > EcsEntityConnect.</summary>

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/cfa6eb1c-82ba-47f6-bee1-7986c1e31be7">   
</p>

</details>

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/3484ed12-5417-4450-9908-1d3eb2858a2b">   
</p>

> Просмотреть все компоненты связанной сущности можно развернув `RUNTIME COMPONENTS`.

> На панели внизу есть вспомогательные кнопки: 1) Отвязать сущность. 2) Удалить сущность. 3) Автоматическое заполнение массива шаблонов. 4) Каскадный вызов автозаполнения для всех дочерних коннектов в иерархии.

---

`AutoEntityCreator` автоматический создает сущность и связывает с GameObject. В инспекторе ему нужно указать `EcsEntityConnect` с которым связывать сущность и [Провайдер мира](#world-provider) в котормо создать сущность.

<details>
<summary>Повесить компонент: Add Component > DragonECS > AutoEntityCreator.</summary>

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/29bfef68-6e77-467c-84d3-14d73a9c614d">   
</p>

</details>

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/55c11f1c-c0e0-435c-af9b-4c06678491a6">   
</p>

> На панели внизу есть вспомогательные кнопки: 1) Автоматическое заполнение ссылки на коннект. 2) Каскадный вызов автозаполнения для всех дочерних экземпляров в иерархии.

</br>

# World Provider
`EcsWorldProvider<TWorld>` - это `ScriptableObject` обертка над `TWorld`, предназначенная для пробрасывания экземпляра мира и настройки через инспектор Unity. Для простых случаев достаточно будет использовать синглтон версию провайдера `EcsDefaultWorldSingletonProvider`. 

```c#
// Синглтон провайдер создается автоматически в папке "Assets/Resource".
EcsDefaultWorldSingletonProvider provider = EcsDefaultWorldSingletonProvider.Instance;
// ...

EcsDefaultWorld world = new EcsDefaultWorld();
// Устанавливаем экземпляр мира в провайдер.
provider.Set(world);

// ...

//Получаем экземпляр мира, если провайдер был пуст, то он создаст новый мир.
EcsDefaultWorld world = provider.Get();

EcsPipeline pipeline = EcsPipeline.New()
    //...
    // Внедряем в системы полученный из провайдера мир.
    .Inject(world)
    //...
    .BuildAndInit();
```

<details>
<summary>Пример реализации провайдера для своего типа мира</summary>

```c#
//Пример реализации своего провайдера для пробрасывания мира своего типа
[CreateAssetMenu(fileName = nameof(EcsMyWorldProvider), menuName = EcsConsts.FRAMEWORK_NAME + "/WorldProviders/" + nameof(EcsMyWorldProvider), order = 1)]
public class EcsMyWorldProvider : EcsWorldProvider<EcsMyWorld> { }

//Пример реализации синглтон версии для мира своего типа
public class EcsMyWorldSingletonProvider : EcsWorldProvider<EcsMyWorld>
{
    private static EcsMyWorldSingletonProvider _instance;
    public static EcsMyWorldSingletonProvider Instance
    {
        get
        {
            if (_instance == null) { _instance = FindOrCreateSingleton<EcsMyWorldSingletonProvider>("SingletonMyWorld"); }
            return _instance;
        }
    }
}
```

</details>

<details>
<summary>Создать ассет провайдера: Asset > Create > DragonECS > WorldProviders > Выбрать тип мира.</summary>
    
<p align="center">
<img width="780px" src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/8aa1bd82-8a15-46ce-b950-3e74252243c6">   
</p>

</details>

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/d01a671a-69e9-44b9-9ad1-e58d0e8857d7">   
</p>

</br>

# FixedUpdate LateUpdate 
```c#
using DCFApixels.DragonECS;
using UnityEngine;
public class EcsRoot : MonoBehaviour
{
    private EcsPipeline _pipeline;
    //...
    private void Update()
    {
        // Стандартный Run из фреймворка.
        _pipeline.Run();
    }
    private void FixedUpdate()
    {
        // Специальный Run для трансляции FixedUpdate.
        _pipeline.FixedRun();
    }
    private void LateUpdate()
    {
        // Специальный Run для трансляции LateUpdate.
        _pipeline.LateRun();
    }
    // ...
}
```

</br>

# Документация проекта
В интеграции так же есть окно документации проекта на основе Мета-Атрибутов. Открывается окно тут `Tools -> DragonECS -> Documentation`. Документация формируется при первом открытии окна и при нажатии кнопки `Update`.

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/f5795823-aeae-45df-8e25-db64df837513">   
</p>

</br>

# Окно настроек
В окне настроек есть несколько опций, включая возможность менять режимы отображения компонентов в инспекторе. Внизу расположены удобные переключатели для используемых в фреймворке define директив процессора.

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/c794be8d-6884-4415-b24a-0a1a28f577a6">   
</p>

</br>

# FAQ
## Не могу повесить EcsEntityConncet или другие компоненты
Такое может происходить после обновления пакета, решается либо через `Assets -> Reimport All` или перезапуск окна Unity с удалением папки `*project name*/Library`.
