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
