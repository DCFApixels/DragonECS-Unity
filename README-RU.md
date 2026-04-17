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

<table>
  <tr></tr>
  <tr>
    <td colspan="3">Readme Languages:</td>
  </tr>
  <tr></tr>
  <tr>
    <td nowrap width="100">
      <a href="https://github.com/DCFApixels/DragonECS-Unity/blob/main/README-RU.md">
        <img src="https://github.com/user-attachments/assets/3c699094-f8e6-471d-a7c1-6d2e9530e721"></br>
        <span>Русский</span>
      </a>  
    </td>
    <td nowrap width="100">
      <a href="https://github.com/DCFApixels/DragonECS-Unity">
        <img src="https://github.com/user-attachments/assets/30528cb5-f38e-49f0-b23e-d001844ae930"></br>
        <span>English(WIP)</span>
      </a>  
    </td>
  </tr>
</table>

</br>

Этот пакет делает работу с DragonECS в Unity удобнее и нагляднее: встроенная визуальная отладка и профайлинг, редакторские шаблоны и инструменты для привязки сущностей к `GameObject`.

> [!WARNING]
> Проект в стадии разработки. API может меняться.  
> Readme еще не завершен, если есть не ясные моменты, вопросы можно задать тут [Обратная связь](https://github.com/DCFApixels/DragonECS/blob/main/README-RU.md#%D0%BE%D0%B1%D1%80%D0%B0%D1%82%D0%BD%D0%B0%D1%8F-%D1%81%D0%B2%D1%8F%D0%B7%D1%8C)

> [!WARNING]
> Встроенные реализации шаблонов `UnityComponent<T>` были перенесены [СЮДА](https://gist.github.com/DCFApixels/c250f2561f09e09ab3e6a4bd4f3013cb#file-unitycomponenttemplates-cs) Так как некоторые модули Unity отключаемы, и например отключение модуля физики приведет к тому что код реализации `UnityComponent<Rigidbody>` или  `UnityComponent<Collider>` не будет компилироваться.

//https://gist.github.com/DCFApixels/c250f2561f09e09ab3e6a4bd4f3013cb#file-unitycomponenttemplates-cs
# Оглавление
- [Установка](#установка)
- [Debug](#debug)
  - [Debug Сервис](#debug-сервис)
  - [Debug Модуль](#debug-модуль)
  - [Визуальная отладка](#визуальная-отладка)
- [Шаблоны](#шаблоны)
- [Связь с GameObject](#связь-с-gameobject)
- [World Provider](#world-provider)
- [Шаблон Пайплайна](#шаблон-пайплайна)
- [FixedUpdate LateUpdate](#fixedupdate-lateupdate)
- [Кастомизация инспектора](#Кастомизация-инспектора)
- [Поддержка Jobs](#Поддержка-Jobs)
- [Документация проекта](#документация-проекта)
- [Окно настроек](#окно-настроек)
- [Reference Repairer](#Reference-Repairer)
- [FAQ](#faq)

</br>

# Установка
Семантика версионирования - [Открыть](https://gist.github.com/DCFApixels/af79284955bf40e9476cdcac79d7b098#file-dcfapixels_versioning-md)
## Окружение
Обязательные требования:
+ Зависимость: [DragonECS](https://github.com/DCFApixels/DragonECS)
+ Минимальная версия C# 8.0;
+ Минимальная версия Unity 2021.2.0;

## Установка для Unity
* ### Unity-модуль
Поддерживается установка в виде Unity-модуля при помощи добавления git-URL [в PackageManager](https://docs.unity3d.com/2023.2/Documentation/Manual/upm-ui-giturl.html): 
```
https://github.com/DCFApixels/DragonECS-Unity.git
```
Или ручного добавления этой строчки в `Packages/manifest.json`:
```
"com.dcfa_pixels.dragonecs-unity": "https://github.com/DCFApixels/DragonECS-Unity.git",
```

* ### В виде исходников
Можно также напрямую скопировать исходники пакета в проект.

</br>

# Debug
## Debug Сервис
`UnityDebugService` - реализация [Debug-сервиса для `EcsDebug`](https://github.com/DCFApixels/DragonECS/blob/main/README-RU.md#ecsdebug). В редакторе он инициализируется автоматически и обеспечивает интеграцию: например, вызовы `EcsDebug.Print` направляются в консоль Unity, а `EcsProfilerMarker` подключается к встроенному профайлеру и т.д.
```c#
//Ручная активация.
UnityDebugService.Activate();

//Выведет сообщение в консоли Unity.
EcsDebug.Print(); 

var someMarker = new EcsProfilerMarker("SomeMarker");
someMarker.Begin();
//время выполнения этого участка будет отражено в профайлере Unity.
someMarker.End();

//Остановка игрового режима.
EcsDebug.Break();
```

## Визуальная отладка

Реализовано в виде объектов-мониторов, в которых отображается состояние разных частей фреймворка. Найти эти мониторы можно в `Play Mode` в разделе `DontDestroyOnLoad`.

```c#
_pipeline = EcsPipeline.New()
    //...
    // Инициализация отладки для пайплайна и миров
    .AddUnityDebug(_world, _eventWorld)
    //...
    .BuildAndInit();
```

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
Отображает в виде матрицы процессы и системы. Системы отображаются в порядке их выполнения. Точка в пересечении системы и процесса означает что эта система является частью этого процесса.

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/c1a7960a-d65a-4f22-9597-fd863ff2b00c">   
</p>

-----

* ### `WorldMonitor` 
Показывает состояние `EcsWorld`. На каждый мир, переданный в `AddUnityDebug(...)`, создается отдельный монитор.

<p align="center">
<img src="https://github.com/user-attachments/assets/83905e7a-a5d1-4470-883a-c3b318cb4726">   
</p>

-----

* ### `WorldQueriesMonitor` 
Расположен вместе с `WorldMonitor`, показывает список всех Where запросов которые выполняли системы. Вверху есть поле для поиска запросов по именам компонентам. Поисковую строку можно разделять символом `/` для одновременного поиска нескольких компонентов. Рядом с каждым Where запросом есть кнопка `Snapshot`, по нажатию откроется окно со списоком всех сущностей которые на данный момент удовлетворяют маске запроса.
<p align="center">
<img width="400px" src="https://github.com/user-attachments/assets/e6edf718-5c73-437e-abeb-c192ace9f927" />
</p>

-----

* ### `EntityMonitor`
Показывает состояние сущности мира, позволяет добавлять/изменять/удалять компоненты по время Play Mode. На каждую сущность в мире создается отдельный монитор. Все мониторы сущностей помещаются в монитор мира.

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/509ff472-05b5-4fd8-a0e6-739d7fa81ab1">   
</p>

-----

</br>

# Шаблоны
Интеграция содержит шаблоны, расширяющие `ITemplateNode`, предназначенные для настройки сущностей из редактора.

## ScriptableEntityTemplate
Хранится как отдельный ассет. Наследуется от `ScriptableObject`.

<details>
<summary>Создать ассет: Asset > Create > DragonECS > ScriptableEntityTemplate.</summary>

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/8362e2d8-b83a-4dfc-91fd-38993746012f">   
</p>

</details>

Чтобы добавить компонент в меню `Add Component` Нужен [Шаблон компонента](#шаблон-компонента).

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/26379ee5-cadd-4838-a3b6-5b46771012c1">   
</p>

-----

## MonoEntityTemplate
Крепится к `GameObject`. Наследуется от `MonoBehaviour`. 

<details>
<summary>Повесить компонент: Add Component > DragonECS > MonoEntityTemplate.</summary>

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/07a43cb7-96e5-440c-965d-2970803df330">   
</p>

</details>

Чтобы добавить компонент в меню `Add Component` Нужен [Шаблон компонента](#шаблон-компонента).

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/7f6b722e-6f98-4d13-b2cd-5d576a3610bd">   
</p>

-----

## Шаблон компонента

Чтобы компонент попал в меню `Add Component` требуется шаблон. Шаблоны компонента это типы реализующие `IComponentTemplate` или компоненты реализующие `ITemplateNode` вместе с `IEcsComponentMember`. 

### Реализация

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
<summary>Другие способы</summary>

#### Реализация `ITemplateNode` у компонента

Такой способ может быть удобен тем что не требует создания отдельного класса шаблона, компонент сам выступает как шаблон, и он так же прост в реализации. Минус данного подхода, что проще случайно переименовать компонент и получить Missing Reference в местах с атрибутом `[SerializeReference]`.
```c#
public struct Health : IEcsComponent, ITemplateNode
{
    public float Points;
    public void Apply(short worldID, int entityID)
    {
        EcsPool<Health>.Apply(worldID, entityID) = this;
    }
}
```

#### Реализация кастомного шаблона

Если не подходят встроенные `ComponentTemplate<T>` или `TagComponentTemplate<T>`, можно создать свой шаблон реализующий `IComponentTemplate`. Например это может пригодиться для кастомного пула. В большинстве случаев достаточно использовать встроенные шаблоны.

```c#
[Serializable] 
struct SomeComponent : IEcsComponent { /* ... */ }
class SomeComponentTemplate : IComponentTemplate
{
    [SerializeField]
    protected SomeComponent component;
    public Type Type { get { return typeof(SomeComponent); } }
    public bool IsUnique { get { return true; } }
    public void Apply(int worldID, int entityID)
    {
        EcsPool<SomeComponent>.Apply(worldID, entityID) = component;
    }
    public object GetRaw() { return component; }
    public void SetRaw(object raw) { component = (SomeComponent)raw; }
    public void OnGizmos(Transform transform, IComponentTemplate.GizmosMode mode) { /*...*/ }
    public void OnValidate(UnityEngine.Object obj) { /*...*/ }
}
```

</details>


> В разделе [Кастомизация инспектора](#Кастомизация-инспектора) описана кастомизация отображения компонентов и использование вне шаблонов сущьностей.

</br>

# Связь с GameObject
Связываются сущности и GameObject-ы с помощью коннектов. Коннекты со стороны GameObject - `EcsEntityConnect`, со стороны сущности - `GameObjectConnect`. `EcsEntityConnect` - управляющий коннект, `GameObjectConnect` - создается/удаляется автоматически.
```c#
EcsEntityConnect connect = /*...*/;
entlong entity = _world.NewEntityLong();

// Связывание сущности с GameObject.
// Автоматически добавляется GameObjectConnect в сущность
// и применяются шаблоны.
connect.ConnectWith(entity);

// Или создать без применения шаблонов.
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

`AutoEntityCreator` автоматический создает сущность и связывает с GameObject. В инспекторе ему нужно указать `EcsEntityConnect` с которым связывать сущность и [Провайдер мира](#world-provider) в котором создать сущность.

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

# Шаблон Пайплайна
Пайплайн как и сущности можно собирать из шаблонов. Шаблоны пайплайна это модули, реализующие интерфейс `IEcsModule`。

По умолчанию расширение содержит 2 вида шаблонов: `ScriptablePipelineTemplate`, `MonoPipelineTemplate`.

## ScriptablePipelineTemplate
Хранится как отдельный ассет. Наследуется от `ScriptableObject`. Действия чтобы создать `ScriptableEntityTemplate` ассет: `Asset > Create > DragonECS > ScriptablePipelineTemplate`.

<p align="center">
<img width="450" src="https://github.com/user-attachments/assets/c61c483d-3f2f-4356-bf9d-62633fc06fce">   
</p>

## MonoPipelineTemplate
Крепится к `GameObject`. Наследуется от `MonoBehaviour`. Повесить компонент: `Add Component > DragonECS > MonoPipelineTemplate`.

<p align="center">
<img width="450" src="https://github.com/user-attachments/assets/4ba4d594-e031-4588-bd36-3273611db820">   
</p>

</br>

# EcsRootUnity
Упрощённая реализация Ecs Root для Unity; собирает пайплайн из шаблонов. Наследуется от `MonoBehaviour`. Чтобы добавить на GameObject: `Add Component > DragonECS > EcsRootUnity`.

<p align="center">
<img width="450" src="https://github.com/user-attachments/assets/3ff42747-0366-4db8-8015-9ea254d72feb">   
</p>


</br>


# FixedUpdate и LateUpdate 
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


# Кастомизация инспектора

## Атрибуты инспектора
+ **[ReferenceDropDown]** -
Применяется к полю с `[SerializeReference]`. Добавляет кнопку выбора типа из списка. Можно ограничить набор доступных типов, передав список в конструктор.
+ **[ReferenceDropDownWithout]** -
Используется вместе с `[ReferenceDropDown]`, чтобы исключить указанные типы (и их наследников) из списка выбора.
+ **[DragonMetaBlock]** -
Отображает значение в инспекторе так же, как компоненты в шаблонах сущности. Учитывает meta-атрибуты (`MetaGroup`, `MetaColor`, `MetaDescription`, `MetaID` и др.).


## Поведение Meta-атрибутов
+ Иерархическая группировка элементов в меню `Add Component` или `[ReferenceDropDown]` задаётся через `[MetaGroup]`.
+ Цвет компонента в инспекторе по умолчанию определяется именем типа. Режим окраски можно изменить в окне настроек. Явный цвет задаётся через `[MetaColor]`.
+ При совпадении имени типа и файла (или при наличии `[MetaID]`) рядом с кнопкой удаления появляется иконка файла: один клик — выделение скрипта в проекте, двойной клик — открытие.
+ Если указан `[MetaDescription]`, рядом отображается иконка подсказки с текстом описания.
+ При восстановлении **Missing Reference** с помощью [**Reference Repairer**](#Reference-Repairer), инструмент ищет соответствие старого и нового имени типа по атрибуту `[MetaID(id)]`.


## Примеры:

Атрибут `DragonMetaBlock`:
```c#
// Отображение поля настраиваемое мета-атрибутами.
// Аналогично компонентам в MonoEntityTemplate или ScriptableEntityTemplate.
[DragonMetaBlock]
public SomeComponent Component;

// Можно применять к любому полю любого типа.
[DragonMetaBlock]
public Foo Foo;
```

`ReferenceDropDown` и `ReferenceDropDownWithout`:
```c#
// Добавляет кнопку выбора реализации ITemplateNode из выпадающего списка.
[SerializeReference]
[ReferenceDropDown]
public ITemplateNode Template;
```

```c#
// Так же можно применять к любому полю любого типа. 
// В списке будут только тип Foo и его наследники, исключая FooExc и его наследников.
[SerializeReference]
[ReferenceDropDown(typeof(Foo))]
[ReferenceDropDownWithout(typeof(FooExc))]
public object Template;
```

Комбинирование и другие варианты использования:
```c#
// Атрибуты можно комбинировать.
[DragonMetaBlock]
[ReferenceDropDown]
public ITemplateNode Template;

// Обертка над ITemplateNode, аналогично примеру выше.
public ComponentTemplateProperty Template;

// Атрибуты корректно работают с массивами и листами.
[DragonMetaBlock]
[ReferenceDropDown]
public ITemplateNode[] Template;
```

</br>

# Поддержка Jobs

DragonECS по умолчанию совместим с Job системой Unity. Пример:
```c#
EcsWorld _world;
class Aspect : EcsAspect
{
    // Пул для unmanaged компонентов.
    public EcsValuePool<Cmp> Cmps = Inc;
}
public void Run()
{
    var job = new Job()
    {
        // Идентично Where, но возвращает unmanaged список сущностей.
        Entities = _world.WhereUnsafe(out Aspect a),
        // Конвертация пула в unmanaged версию
        Cmps = a.Cmps.AsNative(),
        X = 10f,
    };
    JobHandle jobHandle = job.Schedule(job.Entities.Count, 64);
    jobHandle.Complete();
}
```
```c#
// Unmanaged компонент.
public struct Cmp : IEcsValueComponent
{
    public float A;
}
private struct Job : IJobParallelFor
{
    public EcsUnsafeSpan Entities;
    public NativeEcsValuePool<Cmp> Cmps;
    public float X;
    public Job(EcsUnsafeSpan entities, float x)
    {
        Entities = entities;
        X = x;
    }
    public void Execute(int index)
    {
        var e = Entities[index];
        Cmps[e].A += X;
    }
}
```

</br>

# Документация проекта
В интеграции также есть окно документации проекта на основе мета-атрибутов. Открыть документацию: `Tools > DragonECS > Documentation`. Документация формируется при первом открытии окна и при нажатии кнопки `Update`.

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/f5795823-aeae-45df-8e25-db64df837513">   
</p>

</br>

# Окно настроек
В окне настроек доступно несколько опций, включая режимы отображения компонентов в инспекторе. Внизу находятся переключатели для define-переменных, используемых в фреймворке. Открыть окно настроек: `Tools > DragonECS > Settings`.

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/c794be8d-6884-4415-b24a-0a1a28f577a6">   
</p>

</br>

# Reference Repairer
Инструмент для восстановления Missing Reference. Некоторые части интеграции активно задействует `[SerializeReference]`, у которого есть известная проблема с потерей типов при переименовании. `Reference Repairer` упрощает процесс восстановления. Он может собирать все ассеты с потерянными типами, после этого предоставляет окно для указания новых имён потерянных типов и выполнит их восстановление в собранных ассетах. Открыть окно инструмента: `Tools > DragonECS > Reference Repairer`.
> Если потерянные типы имеют атрибут `[MetaID(id)]`, инструмент попытается автоматически сопоставить новое имя типа.
<p align="center">
<img width="700" src="https://github.com/user-attachments/assets/ffb2b78a-db43-445d-a371-6358250b8cee">   
</p>

</br>

# FAQ
## Не могу повесить `EcsEntityConnect` или другие компоненты
Иногда это происходит после обновления пакета. Решения: выполните `Assets -> Reimport All` или перезапустите Unity после удаления папки `Library` в корне проекта.
