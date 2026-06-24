# Tough Love Arena 3D Replica MVP Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a playable 2.5D discrete step-based fighting game MVP in Unity featuring 1v1 single-keyboard gameplay, hit/block detection, pushback mechanics, smooth 3D interpolation, and a simple AI opponent.

**Architecture:** A logical tick-based state engine (independent of frame rate) that calculates character positions as 1D grid units, resolves collisions mathematically, and updates 3D models and animators using visual interpolation.

**Tech Stack:** Unity Engine (C# scripting, Mecanim Animator, Canvas UI).

---

## File Structure

- `Assets/Scripts/Core/Enums.cs` — Defines player states and directions.
- `Assets/Scripts/Core/PlayerData.cs` — Struct/class holding the state variables for a player.
- `Assets/Scripts/Core/GameManager.cs` — Orchestrates the 60Hz tick loop, gathers input, updates states, and resolves interactions.
- `Assets/Scripts/Core/MovementController.cs` — Resolves grid walking, jump trajectories, and player pushing.
- `Assets/Scripts/Core/CombatController.cs` — Manages attack state cycles (Startup, Active, Recovery) and hit detection.
- `Assets/Scripts/Visual/VisualController.cs` — Lerps 3D positions and drives the Animator speeds/parameters.
- `Assets/Scripts/AI/SimpleAI.cs` — Grid-based decision machine for P2.

---

### Task 1: Core State & Enums

**Files:**
- Create: `Assets/Scripts/Core/Enums.cs`
- Create: `Assets/Scripts/Core/PlayerData.cs`

- [ ] **Step 1: Create Enums.cs**

Write the action states and direction mappings.
Create `Assets/Scripts/Core/Enums.cs`:
```csharp
namespace ToughLoveArena.Core
{
    public enum ActionState
    {
        Idle,
        MoveForward,
        MoveBackward,
        Crouch,
        Jump,
        AttackLight,
        AttackHeavy,
        AttackSpecial,
        HitStun,
        BlockStun,
        Knockdown
    }

    public enum FacingDirection
    {
        Left = -1,
        Right = 1
    }
}
```

- [ ] **Step 2: Create PlayerData.cs**

Write the data structures representing the deterministic state of a player.
Create `Assets/Scripts/Core/PlayerData.cs`:
```csharp
using UnityEngine;

namespace ToughLoveArena.Core
{
    [System.Serializable]
    public class PlayerData
    {
        public int PlayerId; // 1 or 2
        public int GridX; // 0 to 12
        public int GridY; // 0 = ground, >0 = jumping
        public FacingDirection Facing;
        public int Health;
        public int MaxHealth = 100;
        public ActionState State;
        public int StateTimer; // Number of ticks remaining in current state
        
        // Attack-specific tracking
        public int AttackStartupTicks;
        public int AttackActiveTicks;
        public int AttackRecoveryTicks;
        public int AttackReach;
        public int AttackDamage;
        public int AttackKnockback;
        
        public bool IsBlocking;

        public PlayerData(int id, int startGridX, FacingDirection startFacing)
        {
            PlayerId = id;
            GridX = startGridX;
            GridY = 0;
            Facing = startFacing;
            Health = MaxHealth;
            State = ActionState.Idle;
            StateTimer = 0;
            IsBlocking = false;
        }
    }
}
```

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/Core/Enums.cs Assets/Scripts/Core/PlayerData.cs
git commit -m "feat: add core enums and PlayerData classes"
```

---

### Task 2: GameManager & Tick Loop

**Files:**
- Create: `Assets/Scripts/Core/GameManager.cs`

- [ ] **Step 1: Create GameManager.cs**

Write the basic 60Hz tick loop component.
Create `Assets/Scripts/Core/GameManager.cs`:
```csharp
using System.Collections;
using UnityEngine;
using ToughLoveArena.Core;

namespace ToughLoveArena.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Grid Configuration")]
        public int GridMin = 0;
        public int GridMax = 12;
        public float CellWidth = 1.5f;

        [Header("Players")]
        public PlayerData Player1;
        public PlayerData Player2;

        public bool IsAiOpponent = false;

        private float _tickRate = 1f / 60f; // 60 FPS Logical Ticks
        private bool _isGameRunning = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
            
            ResetMatch();
        }

        private void Start()
        {
            StartCoroutine(TickLoop());
        }

        public void ResetMatch()
        {
            Player1 = new PlayerData(1, 3, FacingDirection.Right);
            Player2 = new PlayerData(2, 9, FacingDirection.Left);
            _isGameRunning = true;
        }

        private IEnumerator TickLoop()
        {
            while (true)
            {
                if (_isGameRunning)
                {
                    LogicalTick();
                }
                yield return new WaitForSeconds(_tickRate);
            }
        }

        private void LogicalTick()
        {
            // Gather inputs (will be implemented in Task 3)
            ProcessInputs();

            // Resolve movements (will be implemented in Task 4)
            ResolveMovement();

            // Resolve attacks (will be implemented in Task 5)
            ResolveCombat();
        }

        private void ProcessInputs()
        {
            // Placeholder for inputs
        }

        private void ResolveMovement()
        {
            // Placeholder for movement
        }

        private void ResolveCombat()
        {
            // Placeholder for combat
        }
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add Assets/Scripts/Core/GameManager.cs
git commit -m "feat: add GameManager and basic 60Hz tick loop"
```

---

### Task 3: Input Handler

**Files:**
- Create: `Assets/Scripts/Input/InputHandler.cs`
- Modify: `Assets/Scripts/Core/GameManager.cs`

- [ ] **Step 1: Create InputHandler.cs**

Write a utility to buffer player actions based on keyboard state.
Create `Assets/Scripts/Input/InputHandler.cs`:
```csharp
using UnityEngine;

