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
