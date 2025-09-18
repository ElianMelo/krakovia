using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    private Animator animator;
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            animator.SetTrigger("Attack1");
        }
    }
}
