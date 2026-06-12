using UnityEngine;
using UnityEngine.InputSystem;



public class skateflip : MonoBehaviour
{


    [SerializeField] private Animator Grimble; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
           Grimble.SetBool("bool", true);
        }

        else
        {
            Grimble.SetBool("bool", false);
        }
        
    }
}
