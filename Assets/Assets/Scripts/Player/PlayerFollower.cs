using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollower : MonoBehaviour
{
    public Transform player;
    private void FixedUpdate()
    {
        if (player == null) return;
        transform.position = player.position;
    }
}
