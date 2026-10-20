---
name: code-monkey-toolkit-setup-and-overview
description: "Use this skill whenever the user mentions the Code Monkey Toolkit, wants to confirm it is installed, wants to know what it can do, or asks how to get started with it — e.g. 'is the code monkey toolkit set up', 'what tools does this toolkit have', 'open the toolkit window', 'how do I use code monkey toolkit', 'where are the demo scenes'. Covers verifying the asset is installed, opening the in-editor Toolkit window at Tools > Code Monkey Toolkit, browsing tools and their demo scenes, understanding the project folder layout, and routing to the correct capability skill. Do NOT use this skill to actually implement a feature — for that, pick the matching sibling skill (see the skill index below), such as code-monkey-toolkit-ui-and-feedback or code-monkey-toolkit-grid-systems. When in doubt whether any request could involve Code Monkey Toolkit, start here — Prerequisites shows how to confirm the asset is installed and the index points you to the right skill."
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

# Set Up and Explore the Code Monkey Toolkit

The Code Monkey Toolkit is a collection of 56+ drop-in Tools and Elements (UI helpers, front-end menus and graphics settings, gameplay systems, grid systems, character controllers, camera rigs, shaders, and code utilities) that live under a single asset folder and share the `CodeMonkey.Toolkit` namespace. This skill lets an agent confirm the asset is installed, open its in-editor browser window, locate any tool's source and demo scene, understand where things live, and route the user's request to the correct capability skill.

The toolkit also ships **AI Skills** for agents (the "Agent AI Skills" tool) — the SKILL.md files under `Assets/CodeMonkey/Toolkit/AI/Skills/`. Those are the very skills indexed below; this is the entry point that routes to them.

## When to use this skill

- The user names the asset: "is the Code Monkey Toolkit installed?", "set up code monkey toolkit", "open the toolkit".
- The user asks what the toolkit offers: "what tools are in here?", "does this have an interaction system?", "list the code monkey tools".
- The user wants orientation before real work: "where are the demo scenes?", "where's the source for the grid system?".
- As a first step before any other Code Monkey Toolkit skill, to run the install check in Prerequisites.

Not for:
- Implementing a specific feature — route to the matching sibling skill (see the Skill index).
- Runtime UI / popups / dialogs → `code-monkey-toolkit-ui-and-feedback`.
- Grids / tiles / hex → `code-monkey-toolkit-grid-systems`.
- Health, interaction, keys/doors, tick, scene loading → `code-monkey-toolkit-gameplay-systems`.
- Character controllers and cameras → `code-monkey-toolkit-controllers-and-camera`.
- Sprite shaders and screen effects → `code-monkey-toolkit-shaders-and-effects`.
- Main menu / title screen and graphics/options settings → `code-monkey-toolkit-menus-and-settings`.
- Timers, web requests, screenshots, save system, random data, FPS counter, mouse position, GameAssets, OpenAI → `code-monkey-toolkit-utilities`.

## Prerequisites

- Unity 6000.0.60f1 or newer (verified against the Asset Store listing; the project this skill was authored in runs Unity 6).
- Render pipeline: Built-in, URP, or HDRP are all supported by the package. Note: the Shader Graph based sprite shaders (`code-monkey-toolkit-shaders-and-effects`) specifically require URP or HDRP.
- Input: the project uses the new Input System (`com.unity.inputsystem`); the included character controllers read input through it.

Programmatic install check — run any one of these before proceeding:

1. Folder check: confirm the folder `Assets/CodeMonkey/Toolkit` exists.
2. Menu check: confirm the menu item `Tools > Code Monkey Toolkit` is present.
3. Type check (most reliable) — run in an Editor context:

```csharp
using UnityEngine;
using UnityEditor;

internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
    {
        System.Type t = System.Type.GetType("CodeMonkey.Toolkit.CodeMonkeyToolkitSO, Assembly-CSharp")
                        ?? System.Type.GetType("CodeMonkey.Toolkit.MainWindowToolkit, Assembly-CSharp-Editor");
        bool installed = t != null
            || System.IO.Directory.Exists(Application.dataPath + "/CodeMonkey/Toolkit");
        result.Log(installed
            ? "Code Monkey Toolkit: INSTALLED"
            : "Code Monkey Toolkit: NOT FOUND");
    }
}
```

If the check fails, tell the user: "The Code Monkey Toolkit does not appear to be installed in this project. Install it from the Unity Asset Store (https://assetstore.unity.com/packages/tools/utilities/code-monkey-toolkit-309282) and re-import, then try again." Do not attempt to author code against tools that are not present.

## Quick start

Open the in-editor Toolkit browser to see every tool, its description, tags, and demo scene:

1. In the Unity Editor menu bar, choose `Tools > Code Monkey Toolkit`.
2. In the window, click the tool-list button, then use the search field or a tag filter (Utility, UI, System, Controller, Grid, Shader, Polish, Mechanic).
3. Click any tool to open its detail page; use the demo-scene button there to open that tool's demo scene.

Expected observable result: a "Code Monkey Toolkit" editor window opens, listing tools loaded from `CodeMonkeyToolkitSO.toolSOList`, each with a name, description, and tags.

## Workflows

### Workflow: Confirm the asset is installed and report its version

**Goal:** Verify presence and report the installed version before doing any work.

