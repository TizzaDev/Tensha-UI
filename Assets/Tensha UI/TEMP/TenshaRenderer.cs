using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TenshaRenderer : MonoBehaviour
{
    [SerializeField]
    private Material testMaterial;

    private RenderParams paramsCache;
    private MaterialPropertyBlock propsCache;
    private Mesh meshCache;

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

    private void Awake()
    {
        paramsCache = new RenderParams();
        propsCache = new MaterialPropertyBlock();
        paramsCache.material = testMaterial;
        paramsCache.matProps = propsCache;
        paramsCache.renderingLayerMask = GraphicsSettings.defaultRenderingLayerMask;
        meshCache = new Mesh();

        List<Vector3> vertices = new();

        int FACES_AMOUNT = 5*5;

        int SUB_FACES_AMOUNT = (int)Mathf.Sqrt(FACES_AMOUNT);

        for(int i = 0; i < SUB_FACES_AMOUNT; i++)
        {
            for (int j = 0; j < SUB_FACES_AMOUNT; j++)
            {
                float stepX = (float)Screen.width / SUB_FACES_AMOUNT;
                float stepY = (float)Screen.height / SUB_FACES_AMOUNT;

                Vector2 pos = ConvertScreenPositionToWorldPosition(new Vector2(stepX * i, stepY * j));
                Vector2 size = ConvertScreenSizeToWorldSize(new Vector2(stepX, stepY));
                Vector2 padding = ConvertScreenSizeToWorldSize(new Vector2(3f, 3f));

                vertices.Add(pos - padding);
                vertices.Add(new Vector3(pos.x + size.x + padding.x, pos.y - padding.y, 0f));
                vertices.Add(new Vector3(pos.x - padding.x, pos.y + size.y + padding.y, 0f));
                vertices.Add(pos + size + padding);
            }
        }

        meshCache.SetVertices(vertices);

        List<int> triangles = new();

        for (int i = 0; i < FACES_AMOUNT; i++)
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

        for (int i = 0; i < FACES_AMOUNT; i++)
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
            3f
        });

        propsCache.SetVectorArray("_SDFSize", new Vector4[] {
            new Vector2(((float)Screen.width / SUB_FACES_AMOUNT), ((float)Screen.height / SUB_FACES_AMOUNT))
        });

        propsCache.SetVectorArray("_SDFRadii", new Vector4[] {
            new Vector4(40f, 40f, 40f, 40f)
        });

        propsCache.SetVectorArray("_SDFPadding", new Vector4[] {
            new Vector4(3f, 3f, 0f, 0f)
        });
    }



    private void Update()
    {
        Matrix4x4 offset = Matrix4x4.Translate(
            new Vector3(0, 0, -Camera.main.nearClipPlane)
        );

        Graphics.RenderMesh(
                    paramsCache,
                    meshCache,
                    0,
                    Camera.main.cameraToWorldMatrix * offset
                );
    }
}
