using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrapplingHook : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float distance;
    [SerializeField] private float throwDelay;

    [SerializeField] private GameObject grapplingHook;
    [SerializeField] private LayerMask checkLayers;

    [SerializeField] private GameObject grappleIcon;

    private bool allowGrapple;

    private bool grappling;
    private Vector3 grapplePosition;
    private Vector3 aimDir;
    private float timer;

    private Rigidbody2D _rb;
    private PlayerMovementController _pmc;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _pmc = GetComponent<PlayerMovementController>();
    }

    private void Update()
    {

        if (_pmc.isGroundedRaw)
        {
            allowGrapple = true;
            //Debug.Log("asdf");
        }

        if (allowGrapple)
        {
            if (Input.GetKey(KeyCode.Mouse1))
            {
                allowGrapple = false;
            }
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                Vector3 aimPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                aimDir = aimPosition - transform.position;
                aimDir.Normalize();

                RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, aimDir, distance);
                for (int i = 0; i < hits.Length; i++)
                {
                    if (checkLayers == (checkLayers | (1 << hits[i].transform.gameObject.layer)))
                    {
                        grapplePosition = hits[i].point;

                        //StartCoroutine(SFXController.instance.Play(SFX.GRAPPLE, 1f));
                        SFXController.instance.Play(SFX.GRAPPLE, 1f);

                        grapplingHook.GetComponent<LineRenderer>().positionCount = 2;
                        grappling = true;
                    }
                }
            }
        }
        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            grappling = false;
        }

        grappleIcon.GetComponent<Animator>().SetBool("using", !allowGrapple);
    }

    private void FixedUpdate()
    {
        if (grappling)
        {
            if (timer >= throwDelay)
            {
                aimDir = grapplePosition - transform.position;
                aimDir.Normalize();

                _pmc.useInertia = true;
                _rb.velocity = aimDir * speed;
                _pmc.yMov = _rb.velocity.y;

                grapplingHook.GetComponent<LineRenderer>().SetPosition(0, new Vector3(transform.position.x, transform.position.y, 0f));
                grapplingHook.GetComponent<LineRenderer>().SetPosition(1, new Vector3(grapplePosition.x, grapplePosition.y, 0f));
            }
            else
            {
                grapplingHook.GetComponent<LineRenderer>().SetPosition(0, new Vector3(transform.position.x, transform.position.y, 0f));
                Vector3 position = Vector3.Lerp(transform.position, grapplePosition, timer / throwDelay);
                grapplingHook.GetComponent<LineRenderer>().SetPosition(1, new Vector3(position.x, position.y, 0f));
            }
            timer += Time.fixedDeltaTime;
        }
        else
        {
            timer = 0f;
            grapplingHook.GetComponent<LineRenderer>().positionCount = 0;
        }
    }
}
