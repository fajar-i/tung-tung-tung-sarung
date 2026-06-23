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
