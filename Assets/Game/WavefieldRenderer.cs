using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class WavefieldRenderer : MonoBehaviour
{
    // Public
    public RenderTexture WavefieldTexture;
    
    // Private
    private static int WavefieldBuffer = Shader.PropertyToID("WavefieldBuffer");
    
    private Material WavefieldMaterial;
    
    private Texture BesselLUT;
    private static int BesselLUT_ID = Shader.PropertyToID("_BesselLUT");

    private MeshRenderer WavePlane = null;

    private CommandBuffer CommandBuffer;

    private void Awake()
    {
        CommandBuffer = new CommandBuffer();
    }

    private void OnEnable()
    {
        Shader wavefieldShader = Shader.Find("Game/Wavefield");
        WavefieldMaterial = new Material(wavefieldShader);

        BesselLUT = Resources.Load<Texture2D>("BesselGenerator/BesselMap");
        WavefieldMaterial.SetTexture(BesselLUT_ID, BesselLUT);
        
        // Proxy
        WavePlane = GameObject.Find("WavePlane").GetComponent<MeshRenderer>();

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
        RenderTexture activeTarget = RenderTexture.active;
        RenderTexture.active = WavefieldTexture;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = activeTarget;
    }
    
    private Transform Droplet0 = null;
    private Transform Droplet1 = null;

    public void SetDroplet0(Transform d) => Droplet0 = d;
    public void UnsetDroplet0() => Droplet0 = null;

    public void SetDroplet1(Transform d) => Droplet1 = d;
    public void UnsetDroplet1() => Droplet1 = null;

    private void RenderCallback(ScriptableRenderContext context, Camera camera)
    {
        // Set constants
        CommandBuffer.SetGlobalFloat("Droplet0Scale", Droplet0 == null ? 0.0f : 1.0f);
        CommandBuffer.SetGlobalVector("Droplet0Position", Droplet0 == null ? Vector4.zero : new Vector4(Droplet0.transform.position.x, Droplet0.transform.position.z, 0, 0));
        
        CommandBuffer.SetGlobalFloat("Droplet1Scale", Droplet1 == null ? 0.0f : 1.0f);
        CommandBuffer.SetGlobalVector("Droplet1Position", Droplet1 == null ? Vector4.zero : new Vector4(Droplet1.transform.position.x, Droplet1.transform.position.z, 0, 0));
        
        Vector3 domainSize = WavePlane.bounds.extents;
        CommandBuffer.SetGlobalVector("DomainSize", new Vector4(domainSize.x, domainSize.z, 0, 0));
        
        // Draw
        CommandBuffer.GetTemporaryRT(WavefieldBuffer, WavefieldTexture.width, WavefieldTexture.height, 0, FilterMode.Bilinear, WavefieldTexture.format);
        CommandBuffer.Blit(WavefieldTexture, WavefieldBuffer, WavefieldMaterial);
        
        CommandBuffer.Blit(WavefieldBuffer, WavefieldTexture);
        
        context.ExecuteCommandBuffer(CommandBuffer);
        
        CommandBuffer.Clear();
    }
}