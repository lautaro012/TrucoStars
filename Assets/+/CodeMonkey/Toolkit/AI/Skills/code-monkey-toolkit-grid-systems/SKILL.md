---
name: code-monkey-toolkit-grid-systems
description: "Use this skill whenever a user asks about creating or working with grids in the Code Monkey Toolkit — e.g. 'make a grid for my city builder', 'split the world into tiles', 'store data per grid cell', 'hex grid', 'grid-based strategy game', 'convert world position to grid cell', 'snap objects to a grid', or 'colour grid tiles'. Covers all three variants: GridSystem (XZ 3D plane), GridSystemXY (XY 2D/UI plane), and GridSystemHex (hex XZ plane) — grid construction, per-cell data storage/retrieval, world-to-grid and grid-to-world conversion, change events, debug visualisation via LevelGrid and GridSystemVisual, and hex geometry with odd-row stagger. Does NOT cover health/interaction/loading/tick (see code-monkey-toolkit-gameplay-systems), camera/character movement (see code-monkey-toolkit-controllers-and-camera), or UI overlays/tooltips (see code-monkey-toolkit-ui-and-feedback). When in doubt whether a grid question belongs here, use this skill — Prerequisites shows how to confirm the asset is installed."
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

# Code Monkey Toolkit — Grid Systems

The Code Monkey Toolkit ships three generic, event-driven grid systems for different projection needs. All three are pure C# classes with no MonoBehaviour requirement; a thin MonoBehaviour wrapper (`LevelGrid`) provides Inspector-driven setup. Each grid stores arbitrary typed data per cell, converts between world space and grid coordinates, and fires change events that downstream systems can subscribe to.

## When to use this skill

- User wants a rectangular or hexagonal grid for any genre (strategy, puzzle, city builder, tower defence, RPG).
- User needs world-to-grid or grid-to-world coordinate conversion.
- User wants to store per-cell data: integers, custom objects, game states.
- User wants to highlight, colour, or hide grid tiles visually.
- User mentions "hex grid", "grid cell", "tile map data", "snap to grid", or "tile-based movement".

**Not for:**
- Health, interaction, doors, loading screens, or tick logic → `code-monkey-toolkit-gameplay-systems`
- Camera or character movement → `code-monkey-toolkit-controllers-and-camera`
- UI feedback widgets, tooltips, text popups → `code-monkey-toolkit-ui-and-feedback`
- Shaders or dissolve effects → `code-monkey-toolkit-shaders-and-effects`
- General package installation → `code-monkey-toolkit-setup-and-overview`

## Prerequisites

1. Code Monkey Toolkit **1.0.5** installed (Asset Store or UPM).
2. Unity **6000.0.60f1+** (required by the asset).
3. **Programmatic install check** — run this in any MonoBehaviour or the RunCommand tool:

```csharp
// Confirms the grid namespace is available
bool installed = typeof(CodeMonkey.Toolkit.TGridSystem.GridSystem<object>) != null;
Debug.Log(installed ? "GridSystem found ✓" : "GridSystem NOT found");
```

If the type is missing, import the package from:
`https://assetstore.unity.com/packages/tools/utilities/code-monkey-toolkit-309282`

## Quick start

Create a 10×10 world-space grid (XZ plane), write an integer to a cell, and read it back.

```csharp
using UnityEngine;
using CodeMonkey.Toolkit.TGridSystem;

public class MyGrid : MonoBehaviour {
    private GridSystem<GridObject> _grid;

    private void Awake() {
        // width=10, height=10, cellSize=1f, origin=world(0,0,0)
        _grid = new GridSystem<GridObject>(
            10, 10, 1f,
            (GridSystem<GridObject> g, GridPosition pos) => new GridObject(g, pos)
        );
    }

    private void Start() {
        // Write a value at world position (2.5, 0, 3.7)
        GridPosition gp = _grid.GetGridPosition(new Vector3(2.5f, 0f, 3.7f));
        _grid.GetGridObject(gp).SetValue(42);

        // Read it back
        int val = _grid.GetGridObject(new Vector3(2.5f, 0f, 3.7f)).GetValue();
        Debug.Log($"Value at grid ({gp.x},{gp.z}): {val}"); // logs: Value at grid (3,4): 42
    }
}
```

**Observable result:** Console prints `Value at grid (3,4): 42` (coordinates rounded by `Mathf.RoundToInt`).

## Workflows

### Workflow: Create a 3D XZ grid (GridSystem)

**Goal:** Create a grid in the XZ plane for 3D games; store and query data per cell.

**Steps:**
1. Add `using CodeMonkey.Toolkit.TGridSystem;`
2. Instantiate `GridSystem<GridObject>`:
   ```csharp
   var grid = new GridSystem<GridObject>(
       width: 20, height: 15, cellSize: 2f,
       createGridObject: (g, pos) => new GridObject(g, pos),
       origin: new Vector3(-20f, 0f, -15f)   // optional world offset; default null = Vector3.zero
   );
   ```