namespace ToughLoveArena.Input
{
    public class PlayerInputBuffer
    {
        public int MoveDirection; // -1, 0, 1
        public bool JumpPressed;
        public bool CrouchHeld;
        public bool AttackLightPressed;
        public bool AttackHeavyPressed;
        public bool AttackSpecialPressed;

        public void Clear()
        {
            MoveDirection = 0;
            JumpPressed = false;
            CrouchHeld = false;
            AttackLightPressed = false;
            AttackHeavyPressed = false;
            AttackSpecialPressed = false;
        }
    }
}
```

- [ ] **Step 2: Integrate inputs into GameManager.cs**

Modify `Assets/Scripts/Core/GameManager.cs` to read inputs for P1 and P2 during each tick.
Modify `Assets/Scripts/Core/GameManager.cs`:
```csharp
// Add input buffer fields to GameManager class
using ToughLoveArena.Input;

public PlayerInputBuffer P1Input = new PlayerInputBuffer();
public PlayerInputBuffer P2Input = new PlayerInputBuffer();

private void ProcessInputs()
{
    // Player 1
    P1Input.Clear();
    if (UnityEngine.Input.GetKey(KeyCode.A)) P1Input.MoveDirection = -1;
    else if (UnityEngine.Input.GetKey(KeyCode.D)) P1Input.MoveDirection = 1;
    if (UnityEngine.Input.GetKeyDown(KeyCode.W)) P1Input.JumpPressed = true;
    if (UnityEngine.Input.GetKey(KeyCode.S)) P1Input.CrouchHeld = true;
    if (UnityEngine.Input.GetKeyDown(KeyCode.F)) P1Input.AttackLightPressed = true;
    if (UnityEngine.Input.GetKeyDown(KeyCode.G)) P1Input.AttackHeavyPressed = true;
    if (UnityEngine.Input.GetKeyDown(KeyCode.H)) P1Input.AttackSpecialPressed = true;

    // Player 2 (If not AI)
    P2Input.Clear();
    if (!IsAiOpponent)
    {
        if (UnityEngine.Input.GetKey(KeyCode.LeftArrow)) P2Input.MoveDirection = -1;
        else if (UnityEngine.Input.GetKey(KeyCode.RightArrow)) P2Input.MoveDirection = 1;
        if (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow)) P2Input.JumpPressed = true;
        if (UnityEngine.Input.GetKey(KeyCode.DownArrow)) P2Input.CrouchHeld = true;
        if (UnityEngine.Input.GetKeyDown(KeyCode.Keypad1) || UnityEngine.Input.GetKeyDown(KeyCode.I)) P2Input.AttackLightPressed = true;
        if (UnityEngine.Input.GetKeyDown(KeyCode.Keypad2) || UnityEngine.Input.GetKeyDown(KeyCode.O)) P2Input.AttackHeavyPressed = true;
        if (UnityEngine.Input.GetKeyDown(KeyCode.Keypad3) || UnityEngine.Input.GetKeyDown(KeyCode.P)) P2Input.AttackSpecialPressed = true;
    }
}
```

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/Input/InputHandler.cs Assets/Scripts/Core/GameManager.cs
git commit -m "feat: implement single-keyboard input buffering for P1 and P2"
```

