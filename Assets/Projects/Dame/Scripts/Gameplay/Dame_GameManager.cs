using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using OSC;
using MenuSelection;

namespace Dame
{
    public enum GameState { Idle, PieceSelected, Animating, GameOver }

    public class Dame_GameManager : ReceiveParent
    {
        public static Dame_GameManager Instance { get; private set; }

        [Header("Sprites (assignes par l'Editor tool)")]
        public Sprite caseFoncee;
        public Sprite caseClaire;
        public Sprite pionBlanc;
        public Sprite pionNoir;
        public Sprite dameBlanche;
        public Sprite dameNoire;

        [Header("Board")]
        public Dame_Board board;
        public int boardSize = 10;
        private Transform boardParent;

        [Header("UI")]
        public TextMeshProUGUI timerText;
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI currentPlayerText;

        [Header("Sons (assignes par l'Editor tool)")]
        public AudioClip moveSound;
        public AudioClip captureSound;
        public AudioClip crownSound;
        public AudioClip winSound;
        private AudioSource audioSource;

        private GameState state;
        public GameState State => state;
        public int currentPlayer { get; private set; }
        public int scorePlayer1 { get; private set; }
        public int scorePlayer2 { get; private set; }

        private Dame_Cell selectedCell;
        private List<Dame_Cell> validMoves;

        private float timePerMove = 15f;
        private float currentTime;
        private bool gameIsRunning = false;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            Debug.Log("<b>[Dame_GM] Start() appelé</b>");

            state = GameState.Idle;
            currentPlayer = 1;
            scorePlayer1 = 0;
            scorePlayer2 = 0;

            timePerMove = PlayerPrefs.GetFloat(Dame_GeneralVariables.TimePerMoveKey, 15f);

            boardParent = GameObject.Find("StructuresParent")?.transform;
            if (boardParent == null) boardParent = transform;

            var boardGO = new GameObject("Board", typeof(Dame_Board));
            boardGO.transform.SetParent(boardParent);
            board = boardGO.GetComponent<Dame_Board>();
            board.InitializeBoard(boardSize, caseFoncee, caseClaire, pionBlanc, pionNoir, dameBlanche, dameNoire);

            timerText = GameObject.Find("TimerText")?.GetComponent<TextMeshProUGUI>();
            scoreText = GameObject.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
            currentPlayerText = GameObject.Find("CurrentPlayerText")?.GetComponent<TextMeshProUGUI>();

            if (scoreText != null) scoreText.text = "0 - 0";
            if (timerText != null) timerText.text = Mathf.CeilToInt(timePerMove).ToString();
            if (currentPlayerText != null) currentPlayerText.text = "Tour des Blancs";

            OSC_Manager.Instance.receiveP = this;
            gameIsRunning = true;
        }

        void Update()
        {
            if (!gameIsRunning) return;
            currentTime -= Time.deltaTime;
            if (timerText != null) timerText.text = Mathf.CeilToInt(currentTime).ToString();
            if (currentTime <= 0) EndTurnTimeout();
        }

        public override void ReceivePoint(float xPoint, float yPoint)
        {
            Debug.Log($"<color=orange>[Dame_GameManager] ReceivePoint({xPoint:F3}, {yPoint:F3}) state={state} gameIsRunning={gameIsRunning}</color>");
            // Les cases recoivent directement les touches via Universal_Collider2DButton.ReceivePoint
            // On ne fait rien ici (les cases sont des Universal_Collider2DButton)
        }

        public void OnCellTouched(Dame_Cell cell)
        {
            Debug.Log($"<color=cyan>[Dame_GM] OnCellTouched({cell.row},{cell.col}) state={state} gameIsRunning={gameIsRunning}</color>");
            if (!gameIsRunning || state == GameState.Animating) { Debug.Log("<color=red>  -> BLOQUE: game not running or animating</color>"); return; }

            if (state == GameState.Idle) SelectPiece(cell);
            else if (state == GameState.PieceSelected) TryMove(cell);
        }

        void SelectPiece(Dame_Cell cell)
        {
            var piece = cell.GetPiece();
            if (piece == null) { Debug.Log($"<color=yellow>[SelectPiece] Cell({cell.row},{cell.col}) -> PAS DE PIECE</color>"); return; }
            if (piece.playerNumber != currentPlayer) { Debug.Log($"<color=yellow>[SelectPiece] Piece joueur={piece.playerNumber} != currentPlayer={currentPlayer}</color>"); return; }

            selectedCell = cell;
            state = GameState.PieceSelected;
            board.ClearHighlights();
            cell.SetHighlight(true);

            validMoves = cell.GetValidMoves();
            var captures = cell.GetValidCaptures();
            validMoves = captures.Count > 0 ? captures : validMoves;
            board.ShowValidMoves(validMoves);
            Debug.Log($"<color=green>[SelectPiece] OK! Cell({cell.row},{cell.col}) => {validMoves.Count} moves valides</color>");
            currentTime = timePerMove;
        }

