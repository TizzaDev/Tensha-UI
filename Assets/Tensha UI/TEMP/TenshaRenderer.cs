using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TenshaRenderer : MonoBehaviour
{
    [SerializeField]
    private Material testMaterial;
    [SerializeField]
    private float outlineWidth;
    [SerializeField]
    private float outlineDirection = 0f;

    private RenderParams paramsCache;
    private MaterialPropertyBlock propsCache;
    private Mesh meshCache;

    private int FACES_AMOUNT = 3 * 3;
    private Vector2 GetMainCameraWorldSize()
    {
        float screenHeightInUnits = Camera.main.orthographicSize * 2;
        float screenWidthInUnits = screenHeightInUnits * Screen.width / Screen.height; // basically height * screen aspect ratio
        return new Vector2(screenWidthInUnits, screenHeightInUnits);
    }

    private float GetScreenToWorldRatio(Vector2 cameraSize)
    {
        return ((cameraSize.x / Screen.width) + (cameraSize.y / Screen.height)) / 2;
    }

    private Vector2 ConvertScreenPositionToWorldPosition(Vector2 screenPosition)
    {
        Vector2 camSize = GetMainCameraWorldSize();
        float conversionRatio = GetScreenToWorldRatio(camSize);
        return new Vector2(
            -camSize.x / 2 + screenPosition.x * conversionRatio,
            -camSize.y / 2 + screenPosition.y * conversionRatio
        );
    }

    private Vector2 ConvertScreenSizeToWorldSize(Vector2 screenSize)
    {
        Vector2 camSize = GetMainCameraWorldSize();
        float conversionRatio = GetScreenToWorldRatio(camSize);
        return new Vector2(
            screenSize.x * conversionRatio,
            screenSize.y * conversionRatio
        );
    }

    private Vector2 UpdateMesh(int FACES_AMOUNT, float outlineDirection)
    {
        List<Vector3> vertices = new();

        int SUB_FACES_AMOUNT = (int)Mathf.Sqrt(FACES_AMOUNT);

        float stepX = (float)Screen.width / SUB_FACES_AMOUNT;
        float stepY = (float)Screen.height / SUB_FACES_AMOUNT;

        for (int i = 0; i < 1; i++)
        {
            for (int j = 0; j < 1; j++)
            {
                Vector2 pos = ConvertScreenPositionToWorldPosition(new Vector2((stepX+2f) * i, (stepY + 2f) * j));
                Vector2 size = ConvertScreenSizeToWorldSize(new Vector2(stepX, stepY));

                float outlineScaled = ConvertScreenSizeToWorldSize(new Vector2(outlineWidth, outlineWidth)).x;
                //Debug.Log(Mathf.Clamp01((1f + outlineDirection) / 2f));
                outlineScaled *= Mathf.Clamp01((1f+outlineDirection) / 2f);

                vertices.Add(pos - new Vector2(outlineScaled, outlineScaled));
                vertices.Add(new Vector3(pos.x + size.x + outlineScaled, pos.y - outlineScaled, 0f));
                vertices.Add(new Vector3(pos.x - outlineScaled, pos.y + size.y + outlineScaled, 0f));
                vertices.Add(pos + size + new Vector2(outlineScaled, outlineScaled));
            }
        }

        meshCache.SetVertices(vertices);

        return new Vector2(stepX, stepY);
    }

    private void Awake()
    {
        paramsCache = new RenderParams();
        propsCache = new MaterialPropertyBlock();
        paramsCache.material = testMaterial;
        paramsCache.matProps = propsCache;
        paramsCache.renderingLayerMask = GraphicsSettings.defaultRenderingLayerMask;
        meshCache = new Mesh();


        Vector2 size = UpdateMesh(FACES_AMOUNT, outlineDirection);



        List<int> triangles = new();

        for (int i = 0; i < 1; i++)
        {
            triangles.Add(0 + (i * 4));
            triangles.Add(2 + (i * 4));
            triangles.Add(1 + (i * 4));
            triangles.Add(2 + (i * 4));
            triangles.Add(3 + (i * 4));
            triangles.Add(1 + (i * 4));
        }

        meshCache.SetTriangles(triangles, 0);

        List<Vector2> IDData = new();
        List<Vector2> UVData = new();

        for (int i = 0; i < 1; i++)
        {
            IDData.Add(new Vector2(0f, 0f));
            IDData.Add(new Vector2(0f, 0f));
            IDData.Add(new Vector2(0f, 0f));
            IDData.Add(new Vector2(0f, 0f));

            UVData.Add(new Vector2(0f, 0f));
            UVData.Add(new Vector2(1f, 0f));
            UVData.Add(new Vector2(0f, 1f));
            UVData.Add(new Vector2(1f, 1f));
        }

        meshCache.SetUVs(1, IDData);
        meshCache.SetUVs(2, UVData);

        propsCache.SetVectorArray("_FillColor", new Vector4[] {
            new Vector4(1f, 0f, 0f, 1f)
        });

        propsCache.SetVectorArray("_OutlineColor", new Vector4[] {
            new Vector4(0f, 0f, 0f, 1f)
        });

        propsCache.SetFloatArray("_OutlineWidth", new float[] {
            outlineWidth
        });

        propsCache.SetFloatArray("_OutlineDirection", new float[] {
            outlineDirection
        });

        propsCache.SetVectorArray("_SDFSize", new Vector4[] {
            size
        });

        propsCache.SetVectorArray("_SDFRadii", new Vector4[] {
            new Vector4(35f, 35f, 35f, 35f)
        });
    }



    private void Update()
    {
        Matrix4x4 offset = Matrix4x4.Translate(
            new Vector3(0, 0, - Camera.main.nearClipPlane - 0.1f)
        );

        var size = UpdateMesh(FACES_AMOUNT, outlineDirection);
        propsCache.SetFloatArray("_OutlineWidth", new float[] {
            outlineWidth
        });
        propsCache.SetVectorArray("_SDFSize", new Vector4[] {
            size
        });
        propsCache.SetFloatArray("_OutlineDirection", new float[] {
            outlineDirection
        });

        Graphics.RenderMesh(
            paramsCache,
            meshCache,
            0,
            Camera.main.cameraToWorldMatrix * offset
        );
    }
}
