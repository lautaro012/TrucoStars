---
name: code-monkey-toolkit-ui-and-feedback
description: "Use this skill whenever the user asks to show runtime messages, popups, or interactive dialogs using Code Monkey Toolkit — e.g. 'show a chat bubble over the npc', 'pop up damage numbers', 'typewriter text effect', 'add a yes/no dialog', 'ask the player their name', 'make this window draggable', 'block clicks behind a dialog', 'show a tooltip on hover', 'add a scene transition wipe', or 'add a virtual cursor'. Covers ChatBubble, ChatBubble3D, TextPopupWorld, TextPopupUI, TextWriter, Tooltip, QuestionDialog, InputWindow, BlockerUI, TransitionEffect, ResetUIRectTransform, VirtualCursor, DragWindow, and ErrorDetector. Does NOT cover shader visual effects (blur, outline, dissolve, pixelate, cinematic bars, screen shatter) — use code-monkey-toolkit-shaders-and-effects. Does NOT cover grid or gameplay systems — see code-monkey-toolkit-grid-systems or code-monkey-toolkit-gameplay-systems. When in doubt, use this skill — Prerequisites shows how to confirm the asset is installed."
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

# Code Monkey Toolkit — UI and Player Feedback

Runtime UI helpers and feedback components that ship with Code Monkey Toolkit. Most tools
auto-instantiate from a pre-configured prefab in a `Resources/` folder when their static
method is first called — no manual prefab wiring required at call sites. A Canvas must exist
in the scene for Canvas-based tools; world-space tools (ChatBubble, TextPopupWorld) work
without one.

## When to use this skill

- Spawning a speech or emotion bubble above a character (`ChatBubble`, `ChatBubble3D`)
- Floating damage numbers or status text in world or screen space (`TextPopupWorld`, `TextPopupUI`)
- Animating text letter-by-letter, typewriter-style (`TextWriter`)
- Showing a mouse-following tooltip on pointer enter (`TooltipUI`, `TooltipOverOut`)
- Asking the player a yes/no or ok/cancel question (`QuestionDialogUI`)
- Prompting the player to type a name or number (`InputWindowUI`)
- Blocking UI clicks behind a modal dialog (`BlockerUI`)
- Fading/wiping the screen for scene transitions (`ShaderTransitionEffect`)
- Snapping a UI RectTransform to a known position on Play (`ResetUIRectTransform`)
- Adding a software cursor for gamepad UI navigation (`VirtualCursorUI`)
- Making a UI window draggable by its title bar (`DragWindow`)
- Automatically surfacing runtime errors to the player (`ErrorDetectorUI`)

**Not for:**
- Blur, outline, dissolve, pixelate, tint, cinematic bars, screen-shatter shaders →
  use `code-monkey-toolkit-shaders-and-effects`
- Grid systems, health systems, scene loading →
  use `code-monkey-toolkit-grid-systems`, `code-monkey-toolkit-gameplay-systems`
- Camera controllers → use `code-monkey-toolkit-controllers-and-camera`

## Prerequisites

- **Code Monkey Toolkit 1.0.5+** installed from the Asset Store.
- **Unity 6000.0.60f1+**
- **TextMeshPro** (bundled with Unity 6; required by ChatBubble, TextPopup, Tooltip,
  QuestionDialog, InputWindow).
- For `VirtualCursorUI`: **Input System** package with `VirtualMouseInput` component
  available.
- For `ShaderTransitionEffect`: URP or HDRP (the underlying shader is a Shader Graph).

**Programmatic install check** — paste into any `Start()`:

```csharp
#if UNITY_EDITOR
var cmType = System.Type.GetType(
    "CodeMonkey.Toolkit.TChatBubble.ChatBubble, Assembly-CSharp");
if (cmType == null)
    Debug.LogError("Code Monkey Toolkit not found. Install from " +
        "https://assetstore.unity.com/packages/tools/utilities/code-monkey-toolkit-309282");
#endif
```

If the check fails, import the package via the Package Manager, then re-open the scene.

## Quick start

**Goal:** Show a chat bubble above an NPC that auto-destroys after 6 seconds.

1. Ensure a `Canvas` exists in the scene (or add `GameObject > UI > Canvas`).
2. The `ChatBubble` prefab ships inside a `Resources/` folder in the toolkit — no extra
   setup needed.
3. Add or modify a MonoBehaviour on the NPC:

```csharp
using CodeMonkey.Toolkit.TChatBubble;
using UnityEngine;

public class NpcGreeting : MonoBehaviour {
    private void Start() {
        // parent   = this NPC transform; bubble is parented and moves with it
        // localPosition offsets the bubble above the character's head
        ChatBubble.Create(
            parent:       transform,
            localPosition: new Vector3(0.2f, 1.0f, 0f),
            iconType:     ChatBubble.IconType.Happy,
            text:         "Hello traveler!",
            scale:        0.1f,   // world-space scale; 0.1f suits typical 2D sprite games
            destroyTimer: 6f      // auto-destroyed after 6 seconds
        );
    }
}
```

