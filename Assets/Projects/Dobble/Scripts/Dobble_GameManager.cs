using System.Collections.Generic;
using UnityEngine;
using Tool;
using UnityEngine.UI;
using System;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using MenuSelection;
using UnityEngine.SocialPlatforms.Impl;
using ClipperLib;
using UnityEngine.Events;


using System.IO;
using System.Linq;


namespace Dobble
{
    public class Dobble_GameManager : MonoBehaviour
    {

        public static Dobble_GameManager i { get; private set; }

        [Header("Prefabs")]
        [SerializeField] private GameObject symbolPrefab;

        [Header("Settings")]
        [SerializeField] private int numberOfSymbolsOnCard = 6;
        public int NumberOfSymbolsOnCard
        {
            get { return numberOfSymbolsOnCard; }
            set { numberOfSymbolsOnCard = value; }
        }

        [SerializeField] private bool shuffleSymbolsOnCard = false;


        [Header("Personnalisation")]
        [SerializeField] private string teamFolderName;

        [SerializeField] private DobbleTeam[] _teams;
        public IReadOnlyList<DobbleTeam> Teams => _teams;

        private List<Card> cards = new();
        private List<Dobble_ButtonLinked> symbols = new();

        [SerializeField] private Dobble_LoadData loader;
        [SerializeField] public RectTransform refTransform;
        [SerializeField] private Dobble_TeamManager teamManager;
        [SerializeField] private Dobble_Circles circlesGenerator;

        private List<string> symbolsNames = new();
        public IReadOnlyList<string> SymbolsNames => symbolsNames;


        public float shrinkFactor;

        public event Action OnGameEnded;

        [SerializeField] private float gameTime = 60f;
        [SerializeField] private TMP_Text timerText;
        public float GameTime
        {
            get => gameTime;
            set
            {
                gameTime = value;
                timerText.text = Mathf.CeilToInt(gameTime).ToString();
                if (gameTime < 10)
                {
                    timerText.color = Color.red;
                }

            }

        }

        private bool isGameOver = false;

        [Header("UI")]
        [SerializeField] private GameObject VictoryPanel;
        [SerializeField] private TMP_Text VictoryText;

        [HideInInspector] public UnityEvent<int> OnGlobalStageArrived = new UnityEvent<int>();
        [SerializeField] public Dobble_SoundManager soundManager;


        private void Awake()
        {
            i = this;
        }


        private void Update()
        {
            if (isGameOver) return;

            GameTime -= Time.deltaTime;
            if (GameTime <= 0)
            {
                if (isGameOver) return;
                isGameOver = true;
                OnGameEnded?.Invoke();
            }
        }



        private void Start()
        {

            OnGameEnded += () => StartCoroutine(ShowGameOverPanel(_teams));
            GameTime = Dobble_GeneralVariable.GetSceneDurationFromPrefs();
            soundManager.PlayMelody(0);
            LoadSymbols();
            NumberOfSymbolsOnCard = Dobble_GeneralVariable.GetNbrOfSymbolsFromPrefs();
            shrinkFactor = 1f - 0.15f * (NumberOfSymbolsOnCard - 4);
            cards = GenerateDobbleCards();
            cards.Shuffle();

            OnGlobalStageArrived.AddListener(idx => soundManager.OnNext(idx));

            foreach (var s in symbols)
                s.gameObject.SetActive(false);

            for (int j = 0; j < _teams.Length; j++)
            {
                teamManager.SubscribeTeam(_teams[j]);
                QueueNextCard(CardType.Buttons, _teams[j]);
                string playerName = Dobble_GeneralVariable.GetPlayerNamesFromPrefs(j);
                if (string.IsNullOrEmpty(playerName))
                {
                    playerName = "Team " + (j + 1);
                }
                _teams[j].SetPlayerName(playerName);
            }
            QueueNextCard(CardType.Reference);
        }
        private void LoadSymbols()
        {
            symbols = loader.LoadPersonnalisationData(teamFolderName);
        }

        private void CreateCard(CardType type, Card card, DobbleTeam team = null, bool isPlayerCard = false)
        {
            if (type == CardType.Reference)
            {
                StartCoroutine(CreateCardRef(card, isPlayerCard));
            }
            else
            {
                if (team != null)
                {
                    StartCoroutine(CreateTeamCard(card, team));
                    team.card = card;
                }

            }
        }

