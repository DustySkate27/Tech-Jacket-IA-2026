using Unity.VisualScripting;
using UnityEngine;

public class EnemyObstacleAvoidance
{
    private Transform hitBox;
    private float _radius;
    private float _angle;
    private float _personalArea;

    private LayerMask _obsMask;
    Collider[] _colliders;

    public EnemyObstacleAvoidance(Transform transform, float radius, float angle, float personalArea, LayerMask obsMask, int countMaxObs)
    {
        hitBox = transform; //transform del objeto
        _radius = radius; //Radio máximo
        _radius = Mathf.Min(_radius, 1); //Radio mínimo
        _angle = angle; //grados
        _personalArea = personalArea; //
        _obsMask = obsMask; //Con lo que puede chocar
        _colliders = new Collider[countMaxObs]; //array de colliders
    }

    public Vector3 GetDir(Vector3 currentSpeed)
    {
        int count = Physics.OverlapSphereNonAlloc(hitBox.position, _radius, _colliders, _obsMask);

        Collider nearColl = null;
        float nearCollDistance = float.MaxValue;
        Vector3 nearClosestPoint = Vector3.zero;

        for (int i = 0; i < count; i++)
        {
            Vector3 closestPoint = _colliders[i].ClosestPoint(hitBox.position);
            closestPoint.y = hitBox.position.y;

            Vector3 dirToColl = closestPoint - hitBox.position;
            float distance = dirToColl.magnitude;
            float currentAngle = Vector3.Angle(dirToColl, currentSpeed);

            if (currentAngle > _angle / 2) continue;

            if (nearColl == null || distance < nearCollDistance)
            {
                nearColl = _colliders[i];
                nearCollDistance = distance;
                nearClosestPoint = closestPoint;
            }
        }

        if (nearColl == null)
        {
            return currentSpeed;
        }

        Vector3 relativePos = hitBox.InverseTransformDirection(nearClosestPoint);
        Vector3 dirToClosestPoint = (nearClosestPoint - hitBox.position).normalized;
        Vector3 newDir;

        if (relativePos.x < 0)
            newDir = Vector3.Cross(hitBox.up, dirToClosestPoint);
        else
            newDir = -Vector3.Cross(hitBox.up, dirToClosestPoint);
       
        return Vector3.Lerp(currentSpeed, newDir, (_radius - Mathf.Clamp(nearCollDistance - _personalArea, 0, _radius))/_radius);
    }
}
