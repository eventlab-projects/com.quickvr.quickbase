using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using QuickVR;

public class StageSpawnSphere : QuickStageBase
{

    public Transform _spawnCenter = null;
    public float _spawnRadius = 1.0f;

    public override void Init()
    {
        base.Init();

        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.parent = transform;
        sphere.transform.localScale = Vector3.one * 0.25f;

        sphere.transform.position = _spawnCenter.position + (Random.insideUnitSphere * _spawnRadius);
    }

}








