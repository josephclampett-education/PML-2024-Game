using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshRenderer))]
public class BathRenderer : MonoBehaviour
{
    // Public
    [NonSerialized] public GameObject Droplet0 = null;
    [NonSerialized] public GameObject Droplet1 = null;
    
    public RenderTexture WavefieldTexture;
    public Material WavefieldMaterial;
    
    // Private
    private MeshRenderer Renderer;
    private CommandBuffer CommandBuffer;

    private void Awake()
    {
        Renderer = GetComponent<MeshRenderer>();
        CommandBuffer = new CommandBuffer();
    }

    private void OnEnable()
    {
        RenderPipelineManager.activeRenderPipelineCreated += InitCallback;
        RenderPipelineManager.beginCameraRendering += RenderCallback;
    }

    private void OnDisable()
    {
        RenderPipelineManager.activeRenderPipelineCreated -= InitCallback;
        RenderPipelineManager.beginCameraRendering -= RenderCallback;
    }

    private void InitCallback()
    {
        if (WavefieldMaterial == null || WavefieldTexture == null || Renderer == null)
            return;
        
        RenderTexture activeTarget = RenderTexture.active;
        RenderTexture.active = WavefieldTexture;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = activeTarget;
    }

    private void RenderCallback(ScriptableRenderContext context, Camera camera)
    {
        // Set constants
        if (Droplet0 != null)
        {
            WavefieldMaterial.SetFloat(ID_Droplet0Scale, 1.0f);
            WavefieldMaterial.SetVector(ID_Droplet0Position, new Vector2(Droplet0.transform.position.x, Droplet0.transform.position.z));
        }
        else
        {
            WavefieldMaterial.SetFloat(ID_Droplet0Scale, 0.0f);
        }

        if (Droplet1 != null)
        {
            WavefieldMaterial.SetFloat(ID_Droplet1Scale, 1.0f);
            WavefieldMaterial.SetVector(ID_Droplet1Position, new Vector2(Droplet1.transform.position.x, Droplet1.transform.position.z));
        }
        else
        {
            WavefieldMaterial.SetFloat(ID_Droplet1Scale, 0.0f);
        }
        
        WavefieldMaterial.SetVector(ID_DomainSize, new Vector2(Renderer.bounds.extents.x, Renderer.bounds.extents.z));
        
        // Draw
        CommandBuffer.GetTemporaryRT(ID_WavefieldBuffer, WavefieldTexture.width, WavefieldTexture.height, 0, FilterMode.Bilinear, WavefieldTexture.format);
        CommandBuffer.Blit(WavefieldTexture, ID_WavefieldBuffer, WavefieldMaterial);
        
        CommandBuffer.Blit(ID_WavefieldBuffer, WavefieldTexture);
        
        context.ExecuteCommandBuffer(CommandBuffer);
        
        CommandBuffer.Clear();
    }
    
    // IDs
    private static readonly int ID_WavefieldBuffer = Shader.PropertyToID("WavefieldBuffer");
    private static readonly int ID_Droplet0Scale = Shader.PropertyToID("Droplet0Scale");
    private static readonly int ID_Droplet0Position = Shader.PropertyToID("Droplet0Position");
    private static readonly int ID_Droplet1Scale = Shader.PropertyToID("Droplet1Scale");
    private static readonly int ID_Droplet1Position = Shader.PropertyToID("Droplet1Position");
    private static readonly int ID_DomainSize = Shader.PropertyToID("DomainSize");
}