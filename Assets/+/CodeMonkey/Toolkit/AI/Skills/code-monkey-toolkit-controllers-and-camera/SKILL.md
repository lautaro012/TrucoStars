---
name: code-monkey-toolkit-controllers-and-camera
description: "Use this skill whenever a user asks to 'add a first person controller', 'give me a quick top down player', 'set up a simple camera rig', 'add a look-at-camera to my health bar', 'I need a 2D top down character controller', 'make the camera move with WASD', or anything about player movement or camera rigs via Code Monkey Toolkit. Covers FirstPersonController (3D FPS — mouse look, WASD, jump), TopDownCharacterController (2D physics, optional roll/teleport), TopDownCharacterController3D (3D top-down WASD), CameraControllerBasic (3D free camera, WASD+QE), CameraControllerBasic2D (2D free camera), and LookAtCamera (billboard modes). All use the Unity New Input System. Does NOT cover UI, shaders, grids, or gameplay — see the ui-and-feedback, shaders-and-effects, grid-systems, and gameplay-systems sibling skills. When in doubt whether a controller or camera request could involve Code Monkey Toolkit, use this skill — Prerequisites shows how to confirm the asset is installed."
metadata:
  asset: "Code Monkey Toolkit"
  publisher: "Code Monkey"
  asset-version: "1.0.5"
  skill-version: "1.0.0"
  unity: "6000.0.60f1+"
  render-pipelines: "Built-in, URP, HDRP"
  category: "tools/utilities"
  asset-store-url: "https://assetstore.unity.com/packages/tools/utilities/code-monkey-toolkit-309282"
  documentation-url: "https://unitycodemonkey.com/"
  support-url: "https://assetstore.unity.com/packages/tools/utilities/code-monkey-toolkit-309282"
  last-verified: "2026-09-07"
---

# Code Monkey Toolkit — Controllers & Camera

Six ready-to-use prefabs and scripts for rapid prototyping of player movement and camera behaviour. Drop in a First Person Controller, a 2D or 3D Top Down Controller, a free-roam camera (3D or 2D), or a LookAtCamera billboard — all driven by the Unity New Input System with no additional setup beyond dragging the prefab into the scene.

## When to use this skill

**Use when the user says things like:**
- "add a first person controller", "FPS player", "first person movement"
- "give me a top down player", "top down character 2D", "2D WASD movement with roll"
- "3D top down controller", "top-down camera-relative movement"
- "make my camera move with WASD", "simple camera rig", "free camera prototype"
- "make my health bar face the camera", "billboard sprite", "LookAtCamera"
- Any use of Code Monkey Toolkit for player controller or camera rig

**Not for:**
- UI panels, tooltips, chat bubbles → `code-monkey-toolkit-ui-and-feedback`
- Grid systems → `code-monkey-toolkit-grid-systems`
- Shaders, visual effects → `code-monkey-toolkit-shaders-and-effects`
- Gameplay systems (health, interaction, keys) → `code-monkey-toolkit-gameplay-systems`
- Package installation → `code-monkey-toolkit-setup-and-overview`
- Timers, web requests, screenshots → `code-monkey-toolkit-utilities`

## Prerequisites

- **Asset:** Code Monkey Toolkit 1.0.5, imported from the Unity Asset Store
- **Unity:** 6000.0.60f1 or later
- **Input System:** `com.unity.inputsystem` package installed; **Active Input Handling** set to *Input System Package (New)* or *Both* in **Project Settings → Player**. All controllers read from `UnityEngine.InputSystem.Keyboard.current` and `Mouse.current`.

**Programmatic install check (run in editor or Play Mode):**
```csharp
bool installed = System.IO.File.Exists(
    "Assets/CodeMonkey/Toolkit/Tools/FirstPersonController/Scripts/FirstPersonController.cs");
// Or confirm the type resolves at compile time:
// var t = typeof(CodeMonkey.Toolkit.TFirstPersonController.FirstPersonController);
```
Open the toolkit window via **Tools → Code Monkey Toolkit** and confirm controller tools appear in the list. If missing, direct the user to the Asset Store URL above.

## Quick start

Add a First Person Controller to a 3D scene in five steps:

1. In the **Project** window, navigate to `Assets/CodeMonkey/Toolkit/Tools/FirstPersonController/Prefabs/`.
2. Drag **Player.prefab** into the Hierarchy.
3. Disable or delete the default Main Camera — the prefab includes its own child camera.
4. Confirm the `Player Camera` field in the Inspector references the child Camera (pre-wired in the prefab).
5. Enter Play Mode — **WASD** moves, **Mouse** looks, **Space** jumps, cursor is locked automatically.

Observable result: the player capsule moves and the camera follows mouse input; the cursor disappears on play start.

## Workflows

### Workflow: First Person Controller

**Goal:** Add a 3D first-person player to a scene.

**Steps:**
1. Drag `Assets/CodeMonkey/Toolkit/Tools/FirstPersonController/Prefabs/Player.prefab` into the Hierarchy.
2. The prefab root has a `CharacterController` component and a `FirstPersonController` MonoBehaviour (namespace `CodeMonkey.Toolkit.TFirstPersonController`).
3. Serialized fields (Inspector):
   - `Player Camera` (Camera) — must point to the child Camera; **pre-wired in the prefab**
   - `Sensitivity` (float, default **1**) — mouse look speed multiplier
   - `Move Speed` (float, default **10**) — movement speed in metres per second
