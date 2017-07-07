using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class TPLineRenderer : MonoBehaviour
{
    public class Quad
    {
        public Vector3 aPos;
        public Color aCol;

        public Vector3 bPos;
        public Color bCol;

        public Vector3 cPos;
        public Color cCol;

        public Vector3 dPos;
        public Color dCol;
    };

    private Mesh m_mesh;
    private GameObject m_meshObject;

    public ShadowCastingMode m_castShadows = ShadowCastingMode.On;
    public bool m_receiveShadows = true;
    public bool m_motionVectors = true;
    public List<Material> m_materials = new List<Material>();
    public LightProbeUsage m_lightProbes = LightProbeUsage.BlendProbes;
    public ReflectionProbeUsage m_reflectionProbes = ReflectionProbeUsage.Off;

    public List<Vector3> m_positions = new List<Vector3>();
    public List<Vector3> m_vertices = new List<Vector3>();
    public List<Vector2> m_uvs = new List<Vector2>();
    public List<Color> m_colours = new List<Color>();
    public List<int> m_triangles = new List<int>();

    private float m_totalLineLength = 1.0f;

    public bool m_useWorldSpace = true;
    public bool m_backfaceCulling = false;

    [Range(0, 10)]
    public int m_interpolationLevel = 0;

    public float m_minimumWidth = 1.0f;
    public float m_maximumWidth = 1.0f;
    public AnimationCurve m_widthOverLength;

    public Color m_startColour;
    private Color m_orignalStartColour;
    public Color m_endColour;
    private Color m_orignalEndColour;
    public AnimationCurve m_colourOverLength;

    void Awake()
    {
        m_mesh = new Mesh();

        m_meshObject = new GameObject(name + "Line");
        m_meshObject.transform.SetParent(transform);
        m_meshObject.hideFlags = HideFlags.HideInHierarchy;

        MeshFilter meshFilter = m_meshObject.AddComponent<MeshFilter>();
        meshFilter.mesh = m_mesh;

        MeshRenderer meshRenderer = m_meshObject.AddComponent<MeshRenderer>();
        meshRenderer.shadowCastingMode = m_castShadows;
        meshRenderer.receiveShadows = m_receiveShadows;
        meshRenderer.motionVectors = m_motionVectors;
        meshRenderer.materials = m_materials.ToArray();
        meshRenderer.lightProbeUsage = m_lightProbes;
        meshRenderer.reflectionProbeUsage = m_reflectionProbes;

        m_orignalStartColour = m_startColour;
        m_orignalEndColour = m_endColour;
    }

    void OnDestroy()
    {
        Destroy(m_meshObject);
    }

    void UpdateMesh()
    {
        m_mesh.Clear();
        m_mesh.vertices = m_vertices.ToArray();
        m_mesh.uv = m_uvs.ToArray();
        m_mesh.triangles = m_triangles.ToArray();
        m_mesh.colors = m_colours.ToArray();
    }

    void ResetLists()
    {
        m_positions.Clear();
        m_vertices.Clear();
        m_uvs.Clear();
        m_colours.Clear();
        m_triangles.Clear();
    }

    public void SetStartingColour(Color _startingColour)
    {
        m_startColour = _startingColour;

        UpdateColours();

        UpdateMesh();
    }

    public void SetEndingColour(Color _endingColour)
    {
        m_endColour = _endingColour;

        UpdateColours();

        UpdateMesh();
    }

    public void SetColours(Color _startingColour, Color _endingColour)
    {
        m_startColour = _startingColour;
        m_endColour = _endingColour;

        UpdateColours();

        UpdateMesh();
    }

    public void RevertColours()
    {
        m_startColour = m_orignalStartColour;
        m_endColour = m_orignalEndColour;

        UpdateColours();

        UpdateMesh();
    }

    void UpdateColours()
    {
        m_colours.Clear();

        int vertexCount = m_vertices.Count;

        if (!m_backfaceCulling)
        {
            vertexCount /= 2;
        }

        Vector3 pointA = m_positions[0];
        Vector3 pointB = m_positions[1];
        float length = 0.0f;

        for (int iter = 0; iter <= vertexCount - 1; iter++)
        {
            m_colours.Add(CalculateColourOverLength(length));

            if (iter != m_vertices.Count - 1)
            {
                pointA = m_vertices[iter];
                pointB = m_vertices[iter + 1];
            }

            length += Vector3.Distance(pointA, pointB);
        }

        if (!m_backfaceCulling)
        {

            for (int iter = vertexCount; iter <= m_vertices.Count - 1; iter++)
            {
                m_colours.Add(CalculateColourOverLength(length));

                if (iter != m_vertices.Count - 1)
                {
                    pointA = m_vertices[iter];
                    pointB = m_vertices[iter + 1];
                }

                length += Vector3.Distance(pointA, pointB);

            }
        }
    }

    public void SetPositions(Vector3[] _positions)
    {
        ResetLists();

        foreach (Vector3 position in _positions)
        {
            m_positions.Add(position);
        }

        if (m_interpolationLevel == 0)
        {
            NonIterpolationVertices();
        }

        UpdateMesh();
    }

    void NonIterpolationVertices()
    {
        CalculateTotalLineLength();

        Vector3 pointA = m_positions[0];
        Vector3 pointB = m_positions[1];
        float length = 0.0f;

        for (int iter = 0; iter <= m_positions.Count - 1; iter++)
        {
            GenerateVertexPair(iter, length);

            if (iter != m_positions.Count - 1)
            {
                pointA = m_positions[iter];
                pointB = m_positions[iter + 1];
            }

            length += Vector3.Distance(pointA, pointB);
        }

        GenerateClockWiseTriangles();

        if (!m_backfaceCulling)
        {
            GenerateBackFace();
        }

        if (m_useWorldSpace)
        {
            AdjustToWorldSpace();
        }
    }

    void GenerateVertexPair(int _positionIndex, float _length)
    {
        int currentIndex = _positionIndex;
        int nextIndex = _positionIndex + 1;
        if (nextIndex == m_positions.Count)
        {
            nextIndex = currentIndex;
            currentIndex--;
        }

        Vector3 normal = Vector3.Cross(m_positions[currentIndex], m_positions[nextIndex]);
        Vector3 side = Vector3.Cross(normal, m_positions[nextIndex] - m_positions[currentIndex]);
        side.Normalize();

        Vector3 width = (side * (CalculateWidthOverLength(_length) / 2));
        Color colour = CalculateColourOverLength(_length);

        m_vertices.Add(m_positions[_positionIndex] - width);
        m_uvs.Add(new Vector2(m_vertices[m_vertices.Count - 1].x, m_vertices[m_vertices.Count - 1].y));
        m_colours.Add(colour);

        m_vertices.Add(m_positions[_positionIndex] + width);
        m_uvs.Add(new Vector2(m_vertices[m_vertices.Count - 1].x, m_vertices[m_vertices.Count - 1].y));
        m_colours.Add(colour);
    }

    void GenerateClockWiseTriangles()
    {
        int vertexA = 0;
        int vertexB;
        int vertexC;
        int vertexD;

        for(int sectionIter = 1; sectionIter <= m_positions.Count - 1; sectionIter++)
        {
            vertexB = vertexA + 1;
            vertexC = vertexB + 1;
            vertexD = vertexC + 1;

            m_triangles.Add(vertexA);
            m_triangles.Add(vertexB);
            m_triangles.Add(vertexC);
            m_triangles.Add(vertexC);
            m_triangles.Add(vertexD);
            m_triangles.Add(vertexA);

            vertexA += 2;
        }
    }

    void GenerateBackFace()
    {
        int firstVertex = m_vertices.Count;

        for (int iter = 0; iter <= firstVertex - 1; iter++)
        {
            m_vertices.Add(new Vector3(m_vertices[iter].x , m_vertices[iter].y, m_vertices[iter].z));
            m_uvs.Add(new Vector2(m_vertices[m_vertices.Count - 1].x, m_vertices[m_vertices.Count - 1].y));
            m_colours.Add(new Color(m_colours[iter].r, m_colours[iter].g, m_colours[iter].b, m_colours[iter].a));
        }

        GenerateAntiClockWiseTriangles(firstVertex);
    }

    void GenerateAntiClockWiseTriangles(int _firstVertex)
    {
        int vertexA = _firstVertex;
        int vertexB;
        int vertexC;
        int vertexD;

        for (int sectionIter = 1; sectionIter <= m_positions.Count - 1; sectionIter++)
        {
            vertexB = vertexA + 1;
            vertexC = vertexB + 1;
            vertexD = vertexC + 1;

            m_triangles.Add(vertexB);
            m_triangles.Add(vertexA);
            m_triangles.Add(vertexD);
            m_triangles.Add(vertexD);
            m_triangles.Add(vertexC);
            m_triangles.Add(vertexB);

            vertexA += 2;
        }
    }

    void AdjustToWorldSpace()
    {
        Vector3 objectPosition = m_meshObject.transform.position;

        for (int iter = 0; iter <= m_vertices.Count - 1; iter++)
        {
            m_vertices[iter] = m_vertices[iter] - objectPosition;
        }
    }

    float CalculateTotalLineLength()
    {
        float length = 0.0f;

        Vector3 pointA = m_positions[0];
        Vector3 pointB = Vector3.zero;

        for (int iter = 1; iter <= m_positions.Count -1; iter++)
        {
            pointB = m_positions[iter];
            length += Vector3.Distance(pointA, pointB);
            pointA = pointB;
        }

        m_totalLineLength = length;
        return length;
    }

    float CalculateWidthOverLength(float _currentLength)
    {
        if(m_totalLineLength <= 0.0f)
        {
            Debug.LogError("TotalLineLength can't be 0");
            return 0.0f;
        }
        float betweenWidth = m_maximumWidth - m_minimumWidth;
        return m_minimumWidth + (betweenWidth * m_widthOverLength.Evaluate(_currentLength / m_totalLineLength));
    }

    Color CalculateColourOverLength(float _currentLength)
    {
        Color betweenColour = m_endColour - m_startColour;
        return m_startColour + (betweenColour * m_colourOverLength.Evaluate(_currentLength / m_totalLineLength));
    }
}
