# Project Overview

- **Game Title:** First-Person Popup Cubes (demo scene)
- **High-Level Concept:** A small first-person sandbox where the player walks up to one cube and presses **E** to spawn a floating number popup (an incrementing counter), while a second cube automatically spawns an increasing number popup every second on its own.
- **Players:** Single player
- **Inspiration / Reference Games:** Code Monkey Toolkit demo scenes (Interaction System, Text Popup)
- **Tone / Art Direction:** Minimal grey-box / prototype look, reusing the toolkit demo materials.
- **Target Platform:** PC (StandaloneWindows64)
- **Screen Orientation / Resolution:** Landscape (default game view)
- **Render Pipeline:** URP (project active pipeline: `PC_RPAsset`)

# Game Mechanics

## Core Gameplay Loop
1. Player spawns in first person and moves with **WASD**, looks with the **mouse**, jumps with **Space** (via `FirstPersonController`).
2. Player approaches the **Interactable Cube** (center of the scene) and presses **E** while within 3m. Each press spawns a world-space text popup above the cube showing an incrementing counter: `1`, `2`, `3`, ...
3. Beside it, the **Auto Cube** independently spawns a world-space text popup above itself every 1 second, showing a number that increases each time: `1`, `2`, `3`, ...

This demonstrates two feedback patterns from the Code Monkey Toolkit: player-driven interaction (`IInteractable` + `PlayerInteractProximity`) and time-driven events (`FunctionPeriodic`), both surfaced through `TextPopupWorld`.

## Controls and Input Methods
Uses the **New Input System** (project default), read directly by the toolkit scripts:
- **WASD** — move
- **Mouse** — look around
- **Space** — jump
- **E** — interact (handled by `PlayerInteractProximity`, `Keyboard.current.eKey`)

# UI
No screen-space UI is required. All feedback is **world-space** via `TextPopupWorld`, which auto-billboards toward the camera (`AddLookAtCamera(LookAtInverted)`) and auto-destroys after a timer (default 3s). Popups appear floating just above each cube.

```
        [ 3 ]              [ 7 ]        <- world-space TextPopupWorld popups
         ▓▓                 ▓▓
      Interactable        Auto Cube
         Cube            (every 1s)
   (press E to spawn)
   ------------------------------------  <- Ground plane
                 (o)  <- First Person Player
```

# Key Asset & Context

## Existing toolkit assets to reuse (no changes to package)
- **Player prefab:** `Assets/CodeMonkey/Toolkit/Tools/FirstPersonController/Prefabs/Player.prefab`
  - Contains: `CharacterController`, `FirstPersonController` (camera assigned to child `Main Camera`).
  - **Does NOT** contain `PlayerInteractProximity` — must be added to the scene instance.
- **Interaction system:** `CodeMonkey.Toolkit.TInteractionSystemProximity`
  - `PlayerInteractProximity` (MonoBehaviour): on **E** press, finds nearest `IInteractable` within `INTERACT_RANGE = 3f` (via `Physics.OverlapSphere`) and calls `interactable.Interact(InteractAction.Primary, transform)`. Requires interactables to have a `Collider`.
  - `IInteractable` interface members to implement:
    - `bool CanDoInteractAction(InteractAction interactAction)`
    - `void Interact(InteractAction interactAction, Transform interactorTransform)`
    - `Dictionary<InteractAction, string> GetInteractTextDictionary()`
    - `Transform GetTransform()`
- **Text popup:** `CodeMonkey.Toolkit.TTextPopup.TextPopupWorld`
  - `TextPopupWorld.Create(Vector3 worldPosition, string text, float scale = .1f, float destroyTimer = 3f)`
  - Prefab lives at `Assets/CodeMonkey/Toolkit/Tools/TextPopup/Resources/TextPopupWorld.prefab` (loaded via `Resources.Load`), so runtime creation works with no scene wiring.
- **Periodic timer:** `CodeMonkey.Toolkit.TFunctionPeriodic.FunctionPeriodic`
  - `FunctionPeriodic.Create(Action action, float timer)` — runs `action` every `timer` seconds.
- **Demo materials (optional):** `Assets/CodeMonkey/Toolkit/Tools/FirstPersonController/Demo/Material/*.mat` (Blue / Yellow / GreyDark) for coloring the cubes and ground.

## New scripts to create
Place under a new folder `Assets/Scripts/PopupDemo/`.

