using UnityEngine;

public class MovingSpikeDecoration : MonoBehaviour
{
    [Header("Visual Settings")]
    public float animationSpeed = 1f;    // 1 is normal, 0.5 is slow, 2 is fast
    [Range(0, 1)]
    public float startOffset = 0f;       // 0 to 1 (0.5 starts the animation halfway)

    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();

        if (anim != null)
        {
            // Set how fast the animation plays
            anim.speed = animationSpeed;

            // Offset the start time so spikes move in a 'wave' pattern
            // (0 is the first frame, 1 is the last frame)
            anim.Play(0, -1, startOffset);
        }
    }
}