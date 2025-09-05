using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PathRequestManager : MonoBehaviour
{
    Queue<PathRequest> _pathRequestQueue = new Queue<PathRequest>();
    PathRequest _currentPathRequest;
    
    static PathRequestManager _instance;
    PathfindingAStar _pathfinding;
    
    private bool _isProcessingPath = false;
    
    void Awake()
    {
        _instance = this;
    }
    
    public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback)
    {
        PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback);
        _instance._pathRequestQueue.Enqueue(newRequest);
        _instance.TryProcessNext();
    }

    void TryProcessNext()
    {
        if (!_isProcessingPath && _pathRequestQueue.Count > 0)
        {
            _currentPathRequest = _pathRequestQueue.Dequeue();
            _isProcessingPath = true;
            // _pathfinding.StartFindPath(_currentPathRequest.PathStart, _currentPathRequest.PathEnd, _currentPathRequest.Callback);
        }
    }

    public void FinishedProcessingPath(Vector3[] path, bool success)
    {
        _currentPathRequest.Callback(path, success);
        _isProcessingPath = false;
        TryProcessNext();
    }
    
    struct PathRequest
    {
        public Vector3 PathStart;
        public Vector3 PathEnd;
        public Action<Vector3[], bool> Callback;

        public PathRequest(Vector3 _start, Vector3 _end, Action<Vector3[], bool> _callback)
        {
            PathStart = _start;
            PathEnd = _end;
            Callback = _callback;
        }
    }
    
}
