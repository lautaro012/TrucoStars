# Code Monkey Toolkit — Utilities: Full API Reference

All signatures verified against source in `Assets/CodeMonkey/Toolkit/Tools/`.

## FunctionTimer — `CodeMonkey.Toolkit.TFunctionTimer`

| Signature | Returns | Notes |
|---|---|---|
| `FunctionTimer.Create(Action action, float timer)` | `FunctionTimer` | Minimal one-shot |
| `FunctionTimer.Create(Action, float, string functionName)` | `FunctionTimer` | Named timer |
| `FunctionTimer.Create(Action, float, string, bool useUnscaledDeltaTime)` | `FunctionTimer` | Unscaled time support |
| `FunctionTimer.Create(Action, float, string, bool, bool stopAllWithSameName)` | `FunctionTimer` | Full overload |
| `FunctionTimer.StopAllTimersWithName(string)` | void | Cancel all matching |
| `FunctionTimer.StopFirstTimerWithName(string)` | void | Cancel first match |
| `FunctionTimer.CreateObject(Action, float)` | `FunctionTimerObject` | Manual-update variant |
| `FunctionTimerObject.Update()` | bool | Call in your own Update; true = fired |
| `FunctionTimerObject.Update(float deltaTime)` | bool | Custom delta override |

## FunctionPeriodic — `CodeMonkey.Toolkit.TFunctionPeriodic`

| Signature | Returns | Notes |
|---|---|---|
| `FunctionPeriodic.Create(Action action, float timer)` | `FunctionPeriodic` | Repeat indefinitely |
| `FunctionPeriodic.Create(Action, float, string functionName)` | `FunctionPeriodic` | Named repeater |
| `FunctionPeriodic.Create(Action, Func<bool> testDestroy, float timer)` | `FunctionPeriodic` | Auto-stop when true |
| `FunctionPeriodic.Create(Action, Func<bool>, float, string, bool stopAllWithSameName)` | `FunctionPeriodic` | Named + stop-same |
| `FunctionPeriodic.Create(Action, Func<bool>, float, string, bool useUnscaledDeltaTime, bool triggerImmediately, bool stopAllWithSameName)` | `FunctionPeriodic` | Full overload |
| `FunctionPeriodic.Create_Global(Action, Func<bool>, float)` | `FunctionPeriodic` | Survives scene loads |
| `FunctionPeriodic.IsFuncActive(string)` | bool | Check if running |
| `FunctionPeriodic.StopTimer(string)` | void | Stop first match |
| `FunctionPeriodic.StopAllFunc(string)` | void | Stop all matches |
| `instance.SkipTimerTo(float timer)` | void | Fast-forward interval |
| `instance.SetBaseTimer(float)` | void | Change repeat interval |
| `instance.GetBaseTimer()` | float | Current interval |
| `instance.DestroySelf()` | void | Explicit teardown |

## FunctionUpdater — `CodeMonkey.Toolkit.TFunctionUpdater`

| Signature | Returns | Notes |
|---|---|---|
| `FunctionUpdater.Create(Action updateFunc)` | `FunctionUpdater` | Forever, Action |
| `FunctionUpdater.Create(Action, string functionName)` | `FunctionUpdater` | Named, forever |
| `FunctionUpdater.Create(Func<bool> updateFunc)` | `FunctionUpdater` | Stops when true |
| `FunctionUpdater.Create(Func<bool>, string functionName)` | `FunctionUpdater` | Named, stops when true |
| `FunctionUpdater.Create(Func<bool>, string, bool active)` | `FunctionUpdater` | Start paused |
| `FunctionUpdater.Create(Func<bool>, string, bool active, bool stopAllWithSameName)` | `FunctionUpdater` | Full overload |
| `FunctionUpdater.DestroyUpdater(FunctionUpdater)` | void | Explicit teardown |
| `FunctionUpdater.StopUpdaterWithName(string)` | void | Stop first match |
| `FunctionUpdater.StopAllUpdatersWithName(string)` | void | Stop all matches |
| `instance.Pause()` | void | Suspend callbacks |
| `instance.Resume()` | void | Resume callbacks |
| `instance.DestroySelf()` | void | Explicit teardown |

## WebRequests — `CodeMonkey.Toolkit.TWebRequests`

| Signature | Returns | Notes |
|---|---|---|
| `WebRequests.Get(string url, Action<string> onError, Action<string> onSuccess)` | void | HTTP GET → text |
| `WebRequests.Post(string url, Dictionary<string,string> formFields, onError, onSuccess)` | void | HTML form POST |
| `WebRequests.Post(string url, string postData, string contentType, onError, onSuccess)` | void | Raw POST |
| `WebRequests.PostJson(string url, string jsonData, onError, onSuccess)` | void | JSON POST |
| `WebRequests.PostJson(string url, string jsonData, Dictionary<string,string> headers, onError, onSuccess)` | void | JSON POST + headers |
| `WebRequests.Put(string url, string bodyData, onError, onSuccess)` | void | HTTP PUT |
| `WebRequests.GetTexture(string url, onError, Action<Texture2D> onSuccess)` | void | HTTP GET → Texture2D |
| `WebRequests.GetSprite(string url, onError, Action<Sprite> onSuccess)` | void | HTTP GET → Sprite |

