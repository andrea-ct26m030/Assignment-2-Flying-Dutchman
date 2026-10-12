using UnityEngine;

/// <summary>
/// Listens to the Bullet.Hit event and prints a message to the Unity Console
/// whenever a projectile hits something. Place once in the scene.
/// </summary>
public class MessageLogger : MonoBehaviour
{
    private void OnEnable()
    {
        Bullet.Hit += OnBulletHit;
    }

    private void OnDisable()
    {
        Bullet.Hit -= OnBulletHit;
    }

    private void OnBulletHit(Bullet bullet, Collision collision)
    {
        Vector3 point = collision.contactCount > 0
            ? collision.GetContact(0).point
            : bullet.transform.position;

        Debug.Log(
            $"[MessageLogger] Projectile hit {collision.gameObject.name} at {point}.",
            collision.gameObject
        );
    }
}
