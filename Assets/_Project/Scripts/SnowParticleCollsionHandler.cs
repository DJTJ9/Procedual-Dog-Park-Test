using System;
using UnityEngine;
using UnityEngine.Serialization;

public class SnowParticleCollsionHandler : MonoBehaviour
{
    public float SnowAmountPerParticle = 0.001f;
    public MeshRenderer MeshRenderer;
    private Material material;
    private float collisionCount = 0;

    void Start() {
        material = MeshRenderer.material;
    }
    
    private void OnDisable() {
        collisionCount = 0;
    }

    void OnParticleCollision(GameObject other)
    {
        // Wert erhöhen
        collisionCount = Mathf.Min(collisionCount + SnowAmountPerParticle, 1);

        // Den Wert an den Shader senden
        material.SetFloat("_Amount", collisionCount);
        
        // Alternativ global für alle Shader:
        // Shader.SetGlobalFloat("_CollisionCount", collisionCount);
    }
}
