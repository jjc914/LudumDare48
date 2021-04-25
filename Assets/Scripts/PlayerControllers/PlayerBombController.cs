using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBombController : MonoBehaviour
{
    [SerializeField] public GameObject _bombPrefab;

    [SerializeField] private Transform _bombStartPos;
    [SerializeField] private Vector2 throwForce;

    [SerializeField] private GameObject _mapController;
    [SerializeField] private GameObject _bombIcon;

    private PlayerDamageController _pdc;

    public bool allowThrow = true;

    private void Awake()
    {
        _pdc = GetComponent<PlayerDamageController>();
        Physics2D.IgnoreLayerCollision(3, 6);

        allowThrow = true;
    }

    private void Update()
    {
        if (!_pdc.die)
        {
            _bombIcon.GetComponent<Animator>().SetBool("canUse", allowThrow);

            if (allowThrow)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    GameObject bomb = Instantiate(_bombPrefab);
                    bomb.transform.position = _bombStartPos.position;

                    bomb.GetComponent<Rigidbody2D>().AddForce(new Vector2(transform.localScale.x / Mathf.Abs(transform.localScale.x) * throwForce.x, throwForce.y), ForceMode2D.Impulse);
                    bomb.GetComponent<BombController>()._mapController = _mapController;
                    bomb.GetComponent<BombController>()._player = this.gameObject;
                }
            }
        }
    }
}
