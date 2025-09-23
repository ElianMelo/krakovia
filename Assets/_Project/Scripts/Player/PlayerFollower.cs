using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollower : MonoBehaviour
{
    public Transform player;
    private void LateUpdate()
    {
        if (player == null) return;
        transform.position = player.position;
    }
}
