using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCtrl : MonoBehaviour
{
    [Header("Blade Attribute")]
    public float damage;
    public float attackDist;
    public float attackDegree;

    [Header("Blade trail")]
    public GameObject top;
    public GameObject bottom;
    public GameObject trailMesh;
    public int maxTrailFrame;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private int frameCount;
    private Vector3 previousTopPos;
    private Vector3 previousBottomPos;

    private const int NUM_VERTICES = 12;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        trailMesh.GetComponent<MeshFilter>().mesh = mesh;

        vertices = new Vector3[maxTrailFrame * NUM_VERTICES];
        triangles = new int[vertices.Length];

        previousTopPos = top.transform.position;
        previousBottomPos = bottom.transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(frameCount == (maxTrailFrame * NUM_VERTICES))
        {
            frameCount = 0;
        }
        vertices[frameCount] = base.transform.position;
        vertices[frameCount + 1] = top.transform.position;
        vertices[frameCount + 2] = previousTopPos;

        vertices[frameCount + 3] = bottom.transform.position;
        vertices[frameCount + 4] = previousTopPos;
        vertices[frameCount + 5] = top.transform.position;

        vertices[frameCount + 6] = previousTopPos;
        vertices[frameCount + 7] = bottom.transform.position;
        vertices[frameCount + 8] = previousBottomPos;

        vertices[frameCount + 9] = previousTopPos;
        vertices[frameCount + 10] = previousBottomPos;
        vertices[frameCount + 11] = bottom.transform.position;

        triangles[frameCount] = frameCount;
        triangles[frameCount + 1] = frameCount+ 1;
        triangles[frameCount + 2] = frameCount+ 2;
        triangles[frameCount + 3] = frameCount+ 3;
        triangles[frameCount + 4] = frameCount+ 4;
        triangles[frameCount + 5] = frameCount+ 5;
        triangles[frameCount + 6] = frameCount+ 6;
        triangles[frameCount + 7] = frameCount+ 7;
        triangles[frameCount + 8] = frameCount+ 8;
        triangles[frameCount + 9] = frameCount+ 9;
        triangles[frameCount + 10] = frameCount + 10;
        triangles[frameCount + 11] = frameCount + 11;

        mesh.SetVertices(vertices);
        mesh.triangles = triangles;
        mesh.Optimize();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        previousTopPos = top.transform.position;
        previousBottomPos = bottom.transform.position;
        frameCount += NUM_VERTICES;

        trailMesh.transform.position = Vector3.zero;
        trailMesh.transform.eulerAngles = Vector3.zero;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackDist);
    }
}
