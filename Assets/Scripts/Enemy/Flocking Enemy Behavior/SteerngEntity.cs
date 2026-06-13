using UnityEngine;

public class SteeringEntity : MonoBehaviour
{
    [SerializeField] protected float _maxSpeed;
    [SerializeField] protected float _maxForce;
    [SerializeField] protected Vector3 _velocity;
    [SerializeField] protected float _rotationSpeed = 5f;

    public Vector3 Velocity { get { return _velocity; } }

    protected Vector3 Seek(Vector3 target)
    {
        Vector3 desired = (target - transform.position);
        desired.y = 0;                          // ignora diferencia de altura al buscar target
        desired.Normalize();
        return CalculateSteering(desired);
    }

    protected Vector3 CalculateSteering(Vector3 desired)
    {
        Vector3 steering = desired - _velocity;
        steering.y = 0;
        return Vector3.ClampMagnitude(steering, _maxForce); // ← sin Time.deltaTime
    }

    protected void AddForce(Vector3 force)
    {
        force.y = 0;
        _velocity.y = 0;
        _velocity = Vector3.ClampMagnitude(_velocity + force * Time.deltaTime, _maxSpeed); // ← deltaTime acá
    }

    protected void Move()
    {
        if (_velocity == Vector3.zero) return;

        Vector3 flatVelocity = _velocity;
        flatVelocity.y = 0;

        if (flatVelocity != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flatVelocity);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                _rotationSpeed * Time.deltaTime
            );
        }

        transform.position += flatVelocity * Time.deltaTime;
    }
}
