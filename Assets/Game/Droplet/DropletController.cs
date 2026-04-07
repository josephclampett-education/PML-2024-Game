using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class DropletController : MonoBehaviour
{
    // Public
    public float Speed;

    // Private
    private Rigidbody Rigidbody;
    private MeshRenderer Renderer;
    
    private BathRenderer BathRenderer;
    
    private Vector2 MovementInput;
    private Vector2 SmoothedMovementInput;
    
    private static int PlayerCount = 0;
    private int PlayerIndex = -1;

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        Renderer = GetComponent<MeshRenderer>();
        
        BathRenderer = FindFirstObjectByType<BathRenderer>();
    }

    private void OnEnable()
    {
        PlayerIndex = PlayerCount;
        PlayerCount++;

        switch (PlayerIndex)
        {
            case 0:
                BathRenderer.Droplet0 = this.gameObject;
                Renderer.material.color = Color.red;
                break;
            case 1:
                BathRenderer.Droplet1 = this.gameObject;
                Renderer.material.color = Color.blue;
                break;
        }
    }
    
    private void OnDisable()
    {
        PlayerIndex = -1;
        PlayerCount--;

        switch (PlayerIndex)
        {
            case 0:
                BathRenderer.Droplet0 = null;
                break;
            case 1:
                BathRenderer.Droplet1 = null;
                break;
        }
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