---

### Task 4: Movement and Pushing Controller

**Files:**
- Create: `Assets/Scripts/Core/MovementController.cs`
- Modify: `Assets/Scripts/Core/GameManager.cs`

- [ ] **Step 1: Create MovementController.cs**

Write the rules for walking, jumping, and resolving overlays (pushing opponent, wall bounds).
Create `Assets/Scripts/Core/MovementController.cs`:
```csharp
using UnityEngine;

namespace ToughLoveArena.Core
{
    public static class MovementController
    {
        public static void UpdatePlayerMovement(PlayerData p, Input.PlayerInputBuffer input, int minGrid, int maxGrid)
        {
            if (p.StateTimer > 0)
            {
                p.StateTimer--;
                if (p.StateTimer == 0)
                {
                    // Resolve finish of walk or jump states
                    if (p.State == ActionState.MoveForward || p.State == ActionState.MoveBackward)
                    {
                        p.State = ActionState.Idle;
                    }
                    else if (p.State == ActionState.Jump)
                    {
                        p.State = ActionState.Idle;
                        p.GridY = 0;
                    }
                }
                
                // If mid-jump, calculate Y trajectory
                if (p.State == ActionState.Jump)
                {
                    // Arc trajectory over 18 ticks: peak Y=2 in middle ticks
                    int elapsed = 18 - p.StateTimer;
                    if (elapsed <= 6) p.GridY = 1;
                    else if (elapsed <= 12) p.GridY = 2;
                    else p.GridY = 1;
                }
                return;
            }

            // Crouching state
            if (input.CrouchHeld && p.State == ActionState.Idle)
            {
                p.State = ActionState.Crouch;
                return;
            }
            if (!input.CrouchHeld && p.State == ActionState.Crouch)
            {
                p.State = ActionState.Idle;
            }

            if (p.State != ActionState.Idle) return;

            // Jump
            if (input.JumpPressed)
            {
                p.State = ActionState.Jump;
                p.StateTimer = 18; // 18 ticks jump duration
                p.GridY = 1;
                // Move horizontal steps
                int targetX = p.GridX + (int)p.Facing * 2;
                p.GridX = Mathf.Clamp(targetX, minGrid, maxGrid);
                return;
            }

            // Walk
            if (input.MoveDirection != 0)
            {
                bool isForward = (input.MoveDirection == (int)p.Facing);
                p.State = isForward ? ActionState.MoveForward : ActionState.MoveBackward;
                p.StateTimer = 6; // 6 ticks walk duration
                
                int targetX = p.GridX + input.MoveDirection;
                p.GridX = Mathf.Clamp(targetX, minGrid, maxGrid);
            }
        }

        public static void ResolvePushing(PlayerData p1, PlayerData p2, int minGrid, int maxGrid)
        {
            // Overlap check on ground (Y = 0)
            if (p1.GridX == p2.GridX && p1.GridY == 0 && p2.GridY == 0)
            {
                // Push logic based on active state movement
                if (p1.State == ActionState.MoveForward || p1.State == ActionState.Jump)
                {
                    int pushDir = (int)p1.Facing;
                    int targetP2X = p2.GridX + pushDir;
                    
                    if (targetP2X >= minGrid && targetP2X <= maxGrid)
                    {
                        p2.GridX = targetP2X;
                    }
                    else
                    {
                        // Opponent is against the wall, push back the initiator
                        p1.GridX = Mathf.Clamp(p1.GridX - pushDir, minGrid, maxGrid);
                    }
                }
                else if (p2.State == ActionState.MoveForward || p2.State == ActionState.Jump)
                {
                    int pushDir = (int)p2.Facing;
                    int targetP1X = p1.GridX + pushDir;
                    
                    if (targetP1X >= minGrid && targetP1X <= maxGrid)
                    {
                        p1.GridX = targetP1X;
                    }
                    else
                    {
                        p2.GridX = Mathf.Clamp(p2.GridX - pushDir, minGrid, maxGrid);
                    }
                }
            }
            
            // Adjust facing directions automatically so they face each other
            if (p1.GridX < p2.GridX)
            {
                p1.Facing = FacingDirection.Right;
                p2.Facing = FacingDirection.Left;
            }
            else if (p1.GridX > p2.GridX)
            {
                p1.Facing = FacingDirection.Left;
                p2.Facing = FacingDirection.Right;
            }
        }
    }
}
```

