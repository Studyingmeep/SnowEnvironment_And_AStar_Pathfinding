using UnityEngine;

public struct Line
{
    private const float VerticalLineGradient = 1e5f;
    
    private float _gradient;
    private float _yIntercept;
    private Vector2 pointOnLine_1;
    private Vector2 pointOnLine_2;
    
    private float _gradientPerpendicular;

    private bool approachSide;

    public Line(Vector2 pointOnLine, Vector2 pointPerpendicularToLine) : this()
    {
        float dx = pointOnLine.x - pointPerpendicularToLine.x;
        float dy = pointOnLine.y - pointPerpendicularToLine.y;

        if (dx == 0)
        {
            _gradientPerpendicular = VerticalLineGradient;
        }
        else
        {
            _gradientPerpendicular = dy / dx;
        }

        if (_gradientPerpendicular == 0)
        {
            _gradient = VerticalLineGradient;
        }
        else
        {
            _gradient = -1 / _gradientPerpendicular;
        }
        
        _yIntercept = pointOnLine.y - _gradient * pointOnLine.x;
        pointOnLine_1 = pointOnLine;
        pointOnLine_2 = pointOnLine + new Vector2(1, _gradient);
        
        approachSide = false;    
        approachSide = GetSide(pointPerpendicularToLine);
    }
    
    bool GetSide(Vector2 point)
    {
        return (point.x-pointOnLine_1.x) * (pointOnLine_2.y - pointOnLine_1.y) > (point.y - pointOnLine_1.y) * (pointOnLine_2.x - pointOnLine_1.x);
    }

    public bool HasCrossedLine(Vector2 point)
    {
        return GetSide(point) != approachSide;
    }

    public float DistanceFromPoint(Vector2 point)
    {
        float yInterceptPerpendicular = point.y - _gradientPerpendicular * point.x;
        float intersectX = (yInterceptPerpendicular - _yIntercept) / (_gradient - _gradientPerpendicular);
        float intersectY = _gradient * intersectX + _yIntercept;
        return Vector2.Distance(point, new Vector2(intersectX, intersectY));
    }

    public void DrawWithGizmos(float length)
    {
        Vector3 lineDirection = new Vector3(1, 0, _gradient).normalized;
        Vector3 lineCentre = new Vector3(pointOnLine_1.x, 0, pointOnLine_1.y) + Vector3.up;
        Gizmos.DrawLine(lineCentre - lineDirection * length / 2f, lineCentre + lineDirection * length / 2f);
    }
}
