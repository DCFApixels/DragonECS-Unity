<p align="center">
<img width="400" src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/3bb780b7-ab64-4f02-a9be-9632fcfe1b47">
</p>

<p align="center">
<img alt="Version" src="https://img.shields.io/github/package-json/v/DCFApixels/DragonECS-Unity?color=%23ff4e85&style=for-the-badge">
<img alt="GitHub" src="https://img.shields.io/github/license/DCFApixels/DragonECS-Unity?color=ff4e85&style=for-the-badge">
<a href="https://discord.gg/kqmJjExuCf"><img alt="Discord" src="https://img.shields.io/badge/Discord-JOIN-00b269?logo=discord&logoColor=%23ffffff&style=for-the-badge"></a>
<a href="http://qm.qq.com/cgi-bin/qm/qr?_wv=1027&k=IbDcH43vhfArb30luGMP1TMXB3GCHzxm&authKey=s%2FJfqvv46PswFq68irnGhkLrMR6y9tf%2FUn2mogYizSOGiS%2BmB%2B8Ar9I%2Fnr%2Bs4oS%2B&noverify=0&group_code=949562781"><img alt="QQ" src="https://img.shields.io/badge/QQ-JOIN-00b269?logo=tencentqq&logoColor=%23ffffff&style=for-the-badge"></a>
</p>

# Integration with Unity for [DragonECS](https://github.com/DCFApixels/DragonECS)

<table>
  <tr></tr>
  <tr>
    <td colspan="3">Readme Languages:</td>
  </tr>
  <tr></tr>
  <tr>
    <td nowrap width="100">
      <a href="https://github.com/DCFApixels/DragonECS-Unity/blob/main/README-RU.md">
        <img src="https://github.com/user-attachments/assets/7bc29394-46d6-44a3-bace-0a3bae65d755"></br>
        <span>Русский</span>
      </a>  
    </td>
    <td nowrap width="100">
      <a href="https://github.com/DCFApixels/DragonECS-Unity">
        <img src="https://github.com/user-attachments/assets/3c699094-f8e6-471d-a7c1-6d2e9530e721"></br>
        <span>English(WIP)</span>
      </a>  
    </td>
  </tr>
</table>

</br>

This package integrates DragonECS with the Unity editor and runtime. It provides visual debugging and profiling tools, editor templates for entity/component setup, and utilities for binding entities to Unity GameObjects.

