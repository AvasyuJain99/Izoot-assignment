using UnityEngine;
using System.Collections.Generic;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance { get; private set; }

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    [Header("Pool Settings")]
    [SerializeField] private List<Pool> pools;
    [SerializeField] private Transform poolParent;

    private Dictionary<string, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (poolParent == null)
        {
            GameObject poolParentObj = new GameObject("PooledObjects");
            poolParent = poolParentObj.transform;
        }
    }

    private void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, poolParent);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            return null;
        }

        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        if (objectToSpawn == null)
        {
            Pool pool = pools.Find(p => p.tag == tag);
            if (pool != null)
            {
                objectToSpawn = Instantiate(pool.prefab, poolParent);
            }
        }
        else
        {
            objectToSpawn.SetActive(true);
        }

        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        poolDictionary[tag].Enqueue(objectToSpawn);

        return objectToSpawn;
    }

    public void ReturnToPool(GameObject obj, string tag)
    {
        if (obj == null)
            return;

        obj.SetActive(false);
        obj.transform.SetParent(poolParent);
    }
}