- [ ] **Step 2: Integrate Movement in GameManager.cs**

Modify `ResolveMovement()` in `Assets/Scripts/Core/GameManager.cs` to execute these movement routines.
Modify `Assets/Scripts/Core/GameManager.cs`:
```csharp
private void ResolveMovement()
{
    // Update player positions/states
    MovementController.UpdatePlayerMovement(Player1, P1Input, GridMin, GridMax);
    MovementController.UpdatePlayerMovement(Player2, P2Input, GridMin, GridMax);

    // Resolve grid overlapping
    MovementController.ResolvePushing(Player1, Player2, GridMin, GridMax);
}
```

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/Core/MovementController.cs Assets/Scripts/Core/GameManager.cs
git commit -m "feat: implement discrete grid walking, jumping arcs, and pushback mechanics"
```

---

### Task 5: Combat & Hit Registration

**Files:**
- Create: `Assets/Scripts/Core/CombatController.cs`
- Modify: `Assets/Scripts/Core/GameManager.cs`

- [ ] **Step 1: Create CombatController.cs**

Implement startup/active/recovery attack cycles and check mathematical boundaries for hits or blocks.
Create `Assets/Scripts/Core/CombatController.cs`:
```csharp
using UnityEngine;

namespace ToughLoveArena.Core
{
    public static class CombatController
    {
        public static void ProcessAttackInputs(PlayerData p, Input.PlayerInputBuffer input)
        {
            if (p.State != ActionState.Idle && p.State != ActionState.Crouch) return;

            if (input.AttackLightPressed)
            {
                TriggerAttack(p, ActionState.AttackLight, 3, 2, 4, 1, 5, 1);
            }
            else if (input.AttackHeavyPressed)
            {
                TriggerAttack(p, ActionState.AttackHeavy, 6, 3, 8, 2, 15, 2);
            }
            else if (input.AttackSpecialPressed)
            {
                TriggerAttack(p, ActionState.AttackSpecial, 8, 4, 10, 2, 10, 1);
            }
        }

        private static void TriggerAttack(PlayerData p, ActionState state, int startup, int active, int recovery, int reach, int damage, int knockback)
        {
            p.State = state;
            p.AttackStartupTicks = startup;
            p.AttackActiveTicks = active;
            p.AttackRecoveryTicks = recovery;
            p.AttackReach = reach;
            p.AttackDamage = damage;
            p.AttackKnockback = knockback;
            p.StateTimer = startup + active + recovery;
        }

        public static void UpdatePlayerCombat(PlayerData attacker, PlayerData defender, int minGrid, int maxGrid)
        {
            // Blocking check
            // Blocking is active if defender is holding the backward key during the attack frame
            // Checked dynamically below in ResolveHit
            
            if (attacker.StateTimer > 0)
            {
                int elapsed = (attacker.AttackStartupTicks + attacker.AttackActiveTicks + attacker.AttackRecoveryTicks) - attacker.StateTimer;
                
                // If we are exactly in the active ticks range, perform mathematical hit checking
                bool isActive = (elapsed >= attacker.AttackStartupTicks && elapsed < attacker.AttackStartupTicks + attacker.AttackActiveTicks);
                if (isActive)
                {
                    ResolveHitDetection(attacker, defender, minGrid, maxGrid);
                }
            }
            
            // Resolve timers for hitstun / blockstun / knockdown
            ResolveStunTimers(attacker);
        }

