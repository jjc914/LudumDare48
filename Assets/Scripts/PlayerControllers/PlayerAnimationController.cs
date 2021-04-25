using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Vector3 initialScale;
    private Vector3 initialBoundsScale;

    private Rigidbody2D _rb;
    private Animator _an;
    private PlayerMovementController _pmc;
    private PlayerDamageController _pdc;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _an = GetComponent<Animator>();
        _pmc = GetComponent<PlayerMovementController>();
        _pdc = GetComponent<PlayerDamageController>();
        initialScale = transform.localScale;
        initialBoundsScale = transform.GetChild(0).localScale;
    }

    private void Update()
    {
        if (!_pdc.die)
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
            {
                if (Mathf.Abs(_rb.velocity.x) >= Mathf.Epsilon)
                {
                    _an.SetBool("running", true);
                }
                else
                {
                    _an.SetBool("running", false);
                }
                if (Input.GetKey(KeyCode.A))
                {
                    transform.localScale = new Vector3(-initialScale.x, initialScale.y, initialScale.z);
                    transform.GetChild(0).localScale = new Vector3(-initialBoundsScale.x, initialBoundsScale.y, initialBoundsScale.z);
                }
                else if (Input.GetKey(KeyCode.D))
                {
                    transform.localScale = new Vector3(initialScale.x, initialScale.y, initialScale.z);
                    transform.GetChild(0).localScale = new Vector3(initialBoundsScale.x, initialBoundsScale.y, initialBoundsScale.z);
                }
            }
            else
            {
                _an.SetBool("running", false);
            }
        }
        else
        {
            _an.SetTrigger("die");
        }
    }
}
