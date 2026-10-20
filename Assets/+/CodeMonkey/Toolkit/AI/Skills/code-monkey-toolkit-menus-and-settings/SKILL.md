---
name: code-monkey-toolkit-menus-and-settings
description: "Use this skill whenever a user wants to add a front-end shell menu with Code Monkey Toolkit — e.g. 'add a main menu', 'make a title screen', 'add a play/how to play/quit menu', 'add an options menu', 'graphics/quality settings menu', 'let players change resolution', 'add vsync and brightness sliders', 'toggle bloom/vignette at runtime', or 'apply saved graphics settings on startup'. Covers MainMenu (MainMenuUI with Play/HowToPlay/Quit and slide-in sub-containers) and GraphicsSettingsWindow (the GraphicsSettings static API plus a drop-in settings UI prefab and a startup bootstrap that re-applies saved settings). Do NOT use for in-game HUD, runtime popups, tooltips, or dialogs (see code-monkey-toolkit-ui-and-feedback), or for shader/screen visual effects (see code-monkey-toolkit-shaders-and-effects). When in doubt whether a menu or settings request could involve Code Monkey Toolkit, use this skill — Prerequisites shows how to confirm the asset is installed."
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

# Code Monkey Toolkit — Menus & Settings

The Code Monkey Toolkit ships front-end "shell" UI for games and prototypes: a ready-made **Main Menu** (Play / How To Play / Quit with slide-in sub-panels) and a **Graphics Settings Window** (a drop-in options menu plus a `GraphicsSettings` static API that changes resolution, VSync, MSAA, shadows, render scale, and URP post-processing, persisting every choice through `PlayerPrefs`). This skill lets an agent stand up a title screen and an options menu, drive individual settings from code, and re-apply saved settings automatically at startup.

## When to use this skill

- User wants a main menu / title screen with Play, How To Play, and Quit buttons.
- User wants an options or graphics/quality settings menu players can open.
- User wants to change a display setting from code (resolution, VSync, target frame rate, MSAA, shadow distance, render scale).
- User wants to toggle URP post-processing effects (bloom, vignette, motion blur, depth of field) or adjust brightness/gamma at runtime.
- User wants saved graphics settings to be re-applied automatically when the game starts.

**Not for:**
- In-game HUD, runtime popups, tooltips, chat bubbles, or dialogs → `code-monkey-toolkit-ui-and-feedback`
- Shader or screen visual effects (blur, outline, dissolve, cinematic bars) → `code-monkey-toolkit-shaders-and-effects`
- Saving arbitrary game data to a file → `code-monkey-toolkit-utilities` (SaveSystem)
- Character or camera controllers → `code-monkey-toolkit-controllers-and-camera`
- Package installation or initial setup → `code-monkey-toolkit-setup-and-overview`

## Prerequisites

1. Code Monkey Toolkit **1.0.5** installed.
2. Unity **6000.0.60f1+**.
3. **GraphicsSettingsWindow requires the Universal Render Pipeline (URP).** Its post-processing, anti-aliasing, and Volume-effect setters use `UnityEngine.Rendering.Universal` types (`Volume`, `AntialiasingMode`, `ScriptableRendererFeature`). MainMenu is render-pipeline agnostic.
4. **Programmatic install check:**

```csharp
bool mainMenu = typeof(CodeMonkey.Toolkit.TMainMenu.MainMenuUI) != null;
bool graphics = typeof(CodeMonkey.Toolkit.TGraphicsSettings.GraphicsSettings) != null;
Debug.Log((mainMenu && graphics) ? "Menus & Settings found ✓" : "Menus & Settings NOT found");
```

Alternatively confirm the files exist at `Assets/CodeMonkey/Toolkit/Tools/MainMenu/Scripts/MainMenuUI.cs` and `Assets/CodeMonkey/Toolkit/Tools/GraphicsSettingsWindow/Scripts/GraphicsSettings.cs`. If the check fails, import the asset from:
`https://assetstore.unity.com/packages/tools/utilities/code-monkey-toolkit-309282`

## Quick start

Add an options menu that players can open, in three steps:

1. Ensure a `Canvas` with an `EventSystem` exists in the scene (`GameObject > UI > Canvas`).
2. Drag `Assets/CodeMonkey/Toolkit/Tools/GraphicsSettingsWindow/Prefabs/GraphicsSettingsUI.prefab` under the Canvas.
3. Assign the prefab's scene references in the Inspector: the global `Volume`, the target `Camera` (defaults to `Camera.main`), and the URP ambient-occlusion `ScriptableRendererFeature` if used.

**Observable result:** Entering Play Mode shows a settings panel of toggles, sliders, and dropdowns. Changing any control updates the display and persists it via `PlayerPrefs`, so the choice survives a restart.

## Workflows

### Workflow: Add a Main Menu

**Goal:** Put a title screen with Play / How To Play / Quit in the scene.

