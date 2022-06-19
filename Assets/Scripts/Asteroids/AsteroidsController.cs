using UnityEngine;
using Unity.Collections;
using Unity.Jobs;


public class AsteroidsController : MonoBehaviour
{
    struct FractalPart
    {
        public Vector3 Direction;
        public Quaternion Rotation;
        public Vector3 WorldPosition;
        public Quaternion WorldRotation;

        public float SpinAngle;
    }

    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material _material;
    
    [SerializeField] private int _childCount = 150;
    [SerializeField, Range(0, 360)] private int _speedRotation = 80;
    [SerializeField] private float _radius = 15.0f;
    [SerializeField] private int _asteroidsCount = 15;

    private const float _positionOffset = 1.5f;
    
    private const int _depth = 2;
    private NativeArray<FractalPart>[] _parts;
    private NativeArray<Matrix4x4>[] _matrices;
    private ComputeBuffer[] _matricesBuffers;
    private static readonly int _matricesId = Shader.PropertyToID("_Matrices");
    private static MaterialPropertyBlock _propertyBlock;

    private void OnEnable()
    {
        _parts = new NativeArray<FractalPart>[_depth];
        _matrices = new NativeArray<Matrix4x4>[_depth];
        _matricesBuffers = new ComputeBuffer[_depth];

        var stride = 16 * 4;
        for (int i = 0, length = 1; 
            i < _parts.Length; i++, length *= _childCount)
        {
            _parts[i] = new NativeArray<FractalPart>(length, Allocator.Persistent);
            _matrices[i] = new NativeArray<Matrix4x4>(length, Allocator.Persistent);

            _matricesBuffers[i] = new ComputeBuffer(length, stride);
        }

        _parts[0][0] = new FractalPart
        {
            Direction = Vector3.up,
            Rotation = Quaternion.identity,
        };


        var levelParts = _parts[1];
        for (var fpi = 0; fpi < levelParts.Length; fpi += _childCount)
        {
            for (var ci = 0; ci < _childCount; ci++)
            {
                levelParts[fpi + ci] = CreatePart();
            }
        }

        _propertyBlock ??= new MaterialPropertyBlock();
    }

    private void OnDisable()
    {
        for (var i = 0; i < _matricesBuffers.Length; i++)
        {
            _matricesBuffers[i].Release();
            _parts[i].Dispose();
            _matrices[i].Dispose();
        }

        _parts = null;
        _matrices = null;
        _matricesBuffers = null;
    }

    private void OnValidate()
    {
        if (_parts is null || !enabled)
        {
            return;
        }
        OnDisable();
        OnEnable();
    }

    private FractalPart CreatePart() 
    {
        Vector3 norm = (new Vector3(Random.value, 0.0f, Random.value)).normalized;

        return new FractalPart
        {
            Direction = norm * _radius * (0.9f + 0.2f * Random.value),
            Rotation = Quaternion.Euler(0.0f, Random.value * 360, 0.0f),
        };
    }

    private void Update()
    {
        var spinAngelDelta = _speedRotation * Time.deltaTime;
        
        var rootPart = _parts[0][0];

        rootPart.SpinAngle += spinAngelDelta;
        var deltaRotation = Quaternion.Euler(.0f, rootPart.SpinAngle, .0f);
        rootPart.WorldRotation = rootPart.Rotation * deltaRotation;
        _parts[0][0] = rootPart;
        _matrices[0][0] = Matrix4x4.TRS(rootPart.WorldPosition,
        rootPart.WorldRotation, Vector3.one);
        
        JobHandle jobHandle = default;

        jobHandle = new UpdateFractalLevel
        {
            AsteroidsCount = _childCount,
            SpinAngleDelta = spinAngelDelta,
            Parents = _parts[0],
            Parts = _parts[1],

            Matrices = _matrices[1]
        }.Schedule(_parts[1].Length, jobHandle);
        jobHandle.Complete();

        var bounds = new Bounds(rootPart.WorldPosition, 3f * Vector3.one);
        for (var i = 1; i < _matricesBuffers.Length; i++)
        {
            var buffer = _matricesBuffers[i];
            buffer.SetData(_matrices[i]);
            _propertyBlock.SetBuffer(_matricesId, buffer);
            _material.SetBuffer(_matricesId, buffer);
            Graphics.DrawMeshInstancedProcedural(_mesh, 0, _material, bounds,
            buffer.count, _propertyBlock);
        }
    }
    
    private struct UpdateFractalLevel : IJobFor
    {
        public float SpinAngleDelta;
        public int AsteroidsCount;

        [ReadOnly]
        public NativeArray<FractalPart> Parents;
        public NativeArray<FractalPart> Parts;

        [WriteOnly]
        public NativeArray<Matrix4x4> Matrices;

        public void Execute(int index)
        {
            var parent = Parents[0];
            var part = Parts[index];

            part.SpinAngle += SpinAngleDelta;

            part.WorldRotation =
                (part.Rotation * Quaternion.Euler(0f, part.SpinAngle, 0f));
            part.WorldPosition = parent.WorldPosition +
                parent.WorldRotation * (_positionOffset * part.Direction);

            Parts[index] = part;
            Matrices[index] = Matrix4x4.TRS(
                part.WorldPosition, part.WorldRotation, Vector3.one);
        }
    }
}
