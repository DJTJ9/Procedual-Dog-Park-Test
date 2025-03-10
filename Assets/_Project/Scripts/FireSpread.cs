using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class FireSpread : MonoBehaviour
{
    public float fireSpreadRadius; 
    public float fireSpreadChance;
    public GameObject firePrefab;
    public GameObject burnedGround;

    private float fireSpreadDelay;
    private bool fireSpreadStarted;
    

    private void Start() {
        // Randomisiert den delay-Wert zwischen 3 und 5 Sekunden
        fireSpreadDelay = Random.Range(3f, 5f);
        InvokeRepeating(nameof(SpreadFire), fireSpreadDelay, fireSpreadDelay);

        // Spawnt nach 5 Sekunden verbrannten Boden
        StartCoroutine(BurnGround(8f));
        
        // Automatisches Deaktivieren des Feuers nach 10 Sekunden
        StartCoroutine(DeactivateAfterDelay(10f));
    }

    void SpreadFire() {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, fireSpreadRadius);
        foreach (var hitCollider in hitColliders)

            // Stellt sicher, dass das Ziel brennbar ist und noch nicht brennt
            if (hitCollider.CompareTag("Flammable") && !hitCollider.GetComponent<BurnObject>()) {
                // Die Wahrscheinlichkeit, dass das Feuer spreadet.
                if (Random.Range(0f, 100f) <= fireSpreadChance) // 30% Chance
                {
                    // Spawnt ein neues Feuer
                    var newFire = Instantiate(firePrefab, hitCollider.transform.position, Quaternion.identity);

                    // Fügt das BurnObject-Skript hinzu, um das getroffene GameObject zu deaktivieren.
                    hitCollider.gameObject.AddComponent<BurnObject>();
                    Debug.Log($"BurnObject hinzugefügt zu: {hitCollider.gameObject.name}");
                }
            }
    }
    
    private IEnumerator BurnGround(float delay) {
        yield return new WaitForSeconds(delay);
        Instantiate(burnedGround, transform.position, Quaternion.identity);
    }

    private IEnumerator DeactivateAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}