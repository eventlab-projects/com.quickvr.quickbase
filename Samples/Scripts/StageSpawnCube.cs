using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using QuickVR;

public class StageSpawnCube : QuickStageBase
{

    public Transform _spawnCenter = null;
    public float _spawnRadius = 1.0f;

    public override void Init()
    {
        base.Init();

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.parent = transform;
        cube.transform.localScale = Vector3.one * 0.25f;

        cube.transform.position = _spawnCenter.position + (Random.insideUnitSphere * _spawnRadius);
    }

}








