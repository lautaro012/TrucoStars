# Shader Property Reference — Code Monkey Toolkit 2D Shaders

All property code names listed below are confirmed from the `m_OverrideReferenceName` field
in each `.shadergraph` file. Use these exact strings with
`material.SetFloat(name, value)` / `material.SetColor(name, color)` /
`material.SetVector(name, v)`.

---

## ShaderBlur2D

**Shader Graph asset:** `Assets/CodeMonkey/Toolkit/Tools/ShaderBlur2D/Shaders/Blur.shadergraph`
**Shader dropdown path:** Shader Graphs > Blur

| Display Name | Code Name | Type | Range / Default | Notes |
|---|---|---|---|---|
| Texture2D | `_MainTex` | Texture2D | — | Source sprite texture |
| Blur Amount | `_BlurAmount` | Float | 0 – 3, default 0 | Higher = stronger blur |

**Demo usage** (confirmed from `Tools/ShaderBlur2D/Demo/Demo.cs`):
```csharp
material.SetFloat("_BlurAmount", blurAmount); // 0f–3f
```

---

## ShaderDissolve2D

**Shader Graph asset:** `Assets/CodeMonkey/Toolkit/Tools/ShaderDissolve2D/Shaders/Dissolve.shadergraph`
**Shader dropdown path:** Shader Graphs > Dissolve

| Display Name | Code Name | Type | Range / Default | Notes |
|---|---|---|---|---|
| Texture2D | `_BaseMap` | Texture2D | — | Source sprite texture |
| DissolveAmount | `_DissolveAmount` | Float | 0 – 1, default 0 | 0 = solid, 1 = fully dissolved |
| NoiseScale | `_NoiseScale` | Float | — | Controls size of the noise dissolve pattern |
| DissolveColor | `_DissolveColor` | Color | — | Color of the edge glow at the dissolve boundary |

**Demo usage** (confirmed from `Tools/ShaderDissolve2D/Demo/Demo.cs`):
```csharp
material.SetFloat("_DissolveAmount", dissolveAmount); // 0f–1f
```

---

## ShaderOutline2D — Single Color

**Shader Graph asset:** `Assets/CodeMonkey/Toolkit/Tools/ShaderOutline2D/Shaders/Outline.shadergraph`
**Shader dropdown path:** Shader Graphs > Outline

| Display Name | Code Name | Type | Range / Default | Notes |
|---|---|---|---|---|
| Texture2D | `_BaseMap` | Texture2D | — | Source sprite texture |
| Outline Thickness | `_OutlineThickness` | Float | 0 – 3, default 1.0 | Width of the outline in texels |
| Outline Color | `_OutlineColor` | Color | — | Outline border color (RGBA) |

**Runtime example:**
```csharp
mat.SetFloat("_OutlineThickness", 1.5f);
mat.SetColor("_OutlineColor", Color.white);
```

---

## ShaderOutline2D — Multi-Color

**Shader Graph asset:** `Assets/CodeMonkey/Toolkit/Tools/ShaderOutline2D/Shaders/OutlineMultiColors.shadergraph`
**Shader dropdown path:** Shader Graphs > OutlineMultiColors

Same properties as the single-color variant (`_OutlineThickness`, `_OutlineColor`,
`_BaseMap`) plus one additional color property for a second outline layer. The second
color property does not have an override reference name set in this version.

---

## ShaderPixelate2D

**Shader Graph asset:** `Assets/CodeMonkey/Toolkit/Tools/ShaderPixelate2D/Shaders/Pixelate.shadergraph`
**Shader dropdown path:** Shader Graphs > Pixelate

| Display Name | Code Name | Type | Range / Default | Notes |
|---|---|---|---|---|
| BaseMap | `_BaseMap` | Texture2D | — | Source sprite texture |
| Pixelate Amount | `_PixelateAmount` | Float | 0 – 0.8, default 0 | 0 = original resolution, 0.8 = maximum pixelation |

**Demo usage** (confirmed from `Tools/ShaderPixelate2D/Demo/Demo.cs`):
```csharp
material.SetFloat("_PixelateAmount", pixelateAmount); // 0f–0.8f
```

---

## ShaderTint2D — Additive Tint

**Shader Graph asset:** `Assets/CodeMonkey/Toolkit/Tools/ShaderTint2D/Shaders/Tint.shadergraph`
**Shader dropdown path:** Shader Graphs > Tint

| Display Name | Code Name | Type | Notes |
|---|---|---|---|
| Texture2D | `_BaseMap` | Texture2D | Source sprite texture |
| Tint | `_Tint` | Color | Additive color overlay; **alpha controls intensity** (0 = no tint, 1 = full tint) |
| Color | `_Color` | Color | Base color multiplier |

**Demo usage** (confirmed from `Tools/ShaderTint2D/Demo/Demo.cs`):
```csharp
material.SetColor("_Tint", new Color(1f, 0f, 0f, flashAmount)); // red flash, alpha = intensity
```

---

## ShaderTint2D — Solid Tint

**Shader Graph asset:** `Assets/CodeMonkey/Toolkit/Tools/ShaderTint2D/Shaders/TintSolid.shadergraph`
**Shader dropdown path:** Shader Graphs > TintSolid

Same properties as Tint.shadergraph (`_BaseMap`, `_Tint`, `_Color`). Setting `_Tint` alpha
to `1` produces a solid color fill over the sprite, useful for hit-flash effects where the
sprite becomes a single colour momentarily.

---

## ZoomShader

**Shader Graph asset:** `Assets/CodeMonkey/Toolkit/Tools/ZoomShader/Shaders/ZoomShader.shadergraph`
**Shader dropdown path:** Shader Graphs > ZoomShader

| Display Name | Code Name | Type | Default | Notes |
|---|---|---|---|---|
| ObjectScreenPosition | `_ObjectScreenPosition` | Vector2 | (0, 0) | Normalized screen-space position of the zoom centre (x, y each 0–1). Set automatically by the `ZoomShaderScreenPosition` component. |
| ZoomAmount | `_ZoomAmount` | Float | — | Magnification strength; higher = more zoom |

**Helper component** (confirmed from `Tools/ZoomShader/Scripts/ZoomShaderScreenPosition.cs`):
```csharp
// ZoomShaderScreenPosition.Update() sets this automatically:
material.SetVector("_ObjectScreenPosition", normalizedScreenPos);
```

---

## TransitionEffect

**Shader Graph asset:** `Assets/CodeMonkey/Toolkit/Tools/TransitionEffect/Shaders/TransitionEffect.shadergraph`
**Shader dropdown path:** Shader Graphs > TransitionEffect

| Display Name | Code Name | Type | Notes |
|---|---|---|---|
| MaskAmount | `_MaskAmount` | Float | 0 = fully transparent, 1 = fully covered. Driven by `ShaderTransitionEffect` C# class (namespace `CodeMonkey.Toolkit.TShaderTransitionEFfect`). |
| Color | *(no override name)* | Color | Internal: tint of the transition overlay |
| Mask | *(no override name)* | Texture2D | Internal: greyscale wipe texture; replace to change the wipe shape |

**Usage** (confirmed from `Tools/TransitionEffect/Scripts/ShaderTransitionEffect.cs`):
```csharp
// C# sets this automatically via ShaderTransitionEffect.Show/Hide
material.SetFloat("_MaskAmount", maskAmount); // internal; use the C# static API
```

Use `ShaderTransitionEffect.Show(onComplete)` / `ShaderTransitionEffect.Hide(onComplete)` —
do not set `_MaskAmount` directly in most cases; the C# class manages the animation.
