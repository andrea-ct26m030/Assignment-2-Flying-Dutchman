using UnityEngine;

/// <summary>
/// Disco light: cycles the colour through the hue wheel, pulses the intensity
/// (and optionally the range) to the beat, sweeps the light's rotation and lets
/// it orbit around its start position.
/// </summary>
[RequireComponent(typeof(Light))]
public class PartyLight : MonoBehaviour
{
    [Header("Color")]
    [Tooltip("Full hue cycles per second.")]
    [SerializeField] private float hueSpeed = 0.15f;
    [SerializeField, Range(0f, 1f)] private float hueOffset;
    [SerializeField, Range(0f, 1f)] private float saturation = 1f;

    [Header("Beat pulse")]
    [SerializeField] private float beatsPerMinute = 120f;
    [Tooltip("0 = constant intensity, 1 = light goes fully dark between beats.")]
    [SerializeField, Range(0f, 1f)] private float pulseDepth = 0.6f;
    [Tooltip("Shifts the pulse by a fraction of a beat.")]
    [SerializeField, Range(0f, 1f)] private float beatOffset;
    [Tooltip("Range is scaled by up to this factor on the beat (1 = no change).")]
    [SerializeField, Min(1f)] private float rangePulse = 1f;

    [Header("Movement")]
    [Tooltip("Maximum sweep angle in degrees around the local X and Z axis.")]
    [SerializeField] private Vector2 sweepAngle = new Vector2(20f, 20f);
    [SerializeField] private float sweepSpeed = 0.8f;
    [Tooltip("Radius of the circle the light moves on around its start position.")]
    [SerializeField, Min(0f)] private float orbitRadius;
    [SerializeField] private float orbitSpeed = 1f;

    private Light partyLight;
    private float baseIntensity;
    private float baseRange;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Awake()
    {
        partyLight = GetComponent<Light>();
        baseIntensity = partyLight.intensity;
        baseRange = partyLight.range;
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;
    }

    private void Update()
    {
        float time = Time.time;

        // Color: walk around the hue wheel.
        float hue = Mathf.Repeat(hueOffset + time * hueSpeed, 1f);
        partyLight.color = Color.HSVToRGB(hue, saturation, 1f);

        // Intensity / range: sharp attack on the beat, then decay until the next one.
        float beat = Mathf.Repeat(time * beatsPerMinute / 60f + beatOffset, 1f);
        float pulse = Mathf.Exp(-4f * beat);
        partyLight.intensity = baseIntensity * Mathf.Lerp(1f - pulseDepth, 1f, pulse);
        partyLight.range = baseRange * Mathf.Lerp(1f, rangePulse, pulse);

        // Rotation: sweep across the dance floor.
        float phase = hueOffset * Mathf.PI * 2f;
        float sweepX = Mathf.Sin(time * sweepSpeed + phase) * sweepAngle.x;
        float sweepZ = Mathf.Cos(time * sweepSpeed * 0.7f + phase) * sweepAngle.y;
        transform.localRotation = startRotation * Quaternion.Euler(sweepX, 0f, sweepZ);

        // Position: orbit around the start position.
        if (orbitRadius > 0f)
        {
            float angle = time * orbitSpeed + phase;
            transform.localPosition = startPosition + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * orbitRadius;
        }
    }
}
