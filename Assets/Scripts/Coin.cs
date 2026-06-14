using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip pickupSound; // Drag your audio clip here in the Inspector

    //private void OnTriggerEnter2D(Collider2D other)
    //{
    //    // Check if the object entering the trigger has the tag "Player"
    //    if (other.CompareTag("Player"))
    //    {
    //        // Play the sound at the coin's current position
    //        if (pickupSound != null)
    //        {
    //            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
    //        }

    //        // Destroy the coin object so it disappears
    //        Destroy(gameObject);
    //    }
    //}

    //Optional: If your game is 3D, uncomment this method and delete the 2D one above!


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }
            Destroy(gameObject);
        }
    }

}