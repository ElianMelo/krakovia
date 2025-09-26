using UnityEngine;

public class PlayerFollower : MonoBehaviour
{
    public Transform player;
    public float smoothTime = 0.3F;
    private Vector3 velocity = Vector3.zero;

    void Update()
    {
        if (player == null) return;
        transform.position = Vector3.SmoothDamp(transform.position, player.transform.position, ref velocity, smoothTime);
    }
}