3. Convert world → grid: `GridPosition gp = grid.GetGridPosition(worldPos);`
4. Get cell: `GridObject obj = grid.GetGridObject(gp);` or `grid.GetGridObject(worldPos)`
5. Write value and fire change event:
   ```csharp
   obj.SetValue(5);
   grid.TriggerGridObjectChanged(gp);
   ```
6. Subscribe to changes: `grid.OnGridObjectChanged += (s, e) => Debug.Log(e.gridPosition);`
7. Validate: `if (!grid.IsValidGridPosition(gp)) return;`

**Key parameters (verified from GridSystem.cs):**
- `width`, `height` — cell count on each axis
- `cellSize` — world units per cell
- `origin` (`Vector3?`, default `null` = `Vector3.zero`)
- `GridPosition` struct: fields `int x`, `int z` (namespace `TGridSystem`)
- World position: `new Vector3(gp.x, 0, gp.z) * cellSize + origin`

**Expected result:** Grid spans 20×15 cells, 2 world-units each, centred at `origin`.

---

### Workflow: Create a 2D XY grid (GridSystemXY)

**Goal:** Create a grid on the XY plane for 2D or UI-space games.

**Steps:**
1. Add `using CodeMonkey.Toolkit.TGridSystemXY;`
2. Instantiate `GridSystemXY<GridObject>` — **no `origin` parameter in this variant**:
   ```csharp
   var grid = new GridSystemXY<GridObject>(
       width: 8, height: 8, cellSize: 1f,
       createGridObject: (g, pos) => new GridObject(g, pos)
   );
   ```
3. World position maps to XY plane: `new Vector3(gp.x, gp.y, 0) * cellSize`
4. **`GridPosition` struct in this namespace** has fields `int x`, `int y` (not `z`).
5. All other methods (`GetGridPosition`, `GetGridObject`, `IsValidGridPosition`, `OnGridObjectChanged`) behave identically to `GridSystem`.

**Expected result:** Grid lies in the XY plane, suitable for 2D cameras.

---

### Workflow: Create a hex grid (GridHexXZ)

**Goal:** Create an offset-based hex grid in XZ space.

**Steps:**
1. Add `using CodeMonkey.Toolkit.TGridSystemHex;`
2. Instantiate `GridHexXZ<T>` — the factory lambda receives raw `int x, int z`, not a `GridPosition`:
   ```csharp
   var hexGrid = new GridHexXZ<string>(
       width: 6, height: 6, cellSize: 2f,
       originPosition: Vector3.zero,
       createGridObject: (g, x, z) => $"cell({x},{z})"
   );
   ```
3. World → hex coords: `hexGrid.GetXZ(worldPos, out int x, out int z);`
4. Hex → world: `Vector3 wp = hexGrid.GetWorldPosition(x, z);`
   — uses `HEX_VERTICAL_OFFSET_MULTIPLIER = 0.75f` and odd-row stagger (`+0.5f cellSize` on odd z rows).
5. Read: `hexGrid.GetGridObject(x, z)` or `hexGrid.GetGridObject(worldPos)`
6. Write: `hexGrid.SetGridObject(x, z, value)` — fires `OnGridObjectChanged`
7. Validate: `hexGrid.IsValidGridPosition(new Vector2Int(x, z))`

**Expected result:** Hex cells placed in XZ space with correct hex spacing and stagger.

---

### Workflow: Inspector-driven grid (LevelGrid MonoBehaviour)

**Goal:** Set up a grid via the Inspector without writing constructor code.

**Steps:**
1. Add a `LevelGrid` component to a scene GameObject (namespace `TGridSystem` or `TGridSystemXY`).
2. Set **Width**, **Height**, **Cell Size** in the Inspector.
3. Optionally enable **Spawn Grid Debug Objects** and assign a debug prefab (must implement `IGridDebugObject`).
4. Access at runtime via singleton: `LevelGrid.Instance.GetGridObject(worldPos)`, `LevelGrid.Instance.SetValue(worldPos, 7)`.
5. Subscribe: `LevelGrid.Instance.OnAnyValueChanged += (s, e) => RefreshUI();`

**Expected result:** Grid initialises in `Awake`; singleton `LevelGrid.Instance` accessible from any script.

---

### Workflow: Visualise grid tiles (GridSystemVisual)

**Goal:** Show coloured tile overlays using the built-in visual system.

**Steps:**
1. Add `GridSystemVisual` MonoBehaviour to a scene object.
2. Assign `gridSystemVisualSinglePrefab` and populate `gridVisualTypeMaterialList` with materials for each `GridVisualType`: `White`, `Blue`, `Red`, `Yellow`.
3. Call at runtime:
   ```csharp
   GridSystemVisual.Instance.HideAllGridPosition();
   GridSystemVisual.Instance.ShowGridPositionRange(
       centerGridPos, range: 3, GridSystemVisual.GridVisualType.Blue
   );
   // or highlight a specific list:
   GridSystemVisual.Instance.ShowGridPositionList(myList, GridVisualType.Red);
   ```

**Expected result:** Tile overlays appear/disappear per call; material swaps correctly.

## Verification

