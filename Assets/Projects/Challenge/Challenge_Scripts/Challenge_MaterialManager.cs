using System.Collections.Generic;
using UnityEngine;

namespace Challenge
{
    [CreateAssetMenu(fileName = "Challenge_MaterialManager", menuName = "Challenge/MaterialManager")]
    public class Challenge_MaterialManager : ScriptableObject
    {
        [System.Serializable]
        public class MaterialEntry
        {
            public string key;
            public Material material;
        }

        public List<MaterialEntry> entries = new List<MaterialEntry>();

        public Material GetMaterial(string key)
        {
            var entry = entries.Find(e => e.key == key);
            return entry != null ? entry.material : null;
        }
    }
}
