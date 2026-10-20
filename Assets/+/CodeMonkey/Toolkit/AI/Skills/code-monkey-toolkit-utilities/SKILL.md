---
name: code-monkey-toolkit-utilities
description: "Use this skill whenever a user asks to 'run this after 3 seconds', 'repeat this every X seconds', 'call something every frame without a MonoBehaviour', 'take a screenshot', 'show an fps counter', 'get the mouse world position', 'pick a random element from a list', 'call ChatGPT from Unity', 'download a texture from a URL', 'save the game / player data to a file', or 'load the most recent save' in Code Monkey Toolkit. Covers FunctionTimer, FunctionPeriodic, FunctionUpdater, WebRequests, TakeScreenshot, SaveFileScreenshot, SaveSystem (JSON save/load), RandomData, FPSCounter, MousePosition, GameAssets, MonoBehaviourHooks, PointerHooks, and OpenAIAPI (API key required). Not for controllers, cameras, UI widgets, grids, or shaders — see the controllers-and-camera, ui-and-feedback, grid-systems, and shaders-and-effects sibling skills. When in doubt whether a utility request could involve Code Monkey Toolkit, use this skill — Prerequisites shows how to confirm the asset is installed."
metadata:
  asset: "Code Monkey Toolkit"
  publisher: "Code Monkey"
  asset-version: "1.0.5"
  skill-version: "1.1.0"
  unity: "6000.0.60f1+"
  render-pipelines: "Built-in, URP, HDRP"
  category: "tools/utilities"
  asset-store-url: "https://assetstore.unity.com/packages/tools/utilities/code-monkey-toolkit-309282"
  documentation-url: "https://unitycodemonkey.com/"
  support-url: "https://assetstore.unity.com/packages/tools/utilities/code-monkey-toolkit-309282"
  last-verified: "2026-09-07"
---

# Code Monkey Toolkit — Utilities

Thirteen static-class and MonoBehaviour utilities that eliminate boilerplate for the most common programming tasks: delayed and repeating callbacks, per-frame update hooks, HTTP requests, screenshots, random data generation, framerate display, mouse-world-position queries, centralized asset references, lifecycle event hooks, UI pointer hooks, and direct OpenAI API integration.

## When to use this skill

**Use when the user says things like:**
- "run this after 3 seconds", "delay a function call", "fire-and-forget timer"
- "call this every second", "repeat every X seconds", "periodic callback"
- "run code every frame without a MonoBehaviour"
- "take a screenshot", "capture the screen as a Texture2D", "save screenshot to file"
- "show fps counter", "display framerate"
- "get mouse position in 3D world space", "mouse world position 2D"
- "get a random color", "pick a random element", "random name", "random direction"
- "call ChatGPT from my game", "use OpenAI API in Unity", "ask GPT a question"
- "make an HTTP GET/POST request", "download data from a URL", "web request"
- "save game data with a screenshot", "save file with image"
- "save the game", "save player data to a file", "load the most recent save", "json save system"
- "hook into OnDestroy without modifying the class", "listen for pointer clicks"

**Not for:**
- Player controllers, camera rigs → `code-monkey-toolkit-controllers-and-camera`
- UI widgets (tooltips, chat bubbles, text popups) → `code-monkey-toolkit-ui-and-feedback`
- Grid systems → `code-monkey-toolkit-grid-systems`
- Shaders, VFX → `code-monkey-toolkit-shaders-and-effects`
- Gameplay systems (health, interaction) → `code-monkey-toolkit-gameplay-systems`

## Prerequisites

- **Asset:** Code Monkey Toolkit 1.0.5
- **Unity:** 6000.0.60f1 or later
- **OpenAIAPI only:** an OpenAI API key; set `API_KEY` constant in `Assets/CodeMonkey/Toolkit/Tools/OpenAIAPI/Scripts/OpenAIAPI.cs` before use
- **FPSCounter:** requires TextMeshPro (`com.unity.textmeshpro`)
- **MousePosition (New Input System):** uses `Mouse.current.position.value` from `UnityEngine.InputSystem`

