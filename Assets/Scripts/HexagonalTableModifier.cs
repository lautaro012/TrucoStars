// 23/8/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;

public class HexagonalTableModifier : MonoBehaviour
{
    void Start()
    {
        // Modificar el LOD0
        Transform lod0Transform = transform.Find("CardTable_LOD0");
        if (lod0Transform != null)
        {
            ModifyTabletop(lod0Transform);
        }
        else
        {
            Debug.LogError("No se encontró el objeto hijo CardTable_LOD0.");
        }

        // Modificar el LOD1
        Transform lod1Transform = transform.Find("CardTable_LOD1");
        if (lod1Transform != null)
        {
            ModifyTabletop(lod1Transform);
        }
        else
        {
            Debug.LogError("No se encontró el objeto hijo CardTable_LOD1.");
        }
    }

    void ModifyTabletop(Transform lodTransform)
    {
        // Obtener el componente MeshFilter
        MeshFilter meshFilter = lodTransform.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError($"No se encontró un MeshFilter en {lodTransform.name}.");
            return;
        }

        // Obtener el mesh
        Mesh mesh = meshFilter.mesh;

        // Verificar si el mesh es legible
        if (!mesh.isReadable)
        {
            Debug.LogError($"El mesh de {lodTransform.name} no es legible. Habilita 'Read/Write' en las opciones de importación del modelo.");
            return;
        }

        // Obtener los vértices
        Vector3[] vertices = mesh.vertices;

        // Modificar los vértices para formar un hexágono
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];

            // Ajustar solo los vértices del tablero (asumimos y > 0.6 es el tablero)
            if (vertex.y > 0.6f)
            {
                // Proyectar el vértice en una forma hexagonal
                float angle = Mathf.Atan2(vertex.z, vertex.x);
                float distance = new Vector2(vertex.x, vertex.z).magnitude;

                // Ajustar el ángulo al más cercano de un hexágono (60 grados)
                float hexAngle = Mathf.Round(angle / (Mathf.PI / 3)) * (Mathf.PI / 3);

                // Actualizar la posición del vértice
                vertex.x = Mathf.Cos(hexAngle) * distance;
                vertex.z = Mathf.Sin(hexAngle) * distance;
            }

            vertices[i] = vertex;
        }

        // Actualizar el mesh
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        Debug.Log($"El tablero de {lodTransform.name} se ha modificado a una forma hexagonal.");
    }
}