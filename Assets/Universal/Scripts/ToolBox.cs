using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Tool
{
    public static class ToolBox
    {
        private readonly static System.Random rng = new System.Random();

        /// <summary>
        /// Shuffle randomly the list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count; // Récupère le nombre d'éléments dans la liste
            while (n > 1) // Tant qu'il reste au moins deux éléments à mélanger
            {
                n--; // Décrémente n pour utiliser une position valide dans la liste
                int k = rng.Next(n + 1); // Tire un index aléatoire entre 0 et n inclus
                T value = list[k]; // Stocke l'élément à l'index aléatoire k
                list[k] = list[n]; // Place l'élément en position n à la position k
                list[n] = value; // Place l'ancien élément de la position k à la position n
            }
        }

        public static IList<T> ReturnShuffle<T>(this IList<T> list)
        {
            var temp = list;
            int n = temp.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = temp[k];
                temp[k] = temp[n];
                temp[n] = value;
            }
            return temp;
        }

        public static void Shuffle<T>(this T[,] array)
        {
            int lengthRow = array.GetLength(1);

            for (int i = array.Length - 1; i > 0; i--)
            {
                int i0 = i / lengthRow;
                int i1 = i % lengthRow;

                int j = rng.Next(i + 1);
                int j0 = j / lengthRow;
                int j1 = j % lengthRow;

                T temp = array[i0, i1];
                array[i0, i1] = array[j0, j1];
                array[j0, j1] = temp;
            }
        }

        public static T RandomElement<T>(this IList<T> list)
        {
            return list[Random.Range(0, list.Count)];
        }

        public static T RandomElement<T>(this IList<T> list, Predicate<T> predict)
        {
            var l = list.ToList().FindAll(predict);
            if (l.Count <= 0) return default;
            return l[Random.Range(0, l.Count)];
        }

        public static T RandomElement<T>(this T[] list)
        {
            return list[Random.Range(0, list.Length)];
        }

        public static T RandomElement<T>(this T[] list, Predicate<T> predict)
        {
            var l = list.ToList().FindAll(predict);
            if(l.Count <= 0) return default;
            return l[Random.Range(0, l.Count)];
        }

        public static IList<T> RandomElements<T>(this IList<T> list, int nb)
        {
            if (list.Count <= nb) return list;
            var result = new List<T>();
            var temp = list.ToList();
            for (int i = 0; i < nb; i++)
            {
                var element = temp.RandomElement();
                temp.Remove(element);
                result.Add(element);
            }
            return result;
        }

        public static IList<T> RandomElements<T>(this IList<T> list, int nb, Predicate<T> predict)
        {
            var temp = list.ToList().FindAll(predict);
            if (temp.Count <= 0) return default;
            if (temp.Count <= nb) return temp;

            var result = new List<T>();
            if (list.Count <= nb) return list;
            for (int i = 0; i < nb; i++)
            {
                var element = temp.RandomElement();
                temp.Remove(element);
                result.Add(element);
            }
            return result;
        }

        /// <summary>
        /// Total de la liste
        /// </summary>
        /// <param name="list"></param>
        /// <returns>Total de tout les elements de la liste</returns>
        public static int Total(this IList<int> list)
        {
            int total = 0;
            foreach (int nb in list)
                total += nb;
            return total;
        }

        public static float Total(this IList<float> list)
        {
            float total = 0;
            foreach (float nb in list)
                total += nb;
            return total;
        }

        private static bool CheckPos(Vector2 hit, RectTransform rect)
        {
            //float ConvertX(float pos) => (pos + 102.64f) * (1920 / 205.28f);
            //float ConvertY(float pos) => (pos + 57.74f) * (1080 / 115.48f);

            bool worldCanvas = rect.GetComponentInParent<Canvas>().renderMode == RenderMode.WorldSpace;
            bool cameraCanvas = rect.GetComponentInParent<Canvas>().renderMode == RenderMode.ScreenSpaceCamera;

            Vector2 pos = new Vector2(rect.gameObject.transform.position.x, rect.gameObject.transform.position.y);
            //Debug.Log("Is Canvas Overlay : " + (!worldCanvas && !cameraCanvas));
            if(!worldCanvas && !cameraCanvas)
                hit = Camera.main.WorldToScreenPoint(hit);
            

            //Debug.Log("Hit : " + hit + " | Pos : " + pos + " | Rect : w = " + (rect.rect.width * rect.lossyScale.x) + " ; h = " + (rect.rect.height * rect.lossyScale.y) + " | Scale : " + rect.lossyScale);

            if (hit.x < pos.x - (rect.rect.width * rect.lossyScale.x * rect.pivot.x))
                return false;
            if (hit.y < pos.y - (rect.rect.height * rect.lossyScale.y * rect.pivot.y))
                return false;

            if (hit.x > pos.x + (rect.rect.width * rect.lossyScale.x * rect.pivot.x))
                return false;
            if (hit.y > pos.y + (rect.rect.height * rect.lossyScale.y * rect.pivot.y))
                return false;

            return true;
        }

        private static bool CheckPos(Vector3 position, Vector2 hit, RectTransform rect)
        {
            //float ConvertX(float pos) => (pos + 102.64f) * (1920 / 205.28f);
            //float ConvertY(float pos) => (pos + 57.74f) * (1080 / 115.48f);

            bool worldCanvas = rect.GetComponentInParent<Canvas>().renderMode == RenderMode.WorldSpace;
            bool cameraCanvas = rect.GetComponentInParent<Canvas>().renderMode == RenderMode.ScreenSpaceCamera;

            Vector2 pos = new Vector2(rect.gameObject.transform.position.x, rect.gameObject.transform.position.y);
            //Debug.Log("Is Canvas Overlay : " + (!worldCanvas && !cameraCanvas));
            if (!worldCanvas && !cameraCanvas)
                position = hit;


            //Debug.Log("Hit : " + position + " | Pos : " + pos + " | Rect : w = " + (rect.rect.width * rect.lossyScale.x) + " ; h = " + (rect.rect.height * rect.lossyScale.y) + " | Scale : " + rect.lossyScale);

            if (position.x < pos.x - (rect.rect.width * rect.lossyScale.x * rect.pivot.x))
                return false;
            if (position.y < pos.y - (rect.rect.height * rect.lossyScale.y * rect.pivot.y))
                return false;

            if (position.x > pos.x + (rect.rect.width * rect.lossyScale.x * rect.pivot.x))
                return false;
            if (position.y > pos.y + (rect.rect.height * rect.lossyScale.y * rect.pivot.y))
                return false;

            return true;
        }

        private static bool CheckPos(Vector3 hit, GameObject gameObject, bool all)
        {
            if (Camera.main.orthographic)
            {
                var realHit = Camera.main.WorldToScreenPoint(hit);
                Ray ray = Camera.main.ScreenPointToRay(realHit);
                //Debug.Log("Ortho | Touch : " + Physics.Raycast(ray, out RaycastHit raycastHitDebug, float.PositiveInfinity) + " | Hit : " + realHit + " | Ray : " + ray);
                Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 20);

                if (!all)
                {
                    RaycastHit raycastHit;

                    if (Physics.Raycast(ray, out raycastHit, float.PositiveInfinity))
                    {
                        if (raycastHit.transform != null)
                        {
                            return raycastHit.transform.gameObject == gameObject;
                        }
                    }
                    return false;
                }

                RaycastHit[] raycastHits = Physics.RaycastAll(ray, float.PositiveInfinity);
                foreach (var raycasthit in raycastHits)
                {
                    if (raycasthit.transform.gameObject == gameObject)
                    {
                        return true;
                    }
                }
            }
            else
            {
                Ray ray = new Ray(Camera.main.transform.position, hit - Camera.main.transform.position);
                //Debug.Log("Persp | Touch : " + Physics.Raycast(ray, out RaycastHit raycastHitDebug, float.PositiveInfinity) + " | Hit : " + hit + " | Ray : " + ray);
                //Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 20);

                if (!all)
                {
                    RaycastHit raycastHit;

                    if (Physics.Raycast(ray, out raycastHit, float.PositiveInfinity))
                    {
                        if (raycastHit.transform != null)
                        {
                            return raycastHit.transform.gameObject == gameObject;
                        }
                    }
                    return false;
                }

                RaycastHit[] raycastHits = Physics.RaycastAll(ray, float.PositiveInfinity);
                foreach (var raycasthit in raycastHits)
                {
                    if (raycasthit.transform.gameObject == gameObject)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }

        public static bool CheckPos(Vector3 hit, Collider2D collider)
        {
            bool worldCanvas = false;
            bool cameraCanvas = false;    

            if (collider.TryGetComponent(out RectTransform rect))
            {
                worldCanvas = rect.GetComponentInParent<Canvas>().renderMode == RenderMode.WorldSpace;
                cameraCanvas = rect.GetComponentInParent<Canvas>().renderMode == RenderMode.ScreenSpaceCamera;

            }

            var realHit = (!worldCanvas && !cameraCanvas) ? Camera.main.WorldToScreenPoint(hit) : hit;
            Ray ray = new Ray(realHit, -Vector3.forward);
            //Debug.Log("Touch : " + (Physics2D.Raycast(realHit, -Vector3.forward, float.PositiveInfinity).transform != null) + " | Hit : " + realHit + " | Ray : " + ray);
            //Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 20);

            var raycastHit = Physics2D.Raycast(realHit, -Vector3.forward, float.PositiveInfinity);

            if (raycastHit.transform != null)
            {
                if (raycastHit.transform != null)
                {
                    return raycastHit.transform.gameObject == collider.gameObject;
                }
            }
            return false;
        }

        public static bool CheckPos(Vector3 hit, Transform transform, bool all = false)
        {
            if (transform is RectTransform)
                return CheckPos((Vector2)hit, transform as RectTransform);

            if (transform is Transform)
                return CheckPos(hit, transform.gameObject, all);

            Debug.LogError("Check Pos Failed");
            return false;
        }

        public static bool CheckPos(Vector3 pos, Vector3 hit, Transform transform, bool all = false)
        {
            if (transform is RectTransform)
                return CheckPos(pos, (Vector2)hit, transform as RectTransform);

            if (transform is Transform)
                return CheckPos(pos, transform.gameObject, all);

            Debug.LogError("Check Pos Failed");
            return false;
        }

        public static bool CheckPos(Vector3 hit, Bounds box, float verticalOffset = 0f, float horizontalOffset = 0f)
        {
            float top = box.center.y + box.extents.y + verticalOffset;
            float bottom = box.center.y - box.extents.y + verticalOffset;

            float right = box.center.x + box.extents.x + horizontalOffset;
            float left = box.center.x - box.extents.x + horizontalOffset;

            //Debug.Log("Box : " + box + " || Top : " + top + " | Bottom : " + bottom + " | Right : " + right + " | Left : " + left + " || Hit : " + hit);

            if (hit.x > right)
                return false;
            if (hit.x < left)
                return false;

            if (hit.y > top)
                return false;
            if (hit.y < bottom)
                return false;

            return true;
        }

        public static Sprite CreateSpriteFromTexture(Texture2D tex2D) => Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);

        public static Sprite CreateSpriteFromPath(string filePath, bool crop = false)
        {
            Texture2D tex2D;
            Sprite outSprite;

            if (File.Exists(filePath))
            {
                var fileData = File.ReadAllBytes(filePath);
                tex2D = new Texture2D(2, 2);

                if (ImageConversion.LoadImage(tex2D, fileData))
                {
                    Texture2D spriteTexture = tex2D;
            
                    if (!crop) outSprite = Sprite.Create(spriteTexture, new Rect(0, 0, spriteTexture.width, spriteTexture.height), new Vector2(0.5f, 0.5f));
                    else outSprite = Sprite.Create(spriteTexture, new Rect(0, 0, Mathf.Min(spriteTexture.width, spriteTexture.height), Mathf.Min(spriteTexture.width, spriteTexture.height)), new Vector2(0.5f, 0.5f));
                    return outSprite;
                }
            }
            return null;
        }

        public static Sprite CreateSpriteFromPath(string filePath, Vector2 ratio)
        {
            Texture2D tex2D;
            Sprite outSprite;

            if (File.Exists(filePath))
            {
                var fileData = File.ReadAllBytes(filePath);
                tex2D = new Texture2D(2, 2);

                if (ImageConversion.LoadImage(tex2D, fileData))
                {
                    Texture2D spriteTexture = tex2D.ResizeTextureToRatio(ratio);
                    outSprite = Sprite.Create(spriteTexture, new Rect(0, 0, spriteTexture.width, spriteTexture.height), new Vector2(0.5f, 0.5f));
                    return outSprite;
                }
            }
            return null;
        }

        public static Texture2D CreateTextureFromPath(string filePath)
        {
            Texture2D tex2D;

            if (File.Exists(filePath))
            {
                var fileData = File.ReadAllBytes(filePath);
                Debug.Log($"File '{filePath}' :\n{fileData.Length}");
                tex2D = new Texture2D(2, 2);

                if (ImageConversion.LoadImage(tex2D, fileData))
                {
                    return tex2D;
                }
            }
            return null;
        }

        public static Texture2D CreateTextureFromPath(string filePath, Vector2 ratio)
        {
            Texture2D tex2D;

            if (File.Exists(filePath))
            {
                var fileData = File.ReadAllBytes(filePath);
                Debug.Log($"File '{filePath}' :\n{fileData.Length}");
                tex2D = new Texture2D(2, 2);

                if (ImageConversion.LoadImage(tex2D, fileData))
                {
                    Debug.Log(tex2D.ResizeTextureToRatio(ratio).width + ";" + tex2D.ResizeTextureToRatio(ratio).height);
                    return tex2D.ResizeTextureToRatio(ratio);
                }
            }
            return null;
        }

        public static Texture2D ResizeTextureToRatio(this Texture2D original, Vector2 targetRatio)
        {
            if (original == null || targetRatio.x <= 0 || targetRatio.y <= 0)
            {
                Debug.LogError("Texture ou ratio invalide.");
                return null;
            }

            float originalRatio = (float)original.width / original.height;
            float desiredRatio = targetRatio.x / targetRatio.y;

            int newWidth = original.width;
            int newHeight = original.height;

            // Crop dimensions
            if (originalRatio > desiredRatio)
            {
                // Trop large -> on coupe sur les côtés
                newWidth = Mathf.RoundToInt(original.height * desiredRatio);
            }
            else if (originalRatio < desiredRatio)
            {
                // Trop haut -> on coupe en haut et en bas
                newHeight = Mathf.RoundToInt(original.width / desiredRatio);
            }

            // Coordonnées du point de départ pour le crop (centré)
            int startX = (original.width - newWidth) / 2;
            int startY = (original.height - newHeight) / 2;

            Color[] pixels = original.GetPixels(startX, startY, newWidth, newHeight);

            Texture2D cropped = new Texture2D(newWidth, newHeight, original.format, false);
            cropped.SetPixels(pixels);
            cropped.Apply();

            return cropped;
        }

        public static List<string> GetDirectories(string path, string searchPattern = "*",
        SearchOption searchOption = SearchOption.AllDirectories)
        {
            if (searchOption == SearchOption.TopDirectoryOnly)
                return Directory.GetDirectories(path, searchPattern).ToList();

            var directories = new List<string>(GetDirectories(path, searchPattern));

            for (var i = 0; i < directories.Count; i++)
                directories.AddRange(GetDirectories(directories[i], searchPattern));

            return directories;
        }

        private static List<string> GetDirectories(string path, string searchPattern)
        {
            try
            {
                return Directory.GetDirectories(path, searchPattern).ToList();
            }
            catch (UnauthorizedAccessException)
            {
                return new List<string>();
            }
        }

        public static List<string> GetFiles(string path) => Directory.GetFiles(path).ToList();
        public static List<string> GetFiles(string path, string extention) => Directory.GetFiles(path, extention).ToList();
        public static List<string> GetFiles(string path, string[] extentions)
        {
            List<string> files = new List<string>();
            for (int i = 0; i < extentions.Length; i++)
            {
                files.AddRange(Directory.GetFiles(path, extentions[i]).ToList());
            }
            return files;
        }

        public static string GetFileNameFromPath(string path) => Path.GetFileName(path);

        public static TweenerCore<Vector3, Vector3, VectorOptions> DOMoveInTargetLocalSpace(this Transform transform, Transform target, Vector3 targetLocalEndPosition, float duration)
        {
            var t = DOTween.To(
                () => transform.position - target.transform.position, // Value getter
                x => transform.position = x + target.transform.position, // Value setter
                targetLocalEndPosition,
                duration);
            t.SetTarget(transform);
            return t;
        }

        public static bool PointerIsOverUI(Vector2 screenPos)
        {
            var hitObject = UIRaycast(ScreenPosToPointerData(screenPos));
            return hitObject != null && hitObject.layer == LayerMask.NameToLayer("UI");
        }

        static GameObject UIRaycast(PointerEventData pointerData)
        {
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            return results.Count < 1 ? null : results[0].gameObject;
        }

        static PointerEventData ScreenPosToPointerData(Vector2 screenPos) => new(EventSystem.current) { position = screenPos };

        public static Vector3 GetRandomPointInsideCollider(this BoxCollider boxCollider)
        {
            Vector3 extents = boxCollider.size / 2f;
            Vector3 point = new Vector3(
                Random.Range(-extents.x, extents.x),
                Random.Range(-extents.y, extents.y),
                Random.Range(-extents.z, extents.z)
            );

            return boxCollider.transform.TransformPoint(point);
        }

        public static void RebuildLayout(this RectTransform rect, bool rebuildChild = false)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            if (rebuildChild && rect.childCount > 0)
            {
                for (int i = 0; i < rect.childCount; i++)
                {
                    RebuildLayout((rect.GetChild(i) as RectTransform), rebuildChild);
                }
            }
        }

 

        public static string GetGameNameWithoutSuffix(string gameName)
        {
            int lastDashIndex = gameName.LastIndexOf('-');

            if (lastDashIndex != -1)
            {
                Theme.ThemeAppSelector.LastStaticName = gameName.Substring(lastDashIndex + 1);
                gameName = gameName.Substring(0, lastDashIndex);
            }
            else
            {
                Theme.ThemeAppSelector.LastStaticName = string.Empty; 
            }

            return gameName;
        }

    }

}
