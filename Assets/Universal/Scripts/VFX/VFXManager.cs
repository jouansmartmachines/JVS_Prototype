using DeadWar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : ReceiveParent
{
    [SerializeField] Transform parent;
    public void SetPrefab(GameObject prefab) => vfxPrefab = prefab;
    [SerializeField] GameObject vfxPrefab;
    public float VFXDuration => vfxDuration;
    [SerializeField] float vfxDuration = 2f;
    [SerializeField] AudioClip sfxClip;
    [SerializeField, Range(0f, 2f)] float sfxVolume = 1f;
    [SerializeField] bool realPos = false;
    [SerializeField] bool autoPlay = true;

    public override void ReceivePoint(float xPoint, float yPoint)
    {
        if (!autoPlay) return;
        //Debug.Log(xPoint + " ; " + yPoint + " | " + (xPoint * (float)Screen.width) + " ; " + (yPoint * (float)Screen.height));
        float x = (xPoint * (float)Screen.width) - ((vfxPrefab.transform is RectTransform) ? (vfxPrefab.transform as RectTransform).sizeDelta.x / 2f : 0);
        float y = (yPoint * (float)Screen.height) - ((vfxPrefab.transform is RectTransform) ? (vfxPrefab.transform as RectTransform).sizeDelta.y / 2f : 0f);
        var pos = new Vector3(x, y);
        if (realPos)
        {
            if (Camera.main.orthographic)
            {
                pos = Camera.main.ScreenToWorldPoint(pos);
                pos.z = 0;
            }
            else
            {
                //pos.z = renderImpact.planeDistance;
                Vector3 clickPos = Camera.main.ScreenToWorldPoint(pos);
                Ray ray = Camera.main.ScreenPointToRay(pos);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    var vfxPerspective = Instantiate(vfxPrefab, hit.point, Quaternion.identity);
                    Destroy(vfxPerspective, vfxDuration);
                    if(sfxClip != null) AudioSource.PlayClipAtPoint(sfxClip, hit.point, sfxVolume);
                }
                return;
            }
        }
        var vfx = Instantiate(vfxPrefab, pos, Quaternion.identity, parent);
        Destroy(vfx, vfxDuration);
        if (sfxClip != null) AudioSource.PlayClipAtPoint(sfxClip, pos, sfxVolume);
        //Debug.Log(vfx.transform.position.x + " ; " + vfx.transform.position.y + " | " + ((vfxPrefab.transform is RectTransform) ? 
        //    (vfxPrefab.transform as RectTransform).sizeDelta.x / 2f : 0) 
        //    + " ; " + ((vfxPrefab.transform is RectTransform) ? 
        //    (vfxPrefab.transform as RectTransform).sizeDelta.y / 2f : 0f));


    }

    public void PlayVFX(float xPoint, float yPoint)
    {
        float x = (xPoint * (float)Screen.width) - ((vfxPrefab.transform is RectTransform) ? (vfxPrefab.transform as RectTransform).sizeDelta.x / 2f : 0);
        float y = (yPoint * (float)Screen.height) - ((vfxPrefab.transform is RectTransform) ? (vfxPrefab.transform as RectTransform).sizeDelta.y / 2f : 0f);
        var pos = new Vector3(x, y);
        if (realPos)
        {
            pos = Camera.main.ScreenToWorldPoint(pos);
            pos.z = 0;
        }
        var vfx = Instantiate(vfxPrefab, pos, Quaternion.identity, parent);
        Destroy(vfx, vfxDuration);
        if (sfxClip != null) AudioSource.PlayClipAtPoint(sfxClip, pos, sfxVolume);
        Debug.Log(vfx.transform.position.x + " ; " + vfx.transform.position.y + " | " + ((vfxPrefab.transform is RectTransform) ? 
            (vfxPrefab.transform as RectTransform).sizeDelta.x / 2f : 0) 
            + " ; " + ((vfxPrefab.transform is RectTransform) ? 
            (vfxPrefab.transform as RectTransform).sizeDelta.y / 2f : 0f));
    }
}
