using UnityEngine;

public class BlendShapeSync : MonoBehaviour
{
    public SkinnedMeshRenderer headRenderer; // Assign in inspector
    public SkinnedMeshRenderer teethRenderer; // Assign in inspector
    public string blendShapePrefixHead = ""; // Prefix for head blend shapes, set in inspector
    public string blendShapePrefixTeeth = ""; // Prefix for teeth blend shapes, set in inspector

    private int blendShapeIndexHead;
    private int blendShapeIndexTeeth;

    void Start()
    {
        // Ensure you replace "jawOpen" with the exact name of your blendshape if different
        blendShapeIndexHead = headRenderer.sharedMesh.GetBlendShapeIndex($"{blendShapePrefixHead}jawOpen");
        blendShapeIndexTeeth = teethRenderer.sharedMesh.GetBlendShapeIndex($"{blendShapePrefixTeeth}jawOpen");

        if (blendShapeIndexHead == -1 || blendShapeIndexTeeth == -1)
        {
            Debug.LogError("BlendShape 'jawOpen' not found on one of the meshes.");
        }
    }

    void Update()
    {
        if (blendShapeIndexHead != -1 && blendShapeIndexTeeth != -1)
        {
            // Get the current blendshape weight from the HEAD
            float currentWeight = headRenderer.GetBlendShapeWeight(blendShapeIndexHead);

            // Apply the same weight to the TEETH
            teethRenderer.SetBlendShapeWeight(blendShapeIndexTeeth, currentWeight);
        }
    }
}
