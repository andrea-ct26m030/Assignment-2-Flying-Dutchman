using System.Collections.Generic;
using UnityEngine;

public class MagazineUI : MonoBehaviour
{
    [SerializeField] private Cannon cannon;
    [SerializeField] private GameObject bulletIcon;

    private readonly List<GameObject> bulletIcons = new();

    private void Awake()
    {
        for (int i = 0; i < cannon.MagazineSize; i++)
            bulletIcons.Add(Instantiate(bulletIcon, transform, false));

        bulletIcon.SetActive(false);
    }

    private void OnEnable()
    {
        cannon.AmmunitionChanged += UpdateDisplay;
        UpdateDisplay(cannon.Ammunition);
    }

    private void OnDisable()
    {
        cannon.AmmunitionChanged -= UpdateDisplay;
    }

    private void UpdateDisplay(int ammunition)
    {
        for (int i = 0; i < bulletIcons.Count; i++)
            bulletIcons[i].SetActive(i < ammunition);
    }
}
