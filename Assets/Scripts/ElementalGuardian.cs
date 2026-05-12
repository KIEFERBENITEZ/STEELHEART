using UnityEngine;
using System.Collections;

public class ElementalGuardian : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectRange = 3f;
    public LayerMask playerLayer;
    public KeyCode interactKey = KeyCode.E;

    [Header("Timing Settings")]
    public float chargeTime = 5.0f;
    public float castEffectDelay = 0.5f;

    private Animator anim;
    private bool hasInteracted = false;
    private Transform playerTransform; // Store player to keep looking at them

    void Start() => anim = GetComponent<Animator>();

    void Update()
    {
        // 1. Check for player nearby
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectRange, playerLayer);

        if (playerCollider != null)
        {
            playerTransform = playerCollider.transform;

            // 2. FACE THE PLAYER
            // Only flip if we haven't locked into the final animation sequence yet
            if (!hasInteracted)
            {
                LookAtPlayer();
            }

            // 3. INTERACT
            if (Input.GetKeyDown(interactKey) && !hasInteracted)
            {
                StartCoroutine(InteractionSequence(playerCollider.gameObject));
            }
        }
    }

    void LookAtPlayer()
    {
        if (playerTransform == null) return;

        // Calculate direction
        float directionX = playerTransform.position.x - transform.position.x;

        // Flip Logic
        // If she is looking away from the player, swap the '-' sign below
        if (directionX > 0)
        {
            // Player is to the right
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else
        {
            // Player is to the left
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    IEnumerator InteractionSequence(GameObject player)
    {
        hasInteracted = true;

        // FREEZE PLAYER 
        var moveScript = player.GetComponent<PlayerController>();
        if (moveScript != null) moveScript.enabled = false;

        // DIALOGUE
        Debug.Log("Guardian: All the elemental guardians are dead...");
        yield return new WaitForSeconds(2f);
        Debug.Log("Guardian: I am the only one remaining. Take my power!");

        // START CHARGE ANIMATION
        anim.SetTrigger("StartBlessing");

        // THE LONG CHARGE (5 Seconds)
        yield return new WaitForSeconds(chargeTime);

        // TRIGGER CAST ANIMATION
        anim.SetTrigger("FinishBlessing");

        yield return new WaitForSeconds(castEffectDelay);

        // EVOLVE PLAYER
        CharacterEvolution evo = player.GetComponent<CharacterEvolution>();
        if (evo != null) evo.EvolveToLightForm();

        // UNFREEZE PLAYER
        if (moveScript != null) moveScript.enabled = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}