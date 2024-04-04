using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static  class MeshGenerator  {

    public static MeshData generateTerrainMesh(float[,] heightmap, float heightMultiplier, AnimationCurve _heightCurve, int levelOfDetail)
    {
		AnimationCurve heightCurve = new AnimationCurve (_heightCurve.keys);
        int width = heightmap.GetLength(0);
        int height = heightmap.GetLength(1);
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        int meshSimplifictaionIncrement = (levelOfDetail == 0)?1: levelOfDetail * 2;
        int verticiesPerLine = (width - 1) / meshSimplifictaionIncrement + 1;

        MeshData meshData = new MeshData(width, height);
        int vertexIndex = 0;

        for(int y = 0; y < height; y += meshSimplifictaionIncrement)
        {
            for(int x = 0; x < width; x+= meshSimplifictaionIncrement)
            {
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightCurve.Evaluate(heightmap[x, y]) * heightMultiplier, topLeftZ - y);
                meshData.uv[vertexIndex] = new Vector2(x / (float)width, y / (float)height);

                if(x < width -1 && y < height - 1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticiesPerLine + 1, vertexIndex + verticiesPerLine);
                    meshData.AddTriangle(vertexIndex + verticiesPerLine + 1, vertexIndex, vertexIndex + 1);
                }
                vertexIndex++;
            }
        }

        return meshData;

    }

}


public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uv;

    int triangleIndex;

    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight];
        uv = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    public Mesh createMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        return mesh;

    }
}