**Steps:**
1. The MainMenu tool has **no prefab under its folder** — it is configured in its demo scene. Open `Assets/CodeMonkey/Toolkit/Tools/MainMenu/Demo/Demo.unity` and copy the `MainMenu` Canvas hierarchy into your scene, **or** build a Canvas and add the components below.
2. Add `MainMenuUI` (namespace `CodeMonkey.Toolkit.TMainMenu`) to the menu root and wire its serialized fields:
   - `playButton`, `howToPlayButton`, `quitButton` (`Button`).
   - `playContainer` (`MainMenuUI_PlayContainer`) and `howToPlayContainer` (`MainMenuUI_HowToPlayContainer`).
3. On the Play sub-panel add `MainMenuUI_PlayContainer` and assign `startAnchoredPosition` (Vector2), `continueButton`, `newGameButton`, `loadGameButton`, `backButton`.
4. On the How-To-Play sub-panel add `MainMenuUI_HowToPlayContainer` and assign `startAnchoredPosition` and `backButton`.
5. Quit shows a confirmation via `BlockerUI` + `QuestionDialogUI` (both from the toolkit) — no extra wiring needed. Loading the game scene is delegated to SceneLoadingSystem (see `code-monkey-toolkit-gameplay-systems`).

**Expected result:** Play and How To Play slide their sub-containers in (call `Show()` / `Hide()` on the containers to toggle them from code); Quit prompts a Yes/No confirmation before exiting.

---

### Workflow: Add a Graphics/Options settings menu (prefab)

**Goal:** Give players a ready-made options panel.

**Steps:**
1. Confirm the project uses **URP** (see Prerequisites).
2. Add a `Canvas` + `EventSystem` if none exists.
3. Drag `GraphicsSettingsUI.prefab` under the Canvas.
4. On the prefab's `GraphicsSettingsMenuUI` component, assign the scene references: `globalVolume` (`Volume`), `targetCamera` (`Camera`), and `ambientOcclusionFeature` (`ScriptableRendererFeature`) if you use URP AO. The toggles/sliders/dropdowns are pre-wired inside the prefab.

**Expected result:** A functional settings panel. Every control calls the matching `GraphicsSettings` static setter and persists to `PlayerPrefs`.

---

### Workflow: Apply a setting from code (GraphicsSettings static API)

**Goal:** Change a display or post-processing setting directly.

**Steps:**
1. Add `using CodeMonkey.Toolkit.TGraphicsSettings;`
2. Call display setters (no scene refs needed):
   ```csharp
   GraphicsSettings.SetVSync(true);
   GraphicsSettings.SetTargetFrameRate(60);
   GraphicsSettings.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
   GraphicsSettings.SetShadowDistance(50f);
   GraphicsSettings.SetRenderScale(0.9f);
   ```
3. Post-processing / Volume effects require a URP `Volume` (and some a `Camera`):
   ```csharp
   using UnityEngine.Rendering;
   Volume volume = Object.FindFirstObjectByType<Volume>();
   GraphicsSettings.SetBloomEnabled(volume, true);
   GraphicsSettings.SetBloomIntensity(volume, 0.5f);
   GraphicsSettings.SetBrightness(volume, 1.1f);
   GraphicsSettings.SetPostProcessingEnabled(Camera.main, true);
   ```
4. Read current values back with the matching `Get...` methods (e.g. `GraphicsSettings.IsVSyncEnabled()`, `GraphicsSettings.GetRenderScale()`). Every setter persists via `PlayerPrefs`.

**Expected result:** The setting changes immediately and is remembered on the next launch. See `references/graphics-settings-api.md` for the full method list.

---

### Workflow: Auto-apply saved settings at startup

**Goal:** Re-apply the player's saved settings automatically when the game boots.

**Steps:**
1. Drag `Assets/CodeMonkey/Toolkit/Tools/GraphicsSettingsWindow/Prefabs/GraphicsSettingsBootstrap.prefab` into your first/boot scene.
2. On its `GraphicsSettingsBootstrap` component assign `globalVolume` (`Volume`), `targetCamera` (`Camera`, falls back to `Camera.main` if left null), and `ambientOcclusionFeature` (`ScriptableRendererFeature`) if used.
3. On start it calls `GraphicsSettings.LoadAndApplyAll(volume, camera, ambientOcclusionFeature)`, which reads every saved `PlayerPrefs` value and applies it.

**Expected result:** Saved graphics choices take effect before the player sees the game, with no manual calls.

## Verification

