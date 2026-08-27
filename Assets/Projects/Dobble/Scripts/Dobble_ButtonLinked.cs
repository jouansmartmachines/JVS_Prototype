using System.Collections;
using System.Collections.Generic;
using Dobble;
using UnityEngine;
using UnityEngine.UI;

namespace Dobble
{   
    public class Dobble_ButtonLinked : MonoBehaviour
    {

        [SerializeField] private Image _image;
        //[SerializeField] public Universal_Button _button;
        [SerializeField] public Universal_Collider2DButton _button;

        public CircleCollider2D collider;




        public bool IsReveal { get; private set; } = false;

        public Sprite _rightSprite;

        [HideInInspector] public bool IsPersonalised;
        [HideInInspector] public Vector2 PersonalisedPos;

        public string buttonName;

        public DobbleTeam OwningTeam;


        private void Start()
        {

            _button.Event.AddListener(OnClick);

        }
        public void SetupButton(Dobble_ButtonLinked linkButton)
        {

            gameObject.SetActive(true);


            if (IsPersonalised)
            {
                (transform as RectTransform).localPosition = PersonalisedPos;
            }
        }

        
        private void OnClick()
        {
            Debug.Log($"[Dobble] Bouton cliqué : {gameObject.name} (Team: {OwningTeam.PlayerNames})");
     
            //_linkButton2D.Reveal(false);
            OwningTeam?.NotifyButtonClicked(this);

        }

        public void Reveal(bool isMaster = true)
        {
            if (IsReveal) return;

            IsReveal = true;

            _image.sprite = _rightSprite;
            _image.color = Color.white;

            _button.Event.RemoveListener(OnClick);


        }


    }
}



