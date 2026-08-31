using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

namespace Dame
{
    public enum GameState { Idle, PieceSelected, Animating, GameOver }

    public class Dame_GameManager : MonoBehaviour
    {
        public static Dame_GameManager Instance { get; private set; }

        [Header("Board")]
        public Dame_Board board;
        public int boardSize = 10;

        [Header("UI")]
        public TextMeshProUGUI timerText;
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI currentPlayerText;
        public GameObject indicatorPrefab;

        [Header("Audio")]
        public AudioClip moveSound;
        public AudioClip captureSound;
        public AudioClip crownSound;
        public AudioClip winSound;
        private AudioSource audioSource;

        // État du jeu
        public GameState state { get; private set; }
        public int currentPlayer { get; private set; } // 1 = blanc, 2 = noir
        public int scorePlayer1 { get; private set; }
        public int scorePlayer2 { get; private set; }
        public int turnCount { get; private set; }

        // Sélection
        private Dame_Cell selectedCell;
        private Dame_Cell mustCaptureCell; // Pour les prises obligatoires
        private List<Dame_Cell> validMoves;

        // Timer
        private float timePerMove = 15f;
        private float currentTime;
        private bool isChainCapture = false;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            // Charger les références
            LoadReferences();

            // Lire le temps par coup depuis PlayerPrefs
            timePerMove = PlayerPrefs.GetFloat(Dame_GeneralVariables.TimePerMoveKey, 15f);

            // Initialiser le plateau
            board.InitializeBoard(boardSize);

            // Commencer la partie
            StartGame();
        }

        void LoadReferences()
        {
            moveSound = Resources.Load<AudioClip>("Sounds/move");
            captureSound = Resources.Load<AudioClip>("Sounds/capture");
            crownSound = Resources.Load<AudioClip>("Sounds/crown");
            winSound = Resources.Load<AudioClip>("Sounds/win");

            // UI
            if (timerText == null)
            {
                var go = GameObject.Find("TimerText");
                if (go != null) timerText = go.GetComponent<TextMeshProUGUI>();
            }
            if (scoreText == null)
            {
                var go = GameObject.Find("ScoreText");
                if (go != null) scoreText = go.GetComponent<TextMeshProUGUI>();
            }
            if (currentPlayerText == null)
            {
                var go = GameObject.Find("CurrentPlayerText");
                if (go != null) currentPlayerText = go.GetComponent<TextMeshProUGUI>();
            }
        }

        void StartGame()
        {
            currentPlayer = 1;
            scorePlayer1 = 0;
            scorePlayer2 = 0;
            turnCount = 0;
            state = GameState.Idle;
            selectedCell = null;
            validMoves = null;
            isChainCapture = false;

            ResetTimer();
            UpdateUI();
        }

        void Update()
        {
            if (state == GameState.GameOver || state == GameState.Animating) return;

            // Timer
            currentTime -= Time.deltaTime;
            if (timerText != null)
                timerText.text = Mathf.CeilToInt(currentTime).ToString();

            if (currentTime <= 0)
            {
                // Temps écoulé → perd son tour
                OnTimeout();
            }
        }

        void ResetTimer()
        {
            currentTime = timePerMove;
        }

        // Appelé quand le joueur touche une case
        public void OnCellTouched(Dame_Cell cell)
        {
            if (state == GameState.GameOver || state == GameState.Animating) return;

            switch (state)
            {
                case GameState.Idle:
                    HandleCellSelection(cell);
                    break;
                case GameState.PieceSelected:
                    HandleCellDestination(cell);
                    break;
            }
        }

        void HandleCellSelection(Dame_Cell cell)
        {
            Dame_Piece piece = cell.GetPiece();
            if (piece == null) return;

            // Vérifier que la pièce appartient au joueur courant
            if (piece.playerNumber != currentPlayer) return;

            // Vérifier les prises obligatoires
            Dame_Cell forcedCapture = GetForcedCapture(currentPlayer);
            if (forcedCapture != null && forcedCapture != cell)
            {
                // Le joueur DOIT prendre avec cette pièce
                var forcedMoves = forcedCapture.GetValidCaptures();
                if (forcedMoves.Count > 0)
                {
                    // Forcer la sélection de cette pièce
                    SelectPiece(forcedCapture);
                    return;
                }
            }

            // Vérifier que la pièce a des mouvements valides
            var moves = cell.GetValidMoves();
            var captures = cell.GetValidCaptures();
            if (moves.Count == 0 && captures.Count == 0) return;

            SelectPiece(cell);
        }

        void SelectPiece(Dame_Cell cell)
        {
            selectedCell = cell;
            state = GameState.PieceSelected;

            // Calculer les mouvements valides
            validMoves = cell.GetValidMoves();
            var captures = cell.GetValidCaptures();

            // Si des prises sont disponibles, ce sont les seuls mouvements autorisés
            if (captures.Count > 0)
                validMoves = captures;

            // Afficher les mouvements possibles
            board.ShowValidMoves(validMoves);
            cell.SetHighlight(true);
        }

