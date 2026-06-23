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
