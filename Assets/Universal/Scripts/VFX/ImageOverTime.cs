using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageOverTime : MonoBehaviour
{
    [SerializeField] bool playAtStart = true;
    [SerializeField] bool destroyAtEnd = true;
    [SerializeField] float animTotalTime = 1f;
    [SerializeField] List<Sprite> sprites;

    Image image;
    public void Start()
    {
        image = GetComponent<Image>();
        if (playAtStart) PlayAnim();
    }

    public void PlayAnim()
    {
        StartCoroutine(Anim());
    }

    private IEnumerator Anim()
    {
        for (int i = 0; i < sprites.Count; i++)
        {
            image.sprite = sprites[i];
            yield return new WaitForSeconds(animTotalTime / (float)sprites.Count);
        }
        if (destroyAtEnd) Destroy(this.gameObject);
    }
}