        private IEnumerator ShowGameOverPanel(DobbleTeam[] team)
        {
            int maxScore = int.MinValue;
            DobbleTeam bestTeam = null;
            foreach (var t in team)
            {
                if (t.Score > maxScore)
                {
                    maxScore = t.Score;
                    bestTeam = t;
                    VictoryText.text = $"{Localizer.Get("Bravo")} {t.PlayerNames},{Localizer.Get("Won")} !";
                    PlayerPrefs.SetFloat(Dobble_GeneralVariable.HighScoreKey, maxScore);
                }
            }




            var victoryPanel = Instantiate(
                VictoryPanel,
                bestTeam.TeamTransform.parent
            );
            victoryPanel.transform.localPosition = bestTeam.TeamTransform.localPosition;
            victoryPanel.SetActive(true);
            soundManager.PlayOneShot("Game Show Brass Jingle 1");

            OnGlobalStageArrived?.Invoke(1);


            yield return new WaitForSeconds(8f);
            if (BuildState.CurrentState == BuildState.State.normal)
            {
                SceneManager.LoadScene(Dobble_GeneralVariable.i.scoreScene);
            }
            else
            {
                MenuSelectionButton.Instance.gameObject.SetActive(true);

            }
        }
        private List<Card> GenerateDobbleCards()
        {
            List<Card> teamCards = new List<Card>();
            int n = numberOfSymbolsOnCard - 1;

            // --- Étape 1 : Vérifier si n est premier ---
            if (!IsPrime(n))
            {
                int originalN = n;
                int correctedN = GetClosestLowerPrime(n);
                Debug.LogWarning($"⚠️ {originalN} n'est pas premier. Utilisation de {correctedN} pour générer le jeu de base.");

                // Génère le jeu basé sur le nombre premier inférieur
                teamCards = GenerateBaseDobble(correctedN);

                int extraSymbolsPerCard = numberOfSymbolsOnCard - (correctedN + 1);
                if (extraSymbolsPerCard > 0)
                {
                    Debug.Log($"Ajout de {extraSymbolsPerCard} symbole(s) bonus par carte pour atteindre {numberOfSymbolsOnCard} symboles.");
                    int nextSymbol = teamCards.SelectMany(c => c.symbols).Max() + 1;

                    for (int i = 0; i < teamCards.Count; i++)
                    {
                        for (int b = 0; b < extraSymbolsPerCard; b++)
                        {
                            teamCards[i].AddSymbol(nextSymbol++);
                        }
                    }
                }
            }
            else
            {
                teamCards = GenerateBaseDobble(n);
            }

            if (shuffleSymbolsOnCard)
            {
                foreach (var card in teamCards)
                    card.symbols.Shuffle();
            }

            return teamCards;
        }

        private List<Card> GenerateBaseDobble(int n)
        {
            List<Card> teamCards = new List<Card>();

            // Première série
            for (int i = 0; i < n + 1; i++)
            {
                Card card = new Card(n + 1);
                card.AddSymbol(0);
                for (int j = 0; j < n; j++)
                    card.AddSymbol((j + 1) + (i * n));
                teamCards.Add(card);
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Card card = new Card(n + 1);
                    card.AddSymbol(i + 1);
                    for (int k = 0; k < n; k++)
                        card.AddSymbol((n + 1 + n * k + (i * k + j) % n));
                    teamCards.Add(card);
                }
            }

            return teamCards;
        }
        private bool IsPrime(int num)
        {
            if (num < 2) return false;
            for (int i = 2; i * i <= num; i++)
                if (num % i == 0) return false;
            return true;
        }
        private int GetClosestLowerPrime(int n)
        {
            for (int i = n - 1; i >= 2; i--)
                if (IsPrime(i)) return i;
            return 2;
        }

        /*

        public void QueueNextCard(CardType type, DobbleTeam team = null, Card pastcard = null)
        {
            Card firstCard = cards[0];
            if (pastcard != null)
            {
                firstCard = pastcard;
                CreateCard(type, firstCard, team, true);
            }
            else
            {
                CreateCard(type, firstCard, team);

            }
            cards.RemoveAt(0);
            cards.Add(firstCard);
        }

        */

        public void QueueNextCard(CardType type, DobbleTeam team = null, Card pastcard = null)
        {
            Card selectedCard = null;

            if (pastcard != null)
            {
                selectedCard = pastcard;
            }
            else
            {
                for (int i = 0; i < cards.Count; i++)
                {
                    Card potentialCard = cards[i];
                    bool isAlreadyUsed = false;
                    foreach (var t in _teams)
                    {
                        if (t != team && t.card != null && AreCardsIdentical(potentialCard, t.card))
                        {
                            isAlreadyUsed = true;
                            break;
                        }
                    }

                    if (!isAlreadyUsed)
                    {
                        selectedCard = potentialCard;
                        cards.RemoveAt(i); 
                        cards.Add(selectedCard); 
                        break;
                    }
                }
            }

            if (selectedCard != null)
            {
                CreateCard(type, selectedCard, team, pastcard != null);
            }
        }
        private bool AreCardsIdentical(Card a, Card b)
        {
            if (a.symbols.Count != b.symbols.Count) return false;
            return a.symbols.All(s => b.symbols.Contains(s));
        }


