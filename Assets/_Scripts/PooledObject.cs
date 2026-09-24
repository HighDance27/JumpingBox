using System.Collections.Generic;
using UnityEngine;

public class PooledObject : MonoBehaviour
{
    public static PooledObject Instance { get; private set; }

    private readonly Dictionary<GameObject, List<GameObject>> poolMap = new();
    private readonly List<GameObject> activeObjects = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public GameObject Get(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (!poolMap.ContainsKey(prefab))
        {
            poolMap[prefab] = new List<GameObject>();
        }

        List<GameObject> pool = poolMap[prefab];

        //Find an inactive object
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeInHierarchy)
            {
                pool[i].transform.SetPositionAndRotation(pos, rot);
                pool[i].SetActive(true);
                activeObjects.Add(pool[i]);
                return pool[i];
            }
        }

        //If all are in use, create one more
        GameObject newObj = Instantiate(prefab, pos, rot, transform);
        pool.Add(newObj);
        activeObjects.Add(newObj);
        return newObj;
    }

    public void DeactivateAll()
    {
        for (int i = 0; i < activeObjects.Count; i++)
        {
            if (activeObjects[i] != null)
            {
                activeObjects[i].SetActive(false);
            }
        }
        activeObjects.Clear();
    }
}
