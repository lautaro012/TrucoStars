---
name: code-monkey-toolkit-gameplay-systems
description: "Use this skill whenever a user wants gameplay mechanics from Code Monkey Toolkit: add health/damage/healing, a health bar, an interaction system, 'press E to interact', talk to an NPC, open a door with a key ('keys and doors'), a 'loading screen between scenes', throttled tick logic ('run logic every second'), a 'day night cycle' / 'rotate the sun over time' / 'time of day system', or make objects destructible ('shatter/explode into pieces', 'add destruction'). Covers HealthSystem/HealthBarUI, InteractionSystemProximity (IInteractable + press-E), KeyDoorSystem, TickSystem, SceneLoadingSystem, DayNightCycle (GetDayTime24), and SimpleDestruction (fractured-prefab technique). Does NOT cover grids (see code-monkey-toolkit-grid-systems), camera/character movement (see code-monkey-toolkit-controllers-and-camera), or UI popups/tooltips (see code-monkey-toolkit-ui-and-feedback). When in doubt whether a gameplay mechanic belongs here, use this skill — Prerequisites shows how to confirm the asset is installed."
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

# Code Monkey Toolkit — Gameplay Systems

The Code Monkey Toolkit ships several reusable gameplay mechanic systems that handle common game patterns without boilerplate. This skill covers health management, proximity-based interaction, key-and-door mechanics, throttled tick logic, and async scene loading with a loading screen.

## When to use this skill

- User wants to add health, damage, or healing to any game object.
- User wants a health bar that automatically updates when health changes.
- User wants "press E to interact" with NPCs, doors, or any interactable object.
- User wants a player to pick up keys and use them to open doors.
- User wants a loading screen displayed between scene transitions.
- User wants to run logic on a throttled rate (e.g., 5×/sec instead of 60×/sec).
- User wants a day/night cycle that rotates the sun and tracks the time of day.
- User wants objects to shatter or explode into physics-driven pieces (destruction).

**Not for:**
- Grid or hex grid systems → `code-monkey-toolkit-grid-systems`
- Camera controllers or character movement → `code-monkey-toolkit-controllers-and-camera`
- UI feedback widgets (tooltips, text pop-ups) → `code-monkey-toolkit-ui-and-feedback`
- Shaders, dissolve, or visual effects → `code-monkey-toolkit-shaders-and-effects`
- Package installation or initial setup → `code-monkey-toolkit-setup-and-overview`

## Prerequisites

1. Code Monkey Toolkit **1.0.5** installed.
2. Unity **6000.0.60f1+**.
3. **Programmatic install check:**

```csharp
bool installed = typeof(CodeMonkey.Toolkit.THealthSystem.HealthSystem) != null;
Debug.Log(installed ? "HealthSystem found ✓" : "HealthSystem NOT found");
```

If the type is missing, import from:
`https://assetstore.unity.com/packages/tools/utilities/code-monkey-toolkit-309282`

## Quick start

Add a health system to an enemy and destroy it when it dies.

```csharp
using UnityEngine;
using CodeMonkey.Toolkit.THealthSystem;

public class Enemy : MonoBehaviour, IGetHealthSystem {
    private HealthSystem _health;

    private void Awake() {
        _health = new HealthSystem(100f);           // starts at full health (100)
        _health.OnDead += (s, e) => Destroy(gameObject);
    }

    public HealthSystem GetHealthSystem() => _health;

    public void TakeDamage(float amount) => _health.Damage(amount);
}
```

**Observable result:** Calling `enemy.TakeDamage(100f)` destroys the GameObject. `OnDead` fires once when health reaches 0.

## Workflows

### Workflow: HealthSystem (plain C# class)

**Goal:** Add health to any C# object without requiring a MonoBehaviour.

**Steps:**
1. Add `using CodeMonkey.Toolkit.THealthSystem;`
2. Create: `var hs = new HealthSystem(healthMax: 100f);` — current health starts at `healthMax`.
3. Subscribe to events before dealing damage:
   ```csharp
   hs.OnHealthChanged += (s, e) => UpdateBar(hs.GetHealthNormalized());
   hs.OnDamaged       += (s, e) => PlayHurtAnimation();
   hs.OnHealed        += (s, e) => PlayHealEffect();
   hs.OnDead          += (s, e) => Die();
   ```
