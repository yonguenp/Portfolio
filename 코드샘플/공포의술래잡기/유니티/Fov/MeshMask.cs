/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using LOS;
using System;

public class MeshMask : MonoBehaviour
{
    [SerializeField]
    protected LOSLightBase losObject;
    [SerializeField]
    protected MeshFilter viewMeshFilter;
    [SerializeField]
    protected LayerMask _maskObjectLayer;

    Mesh viewMesh = null;    

    void Awake()
    {
        if (viewMeshFilter != null)
        {
            viewMesh = new Mesh();
            viewMesh.name = "View Mesh";
            viewMeshFilter.mesh = viewMesh;
        }
    }

    public void UpdateMesh()
    {
        if (losObject == null) return;
        if (losObject.Mesh == null) return;
        try
        {
            viewMesh.Clear();
            viewMesh.vertices = losObject.Mesh.vertices;
            viewMesh.triangles = losObject.Mesh.triangles;
            viewMesh.RecalculateNormals();
        }
        catch (Exception e)
        {
            SBDebug.Log($"{e.Message} : \n{e.StackTrace}");
        }
    }
}
*/
