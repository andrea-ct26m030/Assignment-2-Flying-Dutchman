using TMPro;
using UnityEngine;

public class DistanceDisplay : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private TMP_Text distanceText;

    private bool touchingTarget;

    private void Update()
    {
        float distance = touchingTarget ? 0f : Vector3.Distance(transform.position, target.position);
        distanceText.text = $"{distance:F1}m";
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.root == target.root)
            touchingTarget = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform.root == target.root)
            touchingTarget = false;
    }
}
