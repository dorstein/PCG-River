﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour {

    public Renderer textureRender;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public void drawTexture(Texture2D texture)
    {
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void genMesh(MeshData meshData, Texture2D texture)
    {
        meshFilter.sharedMesh = meshData.createMesh();
        meshRenderer.sharedMaterial.mainTexture = texture;
    }
}