- `typeof(CodeMonkey.Toolkit.TGridSystem.GridSystem<object>)` resolves without error.
- `typeof(CodeMonkey.Toolkit.TGridSystemXY.GridSystemXY<object>)` resolves without error.
- `typeof(CodeMonkey.Toolkit.TGridSystemHex.GridHexXZ<object>)` resolves without error.
- `LevelGrid.Instance` is non-null after scene Awake when the component is in the scene.
- `grid.GetGridObject(validPos).GetValue()` returns the integer previously set by `SetValue`.
- No `IndexOutOfRangeException` when `IsValidGridPosition` is checked before `GetGridObject`.

## API quick reference

| Entry point | Type | What it does |
|---|---|---|
| `GridSystem<T>(width, height, cellSize, factory, origin?)` | C# constructor | Create an XZ-plane grid |
| `GridSystem<T>.GetGridPosition(Vector3)` | Method | World → `GridPosition` (x, z) |
| `GridSystem<T>.GetWorldPosition(GridPosition)` | Method | `GridPosition` → world `Vector3` |
| `GridSystem<T>.GetGridObject(GridPosition\|Vector3)` | Method | Retrieve the typed cell object |
| `GridSystem<T>.TriggerGridObjectChanged(GridPosition)` | Method | Fire `OnGridObjectChanged` event |
| `GridSystem<T>.IsValidGridPosition(GridPosition\|Vector3)` | Method | Bounds check (returns bool) |
| `GridSystem<T>.OnGridObjectChanged` | Event | `EventHandler<OnGridObjectChangedEventArgs>` |
| `GridObject.SetValue(int)` / `GetValue()` | Methods | Read/write the integer stored per cell |
| `GridObject.OnValueChanged` | Event | Fires when `SetValue` is called |
| `GridSystemXY<T>(width, height, cellSize, factory)` | C# constructor | Create an XY-plane grid (no `origin`) |
| `GridSystemXY.GridPosition` | Struct | Fields: `int x`, `int y` (not z) |
| `GridHexXZ<T>(width, height, cellSize, originPos, factory)` | C# constructor | Create a hex XZ grid |
| `GridHexXZ<T>.GetXZ(Vector3, out int x, out int z)` | Method | World → nearest hex coords |
| `GridHexXZ<T>.GetWorldPosition(int x, int z)` | Method | Hex coords → world `Vector3` |
| `GridHexXZ<T>.SetGridObject(int x, int z, T value)` | Method | Write cell; fires `OnGridObjectChanged` |
| `LevelGrid` (MonoBehaviour) | Component | Inspector-driven XZ grid singleton |
| `LevelGrid.Instance.SetValue(Vector3, int)` | Method | Write int to cell at world pos |
| `LevelGrid.Instance.OnAnyValueChanged` | Event | Fires when any cell's value changes |
| `GridSystemVisual` (MonoBehaviour) | Component | Tile colour overlay singleton |
| `GridSystemVisual.Instance.ShowGridPositionRange(gp, range, type)` | Method | Highlight diamond-shaped range |
| `GridSystemVisual.GridVisualType` | Enum | White, Blue, Red, Yellow |
| `HexVisual` (MonoBehaviour) | Component | Per-hex visual helper: SetText, ShowSelected, SetRedColor, HideSelected |

## Common issues

| Symptom | Cause | Fix |
|---|---|---|
| `IndexOutOfRangeException` on `GetGridObject` | World position outside grid bounds | Call `IsValidGridPosition` before `GetGridObject` |
| Tiles appear in wrong plane | Using `GridSystem` (XZ) for a 2D camera | Switch to `GridSystemXY` (XY plane, no origin offset) |
| `GridPosition.z` not found | `TGridSystemXY.GridPosition` uses `.y`, not `.z` | Use `TGridSystemXY.GridPosition` fields: `x` and `y` |
| Hex cells misaligned | `cellSize` or `originPosition` incorrect | Use `GetWorldPosition(x, z)` to position prefabs at runtime |
| `LevelGrid.Instance` is null | Component missing from scene or destroyed | Add `LevelGrid` to any scene object before Start |
| `GridSystemVisual` shows nothing | Materials not assigned in Inspector | Populate `gridVisualTypeMaterialList` for all four `GridVisualType` values |
| Change event never fires | `TriggerGridObjectChanged` not called after mutation | Call `grid.TriggerGridObjectChanged(gp)` after modifying custom T cells |

## Boundaries

- **No pathfinding built in.** Combine with Unity's AI Navigation package or A* Pathfinding Project for agent pathfinding.
- **No tilemap rendering.** These are pure-data grids; use prefab instancing or Unity's Tilemap component for rendering.
- **`GridSystemXY` has no `origin` offset.** The world origin is always `(0,0)`. Shift parent objects if you need an offset.
- **Hex system uses raw `int` coordinates**, not a `GridPosition` struct — always use `GetXZ(out int x, out int z)`.
- **`GridObject.value` is a plain `int`.** For richer per-cell data, pass a custom class as `TGridObject` to the generic constructor.
- **`GridSystem` places cells on the XZ plane (y = 0).** For non-flat worlds, use `SetCustomGetWorldPositionFunc` and `SetCustomGetGridPositionFunc` to override both conversion directions.
