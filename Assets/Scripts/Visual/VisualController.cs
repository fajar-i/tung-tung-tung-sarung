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

        [Header("Original Clip Lengths (Seconds)")]
        [Tooltip("Panjang clip animasi Idle bawaan dari FBX")]
        public float IdleClipLength = 2.533f;
        [Tooltip("Panjang clip animasi Jalan Depan bawaan dari FBX")]
        public float WalkFwdClipLength = 1.1f;
        [Tooltip("Panjang clip animasi Jalan Belakang bawaan dari FBX")]
        public float WalkBwdClipLength = 1.267f;
        [Tooltip("Panjang clip animasi Lompat bawaan dari FBX")]
        public float JumpClipLength = 0.833f;
        [Tooltip("Panjang clip animasi Jongkok bawaan dari FBX")]
        public float CrouchClipLength = 0.567f;
        [Tooltip("Panjang clip animasi Serang Ringan bawaan dari FBX")]
        public float LightAttackClipLength = 1.5f;
        [Tooltip("Panjang clip animasi Serang Berat bawaan dari FBX")]
        public float HeavyAttackClipLength = 3.533f;
        [Tooltip("Panjang clip animasi Serang Spesial bawaan dari FBX")]
        public float SpecialAttackClipLength = 1.2f;
        [Tooltip("Panjang clip animasi Kena Serang bawaan dari FBX")]
        public float HurtClipLength = 0.767f;
        [Tooltip("Panjang clip animasi Tangkis bawaan dari FBX")]
        public float BlockClipLength = 0.567f;
        [Tooltip("Panjang clip animasi Jatuh bawaan dari FBX")]
        public float KnockdownClipLength = 2.3f;

        [Header("Speed Synchronization Options")]
        [Tooltip("Jika TRUE, animasi akan dipercepat/diperlambat agar pas dengan durasi logical tick. Jika FALSE, menggunakan manual speed di bawah.")]
        public bool SyncSpeedToTicks = false;

        [Header("Manual Animation Speeds (Used when SyncSpeedToTicks is False)")]
        public float IdleSpeed = 1f;
        public float WalkFwdSpeed = 1.2f;
        public float WalkBwdSpeed = 1.2f;
        public float JumpSpeed = 1f;
        public float CrouchSpeed = 1f;
        public float LightAttackSpeed = 2f;
        public float HeavyAttackSpeed = 1.5f;
        public float SpecialAttackSpeed = 1.8f;
        public float HurtSpeed = 1.5f;
        public float BlockSpeed = 1.2f;
        public float KnockdownSpeed = 1.2f;

        [Header("Manual Speed Adjustments")]
        [Tooltip("Multiplier kecepatan tambahan untuk fine-tuning")]
        public float GlobalSpeedMultiplier = 1f;

        private PlayerData _myData;
        private ActionState _lastVisualState;

        private void Start()
        {
            if (CharacterAnimator == null)
            {
                CharacterAnimator = GetComponentInChildren<Animator>();
            }
            if (CharacterAnimator != null)
            {
                CharacterAnimator.applyRootMotion = false;
            }
        }

        private void Update()
        {
            // Sync current data reference from GameManager
            if (GameManager.Instance == null) return;
            
            _myData = (TargetPlayerId == 1) ? GameManager.Instance.Player1 : GameManager.Instance.Player2;
            if (_myData == null) return;

            // Force root motion off
            if (CharacterAnimator != null && CharacterAnimator.applyRootMotion)
            {
                CharacterAnimator.applyRootMotion = false;
            }

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
                    ScaleAnimatorClipSpeed(IdleClipLength, 1, IdleSpeed);
                    break;
                case ActionState.MoveForward:
                    CharacterAnimator.SetTrigger(WalkFwdTrigger);
                    ScaleAnimatorClipSpeed(WalkFwdClipLength, 6, WalkFwdSpeed);
                    break;
                case ActionState.MoveBackward:
                    CharacterAnimator.SetTrigger(WalkBwdTrigger);
                    ScaleAnimatorClipSpeed(WalkBwdClipLength, 6, WalkBwdSpeed);
                    break;
                case ActionState.Crouch:
                    CharacterAnimator.SetTrigger(CrouchTrigger);
                    ScaleAnimatorClipSpeed(CrouchClipLength, 1, CrouchSpeed);
                    break;
                case ActionState.Jump:
                    CharacterAnimator.SetTrigger(JumpTrigger);
                    ScaleAnimatorClipSpeed(JumpClipLength, 18, JumpSpeed);
                    break;
                case ActionState.AttackLight:
                    CharacterAnimator.SetTrigger(LightAttackTrigger);
                    ScaleAnimatorClipSpeed(LightAttackClipLength, 9, LightAttackSpeed);
                    break;
                case ActionState.AttackHeavy:
                    CharacterAnimator.SetTrigger(HeavyAttackTrigger);
                    ScaleAnimatorClipSpeed(HeavyAttackClipLength, 17, HeavyAttackSpeed);
                    break;
                case ActionState.AttackSpecial:
                    CharacterAnimator.SetTrigger(SpecialAttackTrigger);
                    ScaleAnimatorClipSpeed(SpecialAttackClipLength, 22, SpecialAttackSpeed);
                    break;
                case ActionState.HitStun:
                    CharacterAnimator.SetTrigger(HurtTrigger);
                    ScaleAnimatorClipSpeed(HurtClipLength, 12, HurtSpeed);
                    break;
                case ActionState.BlockStun:
                    CharacterAnimator.SetTrigger(BlockTrigger);
                    ScaleAnimatorClipSpeed(BlockClipLength, 8, BlockSpeed);
                    break;
                case ActionState.Knockdown:
                    CharacterAnimator.SetTrigger(KnockdownTrigger);
                    ScaleAnimatorClipSpeed(KnockdownClipLength, 35, KnockdownSpeed);
                    break;
            }
        }

        private void ScaleAnimatorClipSpeed(float originalClipLength, int logicalDurationTicks, float manualSpeed)
        {
            if (SyncSpeedToTicks)
            {
                float targetSecs = logicalDurationTicks / 60.0f;
                if (originalClipLength > 0.01f)
                {
                    CharacterAnimator.speed = (originalClipLength / targetSecs) * GlobalSpeedMultiplier;
                }
            }
            else
            {
                CharacterAnimator.speed = manualSpeed * GlobalSpeedMultiplier;
            }
        }
    }
}
