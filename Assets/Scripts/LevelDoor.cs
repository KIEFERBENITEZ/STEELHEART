using UnityEngine;
using UnityEngine.SceneManagement; // Required for switching scenes

public class LevelDoor : MonoBehaviour
{
    [Header("Visuals")]
    public GameObject pressKeyPrompt;

    [Header("Settings")]
    public string nextLevelName; // Type the name of your next scene here!

    private bool playerInRange = false;

    void Start()
    {
        if (pressKeyPrompt != null) pressKeyPrompt.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            if (pressKeyPrompt != null) pressKeyPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            if (pressKeyPrompt != null) pressKeyPrompt.SetActive(false);
        }
    }

    void Update()
    {
        // Check for 'E' press and change scene
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!string.IsNullOrEmpty(nextLevelName))
            {
                Debug.Log("Opening door to: " + nextLevelName);
                SceneManager.LoadScene(nextLevelName);
            }
            else
            {
                Debug.LogWarning("You forgot to type the level name in the Inspector!");
            }
        }
    }
}