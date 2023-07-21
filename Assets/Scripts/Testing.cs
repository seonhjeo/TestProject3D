using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using Random = UnityEngine.Random;

[BurstCompile]
public struct ReallyToughParallelJob : IJobParallelFor
{
    public NativeArray<float3> positionArray;
    public NativeArray<float> moveYArray;
    public float deltaTime;

    public void Execute(int index)
    {
        positionArray[index] += new float3(0f, moveYArray[index] * deltaTime, 0f);
        
        if (positionArray[index].y > 5f)
        {
            moveYArray[index] = -math.abs(moveYArray[index]);
        }
        if (positionArray[index].y < -5f)
        {
            moveYArray[index] = +math.abs(moveYArray[index]);
        }
            
        float value = 0f;
        for (int i = 0; i < 1000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }
}

[BurstCompile]
public struct ReallyToughParallelJobTransform : IJobParallelForTransform
{
    public NativeArray<float> moveYArray;
    [ReadOnly] public float deltaTime;

    public void Execute(int index, TransformAccess transform)
    {
        transform.position += new Vector3(0f, moveYArray[index] * deltaTime, 0f);
        
        if (transform.position.y > 5f)
        {
            moveYArray[index] = -math.abs(moveYArray[index]);
        }
        if (transform.position.y < -5f)
        {
            moveYArray[index] = +math.abs(moveYArray[index]);
        }
            
        float value = 0f;
        for (int i = 0; i < 1000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }
}

public class Testing : MonoBehaviour
{
    [SerializeField] private bool useJobs;
    [SerializeField] private Transform pfZombie;

    private List<Zombie> _zombieList;

    public class Zombie
    {
        public Transform transform;
        public float moveY;
    }

    private void Start()
    {
        _zombieList = new List<Zombie>();

        for (int i = 0; i < 1000; i++)
        {
            Transform zombieTransform = Instantiate(pfZombie, new Vector3(Random.Range(-8f, 8f), Random.Range(-5f, 5f)), Quaternion.identity);
            
            _zombieList.Add(new Zombie
            {
                transform = zombieTransform,
                moveY = Random.Range(1f, 2f)
            });
        }
    }
    
    private void Update()
    {
        float startTime = Time.realtimeSinceStartup;

        if (useJobs)
        {
            NativeArray<float> moveYArray = new NativeArray<float>(_zombieList.Count, Allocator.TempJob);
            TransformAccessArray transformAccessArray = new TransformAccessArray(_zombieList.Count);

            for (int i = 0; i < _zombieList.Count; i++)
            {
                moveYArray[i] = _zombieList[i].moveY;
                transformAccessArray.Add(_zombieList[i].transform);
            }

            ReallyToughParallelJobTransform reallyToughParallelJobTransform = new ReallyToughParallelJobTransform
            {
                deltaTime = Time.deltaTime,
                moveYArray = moveYArray
            };

            JobHandle jobHandle = reallyToughParallelJobTransform.Schedule(transformAccessArray);
            jobHandle.Complete();
            
            for (int i = 0; i < _zombieList.Count; i++)
            {
                _zombieList[i].moveY = moveYArray[i];
            }
            
            transformAccessArray.Dispose();
            moveYArray.Dispose();
        }
        else
        {
            foreach (Zombie zombie in _zombieList)
            {
                zombie.transform.position += new Vector3(0, zombie.moveY * Time.deltaTime);
            
                if (zombie.transform.position.y > 5f)
                {
                    zombie.moveY = -math.abs(zombie.moveY);
                }
                if (zombie.transform.position.y < -5f)
                {
                    zombie.moveY = +math.abs(zombie.moveY);
                }
            
                float value = 0f;
                for (int i = 0; i < 1000; i++)
                {
                    value = math.exp10(math.sqrt(value));
                }
            }
        }

        Debug.Log((Time.realtimeSinceStartup - startTime) * 1000f + "ms");
    }
}