        private static void ResolveHitDetection(PlayerData attacker, PlayerData defender, int minGrid, int maxGrid)
        {
            // Defensive invulnerabilities
            if (defender.State == ActionState.Knockdown || defender.State == ActionState.HitStun || defender.State == ActionState.BlockStun) return;

            // Attack Reach calculation
            int attackReachX = attacker.GridX + (int)attacker.Facing * attacker.AttackReach;
            
            // 1D check: does the attack reach reach defender's cell?
            bool xMatch = (attacker.Facing == FacingDirection.Right && defender.GridX <= attackReachX && defender.GridX > attacker.GridX) ||
                         (attacker.Facing == FacingDirection.Left && defender.GridX >= attackReachX && defender.GridX < attacker.GridX);
            
            // Height check (low crouch hurtbox avoids jump attacks, etc)
            bool yMatch = (attacker.GridY == defender.GridY) || (attacker.GridY == 0 && defender.GridY == 0);

            if (xMatch && yMatch)
            {
                // Check if defender is holding back (blocking)
                bool isRetreating = (defender.PlayerId == 1 && GameManager.Instance.P1Input.MoveDirection == -(int)defender.Facing) ||
                                    (defender.PlayerId == 2 && GameManager.Instance.P2Input.MoveDirection == -(int)defender.Facing);
                
                if (isRetreating && defender.GridY == 0) // Blocking is only on ground in TLA
                {
                    // Blocked!
                    defender.State = ActionState.BlockStun;
                    defender.StateTimer = 8; // 8 ticks blockstun
                    
                    int pushDir = (int)attacker.Facing;
                    defender.GridX = Mathf.Clamp(defender.GridX + pushDir, minGrid, maxGrid);
                    
                    // Trigger block sound / visual impact event
                    Debug.Log($"P{defender.PlayerId} BLOCKED Attack from P{attacker.PlayerId}");
                }
                else
                {
                    // Hit!
                    defender.State = defender.AttackHeavyTicks > 0 && attacker.State == ActionState.AttackHeavy ? ActionState.Knockdown : ActionState.HitStun;
                    defender.StateTimer = defender.State == ActionState.Knockdown ? 35 : 12; // 35 ticks knockdown, 12 ticks hitstun
                    defender.Health = Mathf.Max(0, defender.Health - attacker.AttackDamage);
                    
                    int pushDir = (int)attacker.Facing;
                    defender.GridX = Mathf.Clamp(defender.GridX + pushDir * attacker.AttackKnockback, minGrid, maxGrid);
                    
                    Debug.Log($"P{defender.PlayerId} HIT by P{attacker.PlayerId}! HP={defender.Health}");
                }
                
                // Clear attacker active hitboxes so it doesn't double-hit on next active frame
                attacker.AttackActiveTicks = 0; 
            }
        }

        private static void ResolveStunTimers(PlayerData p)
        {
            if (p.State == ActionState.HitStun || p.State == ActionState.BlockStun || p.State == ActionState.Knockdown)
            {
                if (p.StateTimer > 0)
                {
                    p.StateTimer--;
                    if (p.StateTimer == 0)
                    {
                        p.State = ActionState.Idle;
                    }
                }
            }
        }
    }
}
```

- [ ] **Step 2: Hook up Combat in GameManager.cs**

Modify `ProcessInputs()` and `ResolveCombat()` inside `Assets/Scripts/Core/GameManager.cs` to integrate combat checks.
Modify `Assets/Scripts/Core/GameManager.cs`:
```csharp
private void ProcessInputs()
{
    // ... Previous movement key polling ...
    
    // Process attacks
    CombatController.ProcessAttackInputs(Player1, P1Input);
    CombatController.ProcessAttackInputs(Player2, P2Input);
}

