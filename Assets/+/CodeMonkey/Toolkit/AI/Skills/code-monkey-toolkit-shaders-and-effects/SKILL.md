---
name: code-monkey-toolkit-shaders-and-effects
description: "Use this skill whenever the user asks to apply visual shader effects to sprites or game objects, or trigger screen-wide visual effects, using Code Monkey Toolkit — phrases like 'add an outline to my sprite', 'blur this sprite', 'dissolve effect', 'pixelate effect', 'scene transition wipe', 'cinematic black bars', 'shatter the screen', 'zoom lens effect', 'tint my character red when hit', 'let the player draw with the mouse', or 'pixel drawing canvas'. Covers ShaderBlur2D, ShaderDissolve2D, ShaderOutline2D, ShaderPixelate2D, ShaderTint2D, ZoomShader, TransitionEffect shader, CinematicBars, BreakTheScreen, DrawMesh, and DrawPixels. Does not cover runtime UI dialogs, chat bubbles, or typewriter text — use code-monkey-toolkit-ui-and-feedback. Does not cover grid or gameplay systems — see code-monkey-toolkit-grid-systems or code-monkey-toolkit-gameplay-systems. When in doubt, use this skill — Prerequisites shows how to confirm the asset is installed."
metadata:
  asset: "Code Monkey Toolkit"
  publisher: "Code Monkey"
  asset-version: "1.0.5"
  skill-version: "1.0.1"
  unity: "6000.0.60f1+"
  render-pipelines: "Built-in, URP, HDRP"
  category: "tools/utilities"
  asset-store-url: "https://assetstore.unity.com/packages/tools/utilities/code-monkey-toolkit-309282"
  documentation-url: "https://unitycodemonkey.com/"
  support-url: "https://assetstore.unity.com/packages/tools/utilities/code-monkey-toolkit-309282"
  last-verified: "2026-09-07"
---

# Code Monkey Toolkit — Shaders and Visual Effects

Shader Graph–based 2D sprite shaders and C# screen-effect helpers that ship with Code
Monkey Toolkit. The 2D shaders (Blur, Dissolve, Outline, Pixelate, Tint, ZoomShader,
TransitionEffect) are Shader Graph assets — assign them via a Material on a SpriteRenderer.
The C# screen effects (`CinematicBars`, `BreakTheScreen`, `ShaderTransitionEffect`) create
UI or GameObject hierarchies at runtime with no manual prefab wiring.

Per-shader property tables are in [references/shader-properties.md](references/shader-properties.md).