**Steps:**
1. Run the install check from Prerequisites.
2. Read the version from the config asset `Assets/CodeMonkey/Toolkit/EditorWindow/ScriptableObjects/CodeMonkeyToolkitSO.asset` (field `currentVersion`), or from the version label shown in the Toolkit window.

**Expected result:** You can state whether the asset is installed and its version (1.0.5 at time of authoring).

### Workflow: Discover the right tool for a request

**Goal:** Map a user request to a specific tool and the skill that covers it.

**Steps:**
1. Open `Tools > Code Monkey Toolkit` and search by keyword, OR read the per-tool description assets under `Assets/CodeMonkey/Toolkit/EditorWindow/ScriptableObjects/Tools/*.asset` (each `ToolSO` has `nameString`, `description`, `tags`).
2. Match the tool to its capability skill using the Skill index below.
3. Hand off to that skill for implementation.

**Expected result:** A named tool and the sibling skill that documents its API.

### Workflow: Open a tool's demo scene and source

**Goal:** Inspect working example code for a tool.

**Steps:**
1. Each tool lives at `Assets/CodeMonkey/Toolkit/Tools/<ToolName>/` with a `Scripts/` folder (the API) and usually a `Demo/` folder with a `Demo.cs` and a demo scene.
2. Open the demo scene from the Toolkit window's tool detail page, or by locating the `.unity` scene under the tool folder.

**Expected result:** The demo scene opens and its `Demo.cs` shows the minimal usage pattern for that tool.

## Verification

- The install check logs "INSTALLED".
- The menu `Tools > Code Monkey Toolkit` opens a window titled "Code Monkey Toolkit".
- `Assets/CodeMonkey/Toolkit/EditorWindow/ScriptableObjects/CodeMonkeyToolkitSO.asset` exists and `currentVersion` reads a value (e.g. `1.0.5`).
- The Console shows no errors after opening the window.

## API quick reference

| Entry point | Type | What it does |
| --- | --- | --- |
| `Tools > Code Monkey Toolkit` | Editor menu item | Opens the Toolkit browser window (`MainWindowToolkit.ShowWindow`). |
| `CodeMonkey.Toolkit.MainWindowToolkit` | EditorWindow class | The in-editor tool browser (Editor-only). |
| `CodeMonkey.Toolkit.CodeMonkeyToolkitSO` | ScriptableObject | Master config asset; holds `currentVersion` and `toolSOList`. |
| `CodeMonkey.Toolkit.ToolSO` | ScriptableObject | Per-tool metadata: `nameString`, `description`, `tags`, `demoScene`. |
| `CodeMonkeyToolkitSO.GetCodeMonkeyToolkitSO()` | static method | Loads the master config asset via `AssetDatabase`. |

## Skill index

| Skill | Use it for |
| --- | --- |
| `code-monkey-toolkit-setup-and-overview` | This skill: install check, orientation, routing. |
| `code-monkey-toolkit-ui-and-feedback` | Chat bubbles, text popups, typewriter text, tooltips, question/input dialogs, blocker UI, transitions, draggable windows, error detector. |
| `code-monkey-toolkit-grid-systems` | GridSystem, GridSystemXY, GridSystemHex — build and query grids of data. |
| `code-monkey-toolkit-gameplay-systems` | Health, interaction (proximity/look-at), keys & doors, tick system, scene loading, day-night cycle, simple destruction. |
| `code-monkey-toolkit-controllers-and-camera` | First-person, top-down (2D/3D) controllers; basic camera controllers; look-at-camera. |
| `code-monkey-toolkit-shaders-and-effects` | 2D sprite shaders (outline, blur, dissolve, pixelate, tint), zoom shader, transitions, cinematic bars, break-the-screen, draw mesh/pixels. |
| `code-monkey-toolkit-menus-and-settings` | Main menu / title screen (MainMenu) and graphics/options settings menu + `GraphicsSettings` API (GraphicsSettingsWindow, URP). |
| `code-monkey-toolkit-utilities` | Timers (FunctionTimer/Periodic/Updater), web requests, screenshots, SaveSystem (JSON save/load), random data, FPS counter, mouse position, GameAssets, MonoBehaviour/Pointer hooks, OpenAI API. |

## Common issues

| Symptom | Cause | Fix |
| --- | --- | --- |
| `Tools > Code Monkey Toolkit` menu missing | Asset not imported, or scripts failed to compile | Re-import the package; fix any Console compile errors so `Assembly-CSharp-Editor` builds. |
| "Cannot find CodeMonkeyToolkitSO!" in Console | The master config asset was deleted or moved out of `Assets/` | Restore `CodeMonkeyToolkitSO.asset` from the package. |
| Window opens empty / no tools listed | `toolSOList` on the config asset is empty | Re-import the package so the `ToolSO` references are restored. |
| Tool references a namespace that won't resolve | Wrong namespace | All runtime tools are in `CodeMonkey.Toolkit`; add `using CodeMonkey.Toolkit;`. |

## Boundaries

- This skill is for orientation and routing only; it does not implement gameplay features — the sibling skills do.
- The Toolkit window also fetches online content (latest videos, update checks, question-of-the-day) from `unitycodemonkey.com`. Those network features are publisher infrastructure, not part of any customer workflow, and require internet access; they are out of scope for these skills.
- The bundled "Shop Simulator" prototype under `Assets/CodeMonkey/Toolkit/Prototypes/` is a sample game, not a reusable tool, and is not covered by these skills.
- For anything the toolkit does not include, fall back to native Unity systems.
