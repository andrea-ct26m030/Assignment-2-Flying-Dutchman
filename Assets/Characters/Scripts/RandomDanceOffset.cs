using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RandomDanceOffset : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        float randomOffset = Random.Range(0f, 1f);

        animator.Play("Base Layer.Dancing", 0, randomOffset);
    }
}