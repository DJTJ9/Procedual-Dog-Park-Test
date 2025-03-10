using System.Collections;
using UnityEngine;

public class MossGrowth : MonoBehaviour
{
    private Material materialInstance; // Individuelle Instanz des Materials
    public float growthDuration = 10f; // Zeit in Sekunden, um von 0 auf 1 zu wachsen

    private void Start()
    {
        // Hole das Material des GameObjects
        Renderer renderer = GetComponent<Renderer>();

        if (renderer != null)
        {
            // Sorgt dafür, dass jede Instanz ein eigenes Material hat
            materialInstance = renderer.material;

            if (materialInstance.HasProperty("_Amount"))
            {
                // Initialisiere den Wert des Moss-Amount auf 0
                materialInstance.SetFloat("_Amount", 0f);

                // Starte das Wachsen des Mosses
                StartCoroutine(AnimateGrowth());
            }
            else
            {
                Debug.LogWarning($"Das Material von {gameObject.name} hat keine '_Amount'-Property.");
            }
        }
        else
        {
            Debug.LogWarning($"Kein Renderer an {gameObject.name} gefunden.");
        }
    }

    private IEnumerator AnimateGrowth()
    {
        float elapsedTime = 0f; // Zeit, die seit Beginn des Wachstums vergangen ist

        while (elapsedTime < growthDuration)
        {
            // Berechne den neuen Wert für `_Amount`
            float newAmount = Mathf.Lerp(0f, 1f, elapsedTime / growthDuration);

            // Setze den neuen Wert im Shader
            materialInstance.SetFloat("_Amount", newAmount);

            // Erhöhe die verstrichene Zeit
            elapsedTime += Time.deltaTime;

            // Warte bis zum nächsten Frame
            yield return null;
        }

        // Stelle sicher, dass der Wert genau 1 wird, da Lerp nicht exakt ist
        materialInstance.SetFloat("_Amount", 1f);

        Debug.Log("Moss Growth abgeschlossen!");
    }
}