4. Runtime control from code:
```csharp
using CodeMonkey.Toolkit.TFirstPersonController;

FirstPersonController fpc = playerGO.GetComponent<FirstPersonController>();
fpc.Disable();      // stop processing input (sets enabled = false)
fpc.Enable();       // resume input
fpc.UnlockMouse();  // Cursor.lockState = None, Cursor.visible = true
fpc.LockMouse();    // Cursor.lockState = Locked, Cursor.visible = false
```
5. Gravity: **7.8 m/s²** (hardcoded). Jump impulse: **1.8** (hardcoded).

**Expected result:** Mouse look and WASD movement work in Play Mode; Space triggers a jump; cursor is locked at start.

---

### Workflow: Top Down Character Controller 2D

**Goal:** Add a physics-based 2D top-down player.

**Steps:**
1. Choose a prefab:
   - `Assets/CodeMonkey/Toolkit/Tools/TopDownCharacterController/Prefabs/TopDownCharacterController2D.prefab` — full version (roll + teleport, class `CharacterController2D`)
   - `TopDownCharacterController2DSimple.prefab` — simplified version (teleport only, class `CharacterController2D_Simple`)
2. Both prefabs include a `Rigidbody2D` and a `Collider2D`.
3. Serialized fields — **CharacterController2D** (full, namespace `CodeMonkey.Toolkit.TTopDownCharacterController`):
   - `Teleport Layer Mask` — layers used for wall-stopping teleport raycast
   - `Can Teleport` (bool, default **true**) — **T** teleports 5 units in last move direction
   - `Can Roll` (bool, default **true**) — **Space** initiates a roll
4. Serialized fields — **CharacterController2D_Simple**:
   - `Teleport Layer Mask`
   - `Can Teleport` (bool, default **true**) — **Space** teleports
5. **Move speed is a constant** (`MOVE_SPEED = 5f`) and is not serialized. To change it, copy and modify the script.

**Expected result:** WASD moves at 5 units/s; Space rolls (start speed **25**, minimum **5**) or teleports **5** units; T teleports in the full version.

---

### Workflow: Top Down Character Controller 3D

**Goal:** Add a camera-relative 3D top-down player.

**Steps:**
1. Drag `Assets/CodeMonkey/Toolkit/Tools/TopDownCharacterController3D/Prefabs/Player.prefab` into the Hierarchy.
2. Ensure a Camera tagged **MainCamera** exists — `TopDownCharacterController3D` fetches `Camera.main.transform` in `Start()` for directional input.
3. Class: `CodeMonkey.Toolkit.TTopDownCharacterController3D.TopDownCharacterController3D`. Hardcoded values: `moveSpeed = 10f`, gravity `7.8 m/s²`, jump impulse `1.8`.
4. Public methods: `Enable()`, `Disable()`, `LockMouse()`, `UnlockMouse()`.

**Expected result:** WASD moves relative to camera forward/right; Space jumps; cursor is locked on start.

---

### Workflow: Camera Controller Basic (3D)

**Goal:** Add a free-roam 3D camera for quick prototyping.

**Steps:**
1. Drag `Assets/CodeMonkey/Toolkit/Tools/CameraControllerBasic/Prefabs/CameraController.prefab` into the Hierarchy, **or** add the `CameraControllerBasic` component (namespace `CodeMonkey.Toolkit.TCameraControllerBasic`) to any parent transform that has a Camera as a child.
2. The script moves and rotates the parent Transform; the Camera follows as a child.
3. Set serialized fields in the Inspector (both default to **0** in the script — set them before play):
   - `Move Speed` (float) — recommended **10–20**
   - `Rotate Amount` (float) — recommended **80–120** (degrees per second)
4. Controls: **WASD** — move along camera forward/right; **Q** — rotate left; **E** — rotate right.

**Expected result:** Camera pans and rotates at configured speeds.

---

### Workflow: Camera Controller Basic 2D

**Goal:** Add a free-roam 2D camera.

**Steps:**
1. Drag `Assets/CodeMonkey/Toolkit/Tools/CameraControllerBasic2D/Prefabs/CameraController2D.prefab` into the Hierarchy, **or** add `CameraControllerBasic2D` (namespace `CodeMonkey.Toolkit.TCameraControllerBasic2D`) to a parent transform.
2. Set `Move Speed` (float, defaults to **0** — must be set; recommend **5–15**).
3. Controls: **WASD** — move along camera up/right axes.

**Expected result:** Camera pans in 2D at the configured speed.

---

### Workflow: LookAtCamera

**Goal:** Make an object (health bar, sprite, UI world canvas) always face the camera.

**Steps:**
1. Add the `LookAtCamera` component (namespace `CodeMonkey.Toolkit.TLookAtCamera`) to the target object.
2. Set the `Method` enum field in the Inspector:
   - `LookAt` — transform faces toward camera position
   - `LookAtInverted` — transform faces away from camera
   - `CameraForward` — transform.forward = camera.forward
   - `CameraForwardInverted` — transform.forward = -camera.forward
