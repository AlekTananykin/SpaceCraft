using System;
using UnityEngine;

[Serializable]
public class PlanetDescriptor
{
    public GameObject Prefab;
    public Vector3 Position;
    public float RotationSpeed;

    public bool IsHasRing;
    public float RingRadius;
    public float RingWidth;
}