4. Modify health:
   - `hs.Damage(25f)` — reduces health; clamps to 0; fires `OnDead` when health ≤ 0.
   - `hs.Heal(10f)` — increases health; clamps to `healthMax`.
   - `hs.HealComplete()` — sets health to `healthMax`.
   - `hs.Die()` — fires `OnDead` directly without changing the health value.
5. Query state: `hs.GetHealth()`, `hs.GetHealthMax()`, `hs.GetHealthNormalized()` (0–1), `hs.IsDead()`.
6. Change the max: `hs.SetHealthMax(200f, fullHealth: true);` — fires `OnHealthMaxChanged` then `OnHealthChanged`.

**Expected result:** Events fire in verified source order: `OnDamaged → OnHealthChanged → (OnDead if health ≤ 0)`.

---

### Workflow: HealthSystemComponent (drag-drop MonoBehaviour)

**Goal:** Add health to a GameObject through the Inspector only.

**Steps:**
1. Add `HealthSystemComponent` to the target GameObject.
2. Set **Health Amount Max** (default `100`). Set **Starting Health Amount** to 0 to start at full health.
3. From code: `GetComponent<HealthSystemComponent>().GetHealthSystem()`.
4. From any system: `HealthSystem.TryGetHealthSystem(go, out HealthSystem hs, logErrors: true)` retrieves the `HealthSystem` from any GameObject whose component implements `IGetHealthSystem`.

**Expected result:** No code required on the target object; health is fully functional from the Inspector.

---

### Workflow: HealthBarUI

**Goal:** Show a fill-image health bar that auto-updates when health changes.

**Steps:**
1. Create a Canvas → Image with **Image Type = Filled**.
2. Add `HealthBarUI` component to the health bar's parent GameObject.
3. In the Inspector, assign the **Image** field to the fill `Image` component.
4. Assign **Get Health System Game Object** to the GameObject that has `HealthSystemComponent` (or any `IGetHealthSystem` implementor). Leave blank and call `healthBarUI.SetHealthSystem(hs)` in code if preferred.

**Expected result:** The fill amount tracks `HealthSystem.GetHealthNormalized()` automatically via `OnHealthChanged`.

---

### Workflow: InteractionSystemProximity (press E to interact)

**Goal:** Let a player interact with nearby objects by pressing E.

**Steps:**
1. Add `PlayerInteractProximity` component to the **player** GameObject.
   - Detect range is **3f world units** (constant; source: `INTERACT_RANGE = 3f`).
   - Supports both Unity Input System and legacy Input Manager via `#if ENABLE_INPUT_SYSTEM`.
   - Uses `Physics.OverlapSphere` — objects must have a 3D **Collider**.
2. Implement `IInteractable` on any interactable object:
   ```csharp
   using CodeMonkey.Toolkit.TInteractionSystemProximity;
   using System.Collections.Generic;
   using UnityEngine;

   public class MyInteractable : MonoBehaviour, IInteractable {
       public bool CanDoInteractAction(IInteractable.InteractAction a) => true;

       public void Interact(IInteractable.InteractAction action, Transform interactor) {
           Debug.Log($"{interactor.name} interacted!");
       }

       public Dictionary<IInteractable.InteractAction, string> GetInteractTextDictionary() =>
           new() { { IInteractable.InteractAction.Primary, "Pick up" } };

       public Transform GetTransform() => transform;
   }
   ```
3. Optionally add `PlayerInteractUI` to the player UI; assign `PlayerInteractProximity` and a `TextMeshProUGUI` element. It shows `GetInteractTextDictionary()[Primary]` when an interactable is in range.
4. Player presses **E** → `PlayerInteractProximity.Update` calls `GetInteractableObject()` which returns the nearest `IInteractable` within 3 units, then calls `Interact(Primary, playerTransform)`.

**Expected result:** Interaction text appears in range; pressing E fires `Interact` on the nearest object.

---

### Workflow: InteractionSystemLookAt (interact with what you aim at)

**Goal:** Let a player interact with the object the camera is pointed at (first-person style), rather than the nearest one.

**Steps:**
1. Add `PlayerInteractLookAt` (namespace `CodeMonkey.Toolkit.TInteractionSystemLookAt`) to the **player** GameObject.
   - It spherecasts from `Camera.main.forward` (`SPHERE_CAST_RADIUS = 0.1f`, `INTERACT_DISTANCE = 3f`; both const). Requires a `Camera` tagged **MainCamera**.
   - Serialized `testForInput` (default `true`) — set false to drive interaction manually.
