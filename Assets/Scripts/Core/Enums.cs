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