> **Render Pipeline note:** the `render-pipelines` metadata field lists `Built-in, URP, HDRP`
> because the *package* (including the C# tools) supports all pipelines. However, **all Shader
> Graph shaders require URP or HDRP** — they render pink/magenta on Built-in RP. See
> Prerequisites.

## When to use this skill

- Adding a colored outline to a sprite (`ShaderOutline2D`)
- Blurring a sprite smoothly (`ShaderBlur2D`)
- Dissolving a sprite with a noise pattern (`ShaderDissolve2D`)
- Pixelating a sprite (`ShaderPixelate2D`)
- Tinting or flash-coloring a sprite (`ShaderTint2D`)
- Applying a zoom-lens distortion at a world position (`ZoomShader`)
- Wiping the screen for a scene transition (`TransitionEffect` / `ShaderTransitionEffect`)
- Adding cinematic black bars to the top and bottom of the screen (`CinematicBars`)
- Shattering the screen into physics pieces (`BreakTheScreen`)
- Letting the player draw freehand vector strokes with the mouse (`DrawMesh`)
- Letting the player paint a 100×100 pixel canvas (`DrawPixels`)

**Not for:**
- Runtime UI popups, chat bubbles, dialogs, typewriter text →
  use `code-monkey-toolkit-ui-and-feedback`
- Grid systems, health systems → use `code-monkey-toolkit-grid-systems`,
  `code-monkey-toolkit-gameplay-systems`
- Camera controllers → use `code-monkey-toolkit-controllers-and-camera`

## Prerequisites

- **Code Monkey Toolkit 1.0.5+** installed.
- **Unity 6000.0.60f1+**
- **URP or HDRP required for all Shader Graph shaders.** ShaderBlur2D, ShaderDissolve2D,
  ShaderOutline2D, ShaderPixelate2D, ShaderTint2D, ZoomShader, and TransitionEffect are all
  Shader Graph assets. Sprites assigned these shaders will render pink/magenta on Built-in RP.
  Switch to URP: `Edit > Render Pipeline > Universal Render Pipeline > Upgrade Project
  Materials`. The C# tools (CinematicBars, BreakTheScreen, DrawMesh, DrawPixels) work on
  all render pipelines.
- **TextMeshPro** not required for shader tools.
- **Input System** not required (DrawMesh and DrawPixels have legacy and new Input System
  paths via `#if ENABLE_INPUT_SYSTEM`).

**Programmatic install check:**

```csharp
#if UNITY_EDITOR
var cmType = System.Type.GetType(
    "CodeMonkey.Toolkit.TCinematicBars.CinematicBars, Assembly-CSharp");
if (cmType == null)
    Debug.LogError("Code Monkey Toolkit not found. Install from " +
        "https://assetstore.unity.com/packages/tools/utilities/code-monkey-toolkit-309282");
#endif
```

## Quick start

**Goal:** Add a white outline to a player sprite.

1. In the Project window, select or create a Material for the player SpriteRenderer.
2. In the Material Inspector, click the Shader dropdown and navigate to
   **Shader Graphs > Outline**. (Alternatively, drag
   `Assets/CodeMonkey/Toolkit/Tools/ShaderOutline2D/Shaders/Outline.shadergraph`
   onto the Shader slot.)
3. Assign the Material to the SpriteRenderer **Material** field.
4. In the Material Inspector, set:
   - **Outline Thickness** (`_OutlineThickness`): `1.0` (range 0–3)
   - **Outline Color** (`_OutlineColor`): white `(1, 1, 1, 1)`
5. The outline appears immediately in the Scene view (URP/HDRP project required).

To animate the outline at runtime:

```csharp
using UnityEngine;

public class OutlineFlash : MonoBehaviour {
    [SerializeField] private SpriteRenderer spriteRenderer;

    public void Flash() {
        // renderer.material creates a per-instance material copy
        spriteRenderer.material.SetFloat("_OutlineThickness", 2f);
        spriteRenderer.material.SetColor("_OutlineColor", Color.yellow);
    }
}
```

> Use `renderer.material` (creates an instance) instead of `renderer.sharedMaterial`
> to avoid affecting all sprites that share the same Material asset.

**Expected result:** Sprite renders with a colored outline border in the Scene/Game view.

## Workflows

### Workflow: Apply a 2D Shader to a SpriteRenderer

**Goal:** Apply Blur, Dissolve, Outline, Pixelate, or Tint to any sprite.

**Steps:**
1. **Create a Material**: right-click in the Project window > **Create > Material**.
2. Set the Shader to the desired Shader Graph (dropdown: **Shader Graphs > \<name\>**, or
   drag the `.shadergraph` file from the toolkit's `Shaders/` subfolder).
3. Assign the Material to the `SpriteRenderer` **Material** slot.
4. Adjust properties in the Inspector; animate at runtime via:
   ```csharp
   GetComponent<SpriteRenderer>().material.SetFloat("_PropertyName", value);
   GetComponent<SpriteRenderer>().material.SetColor("_PropertyName", color);
   ```
5. To animate a single instance without affecting shared materials, always access
   `.material` (auto-creates instance) rather than `.sharedMaterial`.

**Key property names per shader** — see full table in
[references/shader-properties.md](references/shader-properties.md).

| Shader | File | Key property | Type |
|---|---|---|---|
| Blur | `ShaderBlur2D/Shaders/Blur.shadergraph` | `_BlurAmount` | float 0–3 |
| Dissolve | `ShaderDissolve2D/Shaders/Dissolve.shadergraph` | `_DissolveAmount` | float 0–1 |
| Outline | `ShaderOutline2D/Shaders/Outline.shadergraph` | `_OutlineThickness` | float 0–3 |
| Outline (multi-color) | `ShaderOutline2D/Shaders/OutlineMultiColors.shadergraph` | `_OutlineThickness` | float 0–3 |
| Pixelate | `ShaderPixelate2D/Shaders/Pixelate.shadergraph` | `_PixelateAmount` | float 0–0.8 |
| Tint | `ShaderTint2D/Shaders/Tint.shadergraph` | `_Tint` | Color (alpha = intensity) |
| TintSolid | `ShaderTint2D/Shaders/TintSolid.shadergraph` | `_Tint` | Color (alpha → 1 = solid fill) |

**Expected result:** SpriteRenderer displays the effect; tweaking the float or color property
changes intensity in real time.

---

### Workflow: Cinematic Bars

**Goal:** Slide black bars in from the top and bottom of the canvas, then remove them.

```csharp
using CodeMonkey.Toolkit.TCinematicBars;

// Slide bars in: target height 150 canvas-px, animate over 1 second
CinematicBars.Show(targetSize: 150f, time: 1f);

// Later: slide bars out
CinematicBars.Hide(time: 1f);
```

- `CinematicBars` **requires a `Canvas` to already exist in the scene.** It finds the first
  `Canvas`, then auto-creates its own child `RectTransform` with two `Image` components under
  it — no prefab needed. If no `Canvas` is present, `Show`/`Hide` log the error
  "No Canvas was found in Scene! CinematicBars needs a Canvas to work." and do nothing, so
  add a Screen Space – Overlay `Canvas` first.
- `targetSize` is in canvas pixels (e.g., 150 for a 1080p Screen Space – Overlay canvas).
- Works on all render pipelines (pure UGUI, no shader).

**Expected result:** Two black Image panels named `CinematicBars_TopBar` and
`CinematicBars_BottomBar` slide in from the edges over `time` seconds. `Hide` animates
them back to zero height.

---

### Workflow: Screen Shatter (BreakTheScreen)

**Goal:** Shatter the screen into physics pieces on a game-over or boss-hit event.

```csharp
using CodeMonkey.Toolkit.TBreakTheScreen;

// Trigger: takes a screenshot, textures mesh fragments, explodes with physics
BreakTheScreen.Spawn();
```

- The `BreakTheScreen` prefab must be in a `Resources/` folder (included with the toolkit).
- Internally calls `TakeScreenshot` → applies the captured `Texture2D` to the shatter
  material's `_BaseMap` property → disables `Camera.main` → enables physics on the
  pre-built fragment GameObjects.
- After the effect, `Camera.main` is disabled. Load a new scene or re-enable the camera
  from your game logic.

**Expected result:** Screen freezes for one frame; the scene camera is replaced by
shattered pieces flying apart under physics forces.

---

### Workflow: Scene Transition Wipe

**Goal:** Use the shader-based wipe effect to cover the screen, load a scene, then reveal it.

```csharp
using CodeMonkey.Toolkit.TShaderTransitionEFfect;
using UnityEngine.SceneManagement;

// Cover screen
ShaderTransitionEffect.Show(onComplete: () => SceneManager.LoadScene("Level2"));

// In Level2's Start():
ShaderTransitionEffect.Hide(onComplete: () => Debug.Log("Scene visible"));
```

- Auto-spawns from Resources (prefab named `ShaderTransitionEffect`); Canvas required.
- The underlying shader (`TransitionEffect.shadergraph`) drives coverage via `_MaskAmount`
  (float 0–1). Swap the greyscale mask `Texture2D` on the prefab's Material to change the
  wipe pattern.
- **Requires URP or HDRP** (Shader Graph asset).

**Expected result:** Wipe animation plays; `onComplete` fires when fully covered or fully
revealed.

---

### Workflow: ZoomShader

**Goal:** Apply a zoom-lens distortion centred on a game object's screen position.

1. Create a Material using `Assets/CodeMonkey/Toolkit/Tools/ZoomShader/Shaders/ZoomShader.shadergraph`.
2. Assign the Material to the SpriteRenderer or a fullscreen Image.
3. Add the `ZoomShaderScreenPosition` component (namespace `CodeMonkey.Toolkit.TZoomShader`)
   to the object whose world position should be the zoom centre.
4. In the Inspector, assign the same Material to its **Material** field.
5. Set `_ZoomAmount` (float) on the Material to control magnification strength.

The component automatically updates `_ObjectScreenPosition` every frame:

```csharp
// ZoomShaderScreenPosition.Update() does this internally — read-only reference:
Vector2 screenPixels = Camera.main.WorldToScreenPoint(transform.position);
screenPixels = new Vector2(screenPixels.x / Screen.width, screenPixels.y / Screen.height);
material.SetVector("_ObjectScreenPosition", screenPixels);
```

**Expected result:** The viewport area around the object's screen position is magnified by
`_ZoomAmount`.

---

### Workflow: Mouse Drawing (DrawMesh and DrawPixels)

**DrawMesh** — freehand vector stroke drawing:

- Add the `DrawMesh` component (namespace `CodeMonkey.Toolkit.TDrawMesh`) to a scene
  GameObject and assign a `drawMeshMaterial` in its Inspector.
- Player draws by clicking and dragging. Tune programmatically:

```csharp
using CodeMonkey.Toolkit.TDrawMesh;

DrawMesh.Instance.SetThickness(0.6f); // default 0.6
DrawMesh.Instance.SetColor(Color.green); // default green
```

- Each mouse-down creates a new `DrawMeshSingle` GameObject mesh stroke.
- Drawing is skipped when the pointer is over a UI element.

**DrawPixels** — 100×100 pixel canvas:

- Add the `DrawPixels` component (namespace `CodeMonkey.Toolkit.TDrawPixels`) to a scene
  object; assign a `colorsTexture` (palette).
- Player left-clicks to paint. Control the brush:

```csharp
using CodeMonkey.Toolkit.TDrawPixels;

DrawPixels.Instance.SetCursorSize(DrawPixels.CursorSize.Small);   // 1 px
DrawPixels.Instance.SetCursorSize(DrawPixels.CursorSize.Medium);  // 3 px
DrawPixels.Instance.SetCursorSize(DrawPixels.CursorSize.Large);   // 7 px

DrawPixels.Instance.SaveImage((Texture2D tex) => {
    System.IO.File.WriteAllBytes(
        Application.dataPath + "/myDrawing.png", tex.EncodeToPNG());
});
```

**Expected result:** Player draws in real time; `SaveImage` callback receives a `Texture2D`
with the painted content.

## Verification

After integration, confirm these signals:

- **Any 2D shader**: SpriteRenderer displays the effect in Play or Edit Mode; changing the
  key float property (e.g., `_BlurAmount`) changes the effect intensity.
- **Outline**: `_OutlineThickness > 0` produces a visible border.
- **CinematicBars**: two Image children named `CinematicBars_TopBar` and
  `CinematicBars_BottomBar` appear inside the Canvas in Play Mode.
- **BreakTheScreen**: `Camera.main` is disabled after `Spawn()`; physics Rigidbody
  fragments become active.
- **ShaderTransitionEffect**: the `_MaskAmount` property on the material changes over time;
  `onComplete` callback fires.
- **DrawMesh**: new `DrawMeshSingle` GameObjects appear in the Hierarchy while drawing.
- **DrawPixels**: `SaveImage` produces a non-null `Texture2D` containing the painted pixels.
- **Pink/Magenta sprite**: shader not supported by the current render pipeline — see Common
  Issues.

## API quick reference

See [references/shader-properties.md](references/shader-properties.md) for per-shader
property tables.

| Entry point | Type | What it does |
|---|---|---|
| `CinematicBars.Show(targetSize, time)` | `static` | Animate black bars in |
| `CinematicBars.Hide(time)` | `static` | Animate black bars out |
| `BreakTheScreen.Spawn()` | `static` | Screenshot → shatter → physics |
| `ShaderTransitionEffect.Show(onComplete)` | `static` | Cover screen with wipe shader |
| `ShaderTransitionEffect.Hide(onComplete)` | `static` | Reveal screen |
| `DrawMesh.Instance.SetThickness(float)` | instance | Set stroke thickness (default 0.6) |
| `DrawMesh.Instance.SetColor(Color)` | instance | Set stroke color (default green) |
| `DrawPixels.Instance.SetCursorSize(CursorSize)` | instance | Small / Medium / Large brush |
| `DrawPixels.Instance.SaveImage(Action<Texture2D>)` | instance | Export grid as Texture2D |
| `ZoomShaderScreenPosition` | component | Auto-updates `_ObjectScreenPosition` on ZoomShader |
| `material.SetFloat("_BlurAmount", v)` | Material API | Blur intensity 0–3 |
| `material.SetFloat("_DissolveAmount", v)` | Material API | Dissolve progress 0–1 |
| `material.SetFloat("_OutlineThickness", v)` | Material API | Outline width 0–3 |
| `material.SetColor("_OutlineColor", c)` | Material API | Outline border color |
| `material.SetFloat("_PixelateAmount", v)` | Material API | Pixelate strength 0–0.8 |
| `material.SetColor("_Tint", c)` | Material API | Tint color; alpha = intensity |
| `material.SetFloat("_ZoomAmount", v)` | Material API | Zoom lens magnification |
| `material.SetVector("_ObjectScreenPosition", v)` | Material API | Zoom centre (normalized 0–1) |

## Common issues

| Symptom | Cause | Fix |
|---|---|---|
| Sprite renders pink or magenta | Shader Graph not supported by Built-in RP | Switch to URP or HDRP: `Edit > Render Pipeline > Universal Render Pipeline > …` |
| Property change via `SetFloat` has no visible effect | Editing `sharedMaterial` affects all instances, or wrong property name | Use `renderer.material` (creates an instance); verify name against [references/shader-properties.md](references/shader-properties.md) |
| `BreakTheScreen.Spawn()` produces a black screen that never clears | `Camera.main` disabled but no scene change or camera re-enable | Load a new scene or re-enable the camera from your own callback logic |
| `CinematicBars` bars not visible | No Canvas in scene | Ensure a `Canvas` with Screen Space – Overlay is present before calling `Show` |
| `DrawMesh` draws nothing when clicking | Pointer is over a UI element | `IsPointerOverUI()` blocks drawing; move any blocking UI elements |
| `ZoomShaderScreenPosition` throws `NullReferenceException` | `Camera.main` is null (no Camera tagged "MainCamera") | Set the MainCamera tag on the camera, or assign a reference manually |
| TransitionEffect wipe pattern is not the expected shape | Default mask texture on the prefab's Material | Replace the `Mask` Texture2D on the ShaderTransitionEffect prefab's Material |
| `BreakTheScreen` or `ShaderTransitionEffect` missing from Resources | Prefab moved or not named exactly `BreakTheScreen` / `ShaderTransitionEffect` | Restore the prefab name and ensure it is inside a folder named exactly `Resources` |

## Boundaries

- **All Shader Graph shaders require URP or HDRP.** See Prerequisites.
- **Material instancing**: use `renderer.material` (not `renderer.sharedMaterial`) when
  animating per-object properties to avoid affecting all sprites with that Material.
- **DrawPixels grid is fixed at 100×100** in `Awake()`. Extend the source to change resolution.
- **DrawMesh strokes persist**: each stroke is a new GameObject; implement your own
  clear/undo logic.
- **BreakTheScreen disables Camera.main**: plan how to re-enable it or load a new scene.
- **Native Unity alternative**: Unity's built-in `Shader`/`Material` API + Shader Graph for
  custom shaders; `RenderTexture` + `ReadPixels` for screen capture; UGUI `Image` panels for
  cinematic bars.
