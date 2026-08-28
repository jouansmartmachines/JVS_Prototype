using System.Collections.Generic;
using UnityEngine;

namespace Challenge
{
    [RequireComponent(typeof(Collider2D))]
    public class Challenge_MaskD : MonoBehaviour
    {
        public List<Universal_Button> disabledButtons = new List<Universal_Button>();

        private void OnTriggerStay2D(Collider2D other)
        {
            Universal_Button button = other.GetComponent<Universal_Button>();
            if (button != null && button.IsActive && !disabledButtons.Contains(button))
            {
                button.IsActive = false;
                disabledButtons.Add(button);
                Debug.Log($"[MaskD] Désactivation du bouton : {button.name}");
            }
            else
            {
                Debug.Log("Ca ne marche pas");
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            Universal_Button button = other.GetComponent<Universal_Button>();
            if (disabledButtons.Contains(button))
            {
                button.IsActive = true;
                disabledButtons.Remove(button);
                Debug.Log($"[MaskD] Réactivation du bouton : {button.name}");
            }
        }
    }
}