4. Enter Play Mode. A speech bubble appears above the NPC, types out the text via the
   TextWriter effect at 0.03 s/character, then destroys itself after 6 seconds.

## Workflows

### Workflow: Chat Bubbles

**Goal:** Spawn a speech or emotion bubble on a 2D or 3D character.

**Steps:**
1. For 2D (SpriteRenderer-based scene): call
   `ChatBubble.Create(...)` — namespace `CodeMonkey.Toolkit.TChatBubble`.
2. For 3D (MeshRenderer / world space): call
   `ChatBubble3D.Create(...)` — namespace `CodeMonkey.Toolkit.TChatBubble3D`.
3. Both methods have identical signatures. Pass the character's `Transform` as `parent`;
   the bubble is parented and follows it.
4. `IconType` options: `Happy`, `Neutral`, `Angry`.
5. Default `scale = 1f` works for 3D world units; for 2D pixel-perfect scenes use
   `scale = 0.1f` (matches the official demos at `Tools/ChatBubble/Demo/Demo.cs`).
6. The text types out internally via `TextWriter.AddWriter` at 0.03 s/character.
7. Returns the instance; call `.DestroySelf()` to dismiss early.

**Expected result:** A resizing speech bubble appears at the offset position, types the text,
waits `destroyTimer` seconds, then destroys itself.

---

### Workflow: Text Popups and Typewriter Effect

**Goal:** Show floating labels in world or screen space, or animate existing text.

**TextPopupWorld** (world space, faces camera via LookAt):

```csharp
using CodeMonkey.Toolkit.TTextPopup;

TextPopupWorld.Create(
    worldPosition: transform.position + Vector3.up * 0.5f,
    text:          "+100",
    scale:         0.1f,
    destroyTimer:  3f
);
```

**TextPopupUI** (Canvas screen-space):

```csharp
using CodeMonkey.Toolkit.TTextPopup;

TextPopupUI.Create(
    anchoredPosition: new Vector2(0f, 0f),
    text:             "Critical Hit!",
    scale:            2f,
    destroyTimer:     3f
    // convertPositionPivotCenterToLowerLeftCorner: true (default)
);
```

Pass `convertPositionPivotCenterToLowerLeftCorner: false` if you already have canvas-space
coordinates; the default converts from screen-center origin.

**TextWriter** (typewriter on any TMP or Text component):

```csharp
using CodeMonkey.Toolkit.TTextWriter;

// Works with TextMeshProUGUI, TextMeshPro, TextMesh, or legacy Text
TextWriter.AddWriter(
    textMeshProUGUI:        myLabel,
    textToWrite:            "Hello World!",
    timePerCharacter:       0.05f,
    withInvisibleCharacters: true,   // reserves layout space immediately
    useUnscaledDeltaTime:   false,
    onComplete:             () => Debug.Log("Done!")
);
// Stop early: TextWriter.RemoveWriter(myLabel)
```

**Expected result:** Text appears character-by-character; floaters auto-destroy after
`destroyTimer` seconds.

---

### Workflow: Dialogs

**Goal:** Ask the player a yes/no question or prompt for text input.

**QuestionDialogUI** — auto-spawns from Resources, Canvas required:

```csharp
using CodeMonkey.Toolkit.TQuestionDialog;
using CodeMonkey.Toolkit.TBlockerUI;

// Recommended: show BlockerUI first to block clicks behind the dialog
BlockerUI.Show();
QuestionDialogUI.ShowQuestionYesNo(
    questionText: "Are you sure you want to quit?",
    noAction:     () => { BlockerUI.Hide(); },
    yesAction:    () => { BlockerUI.Hide(); Application.Quit(); }
);
// Variants: ShowQuestionOkCancel(q, cancelAct, okAct)
//           ShowQuestionOk(q, okAct)  ← single-button info dialog
```

**InputWindowUI** — prompt for a string or integer:

```csharp
using CodeMonkey.Toolkit.TInputWindow;
using CodeMonkey.Toolkit.TBlockerUI;

BlockerUI.Show();
InputWindowUI.Show(
    titleString:     "Enter your name",
    inputString:     "",   // pre-fill text
    validCharacters: InputWindowUI.ALPHABET_UPPER_CASE
                     + InputWindowUI.ALPHABET_LOWER_CASE,
    characterLimit:  20,
    onCancel: () => BlockerUI.Hide(),
    onOk:     (string name) => { BlockerUI.Hide(); Debug.Log("Name: " + name); }
);
// Integer overload:
// InputWindowUI.Show("Score target", defaultInt: 0, onCancel, (int v) => { … })
```