        void TryMove(Dame_Cell targetCell)
        {
            Debug.Log($"<color=teal>[TryMove] target=({targetCell.row},{targetCell.col}) validCount={validMoves?.Count ?? 0}</color>");
            if (!validMoves.Contains(targetCell))
            {
                Debug.Log("<color=red>  -> MOVE INVALIDE, on deselectionne</color>");
                board.ClearHighlights();
                state = GameState.Idle;
                selectedCell = null;
                return;
            }

            bool isCapture = selectedCell.GetValidCaptures().Contains(targetCell);
            if (isCapture) ExecuteCapture(selectedCell, targetCell);
            else ExecuteMove(selectedCell, targetCell);
        }

        void ExecuteMove(Dame_Cell from, Dame_Cell to)
        {
            var piece = from.GetPiece();
            from.SetPiece(null);
            piece.currentCell = to;
            to.SetPiece(piece);
            piece.transform.position = to.transform.position;
            if (moveSound != null) audioSource.PlayOneShot(moveSound);
            EndTurn();
        }

        void ExecuteCapture(Dame_Cell from, Dame_Cell to)
        {
            var piece = from.GetPiece();
            var enemyRow = (from.row + to.row) / 2;
            var enemyCol = (from.col + to.col) / 2;
            var enemyCell = board.GetCell(enemyRow, enemyCol);
            var enemyPiece = enemyCell?.GetPiece();
            if (enemyPiece != null) { Destroy(enemyPiece.gameObject); enemyCell.SetPiece(null); }

            from.SetPiece(null);
            piece.currentCell = to;
            to.SetPiece(piece);
            piece.transform.position = to.transform.position;

            if (currentPlayer == 1) scorePlayer1++;
            else scorePlayer2++;
            UpdateScoreUI();
            if (captureSound != null) audioSource.PlayOneShot(captureSound);

            CheckCrown(piece, to);

            var moreCaptures = to.GetValidCaptures();
            if (moreCaptures.Count > 0)
            {
                selectedCell = to;
                validMoves = moreCaptures;
                board.ClearHighlights();
                to.SetHighlight(true);
                board.ShowValidMoves(moreCaptures);
                currentTime = timePerMove;
                return;
            }
            EndTurn();
        }

        void CheckCrown(Dame_Piece piece, Dame_Cell cell)
        {
            if (piece.isCrowned) return;
            if ((piece.playerNumber == 1 && cell.row == 0) || (piece.playerNumber == 2 && cell.row == boardSize - 1))
            {
                piece.Crown();
                if (crownSound != null) audioSource.PlayOneShot(crownSound);
            }
        }

        void EndTurn()
        {
            currentPlayer = currentPlayer == 1 ? 2 : 1;
            turnCount++;
            if (currentPlayerText != null)
                currentPlayerText.text = $"Tour des {(currentPlayer == 1 ? "Blancs" : "Noirs")}";
            state = GameState.Idle;
            selectedCell = null;
            validMoves = null;
            board.ClearHighlights();
            currentTime = timePerMove;
            CheckVictory();
        }

        void EndTurnTimeout() { EndTurn(); }

        void CheckVictory()
        {
            int opponent = currentPlayer == 1 ? 2 : 1;
            bool foundPiece = false;
            for (int r = 0; r < boardSize; r++)
                for (int c = 0; c < boardSize; c++)
                {
                    var cell = board.GetCell(r, c);
                    var piece = cell?.GetPiece();
                    if (piece != null && piece.playerNumber == opponent) foundPiece = true;
                }
            if (!foundPiece) EndGame(currentPlayer);
        }

        void UpdateScoreUI()
        {
            if (scoreText != null) scoreText.text = $"{scorePlayer1} - {scorePlayer2}";
        }

        void EndGame(int winner)
        {
            gameIsRunning = false;
            StopAllCoroutines();
            if (winSound != null) audioSource.PlayOneShot(winSound);
            PlayerPrefs.SetInt("Dame_FinalScore", winner == 1 ? scorePlayer1 : scorePlayer2);
            PlayerPrefs.SetFloat(Dame_GeneralVariables.HighScoreKey, winner == 1 ? scorePlayer1 : scorePlayer2);
            StartCoroutine(TransitionToScore(winner));
        }

        IEnumerator TransitionToScore(int winner)
        {
            yield return new WaitForSeconds(2f);
            if (BuildState.CurrentState == BuildState.State.normal)
                SceneManager.LoadScene(Dame_GeneralVariables.Instance.scoreScene);
            else
                MenuSelectionButton.Instance.gameObject.SetActive(true);
        }

        public int turnCount { get; private set; }
    }
}