private void ResolveCombat()
{
    // Execute hit frames and recovery stuns
    CombatController.UpdatePlayerCombat(Player1, Player2, GridMin, GridMax);
    CombatController.UpdatePlayerCombat(Player2, Player1, GridMin, GridMax);
}
```

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/Core/CombatController.cs Assets/Scripts/Core/GameManager.cs
git commit -m "feat: implement frame data combat engine with block/hitstun and knockback"
```

---

### Task 6: Visual Controller & Animation Sync

**Files:**
- Create: `Assets/Scripts/Visual/VisualController.cs`

- [ ] **Step 1: Create VisualController.cs**

Create the component to attach to P1/P2 character 3D GameObjects to smooth their translation and sync Animator states.
Create `Assets/Scripts/Visual/VisualController.cs`:
```csharp
using UnityEngine;
using ToughLoveArena.Core;

namespace ToughLoveArena.Visual
{
    public class VisualController : MonoBehaviour
    {
        public int TargetPlayerId; // 1 or 2
        public float VisualLerpSpeed = 15f;
        
        [Header("Animations")]
        public Animator CharacterAnimator;
        public string IdleTrigger = "Idle";
        public string WalkFwdTrigger = "WalkForward";
        public string WalkBwdTrigger = "WalkBackward";
        public string JumpTrigger = "Jump";
        public string CrouchTrigger = "Crouch";
        public string LightAttackTrigger = "LightAttack";
        public string HeavyAttackTrigger = "HeavyAttack";
        public string SpecialAttackTrigger = "SpecialAttack";
        public string HurtTrigger = "Hurt";
        public string BlockTrigger = "Block";
        public string KnockdownTrigger = "Knockdown";

        private PlayerData _myData;
        private ActionState _lastVisualState;

        private void Start()
        {
            if (CharacterAnimator == null)
            {
                CharacterAnimator = GetComponentInChildren<Animator>();
            }
        }

        private void Update()
        {
            // Sync current data reference from GameManager
            if (GameManager.Instance == null) return;
            
            _myData = (TargetPlayerId == 1) ? GameManager.Instance.Player1 : GameManager.Instance.Player2;
            if (_myData == null) return;

            // 1. Interpolate visual position
            float cellWidth = GameManager.Instance.CellWidth;
            Vector3 targetPos = new Vector3(_myData.GridX * cellWidth, _myData.GridY * cellWidth, 0);
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * VisualLerpSpeed);

            // 2. Set rotation based on direction
            float rotY = (_myData.Facing == FacingDirection.Right) ? 90f : -90f;
            transform.rotation = Quaternion.Euler(0, rotY, 0);

            // 3. Play animations on State changes
            if (_myData.State != _lastVisualState)
            {
                TriggerVisualStateChange(_myData.State);
                _lastVisualState = _myData.State;
            }
        }

        private void TriggerVisualStateChange(ActionState state)
        {
            if (CharacterAnimator == null) return;

            // Clear active flags / reset parameters
            CharacterAnimator.ResetTrigger(IdleTrigger);
            CharacterAnimator.ResetTrigger(WalkFwdTrigger);
            CharacterAnimator.ResetTrigger(WalkBwdTrigger);
            CharacterAnimator.ResetTrigger(JumpTrigger);
            CharacterAnimator.ResetTrigger(LightAttackTrigger);
            CharacterAnimator.ResetTrigger(HeavyAttackTrigger);
            CharacterAnimator.ResetTrigger(SpecialAttackTrigger);
            CharacterAnimator.ResetTrigger(HurtTrigger);
            CharacterAnimator.ResetTrigger(BlockTrigger);
            CharacterAnimator.ResetTrigger(KnockdownTrigger);

            // Scale playback speed dynamically for precise synchronization
            CharacterAnimator.speed = 1f;

            switch (state)
            {
                case ActionState.Idle:
                    CharacterAnimator.SetTrigger(IdleTrigger);
                    break;
                case ActionState.MoveForward:
                    CharacterAnimator.SetTrigger(WalkFwdTrigger);
                    // Match walk animation length with logical tick duration (6 ticks = 0.1s at 60Hz)
                    ScaleAnimatorClipSpeed(WalkFwdTrigger, 6);
                    break;
                case ActionState.MoveBackward:
                    CharacterAnimator.SetTrigger(WalkBwdTrigger);
                    ScaleAnimatorClipSpeed(WalkBwdTrigger, 6);
                    break;
                case ActionState.Crouch:
                    CharacterAnimator.SetTrigger(CrouchTrigger);
                    break;
                case ActionState.Jump:
                    CharacterAnimator.SetTrigger(JumpTrigger);
                    ScaleAnimatorClipSpeed(JumpTrigger, 18);
                    break;
                case ActionState.AttackLight:
                    CharacterAnimator.SetTrigger(LightAttackTrigger);
                    ScaleAnimatorClipSpeed(LightAttackTrigger, 9);
                    break;
                case ActionState.AttackHeavy:
                    CharacterAnimator.SetTrigger(HeavyAttackTrigger);
                    ScaleAnimatorClipSpeed(HeavyAttackTrigger, 17);
                    break;
                case ActionState.AttackSpecial:
                    CharacterAnimator.SetTrigger(SpecialAttackTrigger);
                    ScaleAnimatorClipSpeed(SpecialAttackTrigger, 22);
                    break;
                case ActionState.HitStun:
                    CharacterAnimator.SetTrigger(HurtTrigger);
                    ScaleAnimatorClipSpeed(HurtTrigger, 12);
                    break;
                case ActionState.BlockStun:
                    CharacterAnimator.SetTrigger(BlockTrigger);
                    ScaleAnimatorClipSpeed(BlockTrigger, 8);
                    break;
                case ActionState.Knockdown:
                    CharacterAnimator.SetTrigger(KnockdownTrigger);
                    ScaleAnimatorClipSpeed(KnockdownTrigger, 35);
                    break;
            }
        }

        private void ScaleAnimatorClipSpeed(string stateName, int logicalDurationTicks)
        {
            float targetSecs = logicalDurationTicks / 60.0f;
            
            // Fetch current clip length from animator's active state
            AnimatorStateInfo stateInfo = CharacterAnimator.GetCurrentAnimatorStateInfo(0);
            float currentClipLen = stateInfo.length;
            
            if (currentClipLen > 0.01f)
            {
                CharacterAnimator.speed = currentClipLen / targetSecs;
            }
        }
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add Assets/Scripts/Visual/VisualController.cs
git commit -m "feat: implement 3D translation interpolation and Animator synchronization"
```