**Programmatic install check:**
```csharp
bool installed = System.IO.File.Exists(
    "Assets/CodeMonkey/Toolkit/Tools/FunctionTimer/FunctionTimer.cs");
// Or confirm namespace resolves:
// var t = typeof(CodeMonkey.Toolkit.TFunctionTimer.FunctionTimer);
```
Open **Tools → Code Monkey Toolkit** and confirm utility tools appear. If missing, direct the user to the Asset Store URL above.

## Quick start

Fire a one-shot delayed action with FunctionTimer:

```csharp
using CodeMonkey.Toolkit.TFunctionTimer;

// 3 seconds after this line executes, print a message
FunctionTimer.Create(() => Debug.Log("3 seconds have passed!"), 3f);
```

No scene objects, no MonoBehaviour, no coroutines needed. The timer creates its own hidden GameObject automatically and cleans up after firing.

Observable result: the message appears in the Console exactly 3 seconds after the call (during Play Mode).

## Workflows

### Workflow: Function Timer (one-shot delay)

**Goal:** Execute an `Action` once after a delay.

**Steps:**
```csharp
using CodeMonkey.Toolkit.TFunctionTimer;

// Minimal overload
FunctionTimer.Create(() => DoSomething(), 2.5f);

// Named timer (can be cancelled by name)
FunctionTimer timer = FunctionTimer.Create(
    () => Debug.Log("Done"),
    5f,
    "myTimer",
    useUnscaledDeltaTime: false,
    stopAllWithSameName: true   // cancels any previous "myTimer"
);

// Cancel by name
FunctionTimer.StopAllTimersWithName("myTimer");
FunctionTimer.StopFirstTimerWithName("myTimer");

// Manual update variant (no hidden GameObject)
FunctionTimer.FunctionTimerObject obj = FunctionTimer.CreateObject(() => Done(), 2f);
// Call obj.Update() or obj.Update(deltaTime) in your own Update()
```

**Expected result:** The action fires once after the specified seconds; the hidden `FunctionTimer Object` GameObject is destroyed automatically.

---

### Workflow: Function Periodic (repeating)

**Goal:** Execute an `Action` repeatedly on a fixed interval.

**Steps:**
```csharp
using CodeMonkey.Toolkit.TFunctionPeriodic;

// Repeat every 1 second, indefinitely
FunctionPeriodic.Create(() => Debug.Log("tick"), 1f);

// Repeat until testDestroy returns true
FunctionPeriodic periodic = FunctionPeriodic.Create(
    () => DoWork(),
    () => isDone,   // stop when isDone == true
    1f
);

// Named, survives scene loads
FunctionPeriodic.Create_Global(() => Heartbeat(), null, 30f);

// Check / stop by name
bool active = FunctionPeriodic.IsFuncActive("heartbeat");
FunctionPeriodic.StopTimer("heartbeat");     // first match
FunctionPeriodic.StopAllFunc("heartbeat");   // all matches
```

**Expected result:** Action fires every `timer` seconds; stops when `testDestroy` returns `true` or when explicitly stopped.

---

### Workflow: Function Updater (per-frame callback)

**Goal:** Run code every frame without a MonoBehaviour.

**Steps:**
```csharp
using CodeMonkey.Toolkit.TFunctionUpdater;

// Run forever (Action overload — never auto-stops)
FunctionUpdater updater = FunctionUpdater.Create(() => DoEachFrame());

// Run until the Func returns true (auto-destroys)
FunctionUpdater.Create(() => {
    DoWork();
    return isComplete; // true = stop and destroy
}, "myUpdater");

// Pause / resume
updater.Pause();
updater.Resume();

// Destroy explicitly
FunctionUpdater.DestroyUpdater(updater);
FunctionUpdater.StopUpdaterWithName("myUpdater");
FunctionUpdater.StopAllUpdatersWithName("myUpdater");
```

**Expected result:** The callback runs each Unity Update tick until stopped.

---

### Workflow: Web Requests

**Goal:** Perform HTTP requests from a MonoBehaviour-free context.

**Steps:**
```csharp
using CodeMonkey.Toolkit.TWebRequests;

// GET text
WebRequests.Get("https://example.com/data.json",
    onError: err => Debug.LogError(err),
    onSuccess: text => Debug.Log(text));

// POST raw JSON
WebRequests.PostJson("https://api.example.com/endpoint",
    "{\"message\":\"hello\"}",
    err => Debug.LogError(err),
    text => Debug.Log(text));
```

