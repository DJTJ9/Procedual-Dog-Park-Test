using System.Collections;
using UnityEngine;

public class BurnObject : MonoBehaviour
{
    // private Material materialInstance;

    private void Start()
    {
        // Startet das automatische Deaktivieren nach 10 Sekunden
        StartCoroutine(DeactivateAfterDelay(10f));
        
        /*
         *  Leider hat das verfärben des Grases nicht funktioniert :(
         */
        
        // // Zugriff auf das Material des Objekts
        // MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        //
        // if (meshRenderer != null)
        // {
        //     // Erstelle eine Instanz des Materials, um spezifische Werte zu ändern
        //     materialInstance = meshRenderer.material;
        //
        //     // // Starte die Coroutine zur Farbänderung
        //     // StartCoroutine(ChangeColorAfterDelay(5f));
        // }
        // else
        // {
        //     Debug.LogWarning($"Kein Renderer auf {gameObject.name} gefunden, Shader-Änderung übersprungen.");
        // }
    }

    // private IEnumerator ChangeColorAfterDelay(float delay)
    // {
    //     yield return new WaitForSeconds(delay);
    //
    //     if (materialInstance != null)
    //     {
    //         // Ändert die Farbwerte in der Material-Instanz
    //         materialInstance.SetColor("_GrassBottomColor", Color.black); 
    //         materialInstance.SetColor("_GrassTopColor", Color.black); 
    //
    //         Debug.Log($"Shader-Farbe auf {gameObject.name} wurde nach {delay} Sekunden geändert.");
    //     }
    //     else
    //     {
    //         Debug.LogWarning($"Material auf {gameObject.name} fehlt, keine Farbänderung vorgenommen.");
    //     }
    // }

    private IEnumerator DeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log($"{gameObject.name} wird deaktiviert!");
        gameObject.SetActive(false);
    }
}