---

### Task 7: Simple AI Opponent

**Files:**
- Create: `Assets/Scripts/AI/SimpleAI.cs`
- Modify: `Assets/Scripts/Core/GameManager.cs`

- [ ] **Step 1: Create SimpleAI.cs**

Write the logic for spacing calculations, attack intervals, and defense.
Create `Assets/Scripts/AI/SimpleAI.cs`:
```csharp
using UnityEngine;
using ToughLoveArena.Core;
using ToughLoveArena.Input;

namespace ToughLoveArena.AI
{
    public static class SimpleAI
    {
        private static int _cooldownTicks = 0;

        public static void UpdateAIInput(PlayerData ai, PlayerData opponent, PlayerInputBuffer aiBuffer)
        {
            aiBuffer.Clear();

            // Handle recovery cooling down
            if (_cooldownTicks > 0)
            {
                _cooldownTicks--;
                return;
            }

            if (ai.State != ActionState.Idle && ai.State != ActionState.Crouch) return;

            int dist = Mathf.Abs(opponent.GridX - ai.GridX);
            int facingDirection = (int)ai.Facing;

            // Defensive reaction: Block if opponent is active in attack frames and in range
            bool isOpponentAttacking = (opponent.State == ActionState.AttackLight || 
                                        opponent.State == ActionState.AttackHeavy || 
                                        opponent.State == ActionState.AttackSpecial);
            
            if (isOpponentAttacking && dist <= 2)
            {
                // 70% chance to hold block (retreat) on normal difficulty
                if (Random.value < 0.70f)
                {
                    aiBuffer.MoveDirection = -facingDirection; // hold retreat (Block)
                    return;
                }
            }

            // Offensive Spacing Behaviors
            if (dist > 2)
            {
                // Advance forward
                aiBuffer.MoveDirection = facingDirection;
            }
            else if (dist == 2)
            {
                // Mix up: Jump, Attack Heavy, or Attack Special
                float val = Random.value;
                if (val < 0.3f)
                {
                    aiBuffer.AttackHeavyPressed = true;
                    _cooldownTicks = 40;
                }
                else if (val < 0.6f)
                {
                    aiBuffer.AttackSpecialPressed = true;
                    _cooldownTicks = 45;
                }
                else
                {
                    aiBuffer.MoveDirection = facingDirection; // Step closer
                }
            }
            else if (dist == 1)
            {
                // Close range mixup: Light Punch or Back away
                float val = Random.value;
                if (val < 0.5f)
                {
                    aiBuffer.AttackLightPressed = true;
                    _cooldownTicks = 25;
                }
                else
                {
                    aiBuffer.MoveDirection = -facingDirection; // Step back
                    _cooldownTicks = 12;
                }
            }
        }
    }
}
```

