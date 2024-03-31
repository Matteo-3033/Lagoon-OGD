using UnityEngine;
using UnityEngine.Rendering;

/*
    This script is used to set the correct render pipeline asset at the beginning of the game
    The pipeline could also be set from Edit-> Project Settings -> Graphics -> Scriptable Render Pipeline Settings,
    but this script is useful if you don't want it to be set in the editor (hiding enemies for example)
 */
public class Renderer : MonoBehaviour
{
    [SerializeField] private RenderPipelineAsset renderPipelineAsset;
    
    private void Start()
    {
        GraphicsSettings.defaultRenderPipeline = renderPipelineAsset;
        QualitySettings.renderPipeline = renderPipelineAsset;
    }
}
