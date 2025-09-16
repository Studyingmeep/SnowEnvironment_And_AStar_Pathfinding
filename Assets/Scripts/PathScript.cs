using UnityEngine;

public class PathScript
{
    public readonly Vector3[] lookPoints;
    public readonly Line[] turnBoundaries;
    public readonly int finishLineIndex;
    public readonly int slowDownIndex;

    public PathScript(Vector3[] waypoints, Vector3 startPos, float turnDistance, float stoppingDistance)
    {
        lookPoints = waypoints;
        turnBoundaries = new Line[lookPoints.Length];
        finishLineIndex = lookPoints.Length - 1;
        
        Vector2 previousPoint = V3ToV2(startPos);
        for (int i = 0; i < lookPoints.Length; i++)
        {
            Vector2 currentPoint = V3ToV2(lookPoints[i]);
            Vector2 directionToCurrentPoint = (currentPoint - previousPoint).normalized;
            Vector2 turnBoundaryPoint = (i == finishLineIndex)? currentPoint : currentPoint - directionToCurrentPoint * turnDistance;
            turnBoundaries[i] = new Line(turnBoundaryPoint, previousPoint - directionToCurrentPoint * turnDistance);
            previousPoint = turnBoundaryPoint;
        }

        float distanceFromEndPoint = 0;
        for (int i = lookPoints.Length - 1; i > 0; i--)
        {
            distanceFromEndPoint += Vector3.Distance(lookPoints[i], lookPoints[i - 1]);
            if (distanceFromEndPoint > stoppingDistance)
            {
                slowDownIndex = i;
                break;
            }
        }
    }   
    
    Vector2 V3ToV2(Vector3 v3)
    {
        return new Vector2(v3.x, v3.z);
    }

    public void DrawWithGizmos()
    {
        Gizmos.color = Color.black;
        foreach (Vector3 p in lookPoints)
        {
            Gizmos.DrawCube(p + Vector3.up, Vector3.one);
        }
        
        Gizmos.color = Color.white;
        foreach (Line l in turnBoundaries)
        {
            l.DrawWithGizmos(10);
        }
    }
}
