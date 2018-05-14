using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class ParticlePool : MonoBehaviour
{
    private static ParticlePool _inst = null;
    public static ParticlePool Instance
    {
        get
        {
            if (_inst == null)
            {
                var go = new GameObject("ParticlePool");
                _inst = go.AddComponent<ParticlePool>();
            }
            return _inst;
        }
    }
    private Dictionary<string, Queue<GameObject>> pool = new Dictionary<string, Queue<GameObject>>();

    public GameObject GetGameObject(GameObject template, System.Action<GameObject> init)
    {
        if (pool.ContainsKey(template.name) && pool[template.name].Count > 0)
        {
            var go = pool[template.name].Dequeue();
            if(go != null){
                if (init != null)
                {
                    init(go);
                }
                return go;
            }
        }
        var g = GameObject.Instantiate(template) as GameObject;
        g.name = template.name;
        if (init != null)
        {
            init(g);
        }
        return g;
    }


    public void ReturnGameObject(GameObject go, System.Action<GameObject> reset)
    {
        if (go == null)
        {
            return;
        }
        if (!pool.ContainsKey(go.name))
        {
            pool.Add(go.name, new Queue<GameObject>());
        }
        if (reset != null)
        {
            reset(go);
        }
        pool[go.name].Enqueue(go);
    }


    public static void InitParticle(GameObject g)
    {
        g.SetActive(true);
        var par = g.GetComponentsInChildren<ParticleSystem>();
        var p2 = g.GetComponent<ParticleSystem>();
        foreach (var system in par)
        {
            system.Clear();
            system.Play();
        }
        if (p2 != null)
        {
            p2.Clear();
            p2.Play();
        }

    }

    public static void ResetParticle(GameObject g)
    {
        g.transform.parent = null;
        g.SetActive(false);
    }

    public static IEnumerator CollectParticle(GameObject g, float t)
    {
        yield return new WaitForSeconds(t);
        Instance.ReturnGameObject(g, ResetParticle);
    }
}