        void HandleCellDestination(Dame_Cell cell)
        {
            if (selectedCell == null) return;

            // Si le joueur retouche la même pièce, désélectionner
            if (cell == selectedCell)
            {
                DeselectPiece();
                return;
            }

            // Si le joueur touche une autre de ses pièces, changer de sélection
            Dame_Piece otherPiece = cell.GetPiece();
            if (otherPiece != null && otherPiece.playerNumber == currentPlayer)
            {
                DeselectPiece();
                HandleCellSelection(cell);
                return;
            }

            // Vérifier que la destination est valide
            if (!validMoves.Contains(cell))
            {
                // Mouvement invalide
                return;
            }

            // Exécuter le mouvement
            bool isCapture = Mathf.Abs(cell.row - selectedCell.row) == 2;
            ExecuteMove(selectedCell, cell, isCapture);
        }

        void ExecuteMove(Dame_Cell from, Dame_Cell to, bool isCapture)
        {
            state = GameState.Animating;
            board.ClearHighlights();

            Dame_Piece piece = from.GetPiece();
            if (piece == null) return;

            // Déplacer la pièce
            from.SetPiece(null);
            to.SetPiece(piece);
            piece.transform.position = to.transform.position;
            piece.currentCell = to;

            // Animation simple
            StartCoroutine(MoveAnimation(piece.transform, to.transform.position, 0.15f));

            // Capture
            if (isCapture)
            {
                // Trouver la pièce capturée (entre from et to)
                int capturedRow = (from.row + to.row) / 2;
                int capturedCol = (from.col + to.col) / 2;
                Dame_Cell capturedCell = board.GetCell(capturedRow, capturedCol);
                if (capturedCell != null)
                {
                    Dame_Piece capturedPiece = capturedCell.GetPiece();
                    if (capturedPiece != null)
                    {
                        // Supprimer la pièce capturée
                        capturedCell.SetPiece(null);
                        Destroy(capturedPiece.gameObject);

                        // Score
                        if (currentPlayer == 1) scorePlayer1++;
                        else scorePlayer2++;

                        // Son de capture
                        if (captureSound != null)
                            audioSource.PlayOneShot(captureSound);

                        // Effet popup
                        SpawnCapturePopup(capturedCell.transform.position);
                    }
                }

                // Vérifier les captures en chaîne
                var chainCaptures = to.GetValidCaptures();
                if (chainCaptures.Count > 0 && piece.isCrowned)
                {
                    // La dame peut continuer à capturer
                    selectedCell = to;
                    validMoves = chainCaptures;
                    state = GameState.PieceSelected;
                    board.ShowValidMoves(validMoves);
                    isChainCapture = true;
                    ResetTimer();
                    return;
                }
                else if (chainCaptures.Count > 0 && !piece.isCrowned)
                {
                    // Un pion peut aussi capturer en chaîne
                    selectedCell = to;
                    validMoves = chainCaptures;
                    state = GameState.PieceSelected;
                    board.ShowValidMoves(validMoves);
                    isChainCapture = true;
                    ResetTimer();
                    return;
                }
            }

            // Couronnement
            if (ShouldCrown(to, piece))
            {
                piece.Crown();
                if (crownSound != null)
                    audioSource.PlayOneShot(crownSound);
                SpawnCrownPopup(to.transform.position);
            }

            // Son de mouvement
            if (moveSound != null && !isCapture)
                audioSource.PlayOneShot(moveSound);

            // Fin du tour
            EndTurn();
        }

