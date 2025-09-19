using UnityEngine;
using Unity.Netcode;
using System.Collections;
using Unity.Netcode.Components;

[RequireComponent(typeof(NetworkTransform))]
public class EnemyMovement : NetworkBehaviour
{
    public float moveSpeed = 2f;
    public float wanderRadius = 10f;
    public float idleTime = 2f;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool isMoving = false;

    private void Start()
    {
        startPosition = transform.position;

        if (IsServer)
            StartCoroutine(WanderRoutine());
    }

    private IEnumerator WanderRoutine()
    {
        while (true)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-wanderRadius, wanderRadius),
                0f,
                Random.Range(-wanderRadius, wanderRadius)
            );
            targetPosition = startPosition + randomOffset;

            isMoving = true;

            while (Vector3.Distance(transform.position, targetPosition) > 0.2f)
            {
                Vector3 direction = (targetPosition - transform.position).normalized;
                transform.position += direction * moveSpeed * Time.deltaTime;
                transform.forward = direction;
                yield return null;
            }

            isMoving = false;

            yield return new WaitForSeconds(idleTime);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Application.isPlaying ? startPosition : transform.position, wanderRadius);
    }
}
