using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR.Samples.Workflow
{

    public class StageSpawnCube : QuickStageBase
    {

        public Transform _spawnCenter = null;
        public float _spawnRadius = 1.0f;

        protected override IEnumerator CoUpdate()
        {
            yield return new WaitForSeconds(3);

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.parent = transform;
            cube.transform.localScale = Vector3.one * 0.25f;

            cube.transform.position = _spawnCenter.position + (Random.insideUnitSphere * _spawnRadius);
        }

    }

}




