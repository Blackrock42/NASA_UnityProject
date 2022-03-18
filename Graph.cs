using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

//Thomas Ustica
//March 12, 2022

public class Graph : MonoBehaviour
{
	[SerializeField] Transform pointPrefab;
	[SerializeField, Range(1, 100)] int resolution = 20;
	[SerializeField, Range(.001f, 2f)] float scale = .5f;

	Vector3[] points;

	struct Block
	{
		public int size;
		public Vector3 shape;
		public float[,,] X, Y, Z;

		public Block(float[,,] X, float[,,] Y, float[,,] Z, Vector3 shape)
		{
			this.X = X;
			this.Y = Y;
			this.Z = Z;
			this.shape = shape;

			size = (int)(shape.x * shape.y * shape.z);
		}
	}

	private void Awake()
	{
		List<Block> blocks;

		blocks = ReadPoints("Assets/Resources/PahtCascade-ASCII.xyz");
		//blocks = ReadPoints("Assets/Resources/Test-ASCII.txt"); //Test file

		foreach (Block b in blocks) //Plots every block in the list (comment out to plot singular blocks)
			PlotBlockOutline(b);

		//PlotBlockOutline(blocks[0]); //Use this to plot a singular block
	}

	static List<Block> ReadPoints(string path)
	{
		//Reads all data into a list of blocks

		StreamReader reader = new(path);
		int nBlocks = int.Parse(reader.ReadLine()); //Gets the first integer from the file, which is the number of blocks
		List<Block> blocks = new();
		List<int> IMAX = new();
		List<int> JMAX = new();
		List<int> KMAX = new();

		for (int b = 0; b < nBlocks; b++) //Gets the block dimensions for every block and stores in IMAX, JMAX, and KMAX lists
		{
			string[] IJK = reader.ReadLine().Trim('\n').Split(" ", System.StringSplitOptions.RemoveEmptyEntries);
			IMAX.Add(int.Parse(IJK[0]));
			JMAX.Add(int.Parse(IJK[1]));
			KMAX.Add(int.Parse(IJK[2]));
		}

		//This line reads every remaining value from the file and formats them into a one-dimensional string array, with no new lines and no empty entries
		string[] lines = reader.ReadToEnd().Replace("\n", "").Replace("\r", "").Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
		//This line converts every value in lines to a float and stores them in a new float array
		float[] tokens = System.Array.ConvertAll(lines, x => float.TryParse(x, out float number) ? number : 0);

		int offset = 0;
		for (int b = 0; b < nBlocks; b++) //Loops through every block and gets its X, Y, and Z values
		{
			float[,,] X, Y, Z;
			try
			{
				(X, offset) = ReadPointsChunk_ASCII(tokens, offset, IMAX[b], JMAX[b], KMAX[b]); 
				(Y, offset) = ReadPointsChunk_ASCII(tokens, offset, IMAX[b], JMAX[b], KMAX[b]);
				(Z, offset) = ReadPointsChunk_ASCII(tokens, offset, IMAX[b], JMAX[b], KMAX[b]);

				blocks.Add(new Block(X, Y, Z, new Vector3(IMAX[b], JMAX[b], KMAX[b]))); //Adds each new block in one line
			}
			catch (InvalidDataException e)
			{
				Debug.Log("ERROR: " + e.Message);
			}
		}

		reader.Close();
		return blocks;
	}

	static (float[,,], int) ReadPointsChunk_ASCII(float[] tokens, int offset, int IMAX, int JMAX, int KMAX)
	{
		float[,,] A = new float[IMAX, JMAX, KMAX];
		for (int k = 0; k < KMAX; k++)	//Reads each chunk of IMAX * JMAX * KMAX values representing a block
			for (int j = 0; j < JMAX; j++)
				for (int i = 0; i < IMAX; i++)
				{
					A[i, j, k] = tokens[offset]; 
					offset++;
				}

		return (A, offset); //Returns a tuple
	}
	void PlotBlockOutline(Block b)
	{
		//Plots a single block in Unity3D using a cubic point object prefab
		
		int IMAX = (int)b.shape.x, JMAX = (int)b.shape.y, KMAX = (int)b.shape.z;
		float[,,] X = b.X;
		float[,,] Y = b.Y;
		float[,,] Z = b.Z;

		int cullCounter = 0;
		for (int i = 0; i < IMAX; i++)      //Only one of these nested loops is necessary to plot the entire block
			for (int j = 0; j < JMAX; j++) //This nested for loop plots the entire block using constant I values
			{
				float[] x = GetColumn(X, i, j, -1);
				float[] y = GetColumn(Z, i, j, -1);
				float[] z = GetColumn(Y, i, j, -1);
				if (cullCounter % (100/resolution) == 0) //Unity has trouble rendering millions of objects, so this line ensures only a factor of them are plotted
					PlotPoints(x, y, z);

				cullCounter++;
			}
		for (int j = 0; j < JMAX; j++)
			for (int k = 0; k < KMAX; k++) //This nested for loop plots the entire block using constant J values
			{
				float[] x = GetColumn(X, -1, j, k);
				float[] y = GetColumn(Z, -1, j, k);
				float[] z = GetColumn(Y, -1, j, k);
				if (cullCounter % (100 / resolution) == 0) //Cull factoring
					PlotPoints(x, y, z);

				cullCounter++;
			}
		for (int i = 0; i < IMAX; i++)
			for (int k = 0; k < KMAX; k++) //This nested for loop plots the entire block using constant K values
			{
				float[] x = GetColumn(X, i, -1, k);
				float[] y = GetColumn(Z, i, -1, k);
				float[] z = GetColumn(Y, i, -1, k);
				if (cullCounter % (100 / resolution) == 0) //Cull factoring
					PlotPoints(x, y, z);

				cullCounter++;
			}
	}

	void PlotPoints(float[] x, float[] y, float[] z)
	{
		//Plots a single point in Unity3D using a cubic point prefab

		for (int i = 0; i < x.Length; i++)
		{
			Transform point = Instantiate(pointPrefab, new Vector3(x[i], y[i], z[i]), Quaternion.identity); //Instantiates prefab with given position
			point.localScale = scale * Vector3.one;

			point.SetParent(transform, false); //Sets the parent to the Graph object so they'll be listed under it in the Hierarchy
		}
	}

	float[] GetColumn(float[,,] X, int i, int j, int k)
	{
		//Returns one column of a 3D array

		//The 3D is passed in as argument float[,,] X
		//Each integer (i, j, and k) represents the constant dimensions from which to get the column
		//One of i, j, or k will equal -1, which means that is the dimension to traverse to get the column

		float[] col;
		if (i == -1) //if i is passed in as the variable dimension, then get a column with constant j and k
		{
			col = new float[X.GetLength(0)];
			for (int index = 0; index < X.GetLength(0); index++)
				col[index] = X[index, j, k];
		}
		else if (j == -1) //if j is passed in as the variable dimension, then get a column with constant i and k
		{
			col = new float[X.GetLength(1)];
			for (int index = 0; index < X.GetLength(1); index++)
				col[index] = X[i, index, k];
		}
		else if (k == -1) //if k is passed in as the variable dimension, then get a column with constant i and j
		{
			col = new float[X.GetLength(2)];
			for (int index = 0; index < X.GetLength(2); index++)
				col[index] = X[i, j, index];
		}
		else
		{
			throw new InvalidDataException("No column found to parse"); //throw exception if one of i, j, or k does not equal -1
		}
		return col;
	}
}
