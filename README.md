# AlchemyCraft - Unity 2D Game

A 2D mining and alchemy game built with Unity 6 (URP).

## Setup Guide

### 1. Project Settings
- Create a new **Unity 6** project → template: **2D (URP)**
- **Edit > Project Settings > Physics 2D**
  - Create layer: `Ground` — apply to all platform/terrain GameObjects
- **Edit > Project Settings > Input Manager**
  - Confirm `Horizontal`, `Vertical`, `Jump` axes exist (they do by default)

### 2. Import your pixel art sprites
For each sprite sheet:
1. Select it in the Project window
2. Inspector → Texture Type: **Sprite (2D and UI)**
3. Filter Mode: **Point (no filter)**
4. Compression: **None**
5. Sprite Mode: **Multiple** → open Sprite Editor → Slice

### 3. Create ScriptableObject assets

#### Block Data (4 assets)
Right-click in Project → **Create > AlchemyCraft > Block Data**

| Asset name | blockName | miningTime | dropAmount |
|------------|-----------|------------|------------|
| GrassBlock | Grass | 1.0 | 1 |
| SoilBlock | Soil | 0.8 | 1 |
| WoodBlock | Wood | 1.2 | 1 |
| StoneBlock | Stone | 2.5 | 1 |

For each: drag the matching `TileBase` tile into the `tile` field.

#### Inventory Items (4 assets)
Right-click → **Create > AlchemyCraft > Inventory Item**

Create one per block type (Grass Item, Soil Item, etc.)
- Set `isBlock = true` and link back to the matching BlockData asset
- Drag the icon sprite into the `icon` field

#### Alchemy Recipes (as many as you want)
Right-click → **Create > AlchemyCraft > Alchemy Recipe**

Example:
| Recipe name | Input A | Input B | Output |
|-------------|---------|---------|--------|
| Soil+Stone → Granite | Soil Item | Stone Item | Granite Item |
| Grass+Wood → PlantFibre | Grass Item | Wood Item | PlantFibre Item |

### 4. Scene setup

#### Tilemap
1. **GameObject > 2D Object > Tilemap > Rectangular**
   This creates a Grid with a Tilemap child.
2. Add **Tilemap Collider 2D** to the Tilemap
3. Add **Composite Collider 2D** (Rigidbody2D will be added automatically — set it to **Static**)
4. On Tilemap Collider 2D → tick **Used By Composite**
5. Set the Tilemap's layer to **Ground**
6. Attach **WorldGenerator.cs** to an empty GameObject
   - Drag the Tilemap into `tilemap`
   - Drag all 4 tile assets (from your Tile Palette) into the corresponding fields

#### Player GameObject
1. Create empty GameObject → rename `Player`
2. Add components:
   - `Sprite Renderer` → assign idle sprite
   - `Rigidbody2D` → Gravity Scale: **3**, Freeze Rotation Z: ✓
   - `CapsuleCollider2D` → fit to character sprite
   - `Animator` → assign your Animator Controller
   - `PlayerController.cs`
   - `MiningController.cs`
3. Create empty child → rename `GroundCheck` → position just below feet
4. In PlayerController: drag `GroundCheck` transform → set Ground Layer mask to **Ground**
5. In MiningController:
   - Drag the Tilemap
   - Drag all 4 BlockData assets into `blockDataList`
   - Drag the mining progress bar Image

#### Animator Controller
1. **Window > Animation > Animator**
2. Create 5 states: `Idle`, `Run`, `Jump`, `Fall`, `Land`, `Dash`, `Tilt`
3. Parameters:

| Name | Type |
|------|------|
| Speed | Float |
| yVelocity | Float |
| isGrounded | Bool |
| isDashing | Bool |
| isTilting | Bool |
| JumpTrigger | Trigger |
| DashTrigger | Trigger |
| LandTrigger | Trigger |

4. Transitions:

