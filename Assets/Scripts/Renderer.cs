using UnityEngine;
using UnityEngine.Rendering;

public class Renderer : MonoBehaviour
{
    [SerializeField] private RenderPipelineAsset renderPipelineAsset;
    
    private void Start()
    {
        GraphicsSettings.defaultRenderPipeline = renderPipelineAsset;
        QualitySettings.renderPipeline = renderPipelineAsset;
    }
}
