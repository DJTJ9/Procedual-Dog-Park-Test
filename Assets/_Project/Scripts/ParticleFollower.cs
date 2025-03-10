using UnityEngine;

public class ParticleFollower : MonoBehaviour
{
    public Transform particleSystemTransform;

    void Update()
    {
        if (particleSystemTransform != null)
        {
            Vector3 offsetPosition = transform.position;
            offsetPosition.y += 3; // Verschiebt die y-Achse um +3
            particleSystemTransform.position = offsetPosition;
        }
    }
}