Constants available: `InputWindowUI.ALPHABET_LOWER_CASE`, `ALPHABET_UPPER_CASE`,
`NUMBERS`.

Keyboard shortcuts: Enter/Numpad-Enter → OK; Escape → Cancel.

**Expected result:** Modal dialog or input box appears; callbacks fire on button press or
keyboard shortcut; the dialog hides itself automatically.

---

### Workflow: Scene Transition

**Goal:** Wipe the screen to cover it for a scene load, then reveal the new scene.

```csharp
using CodeMonkey.Toolkit.TShaderTransitionEFfect;
using UnityEngine.SceneManagement;

// Cover screen, then load scene
ShaderTransitionEffect.Show(onComplete: () => SceneManager.LoadScene("Level2"));

// In Level2's Start() or any Awake():
ShaderTransitionEffect.Hide(onComplete: () => Debug.Log("Fully visible"));
```

- Auto-spawns from Resources (prefab named exactly `ShaderTransitionEffect`); Canvas required.
- Animation speed is `changeSpeed` on the prefab (default 6).
- The underlying shader (`TransitionEffect.shadergraph`) uses property `_MaskAmount` (0–1)
  internally; you can swap the greyscale mask texture on the prefab's Material to change the
  wipe pattern.
- **Requires URP or HDRP** (Shader Graph). See `code-monkey-toolkit-shaders-and-effects`
  for the shader details.

**Expected result:** Screen wipes with the configured pattern; `onComplete` fires when the
animation reaches 0 (transparent) or 1 (fully covered).

---

### Workflow: UI Helper Components

**Goal:** Add tooltips, draggable windows, virtual cursor, auto-position, or error overlay.

**TooltipUI + TooltipOverOut** (namespace `CodeMonkey.Toolkit.TTooltip`):
- Auto-spawns from Resources on first `Show`. Canvas required.
- Static API: `TooltipUI.Show("Sword +5 ATK")` / `TooltipUI.Hide()`.
- Or: add `TooltipOverOut` component to any UI element; set **Tooltip Message** in Inspector.
  No code required. Use `SetTooltipMessage(string)` to change at runtime.

**DragWindow** (namespace `CodeMonkey.Toolkit.TDragWindow`):
- Add `DragWindow` component to the title-bar child of a window panel.
- Inspector fields: **Drag Rect Transform** (auto-resolved to `transform.parent` if null),
  **Canvas** (auto-resolved), **Set As Last Sibling On Press**.
- Implements `IDragHandler` and `IPointerDownHandler`; no code required.

**VirtualCursorUI** (namespace `CodeMonkey.Toolkit.TVirtualCursor`):
- Requires the **Input System** package.
- Add `VirtualCursorUI` and `VirtualMouseInput` (Input System) to a cursor sprite child inside
  the Canvas.
- The component keeps the sprite on top each frame and clamps it to screen bounds.

**ResetUIRectTransform** (namespace `CodeMonkey.Toolkit.TResetUIRectTransform`):
- Add component to any UI GameObject.
- Set **Anchored Position** and **Size Delta** in Inspector; values are applied in `Awake()`,
  overriding Editor-time repositioning.

**ErrorDetectorUI** (namespace `CodeMonkey.Toolkit.TErrorDetector`):
- Drag the `ErrorDetectorUI` prefab onto your Canvas.
- Subscribes to `Application.logMessageReceived`; auto-shows on `LogType.Error` or
  `LogType.Exception`.
- Deduplicates identical error messages; player can close the panel or copy text to clipboard.

**Expected result:** Each component is self-contained; no initialization code required beyond
adding the component or prefab.

## Verification

After integration, confirm these signals:

- **ChatBubble/ChatBubble3D**: `ChatBubble(Clone)` or `ChatBubble3D(Clone)` appears in
  Hierarchy as a child of the `parent` Transform in Play Mode.
- **TextPopupWorld/TextPopupUI**: instance appears in Hierarchy then is destroyed after
  `destroyTimer`.
- **TextWriter**: text component updates one character at a time; a hidden `TextWriter`
  GameObject is created in the scene root automatically.
- **QuestionDialogUI/InputWindowUI**: `QuestionDialogUI(Clone)` / `InputWindowUI(Clone)`
  becomes active inside the Canvas when `Show` is called, inactive after button click.
- **BlockerUI**: `BlockerUI(Clone)` is at the top of Canvas sibling order while shown.
- **ShaderTransitionEffect**: `ShaderTransitionEffect(Clone)` Image opacity changes over
  time; `onComplete` callback fires.
