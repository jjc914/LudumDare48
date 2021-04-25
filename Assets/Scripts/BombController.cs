using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombController : MonoBehaviour
{
    [SerializeField] private float delay;
    [SerializeField] private int explosionRadius;

    [SerializeField] private GameObject explosion;

    private GameObject[] enemies;

    public GameObject _mapController;
    public GameObject _player;
    private Rigidbody2D _rb;

    private void Start()
    {
        StartCoroutine(Explode(delay));
    }

    private IEnumerator Explode(float delay)
    {
        _player.GetComponent<PlayerBombController>().allowThrow = false;
        yield return new WaitForSeconds(delay);

        //StartCoroutine(SFXController.instance.Play(SFX.EXPLOSION, 1f));
        SFXController.instance.Play(SFX.EXPLOSION, 1f);

        _mapController.GetComponent<MapEditing>().Explode(transform.position, explosionRadius);

        GameObject explosionInstance = Instantiate(explosion);

        explosionInstance.GetComponent<Animator>().SetTrigger("explode");
        explosionInstance.transform.position = transform.position;

        enemies = _mapController.GetComponent<EnemySpawner>().enemyInstances;

        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null)
            {
                if (Vector3.Distance(transform.position, enemies[i].transform.position) <= explosionRadius)
                {
                    enemies[i].GetComponent<EnemyController>().Die();
                }
            }
        }

        if (!_player.GetComponent<PlayerDamageController>().ignoreDamage)
        {
            if (Vector3.Distance(transform.position, _player.transform.position) <= explosionRadius)
            {
                _player.GetComponent<PlayerDamageController>().TakeDamage(transform.position);
            }
        }

        _player.GetComponent<PlayerBombController>().allowThrow = true;
        Destroy(explosionInstance, 0.3f);
        Destroy(this.gameObject);
    }
}
