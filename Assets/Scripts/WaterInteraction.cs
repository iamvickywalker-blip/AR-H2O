using UnityEngine;

public class WaterInteraction : MonoBehaviour
{
    public GameObject waterDropPrefab;

    public void OnTapped()
    {
        Instantiate(
            waterDropPrefab,
            transform.position,
            transform.rotation,
            transform.parent
        );

        Destroy(gameObject);
    }
}