- `typeof(CodeMonkey.Toolkit.TMainMenu.MainMenuUI)` resolves without error.
- `typeof(CodeMonkey.Toolkit.TGraphicsSettings.GraphicsSettings)` resolves without error.
- After adding `GraphicsSettingsUI.prefab`, the Canvas contains a `GraphicsSettingsMenuUI` component with its `globalVolume`/`targetCamera` fields assigned.
- Calling `GraphicsSettings.SetVSync(true)` then `GraphicsSettings.IsVSyncEnabled()` returns `true`.
- After a `GraphicsSettings` setter runs, the corresponding `PlayerPrefs` key is present (`PlayerPrefs.HasKey(...)`).
- The MainMenu root has a `MainMenuUI` component whose `playButton`, `howToPlayButton`, `quitButton`, `playContainer`, and `howToPlayContainer` fields are non-null.
- No Console errors when entering Play Mode with either menu present.

## API quick reference

| Entry point | Type | What it does |
|---|---|---|
| `CodeMonkey.Toolkit.TMainMenu.MainMenuUI` | MonoBehaviour | Title menu root; wires Play/HowToPlay/Quit buttons + the two containers |
| `MainMenuUI_PlayContainer.Show()` / `Hide()` | Method | Slide the Play sub-panel in/out |
| `MainMenuUI_HowToPlayContainer.Show()` / `Hide()` | Method | Slide the How-To-Play sub-panel in/out |
| `CodeMonkey.Toolkit.TGraphicsSettings.GraphicsSettings` | Static class | All display + post-processing setters/getters; persists to `PlayerPrefs` |
| `GraphicsSettings.SetVSync(bool)` / `IsVSyncEnabled()` | Static | VSync on/off |
| `GraphicsSettings.SetResolution(int, int, FullScreenMode)` | Static | Set resolution + fullscreen mode |
| `GraphicsSettings.SetTargetFrameRate(int)` | Static | Cap frame rate |
| `GraphicsSettings.SetRenderScale(float)` | Static | URP render scale |
| `GraphicsSettings.SetShadowDistance(float)` | Static | Shadow draw distance |
| `GraphicsSettings.SetBloomEnabled(Volume, bool)` | Static | Toggle URP bloom (needs a `Volume`) |
| `GraphicsSettings.SetBrightness(Volume, float)` | Static | Adjust brightness (needs a `Volume`) |
| `GraphicsSettings.LoadAndApplyAll(Volume, Camera, ScriptableRendererFeature)` | Static | Apply every saved setting at once |
| `GraphicsSettingsBootstrap` | MonoBehaviour | Calls `LoadAndApplyAll` on start; drop into the boot scene |
| `GraphicsSettingsMenuUI` | MonoBehaviour | The prefab's UI controller (toggles/sliders/dropdowns) |

The full 40-method `GraphicsSettings` list (all post-processing, anti-aliasing, shadow-cascade, and getter methods) lives in `references/graphics-settings-api.md`. Read it when the common setters above do not cover the requested setting.

## Common issues

| Symptom | Cause | Fix |
|---|---|---|
| Post-processing/bloom setters do nothing | Project is Built-in, not URP | GraphicsSettingsWindow requires URP; switch pipeline or skip Volume effects |
| `NullReferenceException` from a Volume setter | No `Volume` passed / none in scene | Add a global `Volume` and pass it; assign `globalVolume` on the prefab |
| Settings not restored on launch | No `GraphicsSettingsBootstrap` in the boot scene | Add the bootstrap prefab and assign its Volume/Camera |
| Main menu buttons do nothing | Serialized Button/container fields left unassigned | Assign every field on `MainMenuUI` and the two containers |
| Menu invisible / clicks ignored | No `Canvas` or no `EventSystem` | Add `GameObject > UI > Canvas` (adds an EventSystem) |
| Quit does not prompt | BlockerUI/QuestionDialog prefabs missing from Resources | Keep the toolkit's `Resources` folders intact (see ui-and-feedback) |
| `CS0104: 'GraphicsSettings' is an ambiguous reference` | `using UnityEngine.Rendering;` (needed for `Volume`) collides with Unity's own `GraphicsSettings` | Add an alias: `using GraphicsSettings = CodeMonkey.Toolkit.TGraphicsSettings.GraphicsSettings;`, or fully-qualify the toolkit type |

## Boundaries

- **GraphicsSettingsWindow requires URP.** Post-processing, anti-aliasing mode/quality, and Volume-based effects (bloom, vignette, motion blur, depth of field, brightness, gamma) are unavailable on the Built-in pipeline.
- **MainMenu ships no prefab under its folder** — it is configured in its demo scene (`MainMenu/Demo/Demo.unity`). Copy that hierarchy or build the Canvas and wire the components manually.
- **MainMenu depends on sibling tools** — quit confirmation uses `BlockerUI` + `QuestionDialogUI`, and starting the game uses SceneLoadingSystem; keep those tools present.
- **Settings persist only via `PlayerPrefs`** — there is no per-profile or cloud storage; clearing PlayerPrefs resets all settings.
- These tools are the game's front-end shell only. For in-game HUD or runtime dialogs use `code-monkey-toolkit-ui-and-feedback`.