- **TooltipUI**: tooltip follows mouse; hides when cursor leaves the element.
- **ErrorDetectorUI**: run `Debug.LogError("test")` in Play Mode — the error panel appears.

## API quick reference

| Entry point | Type | What it does |
|---|---|---|
| `ChatBubble.Create(parent, localPos, iconType, text, scale, destroyTimer)` | `static` | Spawn 2D speech bubble |
| `ChatBubble3D.Create(parent, localPos, iconType, text, scale, destroyTimer)` | `static` | Spawn 3D speech bubble |
| `TextPopupWorld.Create(worldPos, text, scale, destroyTimer)` | `static` | Floating world-space label |
| `TextPopupUI.Create(anchoredPos, text, scale, destroyTimer, convertPivot)` | `static` | Floating Canvas label |
| `TextWriter.AddWriter(tmp, text, timePerChar, invisible, unscaled, onComplete)` | `static` | Typewriter on any text component |
| `TextWriter.RemoveWriter(tmp)` | `static` | Stop active typewriter |
| `TooltipUI.Show(string)` | `static` | Show mouse-following tooltip |
| `TooltipUI.Show(Func<string>)` | `static` | Dynamic tooltip text |
| `TooltipUI.Hide()` | `static` | Hide tooltip |
| `TooltipOverOut.SetTooltipMessage(string)` | instance | Change tooltip message at runtime |
| `QuestionDialogUI.ShowQuestionYesNo(q, noAct, yesAct)` | `static` | Yes / No dialog |
| `QuestionDialogUI.ShowQuestionOkCancel(q, cancelAct, okAct)` | `static` | Ok / Cancel dialog |
| `QuestionDialogUI.ShowQuestionOk(q, okAct)` | `static` | Single-button info dialog |
| `InputWindowUI.Show(title, input, validChars, limit, onCancel, onOk)` | `static` | String input dialog |
| `InputWindowUI.Show(title, defaultInt, onCancel, onOk)` | `static` | Integer input dialog |
| `InputWindowUI.IsVisible()` | `static` | Poll visibility |
| `BlockerUI.Show()` | `static` | Show click-blocking overlay |
| `BlockerUI.Hide()` | `static` | Hide click-blocking overlay |
| `ShaderTransitionEffect.Show(onComplete)` | `static` | Cover screen with wipe shader |
| `ShaderTransitionEffect.Hide(onComplete)` | `static` | Reveal screen |
| `DragWindow` | component | Drag a RectTransform by pointer |
| `ResetUIRectTransform` | component | Reset position/size in Awake |
| `VirtualCursorUI` | component | Software cursor for gamepad |
| `ErrorDetectorUI` | component | Auto error overlay on LogType.Error |

## Common issues

| Symptom | Cause | Fix |
|---|---|---|
| "Could not find ChatBubble in Resources" | Prefab not in a folder named exactly `Resources` | Verify `Resources/ChatBubble.prefab` exists in the toolkit; check the folder name |
| "No Canvas was found in Scene!" | Canvas missing from the active scene | Add `GameObject > UI > Canvas` before calling any UI static method |
| Dialog buttons unresponsive | `BlockerUI` left visible, intercepting clicks | Always call `BlockerUI.Hide()` in both `yesAction`/`noAction` callbacks |
| TextWriter shows full text immediately | Another script overwrites the text component each frame | Ensure no other script is setting the same component; use `RemoveWriter` first |
| Tooltip stuck visible | Element disabled during hover, skipping `OnPointerExit` | `TooltipOverOut.OnDisable` calls `Hide()` automatically; ensure the component stays enabled |
| `VirtualCursorUI` NullReferenceException on `virtualMouse` | `VirtualMouseInput` component missing | Add `VirtualMouseInput` (Input System) to the same GameObject |
| `ShaderTransitionEffect` renders pink | Shader Graph running on Built-in RP | This shader requires URP or HDRP; see `code-monkey-toolkit-shaders-and-effects` |

## Boundaries

- **Resources folder must be named exactly `Resources`**: the auto-spawn mechanism uses
  `Resources.Load`. Nesting incorrectly or renaming prevents instantiation.
- **One instance per tool**: BlockerUI, TooltipUI, QuestionDialogUI, InputWindowUI, and
  ShaderTransitionEffect each maintain a single static instance; calling `Show` while visible
  replaces content.
- **TextWriter supports**: `Text`, `TextMesh`, `TextMeshPro`, `TextMeshProUGUI`.
- **DragWindow does not clamp to screen**: add position-clamping logic manually if needed.
- **ChatBubble scale for 2D pixel-art**: use `scale: 0.1f`; the bubble is sized in world
  units, not pixels.
- **Native Unity alternative**: `UnityEvent`-based popup panels, or UI Toolkit
  `DialogFactory` — these do not require the toolkit package.
