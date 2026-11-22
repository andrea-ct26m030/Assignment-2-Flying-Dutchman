using UnityEngine;

/// <summary>
/// Mini-map style compass. The ship sits in the centre of the disc and "up" is the
/// ship's flight direction. The arrow rotates towards the target, and the blip shows
/// where the target is: inside the disc when it is within radar range, clamped to the
/// rim when it is farther away.
/// </summary>
public class Compass : MonoBehaviour
{
    [Tooltip("The object the compass points to (the party platform).")]
    [SerializeField] private Transform target;

    [Tooltip("The spaceship whose heading is used as 'up' on the compass.")]
    [SerializeField] private Transform ship;

    [Tooltip("Arrow in the centre of the disc. Its 'up' must be the tip of the arrow.")]
    [SerializeField] private RectTransform arrow;

    [Tooltip("Dot that marks the target on the disc.")]
    [SerializeField] private RectTransform blip;

    [Tooltip("World distance that corresponds to the rim of the disc.")]
    [SerializeField, Min(1f)] private float radarRange = 150f;

    [Tooltip("Distance of the rim from the centre in UI units.")]
    [SerializeField, Min(0f)] private float discRadius = 50f;

    private void LateUpdate()
    {
        if (target == null || ship == null)
        {
            return;
        }

        // Position of the target in the ship's local space: x = right, z = ahead.
        Vector3 local = ship.InverseTransformPoint(target.position);
        Vector2 planar = new Vector2(local.x, local.z);

        if (arrow != null)
        {
            // 0 degrees = ahead, positive = to the right. UI rotation around Z is
            // counter-clockwise, so the sign is flipped.
            float angle = Mathf.Atan2(planar.x, planar.y) * Mathf.Rad2Deg;
            arrow.localRotation = Quaternion.Euler(0f, 0f, -angle);
        }

        if (blip != null)
        {
            // Scale the world distance to the disc and keep the blip on the rim when out of range.
            float distance = Mathf.Min(planar.magnitude / radarRange, 1f);
            blip.anchoredPosition = planar.normalized * (distance * discRadius);
        }
    }
}
