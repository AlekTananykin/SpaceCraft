using UnityEngine;
using Unity.Collections;
using Unity.Jobs;


public class Asteroids : MonoBehaviour
{
    struct Asteroid
    {
        public Vector3 Direction;
        public Quaternion Rotation;

        public float SpeedRotation;

        public Vector3 WorldPosition;
        public Quaternion WorldRotation;
        public float Angle;
    }

    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material _material;
    [SerializeField, Range(1, 8)] private float _radius = 4;
    [SerializeField, Range(1, 8)] private int _asteroidsCount = 4;
    [SerializeField, Range(0, 360)] private float _speedRotation = 80;

    private const float _positionOffset = 1.5f;
    
    private NativeArray<Asteroid> _asteroids;
    private NativeArray<Matrix4x4> _matrices;

    private ComputeBuffer _matricesBuffer;
    private static readonly int _matricesId = Shader.PropertyToID("_Matrices");
    private static MaterialPropertyBlock _propertyBlock;

    private static readonly Vector3[] _directions =
    {
        Vector3.up,
        Vector3.left,
        Vector3.right,
        Vector3.forward,
        Vector3.back
    };
    private static readonly Quaternion[] _rotations =
    {
        Quaternion.identity,
        Quaternion.Euler(.0f, .0f, 90.0f),
        Quaternion.Euler(.0f, .0f, -90.0f),
        Quaternion.Euler(90.0f, .0f, .0f),
        Quaternion.Euler(-90.0f, .0f, .0f)
    };

    private void OnEnable()
    {
        var stride = 16 * 4;
        _matricesBuffer = new ComputeBuffer(_asteroidsCount, stride);
        
        _asteroids = new NativeArray<Asteroid>(_asteroidsCount, Allocator.Persistent);

        _matrices = new NativeArray<Matrix4x4>(_asteroidsCount, Allocator.Persistent);

        for (var i = 0; i < _asteroids.Length; ++i)
        {
            _asteroids[i] = CreateAsteroid();
        }
        _propertyBlock ??= new MaterialPropertyBlock();
    }

    private void OnDisable()
    {
        if (null == _matricesBuffer)
            return;
        
        _matricesBuffer.Release();

        _asteroids.Dispose();
        _matrices.Dispose();

        _matricesBuffer = null;
    }

    private void OnValidate()
    {
        if (!enabled)
        {
            return;
        }
        OnDisable();
        OnEnable();
    }

    private Asteroid CreateAsteroid() 
    {

        return new Asteroid
        {
            Direction = _directions[1] * _radius,
            Rotation = _rotations[1],
            SpeedRotation = Random.value,
            Angle = Random.value
        };
    }

    private void Update()
    {
        var spinAngelDelta = _speedRotation * Time.deltaTime;

        JobHandle jobHandle = default;

        jobHandle = new UpdateAsteroids
        {
            DeltaTime = Time.deltaTime,

            Asteroids = _asteroids,
            Matrices = _matrices
        }.Schedule(_asteroids.Length, jobHandle);
        jobHandle.Complete();
        
        var bounds = new Bounds(new Vector3(), 3f * Vector3.one);

        if (null == _matricesBuffer)
            return;

        _matricesBuffer.SetData(_matrices);

        _propertyBlock.SetBuffer(_matricesId, _matricesBuffer);
        _material.SetBuffer(_matricesId, _matricesBuffer);
        Graphics.DrawMeshInstancedProcedural(_mesh, 0, _material, bounds,
        _matricesBuffer.count, _propertyBlock);
    }

    private struct UpdateAsteroids : IJobFor
    {
        public float DeltaTime;

        public NativeArray<Asteroid> Asteroids;

        [WriteOnly]
        public NativeArray<Matrix4x4> Matrices;

        public void Execute(int index)
        {
            var asteroid = Asteroids[index];

            asteroid.Angle += asteroid.SpeedRotation * DeltaTime;

            asteroid.WorldRotation = asteroid.WorldRotation *
                (Quaternion.Euler(0f, asteroid.Angle, 0f));

            asteroid.WorldPosition = asteroid.WorldPosition +
                asteroid.WorldRotation * (_positionOffset * asteroid.Direction);

            Asteroids[index] = asteroid;
            Matrices[index] = Matrix4x4.TRS(
                asteroid.WorldPosition, asteroid.WorldRotation, Vector3.one);
        }
    }
}