For the remaining overloads — `Post` (form fields), `PostJson` with custom headers, `GetTexture`, and `GetSprite` — see `references/api-reference.md`.

**Expected result:** Callbacks fire on the main thread when the request completes; network errors route to `onError`.

---

### Workflow: Take Screenshot

**Goal:** Capture the screen as a `Texture2D` and/or save it to a PNG file.

**Steps:**
```csharp
using CodeMonkey.Toolkit.TTakeScreenshot;

// Capture full screen (with UI by default)
TakeScreenshot.TakeScreenshotTexture(
    onScreenshotTaken: tex => Debug.Log($"Size: {tex.width}x{tex.height}"),
    savePathPng: Application.persistentDataPath + "/screenshot.png",
    withUI: true);

// Capture without UI
TakeScreenshot.TakeScreenshotTexture(
    onScreenshotTaken: tex => SaveSomewhere(tex),
    savePathPng: null,
    withUI: false);

// Capture specific rect or camera
TakeScreenshot.TakeScreenshotTexture(
    takeScreenshotCamera: Camera.main,
    width: 1920, height: 1080,
    onScreenshotTaken: tex => Process(tex));
```

Capture happens at end-of-frame via `RenderPipelineManager.endCameraRendering` — the texture is ready in the callback, not synchronously.

**Expected result:** `onScreenshotTaken` fires with a valid `Texture2D`; PNG is written to `savePathPng` if provided.

---

### Workflow: Save File with Screenshot

**Goal:** Bundle game save data and a screenshot into a single binary file.

**Steps:**
```csharp
using CodeMonkey.Toolkit.TSaveFileScreenshot;

[Serializable]
public class SaveData { public int level; public float score; }

// Save: serialize data + screenshot together
SaveData data = new SaveData { level = 3, score = 999f };
Texture2D screenshot = /* from TakeScreenshot */;
SaveFileImage.Save(data, screenshot, Application.persistentDataPath + "/save.bytesave");

// Load: deserialize both
SaveFileImage.LoadJson<SaveData>(
    Application.persistentDataPath + "/save.bytesave",
    out SaveData loaded, out Texture2D thumb);
Debug.Log($"Level: {loaded.level}");
```

**Expected result:** A single `.bytesave` file contains both JSON game data and PNG image bytes; both are recovered on load.

---

### Workflow: Random Data

**Goal:** Generate random colours, directions, names, IDs, or pick from a collection.

**Steps:**
```csharp
using CodeMonkey.Toolkit.TRandomData;

Color c = RandomData.GetRandomColor();           // fully random RGBA (alpha=1)
Color seq = RandomData.GetSequencialColor();     // cycles through 9 preset colours
Color fromHex = RandomData.GetColorFromString("FF5733");

Vector3 dir = RandomData.GetRandomDir();         // XY plane random direction
Vector3 dirXZ = RandomData.GetRandomDirXZ();     // XZ plane random direction

string id = RandomData.GetIdString();            // 8-char alphanumeric
string longId = RandomData.GetIdStringLong(16);  // 16-char alphanumeric
string name = RandomData.GetRandomName();        // male or female first name
string city = RandomData.GetRandomCityName();

bool hit = RandomData.TestChance(30, 100);       // 30% chance → true

// Generic pick from array or list
string chosen = RandomData.GetRandom(new[] { "a", "b", "c" });

// Extension methods
List<int> nums = new List<int> { 1, 2, 3 };
int pick = nums.GetRandomElement();
```

**Expected result:** Calls return plausible random data without Unity Random state side effects beyond what `UnityEngine.Random.Range` normally produces.

---

### Workflow: FPS Counter

**Goal:** Display a live framerate counter in the game UI.

**Steps:**
1. Drag `Assets/CodeMonkey/Toolkit/Tools/FPSCounter/Prefabs/FPSCounter.prefab` into the Hierarchy (requires a Canvas in the scene).
2. The prefab is a `TextMeshProUGUI` with a `FPSCounter` component (namespace `CodeMonkey.Toolkit.TFPSCounter`).
3. No public API — it updates the text automatically every frame using a 50-frame rolling average.

**Expected result:** A text element in the corner displays the current framerate, updated every frame.

---

### Workflow: Mouse Position

**Goal:** Get the mouse cursor's world-space position.

