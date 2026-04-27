using UnityEngine;

public class FireTotem : MonoBehaviour
{
    private Animator anim;
    private bool isActivated = false;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            ActivateTotem();

            // Tell the Player's Health script to save this position
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.UpdateCheckpoint(transform.position);
            }
        }
    }

    void ActivateTotem()
    {
        isActivated = true;
        if (anim != null) anim.SetTrigger("Activate");
        Debug.Log("Checkpoint Saved!");
    }
}