using UnityEngine;

/// <summary>
/// Switches all party guests below this object between dancing and waving,
/// depending on how close the player's spaceship is to the platform.
/// The guests' Animator Controller needs a bool parameter named "IsWaving"
/// with transitions Dancing -> Waving and Waving -> Dancing.
/// </summary>
public class PartyGuestReaction : MonoBehaviour
{
    private const string WaveParameter = "IsWaving";

    [Tooltip("The player's spaceship. If left empty, the object with the SpaceshipController is used.")]
    [SerializeField] private Transform spaceship;

    [Tooltip("Guests start waving when the spaceship is closer than this distance.")]
    [SerializeField] private float waveDistance = 40f;

    [Tooltip("Guests go back to dancing when the spaceship is farther away than this distance. " +
             "Should be larger than Wave Distance so the guests do not flicker at the border.")]
    [SerializeField] private float danceDistance = 50f;

    private Animator[] guests;
    private bool isWaving;

    private void Awake()
    {
        if (spaceship == null)
        {
            SpaceshipController player = FindFirstObjectByType<SpaceshipController>();
            if (player != null)
            {
                spaceship = player.transform;
            }
        }

        guests = GetComponentsInChildren<Animator>();
    }

    private void Update()
    {
        if (spaceship == null)
        {
            return;
        }

        float distance = Vector3.Distance(spaceship.position, transform.position);

        if (!isWaving && distance < waveDistance)
        {
            SetWaving(true);
        }
        else if (isWaving && distance > danceDistance)
        {
            SetWaving(false);
        }
    }

    private void SetWaving(bool wave)
    {
        isWaving = wave;
        Debug.Log(wave ? "Spaceship is close: guests start waving" : "Spaceship left: guests go back to dancing");

        foreach (Animator guest in guests)
        {
            if (HasParameter(guest, WaveParameter))
            {
                guest.SetBool(WaveParameter, wave);
            }
        }
    }

    // The platform itself also has an Animator (rotation), which has no IsWaving parameter.
    private static bool HasParameter(Animator animator, string parameterName)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == parameterName)
            {
                return true;
            }
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, waveDistance);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, danceDistance);
    }
}
