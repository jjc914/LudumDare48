using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerDamageController : MonoBehaviour
{
    [SerializeField] private Vector3 force;
    [SerializeField] public int flashCount;
    [SerializeField] public float iFramesLength;

    [SerializeField] private GameObject[] hearts;

    public int hp = 3;
    public bool die;
    public bool ignoreDamage = false;

    private bool firstRun = false;

    private PlayerMovementController _pmc;
    [SerializeField] private LevelLoader _ll;
    [SerializeField] private Tilemap _treasureTilemap;
    private SpriteRenderer _sr;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _pmc = GetComponent<PlayerMovementController>();
        _sr = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            TakeDamage(transform.position + new Vector3(5f, 0f, 0f));
        }

        if (hp <= 0)
        {
            die = true;
            if (!firstRun)
            {
                SFXController.instance.Play(SFX.DEATH, 1f);

                _ll.PlayerDied();

                firstRun = true;
            }
        }
        else
        {
            die = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!die)
        {
            if (collision.CompareTag("Enemy") && !ignoreDamage)
            {
                TakeDamage(collision.transform.position);
            }
        }
    }

    public void TakeDamage(Vector3 source)
    {
        //StartCoroutine(SFXController.instance.Play(SFX.HURT, 1f));
        SFXController.instance.Play(SFX.HURT, 1f);

        Vector3 dir = (source - transform.position).normalized;
        if (dir.x >= 0)
        {
            _pmc.useInertia = true;
            StartCoroutine(StopMovement(0.5f));
            _rb.velocity = -force;
            _pmc.yMov = force.y;
        }
        else
        {
            _pmc.useInertia = true;
            StartCoroutine(StopMovement(0.5f));
            _rb.velocity = force;
            _pmc.yMov = force.y;
        }
        if (hp - 1 >= 0)
        {
            hearts[hp - 1].GetComponent<Animator>().SetTrigger("die");
        }
        hp--;
        StartCoroutine(IFrames(iFramesLength, flashCount));
    }

    private IEnumerator StopMovement(float length)
    {
        _pmc.stopMovement = true;
        yield return new WaitForSeconds(iFramesLength);
        _pmc.stopMovement = false;
    }

    public IEnumerator IFrames(float length, int flashCount)
    {
        ignoreDamage = true;
        for (int i = 0; i < flashCount; i++) 
        {
            _sr.color = new Color(_sr.color.r, _sr.color.g, _sr.color.b, 0.5f);
            yield return new WaitForSeconds(length / (flashCount * 2f));
            _sr.color = new Color(_sr.color.r, _sr.color.g, _sr.color.b, 1f);
            yield return new WaitForSeconds(length / (flashCount * 2f));
        }
        ignoreDamage = false;
    }
}
