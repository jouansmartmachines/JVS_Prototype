using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using System.Runtime.Serialization;
using TMPro;
using DG.Tweening;
using System.Collections;


namespace Dobble
{
    public class Dobble_TeamManager : MonoBehaviour
    {
        private bool isAnimating = false;
        private Dobble_GameManager g;

        void Awake()
        {
            g = Dobble_GameManager.i;
        }


        public void SubscribeTeam(DobbleTeam team)
        {
            team.OnCorrectButtonClicked += HandleTeamClick;
            team.OnFalseButtonClicked += PlayWrongAnimation;
            team.OnCardSwitched += HandleCardSwitched;
            g.OnGameEnded += () => UnsubscribeEveryone(team);
        }

        private void UnsubscribeEveryone(DobbleTeam team)
        {

            team.OnCorrectButtonClicked -=  HandleTeamClick;
            team.OnFalseButtonClicked -= PlayWrongAnimation;
            team.OnCardSwitched -=  HandleCardSwitched;
        }

        private void HandleTeamClick(DobbleTeam team, Dobble_ButtonLinked button)
        {
            if (isAnimating) return;

            // Lance la séquence normale
            PlayRightAnimation(button);
            g.soundManager.PlayOneShot("Correct");
            StartCoroutine(HandleButtonClicked(team));
 

            team.AddScore(1);
            PlayAnimation(team, "Correct");
        }

         private void PlayWrongAnimation(Dobble_ButtonLinked button)
        {
            if (isAnimating) return;
            if (button == null) return;
            g.soundManager.PlayOneShot("wrong");

            button.transform.DOKill();
         
            button.transform
                .DORotate(new Vector3(0, 15, 0f), 0.1f)
                .SetLoops(6, LoopType.Yoyo)
                .OnComplete(() => button.transform.rotation = Quaternion.identity);
        }

        private void PlayRightAnimation(Dobble_ButtonLinked button)
        {
            if (isAnimating) return;
            if (button == null) return;


            button.transform.DOKill();
            Vector3 originalScale = button.transform.localScale;

            Sequence seq = DOTween.Sequence();

            seq.Append(button.transform.DOScale(originalScale * 2f, 0.12f)
                .SetEase(Ease.OutBack)); 

            seq.Append(button.transform.DOScale(originalScale * 0.6f, 0.1f)
                .SetEase(Ease.InOutQuad)); 

            seq.Append(button.transform.DOScale(originalScale * 1.5f, 0.1f)
                .SetEase(Ease.OutQuad)); 

            seq.Append(button.transform.DOScale(originalScale, 0.18f)
                .SetEase(Ease.OutElastic)); 

            seq.OnComplete(() => button.transform.localScale = originalScale);
        }




        private IEnumerator HandleButtonClicked(DobbleTeam team)
        {
            yield return new WaitForSeconds(1.9f);
            team.NextCard();
        }

        private void HandleCardSwitched(DobbleTeam team)
        {
            Transform[] children = new Transform[team.TeamTransform.childCount];

            for (int i = 0; i < team.TeamTransform.childCount; i++)
                children[i] = team.TeamTransform.GetChild(i);

            foreach (Transform child in children)
            {
                child.SetParent(g.refTransform, true);
                foreach (var comp in child.GetComponents<MonoBehaviour>())
                {
                    Destroy(comp);
                }
                child.name = "test";
            }
            
            
            Card lastCard = team.card;
            g.QueueNextCard(CardType.Buttons, team);
            g.QueueNextCard(CardType.Reference,null,lastCard);

        }
        private void PlayAnimation(DobbleTeam team, string trigger)
        {


            isAnimating = true;
            team.teamAnimator.SetTrigger(trigger);
            team.hexagonAnimator.SetTrigger(trigger);
            Material mat = team.spriteHexagon.material;
            StartCoroutine(AnimateChildrenCoroutine(team.TeamTransform, 0.7f,mat,team));

            g.soundManager.PlayOneShot("glissement_carte");
        
        }


        private IEnumerator AnimateChildrenCoroutine(RectTransform parent, float duration, Material mat = null, DobbleTeam team = null)
        {
            Vector3 initialScale = parent.localScale;
            Vector3 targetScale = initialScale * 0.55f;
            mat.SetFloat("_Multiplier", 0f);

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime / 2;
                float t = Mathf.Clamp01(elapsed / duration);
                mat.SetFloat("_Multiplier", t);

                parent.localScale = Vector3.Lerp(initialScale, targetScale, t);

                yield return null;
            }

            yield return new WaitForSeconds(0.51f);

            mat.SetFloat("_Multiplier", 1f);
            mat.SetFloat("_Multiplier", 0f);
            team.hexagonAnimator.Play("Idle", 0, 0f);
            parent.localScale = initialScale;
            team.teamAnimator.Play("Idle", 0, 0f);
            isAnimating = false;
        }
    }

    [Serializable]
    public class DobbleTeam
    {
         

        [SerializeField] private TMP_Text playerNamesTxt; 
        private string playerNames;
        public string PlayerNames
        {
            get => playerNames;
            private set
            {
                playerNames = value;
                playerNamesTxt.text = playerNames.ToString();
            }
        }
        public void SetPlayerName(string name)
        {
            PlayerNames = name; 
        }
        [SerializeField] public Animator teamAnimator;
        [SerializeField] private RectTransform teamTransform;
        public RectTransform TeamTransform => teamTransform;

        public Card card;
       

        [SerializeField] private TMP_Text scoreText;
        
        [SerializeField] private int score = 0;
        public int Score
        {
            get => score;
            private set
            {
                score = value;
                scoreText.text = score.ToString();
            }
        }

        public List<Dobble_ButtonLinked> linkedButtons = new();

        public Dobble_ButtonLinked CorrectButton;

        public event Action<DobbleTeam , Dobble_ButtonLinked> OnCorrectButtonClicked;
        public event Action<Dobble_ButtonLinked> OnFalseButtonClicked;
        public event Action<DobbleTeam> OnCardSwitched;

        [SerializeField] public Animator hexagonAnimator;
        [SerializeField] public SpriteRenderer spriteHexagon;


        public void AddScore(int amount)
        {
            Score += amount;
        }

        public void ClearLinkedButtons()
        {
            linkedButtons.Clear();
        }

        public void AddLinkedButton(Dobble_ButtonLinked button)
        {
            if (button != null)
                linkedButtons.Add(button);
        }

        public void UpdateCorrectButton()
        {
            if (CorrectButton == null && linkedButtons.Count > 0)
                CorrectButton = linkedButtons[0];

            foreach (var name in Dobble_GameManager.i.SymbolsNames)
            {
                var found = linkedButtons.Find(b => b != null && b.buttonName == name);
                if (found != null)
                {
                    CorrectButton = found;
                    break;
                }
            }
        }

        public void NotifyButtonClicked(Dobble_ButtonLinked button)
        {
            if (CorrectButton == null || button == null)
                return;

            bool isCorrect = (button.buttonName == CorrectButton.buttonName);
            if (isCorrect)
                OnCorrectButtonClicked?.Invoke(this,button);
            else
                OnFalseButtonClicked?.Invoke(button);
        }

        public void NextCard()
        {
            OnCardSwitched?.Invoke(this);
        }
    
    }
}
