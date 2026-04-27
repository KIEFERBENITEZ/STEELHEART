using UnityEngine;

public class CharacterEvolution : MonoBehaviour
{
    [Header("Blessed Visuals")]
    public RuntimeAnimatorController blessedAnimator;

    [Header("Blessed Stats Boost")]
    public float newMoveSpeed = 8f;
    public float newJumpForce = 10f;
    public int newAttackDamage = 5;

    [Header("Blessed Skills")]
    public GameObject newAttackPrefab; // Your new magic projectile

    private PlayerController controller;
    private PlayerAttack attack;
    private Animator anim;

    void Start()
    {
        // Get references from the Player object
        controller = GetComponent<PlayerController>();
        attack = GetComponent<PlayerAttack>();
        anim = GetComponent<Animator>();
    }

    public void EvolveToLightForm()
    {
        Debug.Log("Evolution Started!");

        // 1. Swap the Animator to the stronger version
        if (blessedAnimator != null)
        {
            anim.runtimeAnimatorController = blessedAnimator;
        }

        // 2. Update Movement Stats in your PlayerController
        if (controller != null)
        {
            controller.moveSpeed = newMoveSpeed;
            controller.jumpForce = newJumpForce;
        }

        // 3. Update Attack Power
        if (attack != null)
        {
            attack.attackDamage = newAttackDamage;
            // attack.fireballPrefab = newAttackPrefab; // Uncomment if you want to swap the projectile
        }

        // 4. Visual Polish (Optional)
        // You could trigger a particle effect here!
    }
}