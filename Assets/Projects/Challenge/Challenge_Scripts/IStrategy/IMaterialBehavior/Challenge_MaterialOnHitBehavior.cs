using UnityEngine;
using UnityEngine.UI;

namespace Challenge
{
    public class Challenge_MaterialOnHitBehavior : MonoBehaviour
    {
    private ITarget target;
        private GameObject circleLayer;    
        private GameObject doubleHitObject; 
        
        private Color hitColor;
        private bool hasHit = false;
        private bool useColorOnParent = false;

        private Image image0;
        private Image image1;

        public void Initialize(ITarget target)
        {
            this.target = target;
            if (this.target != null)
                this.target.OnHitEvent += OnHit;
        }

        public void SetDoubleHitUI(GameObject go)
        {
            doubleHitObject = go;
        }

        public void SetLayer(GameObject go)
        {
            circleLayer = go;
        }

        public void Configure(Sprite blue, Sprite green, Color cInit, Color cHit)
        {
            if (circleLayer == null) return;
            
            this.hitColor = cHit;

            if (circleLayer.transform.childCount == 0)
            {
                useColorOnParent = true;
                Image img = circleLayer.GetComponent<Image>();
                if (img != null) img.color = cInit;
            }
            else
            {
                useColorOnParent = false;
                Image[] images = circleLayer.GetComponentsInChildren<Image>(true);
                if (images.Length >= 1 && blue != null) images[0].sprite = blue;
                image0 = images[0];
                if (images.Length >= 2 && green != null) images[1].sprite = green;
                image1 = images[1];
            }
        }

        private void OnHit(ITarget t)
        {
            if (hasHit) return;
            hasHit = true;

            if (circleLayer != null)
            {
                if (useColorOnParent)
                {
                    Image img = circleLayer.GetComponent<Image>();
                    if (img != null) img.color = hitColor;
                }
                else
                {
                    image1.gameObject.SetActive(false);
                }
            }
            if (doubleHitObject != null)
            {
                doubleHitObject.SetActive(true);
            }

            Challenge_AudioManager.i.PlayOneShot(SoundType.Evolutif);

            if (target != null)
                target.OnHitEvent -= OnHit;
        }

        public void Activate() => OnHit(target);
        public void Stop() { if (target != null) target.OnHitEvent -= OnHit; }
        private void OnDestroy() => Stop();
    }
}