**Steps:**
```csharp
using CodeMonkey.Toolkit.TMousePosition;

// 2D orthographic — returns world XY, z=0
Vector3 pos2D = MousePosition2D.GetPosition();

// 3D top-down — raycast against the y=0 XZ plane
Vector3 posPlane = MousePositionPlane.GetPosition();

// 3D arbitrary surface — MonoBehaviour, must be in scene
// Add MousePositionRaycast to a scene object, set mouseColliderLayerMask
// Optionally enable moveThisTransformOnUpdate to track the mouse visually
Vector3 posRaycast = MousePositionRaycast.GetPosition(); // uses scene instance
```

- `MousePosition2D` and `MousePositionPlane` are pure static classes — no scene object needed.
- `MousePositionRaycast` is a MonoBehaviour singleton; add it to a scene object and configure `mouseColliderLayerMask`.

**Expected result:** Returns the world position under the cursor for the chosen variant; returns `Vector3.zero` when the ray misses.

---

### Workflow: GameAssets

**Goal:** Reference prefabs and assets from code without `Resources.Load` string literals everywhere.

**Steps:**
1. Open `Assets/CodeMonkey/Toolkit/Tools/GameAssets/Scripts/GameAssets.cs` and **add public fields** for your assets.
2. The prefab at `Assets/CodeMonkey/Toolkit/Tools/GameAssets/Resources/GameAssets.prefab` must be named exactly `GameAssets` and reside in a folder named exactly `Resources`.
3. Drag asset references onto the prefab in the Inspector.
4. Access via the singleton from any script:
```csharp
using CodeMonkey.Toolkit.TGameAssets;

Transform obj = Instantiate(GameAssets.Instance.codeMonkeySpritePrefab);
Sprite spr = GameAssets.Instance.codeMonkeySprite;
```
The `Instance` getter auto-loads the prefab from Resources on first access if not already in the scene.

**Expected result:** Asset references are available from any script without scene dependencies or hardcoded paths.

---

### Workflow: MonoBehaviour Hooks & Pointer Hooks

**Goal:** React to lifecycle or UI pointer events on a GameObject without modifying its script.

**MonoBehaviourHooks** (namespace `CodeMonkey.Toolkit.TMonoBehaviourHooks`):
```csharp
using CodeMonkey.Toolkit.TMonoBehaviourHooks;

MonoBehaviourHooks hooks = go.AddComponent<MonoBehaviourHooks>();
hooks.onStartAction       = () => Debug.Log("Start fired");
hooks.onDestroyAction     = () => Cleanup();
hooks.onUpdateAction      = () => Tick();
hooks.onCollisionEnterAction = col => Debug.Log("Hit " + col.gameObject.name);
hooks.onTriggerEnter2DAction = col2D => Score();
```
Available hooks: `onStart`, `onDestroy`, `onEnable`, `onDisable`, `onUpdate`, `onCollisionEnter/Exit/Stay`, `onCollisionEnter2D/Stay2D/Exit2D`, `onTriggerEnter/Stay/Exit`, `onTriggerEnter2D/Stay2D/Exit2D`, `onBecameVisible`, `onBecameInvisible`, `onMouseDown/Up/UpAsButton/Enter/Exit/Drag/Over`.

**PointerHooks** (namespace `CodeMonkey.Toolkit.TPointerHooks`):
```csharp
using CodeMonkey.Toolkit.TPointerHooks;
using UnityEngine.EventSystems;

PointerHooks ph = uiElement.AddComponent<PointerHooks>();
ph.onPointerDownAction  = data => Debug.Log("Down");
ph.onPointerClickAction = data => ButtonClicked();
ph.onPointerEnterAction = data => ShowHighlight();
ph.onPointerExitAction  = data => HideHighlight();
ph.onPointerMoveAction  = data => TrackPosition(data.position);
ph.onPointerUpAction    = data => Released();
```

**Expected result:** Delegate callbacks fire on the listed Unity events without inheriting from the original component.

---

### Workflow: OpenAI API

**Goal:** Send a prompt to the OpenAI Responses API and receive a text or image response.

**Important:** Set `API_KEY` in `Assets/CodeMonkey/Toolkit/Tools/OpenAIAPI/Scripts/OpenAIAPI.cs` before use. The `private const string API_KEY` field ships with a placeholder value — replace it with your own OpenAI key, and never commit a real key to version control.

