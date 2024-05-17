using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float Speed;

    private Rigidbody Rigidbody;
    
    private WavefieldRenderer Renderer;
    
    private Vector2 MovementInput;
    private Vector2 SmoothedMovementInput;
    
    private static int PlayerCount = 0;
    private int PlayerIndex = -1;

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        
        Renderer = FindFirstObjectByType<WavefieldRenderer>();
    }

    private void OnEnable()
    {
        PlayerIndex = PlayerCount;
        PlayerCount++;

        switch (PlayerIndex)
        {
            case 0:
                GetComponent<MeshRenderer>().material =
                    Resources.Load<Material>("Droplet0Material");
                
                Renderer.SetDroplet0(transform);
                break;
            case 1:
                GetComponent<MeshRenderer>().material =
                    Resources.Load<Material>("Droplet1Material");
                
                Renderer.SetDroplet1(transform);
                break;
        }
        
        Debug.Log("Joined");
    }
    
    private void OnDisable()
    {
        PlayerIndex = -1;
        PlayerCount--;

        switch (PlayerIndex)
        {
            case 0:
                Renderer.UnsetDroplet0();
                break;
            case 1:
                Renderer.UnsetDroplet1();
                break;
        }
        
        Debug.Log("Left");
    }

    private void FixedUpdate()
    {
        Vector2 movementInputSmoothVelocity = Vector3.zero;
        SmoothedMovementInput = Vector2.SmoothDamp(SmoothedMovementInput, MovementInput, ref movementInputSmoothVelocity, 0.06f);

        Vector2 planarVelocity = SmoothedMovementInput * Speed;

        float verticalVelocity = -Mathf.Cos(2 * Mathf.PI * Time.time * 0.4f) * 6;
        
        Rigidbody.linearVelocity = new Vector3(planarVelocity.x, verticalVelocity, planarVelocity.y);
    }

    private void OnMove(InputValue inputValue)
    {
        MovementInput = inputValue.Get<Vector2>();
    }
}