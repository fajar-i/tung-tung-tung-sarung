# Spec Design: Tough Love Arena 3D Replica in Unity

This document outlines the architecture, features, and implementation details for replicating *Tough Love Arena* as a 2.5D fighting game in Unity. It targets local 1v1 play and Player vs AI on WebGL (optimized for itch.io) using rigged 3D models from Mixamo.

---

## 1. Core Architecture (Discrete State Engine)

To guarantee the precise, frame-perfect movement and interaction of the original game, this replica bypasses Unity's default physics system for gameplay logic. Instead, it utilizes a logical state engine driven by a fixed tick rate.

### Logical Grid representation
*   **Stage Width**: $0$ (left edge) to $12$ (right edge).
*   **Cell Size**: Logical distance between cells is $1$ unit. Visually, this maps to $1.5$ meters in Unity world space (`CellWidth = 1.5f`).
*   **Characters**: Cannot occupy the same cell on the ground.

### Player Logical State (`PlayerData`)
*   `GridX` (int): The current horizontal cell index.
*   `GridY` (int): Height offset (0 = ground, >0 = jumping).
*   `FacingDirection` (int): `1` (facing right) or `-1` (facing left).
*   `Health` (int): Active health points (e.g., Max 100).
*   `ActionState` (enum):
    *   `Idle`: Ready for input.
    *   `MoveForward`: Stepping forward.
    *   `MoveBackward`: Stepping backward.
    *   `Crouch`: Squatting, reducing hurtbox height.
    *   `Jump`: Mid-air movement.
    *   `AttackLight` / `AttackHeavy` / `AttackSpecial`: Executing moves.
    *   `HitStun` / `BlockStun`: Frozen after receiving/blocking an attack.
    *   `Knockdown`: Lying on the floor, invulnerable.
*   `StateTimer` (int): Tick duration remaining in the current state.

### Game Loop Tick Lifecycle
The `GameManager` runs a custom loop at 60 ticks per second (60Hz) using a coroutine:
1.  **Read Inputs**: Poll keyboard buttons for Player 1 and Player 2/AI.
2.  **State Ticks**: Update `StateTimer` and transitions for each character.
3.  **Resolve Spacing & Pushing**: Adjust positions if players overlap.
4.  **Evaluate Combat Hits**: Perform mathematical hitbox checks.
5.  **Sync Visuals**: Update the Animator params and start interpolating character transforms toward their target positions.

---

## 2. Movement & Collision Resolution

Movement in *Tough Love Arena* is snap-like and step-based.

### Movement Rules
*   **Walk Forward / Walk Backward**: Moving takes $6$ ticks. When triggered, the character's `GridX` changes by $\pm 1$ in the respective direction. The character state becomes `MoveForward` or `MoveBackward`.
*   **Jump**: Jumps take $18$ ticks. During a jump, the horizontal coordinate increases/decreases by $2$ steps, and vertical offset (`GridY`) traces a preset arc:
    *   Ticks 1–6: ascending (`GridY` changes from 0 to 2)
    *   Ticks 7–12: peak hover (`GridY` stays at 2 or 3)
    *   Ticks 13–18: descending (`GridY` returns to 0)
*   **Crouch**: Instantly modifies hurtbox size, ending on Crouch key release.

### Overlap & Pushing Rules
*   **Pushback**: If Player A initiates a move into Player B's occupied cell, Player B is pushed by $1$ cell in Player A's moving direction, provided Player B has empty cells behind them.
*   **Wall Block**: If Player B is at cell `0` or `12` (against the wall), and Player A tries to move into Player B's cell, Player A's movement is blocked. They stay in their starting cell.
*   **Pass-through**: If Player A jumps over Player B, their `GridY` is high enough to bypass collision. Once Player A's center passes Player B's center, their `FacingDirection` variables swap to face each other.

---

## 3. Combat, Hitboxes, & Hurtboxes

Attack registration is determined using coordinate arithmetic rather than physical colliders.

### Hurtboxes
*   **Standing**: Occupies `GridX`, checking heights `Y = 0` and `Y = 1`.
*   **Crouching**: Occupies `GridX`, checking height `Y = 0` only (avoids high attacks).
*   **Jumping**: Occupies `GridX`, checking heights `Y = GridY` and `Y = GridY + 1`.

