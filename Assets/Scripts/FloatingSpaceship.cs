using UnityEngine;

public class FloatingSpaceship : MonoBehaviour
{
    private Vector3 movementDirection;
    private float speed;

    public void Initialize(Vector3 direction, float movementSpeed)
    {
        movementDirection = direction.normalized;
        speed = movementSpeed;
    }

    private void Update()
    {
        transform.position += movementDirection * speed * Time.deltaTime;
    }
}