        private IEnumerator CreateTeamCard(Card card, DobbleTeam team)
        {
            ClearObjects<Dobble_ButtonLinked>(team.TeamTransform, team.linkedButtons);

            yield return new WaitForSeconds(0.5f);
            //circlesGenerator.GenerateCircles(team.TeamTransform,100,160);
            soundManager.PlayOneShot("nouvelle carte");
            circlesGenerator.GenerateCircles(team.TeamTransform, 100, 150, numberOfSymbolsOnCard);
            List<RectTransform> circleSlots = circlesGenerator.GetCircleRects();
            circleSlots.Shuffle();



            int symbolCount = Mathf.Min(card.symbols.Count, circleSlots.Count);

            const float shrinkFactor = 0.85f;

            for (int i = 0; i < symbolCount; i++)
            {
                int index = card.symbols[i];
                if (index >= symbols.Count) continue;

                RectTransform circleSlot = circleSlots[i];

                Dobble_ButtonLinked symbolInstance = Instantiate(symbols[index], circleSlot);
                symbolInstance.name = symbols[index].name;
                symbolInstance.gameObject.SetActive(true);

                RectTransform rt = symbolInstance.GetComponent<RectTransform>();

                float diameter = circleSlot.sizeDelta.x * shrinkFactor;
                Image img = symbolInstance.GetComponent<Image>();
                float w = img.sprite.rect.width;
                float h = img.sprite.rect.height;

                float scale = diameter / Mathf.Max(w, h);
                rt.sizeDelta = new Vector2(w * scale, h * scale);
                symbolInstance.collider.radius = diameter / 2f;

                rt.localPosition = Vector3.zero;
                rt.localRotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(0f, 360f));

                team.linkedButtons.Add(symbolInstance);
                symbolInstance.OwningTeam = team;
            }

            yield return null;
        }




        private IEnumerator CreateCardRef(Card card, bool isPlayerCard)
        {
            symbolsNames.Clear();
            ClearObjects<Transform>(refTransform);
            yield return new WaitForSeconds(0.5f);

            if (!isPlayerCard)
            {
                circlesGenerator.GenerateCircles(refTransform, 40, 60, numberOfSymbolsOnCard);
                List<RectTransform> circleSlots = circlesGenerator.GetCircleRects();
                circleSlots.Shuffle();
                int symbolCount = Mathf.Min(card.symbols.Count, circleSlots.Count);

                for (int i = 0; i < symbolCount; i++)
                {
                    int index = card.symbols[i];
                    if (index >= symbols.Count) continue;

                    RectTransform circleSlot = circleSlots[i];

                    GameObject symbolGO = Instantiate(symbolPrefab, circleSlot);

                    Image symbolImage = symbolGO.GetComponent<Image>();
                    symbolImage.sprite = symbols[index]._rightSprite;
                    symbolGO.name = symbols[index].name;
                    symbolsNames.Add(symbols[index].buttonName);
                    symbolImage.color = Color.white;
                    symbolGO.SetActive(true);

                    RectTransform rt = symbolGO.GetComponent<RectTransform>();
                    float diameter = circleSlot.sizeDelta.x * shrinkFactor;
                    float w = symbolImage.sprite.rect.width;
                    float h = symbolImage.sprite.rect.height;
                    float scale = diameter / Mathf.Max(w, h);
                    rt.sizeDelta = new Vector2(w * scale, h * scale);
                    rt.localPosition = Vector3.zero;
                    rt.localRotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(0f, 360f));
                }

            }
            else
            {
                int symbolCount = refTransform.childCount;
                for (int i = 0; i < symbolCount; i++)
                {
                    int index = card.symbols[i];
                    if (index >= symbols.Count) continue;
                    refTransform.GetChild(i).gameObject.name = "à supprimer";

                    symbolsNames.Add(symbols[index].buttonName);
                }
            }

            UpdateAllTeamsCorrectButton();
        }


        public void ClearObjects<T>(RectTransform parents, List<T> objects = null) where T : Component
        {
            foreach (Transform parent in parents)
            {
                if (parent.gameObject.name != "test")
                    Destroy(parent.gameObject);
            }

            if (objects != null)
                objects.Clear();
        }

        public void UpdateAllTeamsCorrectButton()
        {
            foreach (var team in _teams)
            {
                team.UpdateCorrectButton();
            }
        }



    }

    [System.Serializable]
    public class Card
    {
        public List<int> symbols;

        public Card(int capacity)
        {
            symbols = new List<int>(capacity);
        }

        public void AddSymbol(int symbol)
        {
            symbols.Add(symbol);
        }
    }

    public enum CardType
    {
        Reference = 0,
        Buttons = 1
    }
}
