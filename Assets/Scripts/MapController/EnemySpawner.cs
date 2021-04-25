using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject enemy;

    [HideInInspector] public GameObject[] enemyInstances;

    private int chunk = -1;

    public void Spawn(Vector3[] locations, int c)
    {
        //return;
        if (enemyInstances == null)
        {
            enemyInstances = new GameObject[locations.Length];
        }

        if (chunk < 0)
        {
            chunk = c;
            enemyInstances = new GameObject[locations.Length];
        }

        if (chunk != c)
        {
            chunk = c;
            for (int i = 0; i < enemyInstances.Length; i++)
            {
                if (enemyInstances[i] != null)
                {
                    Destroy(enemyInstances[i]);
                }
            }

            enemyInstances = new GameObject[locations.Length];
        }
        
        for (int i = 0; i < locations.Length; i++) 
        {
            if (enemyInstances[i] == null)
            {
                GameObject enemyInstance = Instantiate(enemy);
                enemyInstance.transform.position = locations[i];
                enemyInstance.GetComponent<EnemyController>().player = player;

                enemyInstances[i] = enemyInstance;
            }
        }
    }
}
