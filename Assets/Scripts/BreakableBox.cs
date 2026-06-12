using UnityEngine;

public class BreakableBox : MonoBehaviour
{
    [Header("Audio")]
    [Tooltip("Add multiple sounds here. The script will randomly pick one each time a box breaks.")]
    public AudioClip[] breakSounds; // Changed from a single clip to an array
    [Range(0f, 1f)] public float volume = 1f;

    [Header("Effects (Optional)")]
    public GameObject breakEffectPrefab;

    public void Break()
    {
        // 1. Play a random sound effect from our list
        if (breakSounds != null && breakSounds.Length > 0)
        {
            // Pick a random index between 0 and the number of sounds we have
            int randomIndex = Random.Range(0, breakSounds.Length);
            AudioClip chosenSound = breakSounds[randomIndex];

            if (chosenSound != null)
            {
                AudioSource.PlayClipAtPoint(chosenSound, transform.position, volume);
            }
        }

        // 2. Spawn breaking particles/mesh fragments if you have them
        if (breakEffectPrefab != null)
        {
            Instantiate(breakEffectPrefab, transform.position, transform.rotation);
        }

        // 3. Destroy the box object
        Destroy(gameObject);
    }
}