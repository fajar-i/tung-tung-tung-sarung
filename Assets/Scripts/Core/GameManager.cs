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
