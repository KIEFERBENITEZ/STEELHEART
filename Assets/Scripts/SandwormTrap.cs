using UnityEngine;

public class SandwormTrap : MonoBehaviour
{
    public int damage = 1;
    private Animator anim;
    private bool canAttack = true;
    public float cooldown = 2f;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("I SEE THE PLAYER!"); // If you don't see this in the Console, the collider is wrong.
            if (canAttack)
            {
                AttackPlayer(collision.gameObject);
            }
        }
    }

    void AttackPlayer(GameObject playerObj)
    {
        canAttack = false;

        // 1. Play the animation
        anim.SetTrigger("popUP");

        // 2. Deal damage to your PlayerHealth script
        PlayerHealth health = playerObj.GetComponent<PlayerHealth>();
        if (health != null)
        {
            // Pass the trap's position for knockback!
            health.TakeDamage(damage, transform.position);
        }

        // 3. Reset the trap after a few seconds
        Invoke("ResetTrap", cooldown);
    }

    void ResetTrap()
    {
        canAttack = true;
    }
}