- [ ] **Step 2: Integrate AI into GameManager.cs**

Modify `ProcessInputs()` in `Assets/Scripts/Core/GameManager.cs` to hook up the AI's inputs when `IsAiOpponent` is enabled.
Modify `Assets/Scripts/Core/GameManager.cs`:
```csharp
// Inside GameManager.cs ProcessInputs() method:
private void ProcessInputs()
{
    // ... Player 1 poll ...

    // Player 2 / AI poll
    P2Input.Clear();
    if (IsAiOpponent)
    {
        ToughLoveArena.AI.SimpleAI.UpdateAIInput(Player2, Player1, P2Input);
    }
    else
    {
        // ... Original Player 2 Keyboard Poll ...
        if (UnityEngine.Input.GetKey(KeyCode.LeftArrow)) P2Input.MoveDirection = -1;
        else if (UnityEngine.Input.GetKey(KeyCode.RightArrow)) P2Input.MoveDirection = 1;
        if (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow)) P2Input.JumpPressed = true;
        if (UnityEngine.Input.GetKey(KeyCode.DownArrow)) P2Input.CrouchHeld = true;
        if (UnityEngine.Input.GetKeyDown(KeyCode.Keypad1) || UnityEngine.Input.GetKeyDown(KeyCode.I)) P2Input.AttackLightPressed = true;
        if (UnityEngine.Input.GetKeyDown(KeyCode.Keypad2) || UnityEngine.Input.GetKeyDown(KeyCode.O)) P2Input.AttackHeavyPressed = true;
        if (UnityEngine.Input.GetKeyDown(KeyCode.Keypad3) || UnityEngine.Input.GetKeyDown(KeyCode.P)) P2Input.AttackSpecialPressed = true;
    }
}
```

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/AI/SimpleAI.cs Assets/Scripts/Core/GameManager.cs
git commit -m "feat: implement spacing-based AI opponent with defensive reaction blocking"
```

---

## Verification Plan

### Automated Tests
*   Since this is a WebGL Unity project, we can run unit checks using the **Unity Test Runner** framework (in EditMode/PlayMode) once the scripts are loaded.
*   Logical coordinates verification script to assert mathematical collision boundaries (`ResolvePushing` / `ResolveHitDetection`).

### Manual Verification
1.  **Stage Movement**: Open the scene in Unity. Move P1 (A/D) and P2 (Arrow Keys). Verify players cannot walk off the 0-12 boundaries and cannot clip through each other on the ground.
2.  **Visual Lerp**: Verify that when keys are tapped, characters slide smoothly to the next cell instead of snapping abruptly.
3.  **Hurtbox and Hitstun**: Trigger Light Attack (P1: F) and Heavy Attack (P1: G). Verify that P2 enters hitstun, flashes red (if rendering is active), decreases in health, and gets pushed back by the exact knockback steps.
4.  **Block Mechanic**: Move players in range. Tap P1's attack while holding the retreat direction key for P2. Verify P2 performs a block animation, takes blockstun, and receives minor/zero damage.
5.  **AI Test**: Toggle `IsAiOpponent = true` on the GameManager. Verify P2 acts autonomously, steps forward when far, attacks when in range, and defends against incoming punches.
