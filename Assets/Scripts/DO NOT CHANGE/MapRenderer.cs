using UnityEngine;


public class MapRenderer : MonoBehaviour
{
	public const float MapWidthInWorld = 100.0f;
	public const float MapHeightInWorld = 100.0f;

	#region Private interface

	private const float depthPosition = 2.0f;
	private static readonly Color ColourBrown = new Color(130f / 255f, 60f / 255f, 10f / 255f);
	private static readonly Color ColourDarkGreen = new Color(0f, 0.4f, 0f);
	private static readonly Color ColourLightGreen = new Color(0f, 0.8f, 0f);

	private byte[] mapData;
	private int mapDataWidth;
	private int mapDataHeight;
	private float squareWidth;
	private float squareHeight;

	private Mesh mesh;
	private MeshFilter meshFilter;
	private MeshRenderer meshRenderer;

	public void Initialise(byte[] mapData, int mapDataWidth, int mapDataHeight)
	{
		this.mapData = mapData;
		this.mapDataWidth = mapDataWidth;
		this.mapDataHeight = mapDataHeight;
		squareWidth = MapWidthInWorld / (float)mapDataWidth;
		squareHeight = MapHeightInWorld / (float)mapDataHeight;

		CreateGridLines();
		CreateMesh();
	}

	private void CreateMesh()
	{
		int verticesCount = mapDataWidth * mapDataHeight * 4;

		var vertices = new Vector3[verticesCount];
		var uvs = new Vector2[verticesCount];
		var colours = new Color[verticesCount];
		var normals = new Vector3[verticesCount];
		var triangles = new int[6 * mapDataWidth * mapDataHeight];

		// Set the vertices of the mesh
		int vertexIndex = 0;
		for (int z = 0; z < mapDataHeight; ++z)
		{
			float startZ = (float)z / (float)mapDataHeight * MapHeightInWorld;
			float endZ = (float)(z + 1) / (float)mapDataHeight * MapHeightInWorld;

			for (int x = 0; x < mapDataWidth; ++x)
			{
				float startX = (float)x / (float)mapDataWidth * MapWidthInWorld;
				float endX = (float)(x + 1) / (float)mapDataWidth * MapWidthInWorld;
				var colour = MapDataValueToColour(mapData[x + z * mapDataWidth]);

				vertices[vertexIndex] = new Vector3(startX, startZ, depthPosition);
				colours[vertexIndex] = colour;
				uvs[vertexIndex] = new Vector2();       // No texturing so just set to zero - could be expanded in the future
				normals[vertexIndex] = Vector3.forward;      // These should be set based on heights of terrain but we can use Recalulated normals on mesh to calculate for us
				++vertexIndex;

				vertices[vertexIndex] = new Vector3(endX, startZ, depthPosition);
				colours[vertexIndex] = colour;
				uvs[vertexIndex] = new Vector2();       // No texturing so just set to zero - could be expanded in the future
				normals[vertexIndex] = Vector3.forward;      // These should be set based on heights of terrain but we can use Recalulated normals on mesh to calculate for us
				++vertexIndex;

				vertices[vertexIndex] = new Vector3(startX, endZ, depthPosition);
				colours[vertexIndex] = colour;
				uvs[vertexIndex] = new Vector2();       // No texturing so just set to zero - could be expanded in the future
				normals[vertexIndex] = Vector3.forward;      // These should be set based on heights of terrain but we can use Recalulated normals on mesh to calculate for us
				++vertexIndex;

				vertices[vertexIndex] = new Vector3(endX, endZ, depthPosition);
				colours[vertexIndex] = colour;
				uvs[vertexIndex] = new Vector2();       // No texturing so just set to zero - could be expanded in the future
				normals[vertexIndex] = Vector3.forward;      // These should be set based on heights of terrain but we can use Recalulated normals on mesh to calculate for us
				++vertexIndex;
			}
		}

		// Setup the indexes so they are in the correct order and will render correctly
		int trianglesIndex = 0;
		for (int z = 0; z < mapDataHeight; ++z)
		{
			for (int x = 0; x < mapDataWidth; ++x)
			{
				vertexIndex = (x + (mapDataWidth * z)) * 4;

				triangles[trianglesIndex++] = vertexIndex;
				triangles[trianglesIndex++] = vertexIndex + 3;
				triangles[trianglesIndex++] = vertexIndex + 1;
				triangles[trianglesIndex++] = vertexIndex;
				triangles[trianglesIndex++] = vertexIndex + 2;
				triangles[trianglesIndex++] = vertexIndex + 3;
			}
		}

		// Assign all of the data that has been created to the mesh and update it
		mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.uv = uvs;
		mesh.triangles = triangles;
		mesh.colors = colours;
		mesh.normals = normals;
		mesh.RecalculateNormals();
		mesh.UploadMeshData(false);

		meshFilter = gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = mesh;

		meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshRenderer.sharedMaterial = Resources.Load<Material>("MapMaterial");
	}

	private void CreateGridLines()
	{
		GameObject gridLines2DGameObject = new GameObject("GridRenderer");
		gridLines2DGameObject.transform.SetParent(transform, false);

		GridRenderer grid = gridLines2DGameObject.AddComponent<GridRenderer>();
		grid.Initialise(mapDataWidth, mapDataHeight, MapWidthInWorld, MapHeightInWorld, 0.5f * (MapHeightInWorld / 1080));
	}

	private GameObject CreateTargetPoint(GameObject prefab, string name, int x, int y, Color color)
	{
		Vector3 targetPosition = new Vector3(((float)x * squareWidth) + (0.5f * squareWidth), ((float)y * squareHeight) + (0.5f * squareHeight), 0.0f);
		GameObject targetPoint = Instantiate<GameObject>(prefab);
		targetPoint.transform.position = transform.position;
		targetPoint.transform.parent = transform;
		targetPoint.transform.localPosition = targetPosition;
		targetPoint.name = name;
		targetPoint.GetComponent<SpriteRenderer>().color = color;
		return targetPoint;
	}
	

	private static Color MapDataValueToColour(byte value)
	{
		switch (value)
		{
			case 0:
				return Color.blue;
			case 1:
				return ColourBrown;
			case 2:
				return ColourLightGreen;
			case 3:
				return ColourDarkGreen;
			case 4:
				return Color.white;
			case 5:
				return Color.magenta;
		}
		return Color.black;
	}
	#endregion
}
