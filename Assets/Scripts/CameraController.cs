using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform deathPlane;
    private void Update()
    {
        transform.position = new Vector3(player.position.x, Mathf.Max(player.position.y, deathPlane.position.y), transform.position.z);
    }
}
