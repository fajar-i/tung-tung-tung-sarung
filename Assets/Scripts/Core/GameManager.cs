using System.Collections;
using UnityEngine;
using ToughLoveArena.Core;
using ToughLoveArena.Input;

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

        public PlayerInputBuffer P1Input = new PlayerInputBuffer();
        public PlayerInputBuffer P2Input = new PlayerInputBuffer();

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
