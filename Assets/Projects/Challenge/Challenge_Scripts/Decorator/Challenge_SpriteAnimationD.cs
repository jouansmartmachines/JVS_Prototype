using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Challenge
{
    public class Challenge_SpriteAnimationD : Challenge_TargetDecorator
    {
        private Sprite[] frames;
        private float frameDuration = 0.06f;
        private Image thirdLayerImage;

        // ----------------------------------------------------------------

        public override void SetTarget(ITarget t)
        {
            base.SetTarget(t);
            if (target != null)
                target.OnDeath += OnTargetDeath;
        }

        /// <summary>
        /// Appelé par la Recipe.
        /// thirdLayer : le GameObject target.thirdLayer dont on va animer l'Image.
        /// </summary>
        public void Setup(Sprite[] animFrames, float fps, GameObject thirdLayer)
        {
            frames        = animFrames;
            frameDuration = (fps > 0f) ? 1f / fps : 0.06f;

            if (thirdLayer != null)
                thirdLayerImage = thirdLayer.GetComponent<Image>();
        }

        // ----------------------------------------------------------------

        private void OnTargetDeath(ITarget t, DeathCause cause)
        {

            if (cause == DeathCause.Lifetime) return;
            Color tempColor = thirdLayerImage.color;
            tempColor.a = 1f;
            thirdLayerImage.color = tempColor;
            StartCoroutine(PlayAnimation());

        }

        private IEnumerator PlayAnimation()
        {
            // S'assure que le thirdLayer est visible pendant l'anim
            thirdLayerImage.gameObject.SetActive(true);
            thirdLayerImage.preserveAspect = true;


            foreach (var frame in frames)
            {
                if (thirdLayerImage == null) yield break;
                thirdLayerImage.sprite = frame;
                yield return new WaitForSeconds(frameDuration);
            }

            // Cache le thirdLayer une fois l'anim terminée
            if (thirdLayerImage != null)
                thirdLayerImage.gameObject.SetActive(false);
        }

        // ----------------------------------------------------------------

        private void OnDestroy()
        {
            if (target != null)
                target.OnDeath -= OnTargetDeath;
        }
    }
}