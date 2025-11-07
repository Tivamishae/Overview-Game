using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MobPoolManager : MonoBehaviour
{
    public static MobPoolManager Instance { get; private set; }

    [System.Serializable]
    public class Pool
    {
        public GameObject prefab;
        public int preloadCount = 20;
    }

    public List<Pool> pools;
    private Dictionary<GameObject, Queue<GameObject>> poolDict = new();

    private void Awake()
    {
        Instance = this;
        foreach (var pool in pools)
        {
            var queue = new Queue<GameObject>();
            for (int i = 0; i < pool.preloadCount; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                queue.Enqueue(obj);
            }
            poolDict[pool.prefab] = queue;
        }
    }

    private void Start()
    {
        StartCoroutine(WarmupAgents());
    }

    private IEnumerator WarmupAgents()
    {
        foreach (var kvp in poolDict)
        {
            var prefab = kvp.Key;
            var queue = kvp.Value;

            int count = queue.Count;
            for (int i = 0; i < count; i++)
            {
                var obj = queue.Dequeue();
                obj.SetActive(true);

                var agent = obj.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null)
                {
                    // Temporarily enable + warp to a valid navmesh point
                    if (UnityEngine.AI.NavMesh.SamplePosition(Vector3.zero, out UnityEngine.AI.NavMeshHit hit, 50f, UnityEngine.AI.NavMesh.AllAreas))
                        agent.Warp(hit.position);
                }

                yield return null; // one frame so Unity registers it

                // Now return back to pool
                ReturnToPool(prefab, obj);
            }
        }

        Debug.Log(" NavMesh agents pre-initialized and warmed up.");
    }

    public GameObject GetFromPool(GameObject prefab)
    {
        if (!poolDict.ContainsKey(prefab))
            return Instantiate(prefab);

        if (poolDict[prefab].Count > 0)
        {
            GameObject obj = poolDict[prefab].Dequeue();
            obj.GetComponent<Enemy>().SetIdleState();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            GameObject obj = Instantiate(prefab);
            return obj;
        }
    }

    public void ReturnToPool(GameObject prefab, GameObject obj)
    {
        obj.SetActive(false);
        poolDict[prefab].Enqueue(obj);
    }
}

