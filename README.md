<div align=center>   

<p align="center">
  <img src="Graphics\logo.png" width="250">
</p>

### Blazing fast, minimal but complete dependency injection library for <a href="https://unity.com/">Unity</a>

Reflex is an [Dependency Injection](https://stackify.com/dependency-injection/) framework for [Unity](https://unity.com/). Making your classes independent of its dependencies, granting better separation of concerns. It achieves that by decoupling the usage of an object from its creation. This helps you to follow SOLID’s dependency inversion and single responsibility principles. Making your project more **readable, testable and scalable.**

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![](https://github.com/gustavopsantos/reflex/workflows/Tests/badge.svg)
[![PullRequests](https://img.shields.io/badge/PRs-welcome-blueviolet)](http://makeapullrequest.com)
[![Releases](https://img.shields.io/github/release/gustavopsantos/reflex.svg)](https://github.com/gustavopsantos/reflex/releases)
[![openupm](https://img.shields.io/npm/v/com.gustavopsantos.reflex?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.gustavopsantos.reflex/)
[![Unity](https://img.shields.io/badge/Unity-2021+-black.svg)](https://unity3d.com/pt/get-unity/download/archive)

</div>

---

<details>
<summary>📌 Table Of Contents</summary>

- [Overview](#-overview)
- [Installation](#-installation)
  - [Unity Package Manager](#unity-package-manager)
  - [Open Unity Package Manager](#open-unity-package-manager)
  - [Unity Package](#unity-package)
- [Getting Started](#-getting-started)
- [Scopes](#-scopes)
- [Bindings](#-bindings)
- [Resolving](#-resolving)
- [Callbacks](#-callbacks)
- [Manual Injection](#-manual-injection)
- [Extensions](#-extensions)
- [Debugger](#-debugger)
- [Settings](#-settings)
- [Performance](#-performance)
- [Scripting Restrictions](#-scripting-restrictions)
- [Support](#-support)
- [License](#-license)

</details>

---

## 👀 Overview
- **Fast:** ~3x faster than VContainer, ~7x faster than Zenject.
- **GC Friendly:** ~2x less allocations than VContainer, ~9x less allocations than Zenject.
- **AOT Support:** Basically theres no runtime Emit, so it works fine on IL2CPP builds. [<sup>[*]</sup>](#-scripting-restrictions)
- **Contract Table:** Allows usages of APIs like `container.All<IDisposable>`
- **Immutable Container**: Performant thread safety free of lock plus predictable behavior.

Compatible with the following platforms:

- iOS
- Android
- Windows/Mac/Linux
- PS4/PS5
- Xbox One/S/X and Xbox Series X/S
- WebGL

---

## 💾 Installation
You can install Reflex using any of the following methods:

### Unity Package Manager
```
https://github.com/gustavopsantos/reflex.git?path=/Assets/Reflex/#7.1.0
```

1. In Unity, open **Window** → **Package Manager**.
2. Press the **+** button, choose "**Add package from git URL...**"
3. Enter url above and press **Add**.

### Open Unity Package Manager

```bash
openupm install com.gustavopsantos.reflex
```

### Unity Package
1. Download the .unitypackage from [releases](https://github.com/gustavopsantos/reflex/releases) page.
2. Import Reflex.X.X.X.unitypackage

---

## 🚀 Getting Started
1. [Install Reflex](#-installation)
2. Create `ProjectInstaller.cs` with 
```csharp
using Reflex.Core;
using UnityEngine;

public class ProjectInstaller : MonoBehaviour, IInstaller
{
    public void InstallBindings(ContainerBuilder builder)
    {
        builder.AddSingleton("Hello");
    }
}
```
3. In unity project window
5. Create directory `Assets/Resources`
6. Select just created `Resources` dir
7. Right click, Create → Reflex → ProjectScope
8. With just created `ProjectScope` selected
9. Add `ProjectInstaller.cs` created at step 2 as a component
10. Create new scene `Greet`
11. Add `Greet` to `Build Settings` → `Scenes In Build`
12. Create `Greeter.cs` with
```csharp
using UnityEngine;
using System.Collections.Generic;
using Reflex.Attributes;

public class Greeter : MonoBehaviour
{
    [Inject] private readonly IEnumerable<string> _strings;

    private void Start()
    {
        Debug.Log(string.Join(" ", _strings));
    }
}
```
12. Add `Greeter.cs` to any gameobject in `Greet` scene
13. Inside Greet scene, create a new empty gameobject named `SceneScope` and attach `SceneScope` component
14. Create `GreetInstaller.cs` with
```csharp
using Reflex.Core;
using UnityEngine;

public class GreetInstaller : MonoBehaviour, IInstaller
{
    public void InstallBindings(ContainerBuilder builder)
    {
        builder.AddSingleton("World");
    }
}
```
15. Add `GreetInstaller.cs` to `Greet.unity` `SceneScope`
16. Create new scene `Boot`
17. Add `Boot` to `Build Settings` → `Scenes In Build`
18. Create `Loader.cs` with
```csharp
using Reflex.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour
{
    private void Start()
    {
	// If you are loading scenes without addressables
	var scene = SceneManager.LoadScene("Greet", new LoadSceneParameters(LoadSceneMode.Single));
	ReflexSceneManager.PreInstallScene(scene, builder => builder.AddSingleton("Beautiful"));

	// If you are loading scenes with addressables
	Addressables.LoadSceneAsync("Greet", activateOnLoad: false).Completed += handle =>
	{
		ReflexSceneManager.PreInstallScene(handle.Result.Scene, builder => builder.AddSingleton("Beautiful"));
		handle.Result.ActivateAsync();
	};
    }
}
```
19. Assign it to any gameobject at `Boot` scene
20. Thats it, hit play while on `Boot` scene
21. When Greet scene is loaded, there should be 3 instances implementing string contract
22. So when Greeter::Start is called, you should see the following log in the unity console: `Hello Beautiful world`

---

## 📦 Scopes
Container scoping refers to the ability of being able to create a container inheriting the registrations of its parent container while also being able to extend it.

### Project Scope
It is root scope.
It is created just before first scene opens by relying on `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]`
To register bindings to it, create a prefab, name it "ProjectScope", put it inside any Resources folder, and attach a "ProjectScope" component to it.
Then, create your installer as MonoBehaviour and implement IInstaller interface.
Remember to attach your installer to the ProjectScope prefab, as ProjectScope searches for every child implementing IInstaller when its time to create the ProjectScope container.
Theres a menu item to ease the process: Assets > Create > Reflex > ProjectScope
Remember to have a single ProjectScope to avoid undesired behaviour.
Note that ProjectScope prefab is not required, in case Reflex do not found ProjectScope, an empty root will be created.
ProjectScope instance will be disposed once app closes/app quits.
> Note that unity does not call OnDestroy deterministically, so rule of thumb is do not rely on injected dependencies on OnDestroy event functions.

### Scene Scope
It is scoped from ProjectScope, so it contains everything that ProjectScope do.
It is created and injected after Awake, and before Start. 
To register bindings to it, create a gameobject on desired scene, name it "SceneScope", put it as root game object, and attach a "SceneScope" component to it.
Then, create your installer as MonoBehaviour and implement IInstaller interface.
Remember to attach your installer to your SceneScope gameobject, as SceneScope searches for every child implementing IInstaller when its time to create the SceneScope container.
Theres a menu item to ease the process: GameObject > Reflex > Scene Context
Remember to have a single SceneScope to avoid undesired behaviour.
Note that SceneScope gameobject is not required, in case Reflex do not found SceneScope, an empty one will be created.
SceneScope instance will be disposed once scene is unloaded.
> Note that unity does not call OnDestroy deterministically, so rule of thumb is do not rely on injected dependencies on OnDestroy event functions. 

### Manual Scoping
```csharp
using var scopedContainer = parentContainer.Scope(builder =>  
{  
  // Extend your scoped container by adding extra registrations here  
});
```

## 🔩 Bindings

### AddSingleton (From Type)
```csharp
ContainerBuilder::AddSingleton(Type concrete, params Type[] contracts)
```
Adds a deferred object creation based on the type to be constructed and its contracts.
The object will be constructed lazyli, once first request to resolve any of its contracts is called.
Then **same** object will always be returned.
If object implements `IDisposable` it will be disposed when its parent Container are disposed.
Theres no need to pass `IDisposable` as contract to have your object disposed, however, if you want to retrieve all `IDisposable` by any API `Single<TContract>`, `Resolve<TContract>` or `All<TContract>` then yes, you have to specify it.

### AddSingleton (From Value)
```csharp
ContainerBuilder::AddSingleton(object instance, params Type[] contracts)
```
Adds an object already constructed by the user to the container as a singleton, everytime the contracts given is asked to be resolved, the same object will be returned.
If object implements `IDisposable` it will be disposed when its parent Container are disposed.
Theres no need to pass `IDisposable` as contract to have your object disposed, however, if you want to retrieve all `IDisposable` by any API `Single<TContract>`, `Resolve<TContract>` or `All<TContract>` then yes, you have to specify it.

### AddSingleton (From Factory)
```csharp
ContainerBuilder::AddSingleton<T>(Func<Container, T> factory, params Type[] contracts)
```
Adds a deferred object creation based on the given factory and its contracts.
The object will be constructed lazyli, once first request to resolve any of its contracts is called.
The factory will be ran once, and then the **same** object will always be returned.
If object implements `IDisposable` it will be disposed when its parent Container are disposed.
Theres no need to pass `IDisposable` as contract to have your object disposed, however, if you want to retrieve all `IDisposable` by any API `Single<TContract>`, `Resolve<TContract>` or `All<TContract>` then yes, you have to specify it.

### AddTransient (From Type)
```csharp
ContainerBuilder::AddTransient(Type concrete, params Type[] contracts)
```
Adds a deferred object creation based on the type to be constructed and its contracts.
The object will be constructed lazyli, once first request to resolve any of its contracts is called.
Then for any request of any contract, a new object will be created, use this carefully.
If object implements `IDisposable` it will be disposed when its parent Container are disposed.
Theres no need to pass `IDisposable` as contract to have your object disposed, however, if you want to retrieve all `IDisposable` by any API `Single<TContract>`, `Resolve<TContract>` or `All<TContract>` then yes, you have to specify it.

### AddTransient (From Value)
```csharp
ContainerBuilder::AddTransient(object instance, params Type[] contracts)
```
Adds an object already constructed by the user to the container as a transient.
Its gonna be returned only on first time it gets resolved, second time an exception will be throw.
If object implements `IDisposable` it will be disposed when its parent Container are disposed.
Theres no need to pass `IDisposable` as contract to have your object disposed, however, if you want to retrieve all `IDisposable` by any API `Single<TContract>`, `Resolve<TContract>` or `All<TContract>` then yes, you have to specify it.

### AddTransient (From Factory)
```csharp
ContainerBuilder::AddTransient(Func<Container, T> factory, params Type[] contracts)
```
Adds a deferred object creation based on the given factory and its contracts.
The object will be constructed lazyli, once first request to resolve any of its contracts is called.
Then for any request of any contract, a new object will be created, use this carefully.
If object created by factory implements `IDisposable` it will be disposed when its parent Container are disposed.
Theres no need to pass `IDisposable` as contract to have your object disposed, however, if you want to retrieve all `IDisposable` by any API `Single<TContract>`, `Resolve<TContract>` or `All<TContract>` then yes, you have to specify it.

## 🔍 Resolving
### Constructor
If your type is non-mono, and its gonna be created by the container, then the most recommended way to inject dependencies into it its by constructor injection.
Its simply as just requesting the contracts you need as following example:
```csharp
private class Foo
{  
	...
  
	public NumberManager(IInputManager inputManager, IEnumerable<IManager> managers)  
	{  
		...
	}  
}
```

> Note that constructor injection relies on `Resolve<TContract>` API, so in case theres theres two objects with `IInputManager` contract, the last one will be injected. 

### Attribute
Attribute injection is the way to go for **MonoBehaviours**.
You can use it to inject fields, writeable properties and methods like following:
```csharp
class Foo : MonoBehaviour  
{  
	[Inject] private readonly IInputManager _inputManager;  
	[Inject] public IEnumerable<IManager> Managers { get; private set; }  
  
	[Inject]  
	private void Inject(IEnumerable<int> numbers) // Method name here does not matter  
	{  
	  ...
	}  
}
```
> Note that attribute injection also works on non-mono classes.
### Single
`Container::Single<TContract>` actually validates that theres a single binding implementing given contract, and returns it.
If theres more than one the following exception will be thrown.
```
InvalidOperationException: Sequence contains more than one element
```
Its recommended for every binding that you know that there should be a single binding implementing the contract.
### Resolve
`Container::Single<TContract>` runs no validations, and return the last valid object implementing given contract.

### All
`Container::All<TContract>` returns all objects implementing given contract.
Example:
```csharp
private void Documentation_Bindings()  
{  
	var container = new ContainerBuilder("")  
		.AddSingleton(1)  
		.AddSingleton(2)  
		.AddSingleton(3)  
		.Build();  
  
	Debug.Log(string.Join(", ", container.All<int>())); // Prints: 1, 2, 3
}
```

---

## 🪝 Callbacks
### `ContainerBuilder::OnContainerBuilt`
OnContainerBuilt is a instance callback of ContainerBuilder, its called once the container is fully built and initialized properly. 

---

## 💉 Manual Injection

```csharp
AttributeInjector::void Inject(object obj, Container container)
// Injects given object fields, properties and methods that was annotated with Inject attribute
```

```csharp
ConstructorInjector::object Construct(Type concrete, Container container)
// construct object of given type, using the constructor with most parameters, using given container to pull the constructor arguments
```

```csharp
GameObjectInjector::void InjectSingle(GameObject gameObject, Container container)
// Optmized code meant to find injectables (MonoBehaviours) from a given GameObject, to then, inject using AttributeInjector
// This option injects only the first MonoBehaviour found on the given GameObject
```

```csharp
GameObjectInjector::void InjectObject(GameObject gameObject, Container container)
// Optmized code meant to find injectables (MonoBehaviours) from a given GameObject, to then, inject using AttributeInjector
// This option injects all MonoBehaviours found on the given GameObject (not recursively, so it does not account for children) 
```

```csharp
GameObjectInjector::void InjectRecursive(GameObject gameObject, Container container)
// Optmized code meant to find injectables (MonoBehaviours) from a given GameObject, to then, inject using AttributeInjector
// This option injects all MonoBehaviours found on the given GameObject and its childrens recursively 
```

```csharp
GameObjectInjector::void InjectRecursiveMany(List<GameObject> gameObject, Container container)
// Optmized code meant to find injectables (MonoBehaviours) from a given GameObject, to then, inject using AttributeInjector
// This option injects all MonoBehaviours found on the given list of GameObject and its childrens recursively 
```

---

## 🧩 Extensions
```csharp
// Allows you to get a scene container, allowing you to resolve/inject dependencies in a different way during runtime
SceneExtensions::GetSceneContainer(this Scene scene)
{
    return UnityInjector.ContainersPerScene[scene];
}

// Usage example:
var foo = gameObject.scene.GetSceneContainer().Resolve<IFoo>();
```

---

## 🐛 Debugger
![Preview](Graphics/reflex-debugger.png)  
It can be accessed by menu item  Reflex → Debugger.  
To enable reflex debug mode you must go to Edit → Project Settings → Player, then in the Other Settings panel, scroll down to Script Compilation → Scripting Define Symbols and add `REFLEX_DEBUG`. This can be easily achieved by clicking on the bug button at bottom right corner inside Reflex Debugger Window.
> Note that debug mode reduces performance and increases memory pressure, so use it wisely.  

And from there you can check:
- Hierarchy of Containers, Bindings and Instances
- Binding Contracts, Kind and Lifetime
- Resolution Count
- Container call stack (who created the container)
- Binding call stack (who created the binding)
- Instance call stack (who resolved the binding making selected instance to be instantiated)

---

## 🪛 Settings
Its a  `ReflexSettings` scriptable object instance, named `ReflexSettings` that should live inside a `Resources` folder.
It can be created by asset menu item Assets → Create → Reflex → Settings.

Currently, logging verbosity is configured in this file, and default value is set to `Info`

> Non-Obligatory to have but projects without it will fallback using default settings

---

## 📊 Performance
> Resolving ten thousand times a transient dependency with four levels of chained dependencies. See [NestedBenchmarkReflex.cs](Assets/Reflex.Benchmark/NestedBenchmarkReflex.cs).

### Android + Mono
|           | GC      | Time    | GC Ratio | Time Ratio |
|-----------|--------:|--------:|---------:|-----------:|
| Reflex    | 54.7KB  | 9.3ms   | 1x       | 1x         |
| Zenject   | 512KB   | 63.2ms  | 9.36x    | 6.79x      |
| VContainer| 128.9KB | 29.8ms  | 2.35x    | 3.20x      |

### Android + IL2CPP
|           | GC      | Time   | GC Ratio | Time Ratio |
|-----------|--------:|-------:|---------:|-----------:|
| Reflex    | 140.6KB | 7.4ms  | 1x       | 1x         |
| Zenject   | 1024KB  | 23.6ms | 7.28x    | 3.18x      |
| VContainer| 257.8KB | 9.2ms  | 1.83x    | 1.24x      |

### Windows + Mono
|           | GC      | Time   | GC Ratio | Time Ratio |
|-----------|--------:|-------:|---------:|-----------:|
| Reflex    | 109.4KB | 1.2ms  | 1x       | 1x         |
| Zenject   | 1024KB  | 9.2ms  | 9.36x    | 7.66x      |
| VContainer| 257.8KB | 3.3ms  | 2.35x    | 2.75x      |

### Windows + IL2CPP
|           | GC      | Time   | GC Ratio | Time Ratio |
|-----------|--------:|-------:|---------:|-----------:|
| Reflex    | 140.6KB | 2.9ms  | 1x       | 1x         |
| Zenject   | 1024KB  | 9.3ms  | 7.28x    | 3.20x      |
| VContainer| 257.8KB | 5.1ms  | 1.83x    | 1.75x      |

## 🚫 Scripting Restrictions
If you are taking advantage of reflex to inject `IEnumerable<T>` in your constructors **AND** your are building for **IL2CPP**, you will probably get some exceptions like following:

```
System.ExecutionEngineException: Attempting to call method 'System.Linq.Enumerable::Cast<ANY-TYPE>' for which no ahead of time (AOT) code was generated.
```

This happens because compiler does not know at compile time that a specific `System.Linq.Enumerable::Cast<T>` should be included. And currently Reflex does not implement any type of assembly weaving.
> Reflex 4.0.0 had and assembly weaver that was relying on unity UnityEditor.Compilation.CompilationPipeline events and Mono.Cecil. But it was causing conflicts with projects using Burst. So its being removed temporarly until a definitive solution is found.
> Most probably we are going to weave assemblies the same way unity is doing for Burst as well.

Temporary workaround example:

```csharp
class NumberManager
{
    public IEnumerable<int> Numbers { get; }

    public NumberManager(IEnumerable<int> numbers)
    {
        Numbers = numbers;
    }
    
    // https://docs.unity3d.com/Manual/ScriptingRestrictions.html
    [Preserve] private static void UsedOnlyForAOTCodeGeneration()
    {
        Array.Empty<object>().Cast<int>(); // This compiler hint will get rid of: System.ExecutionEngineException: Attempting to call method 'System.Linq.Enumerable::Cast<System.Int32>' for which no ahead of time (AOT) code was generated. 
        throw new Exception("This method is used for AOT code generation only. Do not call it at runtime.");
    }
}
```

## 🤝 Support

Ask your questions and participate in discussions regarding Reflex related and dependency injection topics at the Reflex Discord server. 

<a href="https://discord.gg/XM47TsGScH"><img src="https://amplication.com/images/discord_banner_purple.svg" /></a>

---

## 📜 License
Reflex is distributed under the terms of the MIT License.
A complete version of the license is available in the [LICENSE](LICENSE) file in
this repository. Any contribution made to this project will be licensed under
the MIT License.