        IEnumerator MoveAnimation(Transform piece, Vector3 target, float duration)
        {
            Vector3 start = piece.position;
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                piece.position = Vector3.Lerp(start, target, t / duration);
                yield return null;
            }
            piece.position = target;
            state = GameState.Idle;
        }

        bool ShouldCrown(Dame_Cell cell, Dame_Piece piece)
        {
            if (piece.isCrowned) return false;
            // Pion blanc (joueur 1) couronné à la ligne 0 (haut)
            if (piece.playerNumber == 1 && cell.row == 0) return true;
            // Pion noir (joueur 2) couronné à la ligne 9 (bas)
            if (piece.playerNumber == 2 && cell.row == boardSize - 1) return true;
            return false;
        }

        void EndTurn()
        {
            ResetTimer();
            selectedCell = null;
            validMoves = null;
            isChainCapture = false;

            // Vérifier la victoire
            if (CheckWin())
            {
                EndGame();
                return;
            }

            // Changer de joueur
            currentPlayer = (currentPlayer == 1) ? 2 : 1;
            turnCount++;

            // Vérifier si le joueur suivant peut jouer
            if (!HasValidMoves(currentPlayer))
            {
                // Le joueur ne peut pas jouer → il perd
                EndGame();
                return;
            }

            // Forcer une prise si disponible
            var forced = GetForcedCapture(currentPlayer);
            if (forced != null)
            {
                var captures = forced.GetValidCaptures();
                if (captures.Count > 0)
                {
                    // Auto-sélectionner la pièce qui doit capturer
                    SelectPiece(forced);
                }
            }

            UpdateUI();
        }

        bool HasValidMoves(int player)
        {
            for (int r = 0; r < boardSize; r++)
            {
                for (int c = 0; c < boardSize; c++)
                {
                    var cell = board.GetCell(r, c);
                    if (cell == null) continue;
                    var piece = cell.GetPiece();
                    if (piece == null || piece.playerNumber != player) continue;
                    if (cell.GetValidMoves().Count > 0 || cell.GetValidCaptures().Count > 0)
                        return true;
                }
            }
            return false;
        }

        Dame_Cell GetForcedCapture(int player)
        {
            // Chercher une pièce qui DOIT capturer (prise obligatoire)
            for (int r = 0; r < boardSize; r++)
            {
                for (int c = 0; c < boardSize; c++)
                {
                    var cell = board.GetCell(r, c);
                    if (cell == null) continue;
                    var piece = cell.GetPiece();
                    if (piece == null || piece.playerNumber != player) continue;
                    var captures = cell.GetValidCaptures();
                    if (captures.Count > 0) return cell;
                }
            }
            return null;
        }

        bool CheckWin()
        {
            int whitePieces = 0, blackPieces = 0;
            for (int r = 0; r < boardSize; r++)
            {
                for (int c = 0; c < boardSize; c++)
                {
                    var cell = board.GetCell(r, c);
                    if (cell == null) continue;
                    var piece = cell.GetPiece();
                    if (piece == null) continue;
                    if (piece.playerNumber == 1) whitePieces++;
                    else blackPieces++;
                }
            }
            return whitePieces == 0 || blackPieces == 0;
        }

        void OnTimeout()
        {
            // Perte de tour
            if (isChainCapture)
            {
                // Si le joueur était en pleine chaîne, il perd quand même
            }
            EndTurn();
        }

        void DeselectPiece()
        {
            if (selectedCell != null)
                selectedCell.SetHighlight(false);
            selectedCell = null;
            validMoves = null;
            state = GameState.Idle;
            board.ClearHighlights();
        }

        void UpdateUI()
        {
            if (scoreText != null)
                scoreText.text = $"Blanc: {scorePlayer1}  |  Noir: {scorePlayer2}";

            if (currentPlayerText != null)
            {
                string playerName = currentPlayer == 1 ? "Blancs" : "Noirs";
                currentPlayerText.text = $"Tour des {playerName}";
                currentPlayerText.color = currentPlayer == 1 ? Color.white : Color.black;
            }
        }

        void SpawnCapturePopup(Vector3 pos)
        {
            Texture2D tex = Resources.Load<Texture2D>("Textures/etoile");
            if (tex == null) return;
            var go = new GameObject("CapturePopup", typeof(SpriteRenderer));
            go.transform.position = pos + Vector3.up * 0.5f;
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            sr.color = Color.yellow;
            sr.sortingOrder = 10;
            Destroy(go, 0.5f);
        }

        void SpawnCrownPopup(Vector3 pos)
        {
            Texture2D tex = Resources.Load<Texture2D>("Textures/dame_blanche");
            if (tex == null) return;
            var go = new GameObject("CrownPopup", typeof(SpriteRenderer));
            go.transform.position = pos + Vector3.up * 0.8f;
            go.transform.localScale = Vector3.one * 0.5f;
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            sr.color = Color.yellow;
            sr.sortingOrder = 10;
            Destroy(go, 1f);
        }

        void EndGame()
        {
            state = GameState.GameOver;

            int winner = 0;
            int whitePieces = 0, blackPieces = 0;
            for (int r = 0; r < boardSize; r++)
            {
                for (int c = 0; c < boardSize; c++)
                {
                    var cell = board.GetCell(r, c);
                    if (cell == null) continue;
                    var piece = cell.GetPiece();
                    if (piece == null) continue;
                    if (piece.playerNumber == 1) whitePieces++;
                    else blackPieces++;
                }
            }
            if (whitePieces == 0) winner = 2;
            else if (blackPieces == 0) winner = 1;
            else winner = (currentPlayer == 1) ? 2 : 1; // Le joueur qui ne peut pas jouer perd

            if (winSound != null)
                audioSource.PlayOneShot(winSound);

            // Sauvegarder le score
            PlayerPrefs.SetInt("Dame_Winner", winner);
            PlayerPrefs.SetInt("Dame_ScoreP1", scorePlayer1);
            PlayerPrefs.SetInt("Dame_ScoreP2", scorePlayer2);

            StartCoroutine(TransitionToScore());
        }

        IEnumerator TransitionToScore()
        {
            yield return new WaitForSeconds(2f);
            SceneManager.LoadScene("Score_Dame");
        }
    }
}