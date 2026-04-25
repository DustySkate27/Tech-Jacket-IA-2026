using UnityEngine;

public class ObstacleAvoidance
{
    Transform _entity;
    float _radius;
    float _angle;
    float _personalArea;
    LayerMask _obsMask;
    Collider[] _colls;
    public ObstacleAvoidance(Transform entity, float radius, float angle, float personalArea, LayerMask obsMask, int countMaxObs = 5)
    {
        _entity = entity;
        _radius = radius; 
        _radius = Mathf.Min(_radius, 1);
        _angle = angle;
        _obsMask = obsMask;
        _colls = new Collider[countMaxObs];
        _personalArea = personalArea;
    }
    public Vector3 GetDir(Vector3 currentSpeed)
    {
        int count = Physics.OverlapSphereNonAlloc(_entity.position, _radius, _colls, _obsMask);
        Collider nearColl = null;
        float nearCollDistance = 0;
        Vector3 nearClosetPoint = Vector3.zero;
        for (int i = 0; i < count; i++)
        {
            var currColl = _colls[i];
            Vector3 closetPoint = currColl.ClosestPoint(_entity.position);
            closetPoint.y = _entity.position.y;
            Vector3 dirToColl = closetPoint - _entity.position;
            float distance = dirToColl.magnitude;
            float currAngle = Vector3.Angle(dirToColl, currentSpeed);
            if (currAngle > _angle / 2) continue;
            if (nearColl == null || distance < nearCollDistance)
            {
                nearColl = currColl;
                nearCollDistance = distance;
                nearClosetPoint = closetPoint;
            }
        }
        if (nearColl == null)
        {
            return currentSpeed;
        }
        Vector3 relativePos = _entity.InverseTransformPoint(nearClosetPoint);
        Vector3 dirToClosetPoint = (nearClosetPoint - _entity.position).normalized;
        Vector3 newDir;
        if (relativePos.x < 0)
        {
            newDir = Vector3.Cross(_entity.up, dirToClosetPoint);
        }
        else
        {
            newDir = -Vector3.Cross(_entity.up, dirToClosetPoint);
        }
        Debug.Log("NewDir" + newDir);
        Debug.DrawRay(_entity.position, newDir, Color.red);
        return Vector3.Lerp(currentSpeed, newDir, (_radius - Mathf.Clamp(nearCollDistance - _personalArea, 0, _radius)) / _radius);
    }
}

