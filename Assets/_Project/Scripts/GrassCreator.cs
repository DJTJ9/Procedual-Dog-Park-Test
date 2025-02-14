using NaughtyAttributes;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Serialization;

public class GrassCreator : MonoBehaviour
{
    public GameObject GrassPrefab;
    public int GrassSize = 20;
    public float GrassSubdivisions = 4;
    public float MinGrassHeight;
    public float MaxGrassHeight;
    public float GrassRandomDistribution;
    public Color TopGrassColor;
    public Color BottomGrassColor;
    public Transform GrassParent;

    public GameObject Ball;
    public Material GrassMaterial;

    private const int Zero = 0;
    private const int One = 1;

    private MeshRenderer grassRenderer;

    void Start() {
       InitializeGras();
    }

    // Update is called once per frame
    void Update() {
        GrassMaterial.SetVector("_TramplePosition", Ball.transform.position);
    }

    [Button] 
    private void InitializeGras() {
        grassRenderer = GetComponent<MeshRenderer>();
        grassRenderer.material.SetColor("_GrassTopColor", TopGrassColor);
        grassRenderer.material.SetColor("_GrassBottomColor", BottomGrassColor);
        // grassRenderer.sharedMaterial.SetColor("_GrassTopColor", Color.green);
        // grassRenderer.sharedMaterial.SetColor("_GrassBottomColor", Color.magenta);
        for (int z = -GrassSize; z <= GrassSize; z++) {
            for (int x = -GrassSize; x <= GrassSize; x++) {
                Vector3 position = new Vector3(x / GrassSubdivisions + Random.Range(-GrassRandomDistribution, GrassRandomDistribution), Zero,
                    z / GrassSubdivisions + Random.Range(-GrassRandomDistribution, GrassRandomDistribution));
                GameObject grass = Instantiate(GrassPrefab, transform.position + position, Quaternion.Euler(-90, 0, 0),
                    GrassParent);
                grass.transform.localScale =
                    new Vector3(One, 0.25f * Random.Range(MinGrassHeight, MaxGrassHeight), One);
            }
        }
    }
}