**Steps:**
```csharp
using CodeMonkey.Toolkit.TOpenAIAPI;

OpenAIAPI.AskChatGPT(
    new OpenAIAPI.AskChatGPTData {
        model = "gpt-4o-mini",
        instructions = "You are a game design assistant.", // optional system prompt
        prompt = "Describe the sky in one sentence."
    },
    onError: err => Debug.LogError("OpenAI error: " + err),
    onSuccess: (response, text) => Debug.Log("Reply: " + text)
);
```
`AskChatGPTData` also supports `texture2DArray` (image input) and `useWebSearchTool` — see `references/api-reference.md` for those examples and the full field list.

**Expected result:** `onSuccess` fires with the `Response` object and the extracted text string. Images from image-generation calls are returned via `response.output[n].result` (base64).

---

### Workflow: Save & load game data (SaveSystem)

**Goal:** Persist arbitrary game data to a JSON file and load it back.

**Steps:**
```csharp
using System;
using UnityEngine;
using CodeMonkey.Toolkit.TSaveSystem;

[Serializable] public class MySave { public int level; public float score; }

// Save (overwrite=true writes "save1.sav")
SaveSystem.Save("save1", new MySave { level = 3, score = 99.5f }, true);

// Load a named file, or the most recent save
MySave data = SaveSystem.Load<MySave>("save1");
MySave latest = SaveSystem.LoadMostRecent<MySave>();
```
- `SaveSystem` (namespace `CodeMonkey.Toolkit.TSaveSystem`) is a pure static `JsonUtility`-based class. Files use `.sav`; default folder is `Application.dataPath + "/Saves/"`. `Save(object, saveFolder=null)` writes `defaultSave.sav` (auto-increments); `GetSaveFileArray(saveFolder=null)` lists saves.
- Save classes must be `[Serializable]` with public fields only (no Dictionaries; no polymorphism).

**Expected result:** A `.sav` JSON file appears in the save folder; `Load`/`LoadMostRecent` return an object whose fields equal what was saved. Full method list in `references/api-reference.md`.

## Verification

- **FunctionTimer installed:** `System.IO.File.Exists("Assets/CodeMonkey/Toolkit/Tools/FunctionTimer/FunctionTimer.cs")` → `true`
- **Menu present:** **Tools → Code Monkey Toolkit** appears in the Unity menu bar
- **Namespace resolves:** `typeof(CodeMonkey.Toolkit.TFunctionTimer.FunctionTimer)` compiles without error
- **Play test — FunctionTimer fires:** Create timer with 0.1 s delay, assert callback flag is set within 0.5 s
- **WebRequests reachable:** `WebRequests.Get("https://httpbin.org/get", err => fail, text => pass)` fires `onSuccess` in play mode with network access
- **SaveSystem round-trip:** `typeof(CodeMonkey.Toolkit.TSaveSystem.SaveSystem)` resolves; after `Save`, a `.sav` file exists in the save folder and `Load<T>` returns matching field values

## API quick reference

See `references/api-reference.md` for the full table. Key entry points:

