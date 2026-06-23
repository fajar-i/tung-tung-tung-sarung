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
                    defender.State = attacker.State == ActionState.AttackHeavy ? ActionState.Knockdown : ActionState.HitStun;
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
    }
}
