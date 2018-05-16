using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour {

    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public GameObject prefabParent;
    public MeshRenderer render;
    public GameObject tileParent;

    public void DrawTexture(Texture2D texture)
    {

        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawMesh(MeshData meshData, TextureData data)
    {
        render.material = data.material;
        meshFilter.sharedMesh = meshData.CreateMesh();
        meshFilter.transform.localScale = Vector3.one * FindObjectOfType<MapGenScript>().terrainData.uniformScale;
    }

    public void DrawMeshUnderParentWithPrefabs(MeshData meshData, PreFabData prefabData, TextureData data, Vector2 position)
    {
        GameObject temp = new GameObject();
        temp.transform.position = new Vector3(position.x,0,position.y);
        temp.AddComponent<MeshFilter>();
        temp.AddComponent<MeshRenderer>();
        temp.transform.parent = tileParent.transform;
        MeshRenderer renderer = temp.GetComponent<MeshRenderer>();
        MeshFilter mesh = temp.GetComponent<MeshFilter>();
        
        renderer.material = data.material;
        mesh.sharedMesh = meshData.CreateMesh();
        mesh.transform.localScale = Vector3.one * FindObjectOfType<MapGenScript>().terrainData.uniformScale;
        

        for (int i = 0; i < prefabData.prefabs.Length; i++)
        {
            GameObjectData newObject = prefabData.prefabs[i];
            GameObject myNewPrefab = Instantiate(newObject.gamePiece, newObject.position + temp.transform.position, newObject.rotation);

            myNewPrefab.transform.localScale = new Vector3(myNewPrefab.transform.localScale.x * newObject.scale.x, myNewPrefab.transform.localScale.y * newObject.scale.y, myNewPrefab.transform.localScale.z * newObject.scale.z);
            myNewPrefab.transform.parent = temp.transform;
        }
    }

    public void DestroyAllMeshData()
    {
        var tempArray = new GameObject[tileParent.transform.childCount];

        for (int i = 0; i < tempArray.Length; i++)
        {
            tempArray[i] = tileParent.transform.GetChild(i).gameObject;
        }

        foreach (var child in tempArray)
        {
            DestroyImmediate(child);
        }

    }

    public void DrawPrefabs(PreFabData objects)
    {
        var tempArray = new GameObject[prefabParent.transform.childCount];

        for (int i = 0; i < tempArray.Length; i++)
        {
            tempArray[i] = prefabParent.transform.GetChild(i).gameObject;
        }

        foreach (var child in tempArray)
        {
            DestroyImmediate(child);
        }


        for (int i = 0; i < objects.prefabs.Length; i++)
        {
            GameObjectData newObject = objects.prefabs[i];
            GameObject myNewPrefab = Instantiate(newObject.gamePiece,newObject.position, newObject.rotation);
            
            myNewPrefab.transform.localScale = new Vector3(myNewPrefab.transform.localScale.x * newObject.scale.x, myNewPrefab.transform.localScale.y * newObject.scale.y, myNewPrefab.transform.localScale.z * newObject.scale.z);
            myNewPrefab.transform.parent = prefabParent.transform;
        }
    }

    public void MeshDump(Mesh m)
    {
        print("Dumping Mesh");
        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        System.IO.FileStream fs = new System.IO.FileStream(Application.dataPath + "meshFile.dat", System.IO.FileMode.Create);
        print("Serializing Mesh");
        SerializableMeshInfo smi = new SerializableMeshInfo(m);
        print("Done Serializing");
        print("Generating binaries");
        bf.Serialize(fs, smi);
        print("Binary format saved");
        fs.Close();
        print("mesh data saved to " + Application.dataPath + "meshFile.dat");

        
    }
}


[System.Serializable]
class SerializableMeshInfo
{
    [SerializeField]
    public float[] vertices;
    [SerializeField]
    public int[] triangles;
    [SerializeField]
    public float[] uv;
    [SerializeField]
    public float[] uv2;
    [SerializeField]
    public float[] normals;
    [SerializeField]
    public Color[] colors;

    public SerializableMeshInfo(Mesh m)
    {
        vertices = new float[m.vertexCount * 3];
        
        for (int i = 0; i < m.vertexCount; i++)
        {
            vertices[i * 3] = m.vertices[i].x;
            vertices[i * 3 + 1] = m.vertices[i].y;
            vertices[i * 3 + 2] = m.vertices[i].z;

        }
        

        triangles = new int[m.triangles.Length];
        
        for (int i =0; i < m.triangles.Length; i++)
        {
            triangles[i] = m.triangles[i];
    
        }
        

        uv = new float[m.uv.Length * 2];
        for(int i = 0; i < m.uv.Length; i++)
        {
            uv[i * 2] = m.uv[i].x;
            uv[i * 2 + 1] = m.uv[i].y;
        }

        uv2 = new float[m.uv2.Length];
        for(int i = 0; i < m.uv2.Length; i++)
        {
            uv[i * 2] = m.uv2[i].x;
            uv[i * 2 + 1] = m.uv2[i].y;

        }

        normals = new float[m.normals.Length * 3];
        for (int i = 0; i < m.vertexCount; i++)
        {
            normals[i * 3] = m.normals[i].x;
            normals[i * 3 + 1] = m.normals[i].y;
            normals[i * 3 + 2] = m.normals[i].z;
        }

        colors = new Color[m.colors.Length];
        for(int i = 0; i < m.colors.Length; i++)
        {
            colors[i] = m.colors[i];
        }
    }
    public Mesh GetMesh()
    {
        Mesh m = new Mesh();
        List<Vector3> verticesList = new List<Vector3>();
        for (int i = 0; i < vertices.Length / 3; i++)
        {
            verticesList.Add(new Vector3(
                    vertices[i * 3], vertices[i * 3 + 1], vertices[i * 3 + 2]
                ));
        }
        m.SetVertices(verticesList);
        m.triangles = triangles;
        List<Vector2> uvList = new List<Vector2>();
        for (int i = 0; i < uv.Length / 2; i++)
        {
            uvList.Add(new Vector2(
                    uv[i * 2], uv[i * 2 + 1]
                ));
        }
        m.SetUVs(0, uvList);
        List<Vector2> uv2List = new List<Vector2>();
        for (int i = 0; i < uv2.Length / 2; i++)
        {
            uv2List.Add(new Vector2(
                    uv2[i * 2], uv2[i * 2 + 1]
                ));
        }
        m.SetUVs(1, uv2List);
        List<Vector3> normalsList = new List<Vector3>();
        for (int i = 0; i < normals.Length / 3; i++)
        {
            normalsList.Add(new Vector3(
                    normals[i * 3], normals[i * 3 + 1], normals[i * 3 + 2]
                ));
        }
        m.SetNormals(normalsList);
        m.colors = colors;

        return m;
    }
}

[System.Serializable]
public class MeshObject
{
    public MeshRenderer render;
    public MeshFilter mesh;
    public GameObject objRef;
}