| From | To | Condition | Has Exit Time |
|------|-----|-----------|---------------|
| Idle | Run | Speed > 0.1 | OFF |
| Run | Idle | Speed ≤ 0.1 | OFF |
| Idle/Run | Jump | JumpTrigger | OFF |
| Idle/Run | Dash | DashTrigger | OFF |
| Jump | Fall | yVelocity < -0.1 | OFF |
| Fall | Land | isGrounded = true | OFF |
| Land | Idle | Speed ≤ 0.1 | ON (Exit 0.9) |
| Land | Run | Speed > 0.1 | ON (Exit 0.9) |
| Idle/Run | Tilt | isTilting = true | OFF |
| Tilt | Idle | isTilting = false | OFF |
| Dash | Idle | isDashing = false | OFF |

### 5. UI Setup

#### Inventory bar (bottom of screen)
1. Create **Canvas** → Screen Space Overlay
2. Add 3 `Image` + `Button` components as child slots
3. Create a script on each button:
```csharp
// On the Button component, OnClick → AlchemyUI.OnInventorySlotClicked(0/1/2)
```

#### Alchemy Panel
1. Create a child Panel on the Canvas → rename `AlchemyPanel`
2. Add:
   - 2 `Image` boxes for input slots (slot A and B)
   - 1 `Image` box for result slot
   - 1 `Button` → "Combine"
   - A `+` Text label between A and B, and `=` between B and result
3. Attach `AlchemyUI.cs` to the panel
4. Drag all references in the Inspector
5. Drag all your `AlchemyRecipe` assets into the `recipes` list

#### Mining progress bar
1. Add a `Slider` or `Image` (Fill type: Filled, Method: Horizontal) near the crosshair
2. Drag into `MiningController.miningProgressBar`

### 6. Camera
1. Add `CameraFollow.cs` to the Main Camera
2. Drag `Player` transform into `target`
3. Set Offset Y = 2

### 7. Controls summary

| Action | Key |
|--------|-----|
| Move | A / D or Arrow keys |
| Jump | Space |
| Dash | Left Shift |
| Mine block | Hold LMB |
| Place block | RMB |
| Open alchemy | Tab |

### 8. Example alchemy recipes to add

| Combination | Result |
|-------------|--------|
| Grass + Soil | Fertile Earth |
| Stone + Wood | Reinforced Wood |
| Soil + Stone | Clay |
| Grass + Wood | Plant Fibre |

Create one `AlchemyRecipe` ScriptableObject per row and drag into `AlchemyUI.recipes`.

### 9. Quick debug checklist

- [ ] Player falls through ground → check Tilemap Collider 2D + Composite Collider 2D setup
- [ ] Animations not playing → check Animator parameter names match exactly (case-sensitive)
- [ ] Mining not working → check blockDataList has all 4 BlockData assets assigned
- [ ] Alchemy combine button always greyed out → check recipe Input A/B match the InventoryItem assets (not the BlockData)
- [ ] World not generating → check WorldGenerator has all 4 tile fields filled

## Scripts Overview

### Core Scripts
- **PlayerController.cs** - Player movement, jumping, dashing, and slope tilting
- **MiningController.cs** - Mining blocks and placing blocks from inventory
- **WorldGenerator.cs** - Procedural world generation with Perlin noise
- **Inventory.cs** - Singleton inventory system with 3 slots and stacking
- **AlchemyUI.cs** - Alchemy system with drag-and-drop and click-combine
- **CameraFollow.cs** - Smooth camera following with optional world bounds

### ScriptableObjects
- **BlockData.cs** - Block type definitions (tiles, mining time, drops)
- **InventoryItem.cs** - Item definitions (icons, block type links)
- **AlchemyRecipe.cs** - Recipe definitions (input/output pairs)

## Features
- Terrain generation with grass/soil/stone layers
- Tree generation
- Block mining with progress bar
- Inventory system with 3 slots
- Alchemy system for combining items
- Smooth animations with slope tilting
- Dash ability with cooldown
- Optional world bounds for camera