| Entry point | Type | What it does |
|---|---|---|
| `TFunctionTimer.FunctionTimer.Create(Action, float)` | static | One-shot delayed callback |
| `TFunctionPeriodic.FunctionPeriodic.Create(Action, float)` | static | Repeating callback |
| `TFunctionUpdater.FunctionUpdater.Create(Func<bool>, string)` | static | Per-frame callback, stop when true |
| `TWebRequests.WebRequests.Get(url, onError, onSuccess)` | static | HTTP GET → string |
| `TWebRequests.WebRequests.PostJson(url, json, onError, onSuccess)` | static | HTTP POST JSON → string |
| `TWebRequests.WebRequests.GetTexture(url, onError, onSuccess)` | static | HTTP GET → Texture2D |
| `TTakeScreenshot.TakeScreenshot.TakeScreenshotTexture(...)` | static | Capture screen → Texture2D + optional PNG |
| `TSaveFileScreenshot.SaveFileImage.Save<T>(T, Texture2D, path)` | static | Bundle data + screenshot → file |
| `TSaveFileScreenshot.SaveFileImage.LoadJson<T>(path, out T, out Texture2D)` | static | Load data + thumbnail |
| `TRandomData.RandomData.GetRandomColor()` | static | Random Color (alpha=1) |
| `TRandomData.RandomData.GetRandom<T>(T[])` | static | Pick random array element |
| `TFPSCounter.FPSCounter` | MonoBehaviour | 50-frame rolling-average FPS display |
| `TMousePosition.MousePosition2D.GetPosition()` | static | Mouse XY world pos (2D) |
| `TMousePosition.MousePositionPlane.GetPosition()` | static | Mouse XZ world pos (top-down plane) |
| `TMousePosition.MousePositionRaycast.GetPosition()` | static | Mouse world pos via 3D raycast |
| `TGameAssets.GameAssets.Instance` | singleton | Centralized asset reference bag |
| `TMonoBehaviourHooks.MonoBehaviourHooks` | MonoBehaviour | Action hooks for all lifecycle events |
| `TPointerHooks.PointerHooks` | MonoBehaviour | Action hooks for all IPointer events |
| `TOpenAIAPI.OpenAIAPI.AskChatGPT(AskChatGPTData, onError, onSuccess)` | static | Call OpenAI Responses API |
| `TSaveSystem.SaveSystem.Save(fileName, object, overwrite, saveFolder=null)` | static | Write data to a `.sav` JSON file |
| `TSaveSystem.SaveSystem.Load<T>(fileName, saveFolder=null)` | static | Load data from a named save file |
| `TSaveSystem.SaveSystem.LoadMostRecent<T>(saveFolder=null)` | static | Load the newest save file |

## Common issues

| Symptom | Cause | Fix |
|---|---|---|
| FunctionTimer never fires | Timer created in editor (not Play Mode) | `FunctionTimer.Create` uses `Update` — must be in Play Mode |
| `OpenAIAPI` returns 401 Unauthorized | `API_KEY` not set | Open `OpenAIAPI.cs` and replace the placeholder key |
| `TakeScreenshot` callback never fires | SRP pipeline not firing `endCameraRendering` | Ensure a Camera renders each frame; built-in pipeline is not supported — use URP or HDRP |
| `MousePositionRaycast.GetPosition()` returns zero | No `MousePositionRaycast` component in scene | Add `MousePositionRaycast` MonoBehaviour to a scene object and set `mouseColliderLayerMask` |
| FPSCounter shows no text | Missing TextMeshProUGUI on the prefab object | Confirm TMP package is installed; the prefab requires TMP |
| `GameAssets.Instance` returns null | Prefab not named `GameAssets` or not in `Resources/` | Prefab must be named exactly `GameAssets` and placed inside a folder named exactly `Resources` |
| SaveSystem loads empty/default data | Save class not `[Serializable]`, or uses Dictionaries/properties | Use a `[Serializable]` class with public fields only (JsonUtility rules) |
| `PATH DOES NOT EXIST` / `DirectoryNotFoundException` on Save | A custom `saveFolder` must already exist — Save does not create it | `System.IO.Directory.CreateDirectory(saveFolder)` before the first Save |
| Saves lost or `IOException` in a build | Default folder uses read-only `Application.dataPath` | Pass a `saveFolder` such as `Application.persistentDataPath + "/Saves/"` (and create it) |

## Boundaries

- **FunctionTimer / FunctionPeriodic / FunctionUpdater are Play Mode only.** They rely on Unity's `Update` loop.
- **TakeScreenshot requires URP or HDRP.** It hooks `RenderPipelineManager.endCameraRendering`; the Built-in pipeline does not raise this event.
- **OpenAIAPI key is in source.** Replace the hardcoded constant before shipping; never commit a real key to version control.
- **WebRequests uses `UnityWebRequest` (coroutine-based).** Not suitable for blocking synchronous calls; all results arrive asynchronously on the main thread.
- **GameAssets is a single global singleton.** Projects with multiple subsystems may prefer ScriptableObject-based registries.
- **SaveSystem uses `JsonUtility`** (no Dictionaries/polymorphism, public fields only; no encryption/cloud sync). To bundle a screenshot with data, use `SaveFileScreenshot` instead.
- Fall back to Unity coroutines, `async`/`await`, or a dedicated HTTP library when you need cancellation tokens, streaming responses, or fine-grained error handling.
