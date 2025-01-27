using UnityEngine;
using TMPro;
public class WobblyText : MonoBehaviour
{
    [SerializeField] private TMP_Text textComponent;

    [Header("Wave Properties")]
    public float amplitude = 10f;
    public float waveLength = 0.01f;
    public float speed = 2f;


    private void Update()
    {
        textComponent.ForceMeshUpdate();
        var textInfo = textComponent.textInfo;

        for(int i = 0; i < textInfo.characterCount; ++i)
        {
            var charInfo = textInfo.characterInfo[i];

            if(!charInfo.isVisible){
                continue;
            }

            var meshInfo = textInfo.meshInfo[charInfo.materialReferenceIndex];

            for(int j = 0; j < 4; ++j)
            {
                var index = charInfo.vertexIndex + j;
                var orig = meshInfo.vertices[index];

                //edit text pos here
                meshInfo.vertices[index] = orig + new Vector3(0, Mathf.Sin(Time.time * speed + orig.x * waveLength) * amplitude, 0f);

                //meshInfo.colors32[index] = Color.red
            }
        }

        for(int i = 0; i < textInfo.meshInfo.Length; ++i)
        {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            //meshInfo.mesh.colors32 = meshInfo.colors32;
            textComponent.UpdateGeometry(meshInfo.mesh, i);
        }
    }
}
