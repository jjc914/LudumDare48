using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] public Vector2 bottomLeftBound;
    [SerializeField] public Vector2 topRightBound;

    [SerializeField] private float smoothSpeed;

    private void Start()
    {
        float xPos = Mathf.Clamp(player.transform.position.x, bottomLeftBound.x, topRightBound.x);
        float yPos = Mathf.Clamp(player.transform.position.y, bottomLeftBound.y, topRightBound.y);
        transform.position = new Vector3(xPos, yPos, transform.position.z);
    }

    private void LateUpdate()
    {
        float xPos = Mathf.Clamp(player.transform.position.x, bottomLeftBound.x, topRightBound.x);
        float yPos = Mathf.Clamp(player.transform.position.y, bottomLeftBound.y, topRightBound.y);

        Vector3 desiredPos = new Vector3(xPos, yPos, transform.position.z);

        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
    }
}