## TakeScreenshot — `CodeMonkey.Toolkit.TTakeScreenshot`

| Signature | Returns | Notes |
|---|---|---|
| `TakeScreenshot.TakeScreenshotTexture(Action<Texture2D> onScreenshotTaken, string savePathPng = null, bool withUI = true)` | void | Full screen, Camera.main |
| `TakeScreenshot.TakeScreenshotTexture(Rect rect, Action<Texture2D>, string savePathPng = null, bool withUI = true)` | void | Rect crop, Camera.main |
| `TakeScreenshot.TakeScreenshotTexture(Camera cam, int width, int height, Action<Texture2D>, string savePathPng = null, bool withUI = true)` | void | Custom camera + size |
| `TakeScreenshot.TakeScreenshotTexture(Camera cam, Rect rect, Action<Texture2D>, string savePathPng = null, bool withUI = true)` | void | Custom camera + rect |

Fires via `RenderPipelineManager.endCameraRendering` — URP/HDRP only.

## SaveFileImage — `CodeMonkey.Toolkit.TSaveFileScreenshot`

| Signature | Returns | Notes |
|---|---|---|
| `SaveFileImage.Save<T>(T saveGameData, Texture2D screenshot, string savePath = null)` | `List<byte>` | Serialize `[Serializable]` type + PNG |
| `SaveFileImage.Save(string json, Texture2D screenshot, string savePath = null)` | `List<byte>` | Raw JSON + PNG |
| `SaveFileImage.Load(string path, out string gameDataJson, out Texture2D screenshot)` | void | Read from file |
| `SaveFileImage.Load(byte[], out string gameDataJson, out Texture2D screenshot)` | void | Read from bytes |
| `SaveFileImage.LoadJson<T>(string path, out T saveData, out Texture2D screenshot)` | void | Typed load from file |
| `SaveFileImage.LoadJson<T>(byte[], out T saveData, out Texture2D screenshot)` | void | Typed load from bytes |

## RandomData — `CodeMonkey.Toolkit.TRandomData`

| Signature | Returns | Notes |
|---|---|---|
| `RandomData.GetRandomColor()` | Color | Alpha = 1 |
| `RandomData.GetSequencialColor()` | Color | Cycles 9 preset colours |
| `RandomData.ResetSequencialColors()` | void | Reset cycle index |
| `RandomData.GetColor255(float r, float g, float b, float a = 255)` | Color | 0–255 range inputs |
| `RandomData.GetColorFromString(string hex)` | Color | 6 or 8 char hex (RRGGBB or RRGGBBAA) |
| `RandomData.GetStringFromColor(Color)` | string | → 6 char hex |
| `RandomData.GetStringFromColorWithAlpha(Color)` | string | → 8 char hex |
| `RandomData.GetRandomDir()` | Vector3 | XY normalized |
| `RandomData.GetRandomDirXZ()` | Vector3 | XZ normalized |
| `RandomData.GetAngleFromVectorFloat(Vector3)` | float | 0–360° angle |
| `RandomData.GetAngleFromVectorFloatXZ(Vector3)` | float | XZ plane angle |
| `RandomData.GetRandom<T>(T[])` | T | Random array element |
| `RandomData.GetRandomElement<T>(List<T>)` | T | Random list element |
| `RandomData.GetRandomElement<T>(T[])` | T | Random array element |
| `RandomData.GetIdString()` | string | 8-char lowercase alphanum |
| `RandomData.GetIdStringLong(int chars = 16)` | string | N-char mixed-case alphanum |
| `RandomData.GetRandomName(bool withSurname = false)` | string | Male or female first name |
| `RandomData.GetRandomMaleName(bool withSurname = false)` | string | Male first name |
| `RandomData.GetRandomFemaleName(bool withSurname = false)` | string | Female first name |
| `RandomData.GetRandomCityName()` | string | Random city name from list |
| `RandomData.GetRandomPositionWithinRectangle(float xMin, float xMax, float yMin, float yMax)` | Vector3 | Random XY position |
| `RandomData.GetRandomPositionWithinRectangleXZ(...)` | Vector3 | Random XZ position |
| `RandomData.TestChance(int chance, int chanceMax = 100)` | bool | `chance / chanceMax` probability |

Extension methods on `List<T>` and `T[]`: `.GetRandomElement()`.

## FPSCounter — `CodeMonkey.Toolkit.TFPSCounter`

`FPSCounter` (MonoBehaviour on a `TextMeshProUGUI`). No public API. Updates text every frame using a 50-frame rolling average of `Time.unscaledDeltaTime`. Prefab: `Assets/CodeMonkey/Toolkit/Tools/FPSCounter/Prefabs/FPSCounter.prefab`.

