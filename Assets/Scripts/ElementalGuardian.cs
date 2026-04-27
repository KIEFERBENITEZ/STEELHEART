using UnityEngine;
using System.Collections;

public class ElementalGuardian : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectRange = 3f;
    public LayerMask playerLayer;
    public KeyCode interactKey = KeyCode.E;

    [Header("Timing Settings")]
    // Set this to 5 in the Inspector
    public float chargeTime = 5.0f;
    // Small delay so the Evolution happens during the 'Cast' animation
    public float castEffectDelay = 0.5f;

    private Animator anim;
    private bool hasInteracted = false;

    void Start() => anim = GetComponent<Animator>();

    void Update()
    {
        if (hasInteracted) return;

        // Check for player nearby
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectRange, playerLayer);

        if (playerCollider != null)
        {
            // You can show a "Press E" UI here
            if (Input.GetKeyDown(interactKey))
            {
                StartCoroutine(InteractionSequence(playerCollider.gameObject));
            }
        }
    }

    IEnumerator InteractionSequence(GameObject player)
    {
        hasInteracted = true;

        // 1. FREEZE PLAYER 
        // Prevents the player from walking away during the 5-second build-up
        var moveScript = player.GetComponent<PlayerController>();
        if (moveScript != null) moveScript.enabled = false;

        // 2. DIALOGUE
        Debug.Log("Guardian: All the elemental guardians are dead...");
        yield return new WaitForSeconds(2f);
        Debug.Log("Guardian: I am the only one remaining. Take my power!");

        // 3. START CHARGE ANIMATION
        // Make sure EG_charge is set to LOOP in the animation file settings
        anim.SetTrigger("StartBlessing");
        Debug.Log("Guardian is charging energy...");

        // 4. THE LONG CHARGE (5 Seconds)
        yield return new WaitForSeconds(chargeTime);

        // 5. TRIGGER CAST ANIMATION
        // This tells the Animator to move from EG_charge to EG_cast
        anim.SetTrigger("FinishBlessing");
        Debug.Log("Guardian: BECOME THE LIGHT!");

        // Wait a split second so the player sees the "Cast" pose before changing
        yield return new WaitForSeconds(castEffectDelay);

        // 6. EVOLVE PLAYER
        CharacterEvolution evo = player.GetComponent<CharacterEvolution>();
        if (evo != null)
        {
            evo.EvolveToLightForm();
        }

        // 7. UNFREEZE PLAYER
        if (moveScript != null) moveScript.enabled = true;

        Debug.Log("The transformation is complete!");
    }

    // Visualizes the interaction range in the Scene View (Cyan Circle)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}