> [!WARNING]
> The project is a work in progress, API may change.
> 
> While the English version of the README is incomplete, the [Russian README](https://github.com/DCFApixels/DragonECS-Unity/blob/main/README-RU.md) contains additional details.

//https://gist.github.com/DCFApixels/c250f2561f09e09ab3e6a4bd4f3013cb#file-unitycomponenttemplates-cs

# Table of contents
- [Installation](#installation)
- [Debug](#debug)
  - [Debug service](#debug-service)
  - [Visual debugging](#visual-debugging)
- [Templates](#templates)
- [Binding to GameObjects](#binding-to-gameobjects)
- [World Provider](#world-provider)
- [Pipeline template](#pipeline-template)
- [FixedUpdate and LateUpdate](#fixedupdate-and-lateupdate)
- [Inspector Customization](#inspector-customization)
- [Jobs Support](#jobs-support)
- [Project documentation](#project-documentation)
- [Settings window](#settings-window)
- [Reference Repairer](#reference-repairer)
- [FAQ](#faq)

</br>

# Installation
Versioning semantics - [Open](https://gist.github.com/DCFApixels/af79284955bf40e9476cdcac79d7b098#file-dcfapixels_versioning-md)

## Environment
Requirements:
- Dependency: [DragonECS](https://github.com/DCFApixels/DragonECS)
- Minimum C# version: 8.0
- Minimum Unity version: 2021.2.0

## Unity Installation
* ### Unity package
Installation as a Unity package is supported by adding the Git URL [in PackageManager](https://docs.unity3d.com/2023.2/Documentation/Manual/upm-ui-giturl.html):
```
https://github.com/DCFApixels/DragonECS-Unity.git
```
Or add the package entry to `Packages/manifest.json`:
```
"com.dcfa_pixels.dragonecs-unity": "https://github.com/DCFApixels/DragonECS-Unity.git",
```

### Source install
The package sources can be copied directly into the project.

</br>

# Debug
## Debug service
`UnityDebugService` implementation of the [Debug service for `EcsDebug`](https://github.com/DCFApixels/DragonECS/blob/main/README.md#ecsdebug). In the editor it initializes automatically and integrates with Unity systems: `EcsDebug.Print` is forwarded to the Unity console, `EcsProfilerMarker` is connected to the Unity profiler, and related debug functionality is exposed.
```c#
// Activate manually
UnityDebugService.Activate();

// Print to Unity console
EcsDebug.Print("Example message");

var someMarker = new EcsProfilerMarker("SomeMarker");
someMarker.Begin();
// Measured time will be visible in the Unity profiler.
someMarker.End();

// Stop play mode
EcsDebug.Break();
```

## Visual debugging

Visual debugging is provided as editor monitor objects that display internal framework state during `Play Mode`. Monitors are placed in the `DontDestroyOnLoad` section.

```c#
_pipeline = EcsPipeline.New()
    //...
    // Debugging initialization for the pipeline and worlds.
    .AddUnityDebug(_world, _eventWorld)
    //...
    .BuildAndInit();
```

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/54e3f6d1-13c4-4226-a983-c672a29d33bb">   
</p>

-----

* ### `PipelineMonitor`
Displays `EcsPipeline` state. Systems are displayed in the order of their execution.

<p align="center">
<img width="270px" src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/3682fd0f-f47a-40ed-9d4c-cbad5d512e5d">   
</p>

-----

* ### `PipelineProcessMonitor` 
Displays processes and systems in a matrix layout. Systems are shown in execution order. A mark at the intersection of a system and a process indicates that the system is part of that process.

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/c1a7960a-d65a-4f22-9597-fd863ff2b00c">   
</p>

-----

* ### `WorldMonitor`
Displays `EcsWorld` state. A separate monitor is created for each world passed to `AddUnityDebug(...)`.

<p align="center">
<img src="https://github.com/user-attachments/assets/83905e7a-a5d1-4470-883a-c3b318cb4726">   
</p>

-----

* ### `WorldQueriesMonitor` 
Located together with `WorldMonitor`, shows a list of all Where queries that systems have executed. At the top there is a search field for filtering queries by component names. The search string can be split with a `/` character to search for multiple components at once. Next to each Where query there is a `Snapshot` button; clicking it opens a window showing a list of all entities that currently match the query mask.

<p align="center">
<img width="400px" src="https://github.com/user-attachments/assets/e6edf718-5c73-437e-abeb-c192ace9f927" />
</p>

-----

* ### `EntityMonitor`
Displays entity state and allows adding, modifying or removing components at runtime (Play Mode). One monitor is created per entity; entity monitors are grouped under the corresponding world monitor.

<p align="center">
<img src="https://github.com/user-attachments/assets/fc7ac96d-a9a4-45bd-9695-e80f790495ef">   
</p>

-----

</br>

# Templates
The integration provides templates that extend `ITemplateNode` for editor-driven entity configuration.

## ScriptableEntityTemplate
Stored as a `ScriptableObject` asset.

<details>
<summary>Create asset: Asset > Create > DragonECS > ScriptableEntityTemplate.</summary>

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/8362e2d8-b83a-4dfc-91fd-38993746012f">   
</p>

</details>

To add a component to the `Add Component` menu, a [component template](#component-template) is required.

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/26379ee5-cadd-4838-a3b6-5b46771012c1">   
</p>

-----

## MonoEntityTemplate
Attachable to a `GameObject`. Inherits from `MonoBehaviour`.

<details>
<summary>Add component: Add Component > DragonECS > MonoEntityTemplate.</summary>

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/07a43cb7-96e5-440c-965d-2970803df330">   
</p>

</details>

To add a component to the `Add Component` menu, a [component template](#component-template) is required.

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/7f6b722e-6f98-4d13-b2cd-5d576a3610bd">   
</p>

-----

## Component Template

To expose a component in the `Add Component` menu, a template is required. Component templates are types implementing `IComponentTemplate` or components implementing `ITemplateNode` together with `IEcsComponentMember`.

### Implementation

* Simple example:
```c#
// Add [Serializable] attribute to the component type.
[Serializable]
struct SomeComponent : IEcsComponent { /* ... */ }
class SomeComponentTemplate : ComponentTemplate<SomeComponent> { }
```

```c#
// Same for tag components.
[Serializable]
struct SomeTagComponent : IEcsTagComponent { }
class SomeTagComponentTemplate : TagComponentTemplate<SomeTagComponent> { }
```

<details>
<summary>Other approaches</summary>

#### Implementing `ITemplateNode` on a component

This approach can be convenient because it does not require a separate template class; the component itself acts as the template and is straightforward to implement. The downside is higher risk of missing references when types are renamed if `[SerializeReference]` is used.
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

> The section [Inspector Customization](#Inspector-Customization) describes customization of component display and usage outside of entity templates.

#### Custom template implementation

If built-in `ComponentTemplate<T>` or `TagComponentTemplate<T>` do not fit the requirements, implement a custom template by implementing `IComponentTemplate`. This can be useful for custom pools. In most cases the built-in templates are sufficient.

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
</br>

### Customizing type display
The `Add Component` dropdown supports hierarchical grouping based on the `[MetaGroup]` attribute.

By default components in the Inspector receive a deterministic color derived from the component name. The display mode can be changed in the settings window. A specific color can be set using the `[MetaColor]` attribute.

If the integration locates the corresponding script (by matching the type name to a file or via `[MetaID]`), a file icon appears next to the remove button — single-click selects the script in the project, double-click opens it.

If `[MetaDescription]` is present, a tooltip icon is shown with the description text.

</br>

### Using component templates outside standard entity templates
Component templates can be used outside of `MonoEntityTemplate` and `ScriptableEntityTemplate` in arbitrary classes. Two approaches are provided:

Attribute `[ComponentTemplateField]`:
```c#
// Display a field as a component, customizable with meta attributes.
// Similar to components in MonoEntityTemplate or ScriptableEntityTemplate.
[SerializeField, ComponentTemplateField]
private SomeComponent _someComponent1;
```
```c#
// For SerializeReference adds a button to pick available ITemplateNode implementation
[SerializeReference, ComponentTemplateField]
private ITemplateNode _someComponent1;
```

Wrapper `ComponentTemplateProperty`:
```c#
// Wrapper around ITemplateNode, similar to the ComponentTemplateField example.
private ComponentTemplateProperty _someComponent2;
```

Both approaches work for arrays too:
```c#
[SerializeReference, ComponentTemplateField]
private IComponentTemplate[] _components;
// or
private ComponentTemplateProperty[] _components;
```

</br>

# Binding to GameObjects
Entities and GameObjects are linked using connects. From the GameObject side use `EcsEntityConnect`; from the entity side `GameObjectConnect` is created/removed automatically. `EcsEntityConnect` acts as the manager connect.
```c#
EcsEntityConnect connect = /*...*/;
entlong entity = _world.NewEntityLong();

// Connect entity with GameObject.
// GameObjectConnect is added to the entity automatically
// and templates are applied.
connect.ConnectWith(entity);

// Or create without applying templates.
connect.ConnectWith(entity, false);

// Disconnect.
// GameObjectConnect will be removed automatically.
connect.Disconnect();
```
<details>
<summary>Add component: Add Component > DragonECS > EcsEntityConnect.</summary>

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/cfa6eb1c-82ba-47f6-bee1-7986c1e31be7">   
</p>

</details>

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/3484ed12-5417-4450-9908-1d3eb2858a2b">   
</p>

> To view all components of the linked entity, expand `RUNTIME COMPONENTS`.

> The bottom panel contains utility buttons: 1) Disconnect entity. 2) Delete entity. 3) Auto-fill template array. 4) Cascade auto-fill for all child connects in the hierarchy.

---

`AutoEntityCreator` automatically creates an entity and binds it to the GameObject. In the inspector specify the `EcsEntityConnect` to use and the [world provider](#world-provider) where the entity should be created.

<details>
<summary>Add component: Add Component > DragonECS > AutoEntityCreator.</summary>

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/29bfef68-6e77-467c-84d3-14d73a9c614d">   
</p>

</details>

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/55c11f1c-c0e0-435c-af9b-4c06678491a6">   
</p>

> The bottom panel includes helper buttons: 1) Auto-fill the connect reference. 2) Cascade auto-fill for all child instances in the hierarchy.

</br>

# World Provider
`EcsWorldProvider<TWorld>` is a `ScriptableObject` wrapper over `TWorld` intended to expose a world instance via the Unity inspector. For simple scenarios the singleton provider `EcsDefaultWorldSingletonProvider` is sufficient.

```c#
// The singleton provider is created automatically under "Assets/Resource".
EcsDefaultWorldSingletonProvider provider = EcsDefaultWorldSingletonProvider.Instance;
// ...

EcsDefaultWorld world = new EcsDefaultWorld();
// Set the world instance to the provider.
provider.Set(world);

// ...

// Get the world instance; the provider will create a default world if empty.
EcsDefaultWorld world = provider.Get();

EcsPipeline pipeline = EcsPipeline.New()
    //...
    // Inject the provider world into systems.
    .Inject(world)
    //...
    .BuildAndInit();
```

<details>
<summary>Example: provider implementation for a custom world type</summary>

```c#
// Example implementation of a provider to expose a custom world type
[CreateAssetMenu(fileName = nameof(EcsMyWorldProvider), menuName = EcsConsts.FRAMEWORK_NAME + "/WorldProviders/" + nameof(EcsMyWorldProvider), order = 1)]
public class EcsMyWorldProvider : EcsWorldProvider<EcsMyWorld> { }

// Example singleton provider implementation for a custom world type
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
<summary>Create provider asset: Asset > Create > DragonECS > WorldProviders > Select world type.</summary>
    
<p align="center">
<img width="780px" src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/8aa1bd82-8a15-46ce-b950-3e74252243c6">   
</p>

</details>

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/d01a671a-69e9-44b9-9ad1-e58d0e8857d7">   
</p>

</br>

# Pipeline template
Pipelines and entities can be assembled from templates. Pipeline templates are modules implementing the `IEcsModule` interface.

The package provides two pipeline template types by default: `ScriptablePipelineTemplate` and `MonoPipelineTemplate`.

## ScriptablePipelineTemplate
Stored as a `ScriptableObject` asset. Create via `Asset > Create > DragonECS > ScriptablePipelineTemplate`.

<p align="center">
<img width="450" src="https://github.com/user-attachments/assets/c61c483d-3f2f-4356-bf9d-62633fc06fce">   
</p>

## MonoPipelineTemplate
Attachable to a `GameObject`. Inherits from `MonoBehaviour`. Add via `Add Component > DragonECS > MonoPipelineTemplate`.

<p align="center">
<img width="450" src="https://github.com/user-attachments/assets/4ba4d594-e031-4588-bd36-3273611db820">   
</p>

</br>

# EcsRootUnity
Lightweight Ecs root implementation for Unity. Builds a pipeline from pipeline templates. Derives from `MonoBehaviour`. Add via `Add Component > DragonECS > EcsRootUnity`.

<p align="center">
<img width="450" src="https://github.com/user-attachments/assets/3ff42747-0366-4db8-8015-9ea254d72feb">   
</p>


</br>


# FixedUpdate and LateUpdate
```c#
using DCFApixels.DragonECS;
using UnityEngine;
public class EcsRoot : MonoBehaviour
{
    private EcsPipeline _pipeline;
    //...
    private void Update()
    {
        // Standard pipeline run.
        _pipeline.Run();
    }
    private void FixedUpdate()
    {
        // Pipeline run for FixedUpdate.
        _pipeline.FixedRun();
    }
    private void LateUpdate()
    {
        // Pipeline run for LateUpdate.
        _pipeline.LateRun();
    }
    // ...
}
```

</br>

# Inspector Customization

## Inspector Attributes
+ **[ReferenceDropDown]** -
Applied to a field with `[SerializeReference]`. Adds a type selection button from a list. The set of available types can be restricted by passing a list to the constructor.

+ **[ReferenceDropDownWithout]** -
Used together with `[ReferenceDropDown]` to exclude the specified types (and their descendants) from the selection list.

+ **[DragonMetaBlock]** -
Displays the value in the inspector similarly to how components are displayed in entity templates. Takes meta-attributes into account (`MetaGroup`, `MetaColor`, `MetaDescription`, `MetaID`, etc.).

## Behavior of Meta‑Attributes
+ Hierarchical grouping of items in the `Add Component` menu or `[ReferenceDropDown]` is defined via `[MetaGroup]`.
+ The component color in the inspector is determined by the type name by default. The coloring mode can be changed in the settings window. An explicit color is set via `[MetaColor]`.
+ When the type name matches the file name (or when `[MetaID]` is present), a file icon appears next to the delete button: a single click selects the script in the project, a double click opens it.
+ If `[MetaDescription]` is specified, a tooltip icon with the description text is displayed next to it.
+ When restoring a Missing Reference using Reference Repairer, the tool searches for a match between the old and new type names by the `[MetaID(id)]` attribute.


## Examples:

`DragonMetaBlock` attribute:
```c#
// Display of the field customizable via meta-attributes.
// Similar to components in MonoEntityTemplate or ScriptableEntityTemplate.
[DragonMetaBlock]
public SomeComponent Component;

// Can be applied to any field of any type.
[DragonMetaBlock]
public Foo Foo;
```

`ReferenceDropDown` and `ReferenceDropDownWithout`:
```c#
// Adds a button to select an implementation of ITemplateNode from a drop-down list.
[SerializeReference]
[ReferenceDropDown]
public ITemplateNode Template;
```

```c#
// Also applicable to any field of any type.
// The list will contain only type Foo and its descendants, excluding FooExc and its descendants.
[SerializeReference]
[ReferenceDropDown(typeof(Foo))]
[ReferenceDropDownWithout(typeof(FooExc))]
public object Template;
```

Combination and other use cases:
```c#
// Attributes can be combined.
[DragonMetaBlock]
[ReferenceDropDown]
public ITemplateNode Template;

// A wrapper over ITemplateNode, similar to the example above.
public ComponentTemplateProperty Template;

// Attributes work correctly with arrays and lists.
[DragonMetaBlock]
[ReferenceDropDown]
public ITemplateNode[] Template;
```

</br>

# Jobs Support

DragonECS is compatible with Unity's Job system by default. Example:
```c#
EcsWorld _world;
class Aspect : EcsAspect
{
    // Pool for unmanaged components.
    public EcsValuePool<Cmp> Cmps = Inc;
}
public void Run()
{
    var job = new Job()
    {
        // Same as Where, but returns an unmanaged entity list.
        Entities = _world.WhereUnsafe(out Aspect a),
        // Convert the pool to its unmanaged version
        Cmps = a.Cmps.AsNative(),
        X = 10f,
    };
    JobHandle jobHandle = job.Schedule(job.Entities.Count, 64);
    jobHandle.Complete();
}
```
```c#
// Unmanaged component.
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

# Project documentation
A documentation window based on meta-attributes is available via `Tools > DragonECS > Documentation`. Documentation is generated on first open and when the `Update` button is pressed.

<p align="center">
<img src="https://github.com/DCFApixels/DragonECS-Unity/assets/99481254/f5795823-aeae-45df-8e25-db64df837513">   
</p>

</br>

# Settings window
The settings window exposes several options, including component display modes in the Inspector. At the bottom, toggles for framework define symbols are available. Open via `Tools > DragonECS > Settings`.

<p align="center">
<img src="https://github.com/user-attachments/assets/905c03dd-d277-48b9-9963-455a09c4ceda">   
</p>

</br>

# Reference Repairer
Some parts of the integration heavily use `[SerializeReference]`, which can lose type information after renames. The `Reference Repairer` tool collects assets with missing types and provides an interface to map new type names and repair collected assets. Open via `Tools > DragonECS > Reference Repairer`.
> If missing types have a `[MetaID(id)]` attribute, the tool attempts automatic mapping.
<p align="center">
<img width="700" src="https://github.com/user-attachments/assets/ffb2b78a-db43-445d-a371-6358250b8cee">   
</p>

</br>

# FAQ
## Cannot add `EcsEntityConnect` or other components
This issue may appear after a package update. Recommended remediation steps: run `Assets -> Reimport All`, or close Unity, remove the project's `Library` folder and reopen the project.