## MousePosition — `CodeMonkey.Toolkit.TMousePosition`

| Signature | Returns | Notes |
|---|---|---|
| `MousePosition2D.GetPosition()` | Vector3 | `Camera.main.ScreenToWorldPoint(Mouse.current.position.value)`, z=0 |
| `MousePositionPlane.GetPosition()` | Vector3 | Raycast against `new Plane(Vector3.up, Vector3.zero)`; returns `Vector3.zero` on miss |
| `MousePositionRaycast.GetPosition()` | Vector3 | 3D Physics.Raycast using `Instance.mouseColliderLayerMask`; returns `Vector3.zero` on miss |

`MousePositionRaycast` fields: `mouseColliderLayerMask` (LayerMask), `moveThisTransformOnUpdate` (bool — moves the component's transform to the hit point each frame).

## GameAssets — `CodeMonkey.Toolkit.TGameAssets`

`GameAssets` (MonoBehaviour singleton). `GameAssets.Instance` → loads from `Resources/GameAssets` prefab on first access. Add public fields to the class for project-specific asset references.

## MonoBehaviourHooks — `CodeMonkey.Toolkit.TMonoBehaviourHooks`

All fields are `public Action` or `public Action<T>` delegates assigned at runtime. See SKILL.md Workflow section for the complete field list.

## PointerHooks — `CodeMonkey.Toolkit.TPointerHooks`

Implements `IPointerDownHandler`, `IPointerClickHandler`, `IPointerEnterHandler`, `IPointerExitHandler`, `IPointerMoveHandler`, `IPointerUpHandler`. All fields are `public Action<PointerEventData>`.

## OpenAIAPI — `CodeMonkey.Toolkit.TOpenAIAPI`

| Signature | Returns | Notes |
|---|---|---|
| `OpenAIAPI.AskChatGPT(AskChatGPTData, Action<string> onError, Action<Response, string> onSuccess)` | void | Convenience wrapper |
| `OpenAIAPI.AskChatGPT(ResponsesSend, onError, onSuccess)` | void | Full control over request |

**`AskChatGPTData` fields:**

| Field | Type | Notes |
|---|---|---|
| `model` | string | e.g. `"gpt-4o"`, `"gpt-4o-mini"` |
| `instructions` | string | System instructions (optional) |
| `chatPromptId` | string | Reuse a stored prompt template (optional) |
| `prompt` | string | User message |
| `texture2DArray` | `Texture2D[]` | Images to include (base64 encoded) |
| `fileUrlArray` | `string[]` | File URLs to include |
| `useWebSearchTool` | bool | Enable web search |
| `imageGeneration` | `ImageGeneration` | Enable image generation; set `size` and `quality` |

Endpoint: `https://api.openai.com/v1/responses`. Uses `WebRequests.PostJson` internally.

### OpenAIAPI — additional examples

```csharp
using CodeMonkey.Toolkit.TOpenAIAPI;

// With image input
OpenAIAPI.AskChatGPT(
    new OpenAIAPI.AskChatGPTData {
        model = "gpt-4o",
        prompt = "What is in this image?",
        texture2DArray = new Texture2D[] { myTexture }
    },
    err => Debug.LogError(err),
    (response, text) => Debug.Log(text)
);

// Enable web search tool
OpenAIAPI.AskChatGPT(
    new OpenAIAPI.AskChatGPTData {
        model = "gpt-4o",
        prompt = "What is the latest Unity version?",
        useWebSearchTool = true
    },
    err => Debug.LogError(err),
    (response, text) => Debug.Log(text)
);
```

## SaveSystem — `CodeMonkey.Toolkit.TSaveSystem`

Pure static class. Serializes with `UnityEngine.JsonUtility`. Files use the `.sav` extension; default directory is `Application.dataPath + "/Saves/"`. Save objects must be `[System.Serializable]` and follow JsonUtility rules (public fields only; no Dictionaries; no polymorphism).

| Signature | Returns | Notes |
|---|---|---|
| `SaveSystem.Save(object saveObject, string saveFolder = null)` | void | Writes `defaultSave.sav` with overwrite=false (auto-increments `defaultSave_2.sav`, …) |
| `SaveSystem.Save(string fileName, object saveObject, bool overwrite, string saveFolder = null)` | void | Named save; `overwrite=true` replaces the file |
| `SaveSystem.Load<TSaveObject>(string fileName, string saveFolder = null)` | `TSaveObject` | Load a named save |
| `SaveSystem.LoadMostRecent<TSaveObject>(string saveFolder = null)` | `TSaveObject` | Load the newest save in the folder |
| `SaveSystem.GetSaveFileArray(string saveFolder = null)` | `FileInfo[]` | List existing save files |

In builds, `Application.dataPath` is read-only — pass a `saveFolder` such as `Application.persistentDataPath + "/Saves/"`.
