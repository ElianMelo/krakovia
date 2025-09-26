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