1. **`CounterCubeInteractable.cs`** — implements `IInteractable`.
   - Namespace/using: `CodeMonkey.Toolkit.TInteractionSystemProximity`, `CodeMonkey.Toolkit.TTextPopup`, `System.Collections.Generic`, `UnityEngine`.
   - Private `int counter = 0;`
   - `Interact(...)`: `counter++;` then `TextPopupWorld.Create(transform.position + Vector3.up * 1.5f, counter.ToString());`
   - `GetInteractTextDictionary()`: returns `{ Primary, "Spawn number" }`.
   - `GetTransform()`: returns `transform`.
   - `CanDoInteractAction(...)`: returns `interactAction == InteractAction.Primary`.

2. **`AutoPopupCube.cs`** — MonoBehaviour.
   - Namespace/using: `CodeMonkey.Toolkit.TFunctionPeriodic`, `CodeMonkey.Toolkit.TTextPopup`, `UnityEngine`.
   - `[SerializeField] private float interval = 1f;`
   - Private `int counter = 0;`
   - `Start()`: `FunctionPeriodic.Create(SpawnPopup, interval);`
   - `SpawnPopup()`: `counter++;` then `TextPopupWorld.Create(transform.position + Vector3.up * 1.5f, counter.ToString());`
   - (Note: `FunctionPeriodic` spawns a global helper GameObject; it keeps running for the scene lifetime, which is fine for this demo.)

## New scene
- `Assets/Scenes/PopupDemo.unity` containing: Ground plane, Directional Light, Player instance (+ `PlayerInteractProximity`), Interactable Cube, Auto Cube.

# Implementation Steps

### Step 1 — Create the two custom scripts
- **Description:** Create `Assets/Scripts/PopupDemo/CounterCubeInteractable.cs` and `Assets/Scripts/PopupDemo/AutoPopupCube.cs` exactly as specified in *Key Asset & Context*. Let Unity compile and confirm no console errors.
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** Yes (both files independent)

### Step 2 — Create the new scene with environment
- **Description:** Create `Assets/Scenes/PopupDemo.unity`. Add a **Directional Light** and a **Ground** plane (e.g. Plane scaled to ~2, positioned at y=0) so the `CharacterController` has a floor. Optionally assign `GreyDark.mat` to the ground.
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** Yes (independent of Step 1)

### Step 3 — Add the player
- **Description:** Instantiate `Player.prefab` into the scene at ~`(0, 1, -4)` looking toward the cubes. Add the `PlayerInteractProximity` component to the player root object (same object that has `FirstPersonController` / `CharacterController`, since it uses `transform.position` for the overlap sphere). Verify the prefab's `playerCamera` reference is intact.
- **Assigned role:** developer
- **Dependencies:** Step 2
- **Parallelizable:** No

### Step 4 — Add the Interactable Cube
- **Description:** Create a Cube (has `BoxCollider` by default) at ~`(-1.5, 0.5, 0)`. Attach `CounterCubeInteractable`. Optionally assign `Blue.mat`. Ensure it is within reach of the player path and its collider is enabled (required by `PlayerInteractProximity` overlap check).
- **Assigned role:** developer
- **Dependencies:** Step 1
- **Parallelizable:** No (needs Step 1 script)

### Step 5 — Add the Auto Cube
- **Description:** Create a Cube at ~`(1.5, 0.5, 0)`. Attach `AutoPopupCube` (leave `interval = 1`). Optionally assign `Yellow.mat`.
- **Assigned role:** developer
- **Dependencies:** Step 1
- **Parallelizable:** Yes (independent of Step 4)

### Step 6 — Save scene & set as active
- **Description:** Save `PopupDemo.unity`. Optionally add it to Build Settings as the startup scene.
- **Assigned role:** developer
- **Dependencies:** Steps 2–5
- **Parallelizable:** No

# Verification & Testing

- **Compile check:** After Step 1, Console shows no compile errors; `CounterCubeInteractable` and `AutoPopupCube` resolve the toolkit namespaces.
- **Enter Play Mode** in `PopupDemo.unity`:
  1. **Movement:** WASD moves, mouse looks, Space jumps, player stays on the ground (does not fall through).
  2. **Interactable cube:** Walk within ~3m of the Interactable Cube, press **E** repeatedly — a popup appears above the cube each press showing `1`, then `2`, `3`, ... (counter increments per press, not per frame). Pressing **E** far away (>3m) does nothing.
  3. **Auto cube:** Without any input, the Auto Cube spawns a popup above itself every ~1 second showing `1`, `2`, `3`, ... increasing each time.
  4. **Popups** float above their cube, face the camera as you move around, and disappear after ~3 seconds.
- **Edge cases:**
  - Standing exactly between both cubes and pressing E interacts with the **nearest** interactable only (Auto Cube has no `IInteractable`, so only the Interactable Cube responds).
  - Rapid E presses each produce one increment (uses `wasPressedThisFrame`).