### Attack Frame Data
Every attack has three phases (totaling a set number of ticks):
1.  **Startup Ticks**: Attack is preparing, no hitbox active.
2.  **Active Ticks**: Hitbox is projected. The hit area is checked against the opponent's hurtbox.
3.  **Recovery Ticks**: Attack is retracting, player is vulnerable.

#### Base Attacks (Example Dataset)
| Attack Type | Startup Ticks | Active Ticks | Recovery Ticks | Reach (Grid Offset) | Damage | Knockback (Steps) |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Light Attack** | 3 | 2 | 4 | $+1$ step | 5 | 1 |
| **Heavy Attack** | 6 | 3 | 8 | $+2$ steps | 15 | 2 (Knockdown) |
| **Special Attack**| 8 | 4 | 10 | $+2$ steps | 10 | 1 |

### Hit Resolution
*   **Hit**: Opponent enters `HitStun` (e.g., duration = 12 ticks). Health decreases by the attack's damage. Opponent is pushed back by the attack's knockback distance.
*   **Block**: If the opponent was holding the retreat key, they block the hit. They take no damage (or minor chip damage), enter `BlockStun` (duration = 6 ticks), and take a smaller pushback distance.
*   **Knockdown**: Heavy hits knock the opponent down. The opponent enters the `Knockdown` state, is invulnerable for the knockdown duration (e.g., 30 ticks), and then stands back up to `Idle`.

---

## 4. Visual Presentation & Animation Sync

This component controls the visual translation of the logical data onto rigged 3D models.

### Visual Interpolation
*   In the standard Unity `Update()` loop, the character's GameObject smoothly moves toward the target world coordinates:
    ```csharp
    Vector3 targetWorldPosition = new Vector3(GridX * CellWidth, GridY * CellWidth, 0);
    transform.position = Vector3.MoveTowards(transform.position, targetWorldPosition, Time.deltaTime * movementSpeed);
    ```
*   This removes any choppy "teleporting" look, producing smooth walking and jumping visuals.

### Animator Controller Sync
*   **No Root Motion**: Root motion on Mixamo animations must be disabled. This ensures character movement is governed strictly by the game code rather than the clip's animations.
*   **State-driven Triggers**: When a character enters a logical state (e.g., `AttackLight`), the script sets an Animator parameter (e.g., `animator.SetTrigger("LightAttack")`).
*   **Animation Speed Scaling**: To match visual clips with the exact tick duration of actions:
    ```csharp
    // Adjust animation speed dynamically
    float clipDuration = GetClipLength("LightAttack");
    float targetDuration = ticksDuration / 60.0f; // duration in seconds
    animator.speed = clipDuration / targetDuration;
    ```

---

## 5. Input Mapping, AI, & Game Flow

Controls and loop states are optimized for local deployment and WebGL compilation.

### Controls Configuration
*   **Player 1**:
    *   Movement: `A` (Left), `D` (Right), `S` (Crouch), `W` (Jump)
    *   Attacks: `F` (Light), `G` (Heavy), `H` (Special)
*   **Player 2 / AI**:
    *   Movement: `Left Arrow`, `Right Arrow`, `Down Arrow`, `Up Arrow`
    *   Attacks: `I` (Light), `O` (Heavy), `P` (Special)

### Simple AI State Machine
If Player 2 is selected as AI, it runs a behavior update alongside the Tick cycle:
*   **Range Check**: Computes distance `D = Mathf.Abs(P1.GridX - AI.GridX)`.
*   **State Actions**:
    *   `D > 2`: AI walks toward the player (`MoveForward`).
    *   `D == 1 or 2`: AI randomly chooses to punch (`AttackLight`), heavy sweep (`AttackHeavy`), or crouch block.
*   **Reaction defense**: If the player is in an active attack state and the AI is in range, the AI checks a difficulty probability. If it passes, it inputs a retreat direction (Block) on the same tick.

### Game Flow UI
*   **HUD**: Lightweight Canvas UI containing HP bars, active combos, round clocks (99 seconds limit), and round counters.
*   **Flow States**:
    *   MainMenu -> CharacterSelect -> IntroScene -> CombatRound -> RoundOut -> VictoryScreen/Reset.