2. Implement `IInteractable` from **this** namespace on the target (same four members as the proximity version: `CanDoInteractAction`, `Interact`, `GetInteractTextDictionary`, `GetTransform`). Note the interface lives in `CodeMonkey.Toolkit.TInteractionSystemLookAt` — it is distinct from the proximity interface.
3. Optionally add `PlayerInteractUI` (this tool's own) to show the interact prompt.
4. Player looks at an interactable and presses **E** → `GetInteractableObject()` returns the closest hit `IInteractable` and calls `Interact(Primary, transform)`.

**Expected result:** Aiming the camera at an interactable and pressing E fires its `Interact`; objects outside the camera ray are ignored even if nearby.

---

### Workflow: KeyDoorSystem

**Goal:** Player picks up a physical key; colliding with a door opens it automatically.

**Steps:**
1. Create a `Key` ScriptableObject via:
   **Assets → Create → Code Monkey → Toolkit → Key Door System → Create Key**
   Set the `keyColor` (public `Color` field).
2. Create a **key pickup prefab**: add `DoorKey` MonoBehaviour; assign the `Key` asset to **Key** field.
3. Create a **door prefab**: add `DoorLock` MonoBehaviour; assign the matching `Key` asset; toggle **Remove Key On Use**. Add an `Animator` with **"Open"** and **"Close"** trigger parameters.
4. Add `DoorKeyHolder` to the **player** GameObject. Ensure the player has a **Trigger Collider** (`Is Trigger = true`).
5. Optionally add `DoorKeyUI` to a Canvas; assign `keyUISinglePrefab` and the player's `DoorKeyHolder` reference.

**Runtime flow (verified from source):**
- Player trigger enters key prefab collider → `DoorKeyHolder.OnTriggerEnter` adds key to list, destroys the pickup, fires `OnDoorKeyAdded`.
- Player trigger enters door collider → if matching key is held, calls `DoorLock.OpenDoor()` (animator trigger "Open"), optionally removes key, fires `OnDoorKeyUsed`.

**Expected result:** Key disappears on pickup; door plays Open animation when the correct key-holding player touches it.

---

### Workflow: TickSystem (throttled update events)

**Goal:** Run logic at ~5 ticks/sec instead of every frame.

**Steps:**
1. Call `TickSystem.Create()` in any `Awake` or `Start`, **or** place the TickSystem prefab in the scene.
2. Subscribe to the event channel you need:
   ```csharp
   using CodeMonkey.Toolkit.TTickSystem;

   TickSystem.OnTick   += (s, e) => DoWork();          // every tick  (~5/sec)
   TickSystem.OnTick_2 += (s, e) => DoWork();          // every 2nd tick (~2.5/sec)
   TickSystem.OnTick_5 += (s, e) => DoWork();          // every 5th tick (~1/sec)
   TickSystem.OnTick_10 += (s, e) => DoWork();         // every 10th tick (~0.5/sec)
   ```
3. `OnTickEventArgs.tick` contains the running tick counter.
4. Query current tick: `TickSystem.GetTick()`.

**Key defaults (verified from TickSystem.cs):**
- `TICK_TIMER_MAX = 0.2f` → ~5 ticks per second.
- Timer uses `Time.deltaTime` internally (time-scale dependent). The parameter passed by `TickSystemMonoBehaviour` (`Time.unscaledDeltaTime`) is **currently ignored** by the static class — the static `Update` method reads `Time.deltaTime` directly.

**Expected result:** Subscribed lambda executes roughly every 0.2 s of scaled game time.

---

### Workflow: SceneLoadingSystem

**Goal:** Transition from one scene to another via an async loading screen.

**Steps:**
1. Open `SceneLoader.cs` (namespace `CodeMonkey.Toolkit.TSceneLoadingSystem`) and extend the `Scene` enum to include your scenes:
   ```csharp
   public enum Scene {
       Loading,        // required — the loading scene; do not rename
       MainMenu,
       GameLevel1,
   }
   ```
   Scene enum names must match the scene file names **exactly**.
2. Add all scenes (including the Loading scene) to the **Build Profiles → Scene List** (`File → Build Profiles → Scene List`).
3. Create or adapt your **Loading scene**:
   - Add `SceneLoaderCallback` MonoBehaviour to any GameObject — this triggers `SceneLoader.LoaderCallback()` on the first `LateUpdate`.
   - Optionally add `SceneLoadingProgressBar` with a Filled `Image` for a progress display.
4. Trigger a transition from any script:
   ```csharp
   using CodeMonkey.Toolkit.TSceneLoadingSystem;
   SceneLoader.Load(SceneLoader.Scene.GameLevel1);
   ```
5. `SceneLoader.GetLoadingProgress()` returns a 0–1 float during async loading.

**Expected result:** The Loading scene appears first, progress bar fills, then the target scene loads.

---

### Workflow: DayNightCycle

**Goal:** Rotate the sun over time and read the current time of day.

**Steps:**
1. Drag `Assets/CodeMonkey/Toolkit/Tools/DayNightCycle/Prefabs/DayNightCycle.prefab` into the scene. It rotates its directional light and advances time automatically.
2. Set **Seconds Per Day** in the Inspector (serialized `secondsPerDay`, default **30f**) — how many real seconds a full 24-hour day takes.
3. Read the time from code via the singleton (namespace `CodeMonkey.Toolkit.TDayNightCycle`):
   ```csharp
   using CodeMonkey.Toolkit.TDayNightCycle;

   float t01 = DayNightCycle.Instance.GetDayTime();    // 0..1 (0 = midnight, 0.5 = midday)
   float hours = DayNightCycle.Instance.GetDayTime24(); // 0..24
   if (hours > 20f || hours < 6f) SpawnNightEnemies();
   ```
4. Optionally drag `.../DayNightCycle/Prefabs/DayNightCycleUI.prefab` onto a `Canvas` for an on-screen clock (an `Image.fillAmount` dial + a `TextMeshProUGUI` time label). The companion `DayNightCycleVisual` drives an `Animator` float parameter `"DayTime"` if you want animated skies.

**Expected result:** The scene's sun rotates continuously; `DayNightCycle.Instance.GetDayTime24()` returns a value in `0..24` that advances over `secondsPerDay` seconds.

---

### Workflow: SimpleDestruction (fractured-prefab technique)

**Goal:** Make an object shatter into physics-driven pieces.

This is a **reproducible technique**, not a drag-and-drop system — the toolkit ships the demo and the `CubePieces` explosion helper; you supply (or reuse) a pre-fractured mesh prefab.

**Steps:**
1. Prepare a **fractured prefab**: a parent GameObject whose children are the individual pieces, each with its own `MeshRenderer`, `Collider`, and `Rigidbody`. You can reuse `Assets/CodeMonkey/Toolkit/Tools/SimpleDestruction/Demo/CubePieces.prefab` as a reference/starting point.
2. Add the `CubePieces` component (namespace `CodeMonkey.Toolkit.TSimpleDestruction.Demo`) to the fractured prefab root. On `Start()` it loops over each child and applies an outward burst to any child with a `Rigidbody`:
   ```csharp
   // CubePieces.cs (verified) — applied to every child Rigidbody on Start
   piecesRigidbody.AddExplosionForce(300f, transform.position, 10f, 1f);
   ```
3. On your destruction trigger, disable or destroy the intact object and instantiate the fractured prefab at the same position and rotation:
   ```csharp
   intactObject.SetActive(false);
   Instantiate(fracturedPrefab, intactObject.transform.position, intactObject.transform.rotation);
   ```

**Expected result:** The intact object is replaced by its fractured prefab; the pieces burst apart under `AddExplosionForce` and fall with physics.

## Verification

- `typeof(CodeMonkey.Toolkit.THealthSystem.HealthSystem)` resolves without error.
- `typeof(CodeMonkey.Toolkit.TInteractionSystemProximity.IInteractable)` resolves without error.
- `typeof(CodeMonkey.Toolkit.TInteractionSystemLookAt.PlayerInteractLookAt)` resolves without error.
- `typeof(CodeMonkey.Toolkit.TKeyDoorSystem.DoorKeyHolder)` resolves without error.
- `typeof(CodeMonkey.Toolkit.TTickSystem.TickSystem)` resolves without error.
- `typeof(CodeMonkey.Toolkit.TSceneLoadingSystem.SceneLoader)` resolves without error.
- `HealthSystem.OnDead` fires exactly once after `Damage(healthMax)`.
- No Console errors when pressing E near a correctly configured `IInteractable` GameObject.
- `DoorKeyHolder.GetDoorKeyHoldingList().Count` increases by 1 after the player overlaps a `DoorKey` trigger.
- `typeof(CodeMonkey.Toolkit.TDayNightCycle.DayNightCycle)` resolves and `DayNightCycle.prefab` exists under `Tools/DayNightCycle/Prefabs/`.
- At runtime `DayNightCycle.Instance.GetDayTime24()` returns a value within `0..24`.
- `Tools/SimpleDestruction/Demo/CubePieces.prefab` exists and its child pieces each have a `Rigidbody`.

## API quick reference

| Entry point | Type | What it does |
|---|---|---|
| `new HealthSystem(float healthMax)` | C# constructor | Create health starting at full |
| `HealthSystem.Damage(float)` | Method | Reduce health; fires `OnDamaged`, `OnDead` |
| `HealthSystem.Heal(float)` | Method | Increase health up to `healthMax` |
| `HealthSystem.HealComplete()` | Method | Set health to `healthMax` |
| `HealthSystem.IsDead()` | Method | `true` when health ≤ 0 |
| `HealthSystem.GetHealthNormalized()` | Method | 0–1 ratio for UI fill |
| `HealthSystem.TryGetHealthSystem(go, out hs)` | Static method | Retrieve HS from any `IGetHealthSystem` GameObject |
| `HealthSystemComponent` (MonoBehaviour) | Component | Inspector-driven health; `healthAmountMax` (default `100f`), `startingHealthAmount` |
| `HealthBarUI` (MonoBehaviour) | Component | Auto-updating fill bar; wires to `OnHealthChanged` |
| `IGetHealthSystem` | Interface | Implement to expose a `HealthSystem` from any script |
| `IInteractable` | Interface | Implement on any interactable object |
| `IInteractable.InteractAction` | Enum | `Primary`, `Secondary` |
| `PlayerInteractProximity` (MonoBehaviour) | Component | E key, `INTERACT_RANGE = 3f`, finds nearest `IInteractable` |
| `PlayerInteractUI` (MonoBehaviour) | Component | Shows interaction prompt from `GetInteractTextDictionary()[Primary]` |
| `TInteractionSystemLookAt.PlayerInteractLookAt` (MonoBehaviour) | Component | E key, camera-forward `SphereCastAll`, `INTERACT_DISTANCE = 3f`; requires MainCamera |
| `TInteractionSystemLookAt.IInteractable` | Interface | Look-at interactable (separate namespace from the proximity interface) |
| `Key` (ScriptableObject) | Asset | Key identity with `Color keyColor`; create via Assets menu |
| `DoorKey` (MonoBehaviour) | Component | Links a `Key` asset to a pickup prefab |
| `DoorLock` (MonoBehaviour) | Component | Door that opens with a matching `Key`; triggers animator "Open"/"Close" |
| `DoorKeyHolder` (MonoBehaviour) | Component | Player key inventory; `OnDoorKeyAdded`, `OnDoorKeyUsed` events |
| `DoorKeyUI` (MonoBehaviour) | Component | Canvas display of held keys |
| `TickSystem.Create()` | Static method | Spawns the tick driver `GameObject` at runtime |
| `TickSystem.OnTick` | Static event | Every tick (~5×/sec, `TICK_TIMER_MAX = 0.2f`) |
| `TickSystem.OnTick_2/5/10` | Static events | Every 2nd/5th/10th tick |
| `TickSystem.GetTick()` | Static method | Returns running tick integer |
| `SceneLoader.Load(Scene)` | Static method | Loads through the Loading scene asynchronously |
| `SceneLoader.GetLoadingProgress()` | Static method | 0–1 async load progress |
| `SceneLoaderCallback` (MonoBehaviour) | Component | Place in Loading scene; triggers `LoaderCallback()` on first LateUpdate |
| `SceneLoadingProgressBar` (MonoBehaviour) | Component | Sets `Image.fillAmount` from `GetLoadingProgress()` |
| `DayNightCycle.Instance` | Static property | Singleton set in `Awake`; drag `DayNightCycle.prefab` into the scene |
| `DayNightCycle.GetDayTime()` / `GetDayTime24()` | Method | Time as 0–1 / 0–24; `secondsPerDay` (default `30f`) sets day length |
| `DayNightCycleUI` (MonoBehaviour) | Component | Optional on-screen clock (Image dial + TMP label) |
| `CubePieces` (MonoBehaviour) | Component | Fractured-prefab helper; `AddExplosionForce(300f, pos, 10f, 1f)` on each child Rigidbody in `Start` |

## Common issues

| Symptom | Cause | Fix |
|---|---|---|
| `OnDead` fires twice | `Die()` called manually AND by `Damage` | Let `Damage` trigger death automatically; call `Die()` only when bypassing health |
| Health bar stays at 0 or 1 | `getHealthSystemGameObject` not assigned | Assign the target GO in Inspector or call `SetHealthSystem()` in code |
| Interaction not triggering on E | Object has no 3D Collider or wrong layer | Add a `Collider`; `Physics.OverlapSphere` searches all layers by default |
| Door does not animate | Animator missing "Open" trigger | Add Animator component with an "Open" trigger state transition |
| Key not auto-collected | Player lacks trigger Collider | Add a Collider with **Is Trigger = true** to the player |
| `TickSystem.OnTick` never fires | `TickSystem.Create()` not called; prefab not in scene | Call `TickSystem.Create()` in `Awake` of any persistent script |
| Scene load fails with missing scene error | Scene name in enum differs from file name, or scene not in Build list | Enum value must exactly match scene filename; add scene to Build Profiles |
| Look-at interaction not triggering on E | Camera not tagged MainCamera, or object has no Collider | `PlayerInteractLookAt` spherecasts from `Camera.main.forward`; ensure a MainCamera and a Collider on the target |
| Unsure whether to use Proximity or LookAt interaction | Both exist as separate tools with their own `IInteractable` | Use `InteractionSystemProximity` for "nearest within range"; use `InteractionSystemLookAt` for "what the camera is aimed at" |
| Sun does not move / no day-night change | `DayNightCycle.prefab` not in scene, or its directional light was removed | Drag the prefab in unmodified; it must retain its child directional light |
| `DayNightCycle.Instance` is null | Prefab not in scene or accessed before its `Awake` | Ensure the prefab is present and read `Instance` from `Start`/later |
| DayNightCycleUI shows nothing | No `Canvas` in scene | Place `DayNightCycleUI.prefab` under a `Canvas` |
| Destruction pieces don't move | Child pieces have no `Rigidbody`, or the mesh is one solid collider | Give each piece its own `Rigidbody`; use a pre-fractured (multi-child) prefab |

## Boundaries

- **Two interaction systems exist and are independent.** `InteractionSystemProximity` (`PlayerInteractProximity`, `Physics.OverlapSphere`, nearest within `INTERACT_RANGE = 3f`) and `InteractionSystemLookAt` (`PlayerInteractLookAt`, `Physics.SphereCastAll` along `Camera.main.forward`, `INTERACT_DISTANCE = 3f`). **Each declares its own `IInteractable` in its own namespace** (`...TInteractionSystemProximity.IInteractable` vs `...TInteractionSystemLookAt.IInteractable`) — an object's `IInteractable` implementation targets one system; implement both interfaces if you need both.
- **`TickSystem` is time-scale dependent.** The internal timer uses `Time.deltaTime` — pausing the game (timeScale = 0) stops ticks. To make ticks time-scale independent you must modify `TickSystem.cs` itself.
- **`SceneLoader.Scene` enum requires source edits.** There is no Inspector or ScriptableObject scene list; you must add enum entries manually in `SceneLoader.cs`.
- **`DoorKeyHolder` uses `OnTriggerEnter` (3D only).** 2D physics (`OnTriggerEnter2D`) is not used; this system is designed for 3D games.
- **`HealthSystem` state is not serialised.** Save and restore `GetHealth()` / `GetHealthMax()` manually for persistence across sessions.
- **`PlayerInteractProximity` interact range is a hardcoded `const float INTERACT_RANGE = 3f`** in source. To change it, copy and modify the script.
- **`DayNightCycle` rotates the directional light and tracks time only.** It does not blend skyboxes, change fog/ambient lighting, or simulate weather; add those yourself using `GetDayTime()`.
- **`SimpleDestruction` is a demo technique, not a reusable API.** There is no `Scripts/` folder — only the demo `CubePieces` helper. You must supply a pre-fractured mesh prefab (the toolkit does not fracture meshes at runtime), and it relies on 3D physics (`Rigidbody.AddExplosionForce`).
