using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

public class Unit : MonoBehaviour
{
    private const float minPathUpdateTime = 0.2f;
    private const float pathUpdateMoveThreshold = 0.5f;
    
    public Transform target;
    public float speed = 20;
    public float turnSpeed = 3;
    public float turnDistance = 5;
    public float stoppingDistance = 10;
    
    PathScript _path;

    private void Start()
    {
        StartCoroutine(UpdatePath());
    }

    public void OnPathFound(Vector3[] wayPoints, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            _path = new PathScript(wayPoints, transform.position, turnDistance, stoppingDistance);
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    IEnumerator UpdatePath()
    {
        if (Time.timeSinceLevelLoad < 0.3f)
        {
            yield return new WaitForSeconds(0.3f);
        }
        PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
        
        float squareMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 targetPositionOld = target.position;
        
        while (true)
        {
            yield return new WaitForSeconds(minPathUpdateTime);
            if ((target.position - targetPositionOld).sqrMagnitude > squareMoveThreshold)
            {
                PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
                targetPositionOld = target.position;
            }
        }
    }

    IEnumerator FollowPath()
    {
        bool followingPath = true;
        int _pathIndex = 0;
        transform.LookAt(_path.lookPoints [0]);
        
        float speedPercent = 1;
        
        Vector2 moveDirection = Vector2.zero;

        while (followingPath)
        {
            Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
            if (_path.turnBoundaries[_pathIndex].HasCrossedLine(pos2D))
            {
                if (_pathIndex == _path.finishLineIndex)
                {
                    followingPath = false;
                    break;
                }
                else
                {
                    _pathIndex++;
                }
            }

            if (followingPath)
            {
                if (_pathIndex >= _path.slowDownIndex && stoppingDistance > 0)
                {
                    speedPercent = Mathf.Clamp01(_path.turnBoundaries[_path.finishLineIndex].DistanceFromPoint(pos2D) /
                                                 stoppingDistance);
                    if (speedPercent < 0.01f)
                    {
                        followingPath = false;
                    }
                }
                
                Vector2 targetPos2D = new Vector2(_path.lookPoints[_pathIndex].x, _path.lookPoints[_pathIndex].z);
                Vector2 currentPos2D = new Vector2(transform.position.x, transform.position.z);
                Vector2 targetDirection = (targetPos2D - currentPos2D).normalized;

                moveDirection = Vector2.Lerp(moveDirection, targetDirection, Time.deltaTime * turnSpeed).normalized;

                Vector3 move3D = new Vector3(moveDirection.x, 0, moveDirection.y);
                transform.position += move3D * (speed * speedPercent * Time.deltaTime);
            }
            
            yield return null;
        }
    }

    public void OnDrawGizmos()
    {
        if (_path != null)
        {
            _path.DrawWithGizmos();
        }
    }
}
