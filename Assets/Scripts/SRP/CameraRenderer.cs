using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEngine;

public class CameraRenderer
{
    private ScriptableRenderContext _context;
    private Camera _camera;
    private CullingResults _cullingResults;

    private const string _bufferName = "Camera Render";
    private readonly CommandBuffer _commandBuffer 
        = new CommandBuffer{name = _bufferName};

    private static readonly List<ShaderTagId> drawingShaderTagIds =
        new List<ShaderTagId> {new ShaderTagId("SRPDefaultUnlit") };

    public void Render(ScriptableRenderContext context, Camera camera)
    {
        _camera = camera;
        _context = context;


        PrepareForSceneWindow();

        if (!_camera.TryGetCullingParameters(out var parameters))
            return;

        _cullingResults = _context.Cull(ref parameters);

        Settings();

        DrawVisible();
        DrawUnsupportedShaders();
        
#if UNITY_EDITOR
        DrawGizmos();
#endif
        Submit();
    }

    private void DrawVisible()
    {
        var drawingSettings = CreateDrawingSettings(drawingShaderTagIds,
            SortingCriteria.CommonOpaque, out var sortingSettings);
        
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

        _context.DrawRenderers(_cullingResults, ref drawingSettings, ref
            filteringSettings);

        _context.DrawSkybox(_camera); 

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;

        _context.DrawRenderers(
            _cullingResults, ref drawingSettings, ref filteringSettings);

    }

    private void Settings()
    {
        _commandBuffer.ClearRenderTarget(true, true, Color.clear);
        _commandBuffer.BeginSample(_camera.name);
        ExecuteCommandBuffer();
        _context.SetupCameraProperties(_camera);
    }

    private void Submit()
    {
        _commandBuffer.EndSample(_bufferName);
        ExecuteCommandBuffer();
        _context.Submit();
    }

    private void ExecuteCommandBuffer()
    {
        _context.ExecuteCommandBuffer(_commandBuffer);
        _commandBuffer.Clear();
    }

    private void DrawGizmos()
    {
        if (!Handles.ShouldRenderGizmos())
        {
            return;
        
        _context.DrawGizmos(_camera, GizmoSubset.PreImageEffects);}
        _context.DrawGizmos(_camera, GizmoSubset.PostImageEffects);
    }

    private DrawingSettings CreateDrawingSettings(
        List<ShaderTagId> shaderTags, SortingCriteria
        sortingCriteria, out SortingSettings sortingSettings)
    {
        sortingSettings = new SortingSettings(_camera)
        {
            criteria = sortingCriteria,
        };
        var drawingSettings = new DrawingSettings(shaderTags[0], sortingSettings);
        for (var i = 1; i < shaderTags.Count; i++)
        {
            drawingSettings.SetShaderPassName(i, shaderTags[i]);
        }
        return drawingSettings;
    }

#if UNITY_EDITOR
    private static readonly ShaderTagId[] _legacyShaderTagIds =
    {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
    };

    private static Material _errorMaterial = new
        Material(Shader.Find("Hidden/InternalErrorShader"));
    
    private void DrawUnsupportedShaders()
    {
        var drawingSettings = new DrawingSettings(
            _legacyShaderTagIds[0], new SortingSettings(_camera))
        {
             overrideMaterial = _errorMaterial,
        };
        for (var i = 1; i < _legacyShaderTagIds.Length; i++)
        {
            drawingSettings.SetShaderPassName(i, _legacyShaderTagIds[i]);
        }
        var filteringSettings = FilteringSettings.defaultValue;

        _context.DrawRenderers(_cullingResults, ref drawingSettings, 
            ref filteringSettings);
    }
#endif

    private void PrepareForSceneWindow () 
    {
		ScriptableRenderContext.EmitWorldGeometryForSceneView(_camera);
	}
}
