using UnityEngine;

namespace Game.Unity.View
{
    /// <summary>
    /// Semantic key to prefab. Presenters ask for "a tree" or "a wall", never for a path
    /// or a primitive — so swapping art is a change here and nowhere else.
    /// Prefabs are assigned when the scene is authored.
    /// </summary>
    public sealed class AssetLibrary : MonoBehaviour
    {
        [SerializeField] private GameObject[] _treePrefabs;
        [SerializeField] private GameObject _buildingPiecePrefab;

        public GameObject RandomTreePrefab()
        {
            if (_treePrefabs == null || _treePrefabs.Length == 0)
            {
                return null;
            }

            return _treePrefabs[Random.Range(0, _treePrefabs.Length)];
        }

        public GameObject Spawn(string semanticKey, Vector3 position, Quaternion rotation)
        {
            var prefab = semanticKey == "building.piece" ? _buildingPiecePrefab : RandomTreePrefab();
            if (prefab == null)
            {
                // A missing prefab must be visible, not silent.
                var fallback = GameObject.CreatePrimitive(PrimitiveType.Cube);
                fallback.transform.SetPositionAndRotation(position, rotation);
                return fallback;
            }

            return Instantiate(prefab, position, rotation);
        }
    }
}
