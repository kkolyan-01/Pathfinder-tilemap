using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartPoint : MonoBehaviour
{
    [SerializeField] private Transform _target;
    private List<Vector2> _path;

    private PathFinderTilemap _pathFinder;

    private void Start()
    {
        _pathFinder = GetComponent<PathFinderTilemap>();
    }

    private void Update()
    {
        Vector3Int targetCell = Vector3Int.FloorToInt(_target.position);
        _path = _pathFinder.GetPathToTarget(targetCell);
        print(_path.Count);
        
    }
}
