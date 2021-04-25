using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float viewDistance;

    [SerializeField] private float xMin;
    [SerializeField] private float xMax;
    [SerializeField] private float yMin;
    [SerializeField] private float yMax;

    private bool move = true;

    [SerializeField] private LayerMask checkLayers;
    [SerializeField] public GameObject player;

    private Animator _anim;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (move)
        {
            bool blocked = false;
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, (player.transform.position - transform.position).normalized, viewDistance);
            for (int i = 0; i < hits.Length; i++)
            {
                if (checkLayers == (checkLayers | (1 << hits[i].transform.gameObject.layer)))
                {
                    blocked = true;
                }
            }

            if (!blocked)
            {
                transform.Translate(((player.transform.position - transform.position).normalized * speed + new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax))) * Time.deltaTime);
            }
        }
    }

    public void Die()
    {
        move = false;
        _anim.SetTrigger("die");
        //StartCoroutine(SFXController.instance.Play(SFX.BAT_HURT, 1));
        SFXController.instance.Play(SFX.BAT_HURT, 1);
        Destroy(this.gameObject, 0.15f);
    }
}
