using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public Transform target;
    private float _speed = 20;
    private Vector3[] _path;
    private int _targetIndex;

    private void Start()
    {
        PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            _path = newPath;
            _targetIndex = 0;
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    IEnumerator FollowPath()
    {
        Vector3 currentWaypoint = _path[0];

        while (true)
        {
            if (transform.position == currentWaypoint)
            {
                _targetIndex++;
                if (_targetIndex >= _path.Length)
                {
                    yield break;
                }
                currentWaypoint = _path[_targetIndex];
            }
            
            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, _speed * Time.deltaTime);
            yield return null;
            
        }
    }

    public void OnDrawGizmos()
    {
        if (_path != null)
        {
            for (int i = _targetIndex; i < _path.Length; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(_path[i], Vector3.one);

                if (i == _targetIndex)
                {
                    Gizmos.DrawLine(transform.position, _path[i]);
                }
                else
                {
                    Gizmos.DrawLine(_path[i-1], _path[i]);
                }
            }
        }
    }
}