3. Via code:
```csharp
using CodeMonkey.Toolkit.TLookAtCamera;

// Change method at runtime
GetComponent<LookAtCamera>().SetMethod(LookAtCamera.Method.CameraForward);

// Add component and configure via extension methods
gameObject.AddLookAtCamera(LookAtCamera.Method.LookAt);
transform.AddLookAtCamera(LookAtCamera.Method.LookAtInverted);
```
4. Requires a Camera tagged **MainCamera** to be non-null at `Awake`.

**Expected result:** The object re-orients every frame to maintain the chosen relationship with the main camera, even as the camera or object moves.

## Verification

- **Asset installed:** `System.IO.File.Exists("Assets/CodeMonkey/Toolkit/Tools/FirstPersonController/Scripts/FirstPersonController.cs")` → `true`
- **Menu present:** Open Unity, check **Tools → Code Monkey Toolkit** exists in the menu bar
- **Namespace resolves:** `typeof(CodeMonkey.Toolkit.TFirstPersonController.FirstPersonController)` compiles without error
- **Play test — FPC moves:** enter Play Mode, press W for 0.5 s, assert `player.transform.position != initialPosition`
- **Play test — LookAtCamera:** with `Method.CameraForward`, assert `Mathf.Abs(Vector3.Dot(billboard.transform.forward, Camera.main.transform.forward) - 1f) < 0.001f`

## API quick reference

| Entry point | Type | Key fields / notes |
|---|---|---|
| `CodeMonkey.Toolkit.TFirstPersonController.FirstPersonController` | MonoBehaviour | `playerCamera` (Camera), `sensitivity` = 1, `moveSpeed` = 10 |
| `FirstPersonController.LockMouse()` / `UnlockMouse()` | void | Cursor lock state |
| `FirstPersonController.Enable()` / `Disable()` | void | Pause/resume input |
| `CodeMonkey.Toolkit.TTopDownCharacterController.CharacterController2D` | MonoBehaviour | `canTeleport` = true, `canRoll` = true; MOVE_SPEED const = 5 |
| `CodeMonkey.Toolkit.TTopDownCharacterController.CharacterController2D_Simple` | MonoBehaviour | `canTeleport` = true; MOVE_SPEED const = 5 |
| `CodeMonkey.Toolkit.TTopDownCharacterController3D.TopDownCharacterController3D` | MonoBehaviour | No serialized fields; `Enable()`, `Disable()` |
| `CodeMonkey.Toolkit.TCameraControllerBasic.CameraControllerBasic` | MonoBehaviour | `moveSpeed`, `rotateAmount` (both default 0) |
| `CodeMonkey.Toolkit.TCameraControllerBasic2D.CameraControllerBasic2D` | MonoBehaviour | `moveSpeed` (default 0) |
| `CodeMonkey.Toolkit.TLookAtCamera.LookAtCamera` | MonoBehaviour | `method` enum (LookAt, LookAtInverted, CameraForward, CameraForwardInverted) |
| `LookAtCamera.SetMethod(Method)` | void | Change billboard mode at runtime |
| `LookAtCameraExtensions.AddLookAtCamera(Method)` | Extension on `GameObject`/`Transform` | Add and configure component in one call |

## Common issues

| Symptom | Cause | Fix |
|---|---|---|
| `NullReferenceException` in `FirstPersonController.Awake` | `playerCamera` not assigned | Open the prefab; drag child Camera into the `Player Camera` field |
| No movement in any controller | New Input System not active | **Project Settings → Player → Active Input Handling → Input System Package (New)** |
| Camera doesn't move (`CameraControllerBasic`) | `moveSpeed` and `rotateAmount` are 0 | Set both to non-zero values in the Inspector |
| `LookAtCamera` throws NullReferenceException | `Camera.main` is null | Ensure a Camera tagged **MainCamera** is present before Play |
| 2D player slides or drifts | Rigidbody2D physics material | Confirm `Rigidbody2D` is present; MOVE_SPEED is a constant (5 m/s) and cannot be changed without modifying source |
| `TopDownCharacterController3D` moves wrong direction | Camera orientation | Position the main camera at the desired top-down angle before Play |

## Boundaries

- **2D move speed is not serialized.** `MOVE_SPEED = 5f` is a constant in both `CharacterController2D` and `CharacterController2D_Simple`. Copy and modify the script to change it.
- **No animation integration.** Controllers handle Transform or Rigidbody movement only; Animator hookups must be added separately.
- **No multiplayer.** No network synchronization is included.
- **Controls are hardcoded.** WASD, Mouse, Space, Q/E, T cannot be remapped without modifying source. For configurable input, integrate Unity's Input Action assets directly.
- **`TopDownCharacterController3D` gravity is hardcoded** at 7.8 m/s² and is not serialized.
- Fall back to Unity's built-in `CharacterController`, `Rigidbody`, or `Rigidbody2D` directly when you need custom physics, animation-driven movement, or runtime-configurable speeds.
