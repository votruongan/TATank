using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DigController : TimedController
{
	[Header("FOR DEBUG")]
	public bool isDiggable;
	public ForegroundController foreController;
	public List<Vector2> digVertices;
	public float dividedAngle;
	[Header("SETTINGS")]
	public int verticesDivision = 3;

	public void CheckIfDiggable(){
		isDiggable = foreController.IsForegroundLoaded();
	}

	protected override void Execute(){
		// place hole in front of foreground Sprite
		// if (!(isDiggable)){
		// 	Debug.Log("Not diggable!");
		// 	return;
		// }
		// Instantiate(prefab,target,Quaternion.identity);
		// reCalculate foreground Collider
		Vector2 center = new Vector2(target.x, target.y);
		// PrepareDigVertices(center);
		Vector2[] shape = RegularPolygon(20, new Vector2(-4.13f, -4.07f), 1.0f);

		List<Vector2[]> newPaths = new List<Vector2[]>();
		for (int i = 0; i < foreController.foreCollider.pathCount; i++){
			Vector2[] path = foreController.foreCollider.GetPath(i);
			if (doIntersect(path, shape)) {
				foreach(List<Vector2> newPath in Dig(path, shape)) {
					newPaths.Add(newPath.ToArray());
				}
			} else {
				newPaths.Add(path);
			}
		}

		int count = 0;
		foreController.foreCollider.pathCount = newPaths.Count;
		foreach(Vector2[] newPath in newPaths) {
			foreController.foreCollider.SetPath(count, newPath);
			++count;
		}
		// foreController.RefreshCollider(foreRefresher.gameObject);
	}

	Vector2[] RegularPolygon(int numVertices, Vector2 center, float radius) {
		Vector2[] polygon = new Vector2[numVertices];
		for(int i = 0; i < numVertices; ++i) {
			polygon[numVertices - i - 1].x = center.x + radius * Mathf.Cos(2 * Mathf.PI * i / numVertices);
			polygon[numVertices - i - 1].y = center.y + radius * Mathf.Sin(2 * Mathf.PI * i / numVertices);
		}
		return polygon;
	}
	// Set vertices list along the bound of hole
	void PrepareDigVertices(Vector2 pos){
		float Radius = transform.localScale.x / 10;
		// // Horizontal through center
		// digVertices.Add(new Vector2(pos.x, pos.y + Radius));
		// digVertices.Add(new Vector2(pos.x, pos.y - Radius));
		// // Vertical through center
		// digVertices.Add(new Vector2(pos.x + Radius, pos.y));
		// digVertices.Add(new Vector2(pos.x - Radius, pos.y));
		for (int i = 0; i <verticesDivision; i++){
			float dx = Mathf.Sin(i*dividedAngle)*Radius;
			float dy = Mathf.Cos(i*dividedAngle)*Radius;
			// First corner in the graph
			digVertices.Add(new Vector2(pos.x + dx,
										pos.y + dy));
			// Second corner in the graph
			digVertices.Add(new Vector2(pos.x - dx,
										pos.y + dy));
			// Third corner in the graph
			digVertices.Add(new Vector2(pos.x - dx,
										pos.y - dy));
			// Fourth corner in the graph
			digVertices.Add(new Vector2(pos.x + dx,
										pos.y - dy));
		}
	}

    // Start is called before the first frame update
    void Start()
    {
    	dividedAngle = 90/verticesDivision;
        foreController = GameObject.Find("Foreground").GetComponent<ForegroundController>();
    }	

	// Dig the given inputTerrain by the give inputShape 
	// IMPORTANT: orientation of shape and terrain must be opposite
	LinkedList<List<Vector2>> Dig(Vector2[] inputTerrain, Vector2[] inputShape) {
		LinkedList<List<Vector2>> terrains = SubtractShape(inputTerrain, inputShape);
		LinkedList<List<Vector2>> lines = GetComplementSet(inputShape, SubtractShape(inputShape, inputTerrain));

		if (lines.Count != 0) {
			foreach(List<Vector2> line in lines) {
				LinkedListNode<List<Vector2>> currentNode = terrains.First;
				LinkedListNode<List<Vector2>> beginNode = currentNode;
				float minDistance = Vector2.Distance(line[0], beginNode.Value[beginNode.Value.Count - 1]);
				
				while(currentNode.Next != null) {
					currentNode = currentNode.Next;
					float distance = Vector2.Distance(line[0], currentNode.Value[currentNode.Value.Count - 1]);
					if (minDistance > distance) {
						beginNode = currentNode;
						minDistance = distance;
					}
				}

				foreach(Vector2 point in line) {
					beginNode.Value.Add(point);
				}

				currentNode = terrains.First;
				LinkedListNode<List<Vector2>> endNode = currentNode;
				minDistance = Vector2.Distance(line[line.Count - 1], endNode.Value[0]);

				while (currentNode.Next != null) {
					currentNode = currentNode.Next;
					float distance = Vector2.Distance(line[line.Count - 1], currentNode.Value[0]);
					if (minDistance > distance) {
						endNode = currentNode;
						minDistance = distance;
					}
				}

				if (beginNode != endNode) {
					foreach(Vector2 point in endNode.Value) {
						beginNode.Value.Add(point);
					}
					terrains.Remove(endNode);
				} 
			}
		} 
		return terrains;
	}

	LinkedList<List<Vector2>> GetComplementSet(Vector2[] universe, LinkedList<List<Vector2>> subsets) {
		// Initialization
		LinkedList<List<Vector2>> complementSet = new LinkedList<List<Vector2>>();
		int length = universe.Length;

		// Flatter subsets
		List<Vector2> flatten = new List<Vector2>();
		foreach(List<Vector2> subset in subsets) {
			foreach(Vector2 point in subset) {
				flatten.Add(point);
			}
		}

		if (flatten.Count < length) {
			// Find first point not belong to flatten list
			bool prevBelongToFlatten = flatten.Contains(universe[length - 1]);
			int pos = 0;
			while (pos < length) {
				bool currBelongToFlatten = flatten.Contains(universe[pos]);
				if (prevBelongToFlatten && !currBelongToFlatten)
					break;
				prevBelongToFlatten = currBelongToFlatten;
				++pos;
			}

			// Find complement set
			prevBelongToFlatten = true;
			for(int i = 0; i < length; ++i) {
				// Debug.Log(pos + ": " + length);
				bool currBelongToFlatten = flatten.Contains(universe[pos]);
				if (prevBelongToFlatten && !currBelongToFlatten) {
					complementSet.AddLast(new List<Vector2>());
				}
				if (!currBelongToFlatten) {
					complementSet.Last.Value.Add(universe[pos]);
				}
				prevBelongToFlatten = currBelongToFlatten;
				pos = (pos + 1) % length;
			}
		}

		// foreach(List<Vector2> fragment in complementSet) {
		// 	Debug.Log("New fragment: ");
		// 	foreach(Vector2 point in fragment) {
		// 		Debug.Log(point);
		// 	}
		// }
		return complementSet;
	}

	//Subtract the shape1 by the shape2
	LinkedList<List<Vector2>> SubtractShape(Vector2[] shape1, Vector2[] shape2){

		// Initialization
		LinkedList<List<Vector2>> output = new LinkedList<List<Vector2>>();
		int length = shape1.Length;

		// Find first point in shape1 that just come from shape2
		int pos = -1, prev = length - 1;
		for(int i = 0; i < length; ++i) {
			// Debug.Log(doIntersect(shape1[prev], shape1[i], shape2) + " " + isInside(shape2, shape1[i]));
			if (doIntersect(shape1[prev], shape1[i], shape2) && !isInside(shape2, shape1[i])) {
				pos = i;
				break;
			}
			prev = i;
		}

		// Subtract shape1 by shape2
		if (pos != -1) {
			prev = (pos - 1 + length) % length;
			for(int i = 0; i < length; ++i) {
				if (doIntersect(shape1[prev], shape1[pos], shape2) && !isInside(shape2, shape1[pos])) { 
					output.AddLast(new List<Vector2>());
				}
				if (!isInside(shape2, shape1[pos])) {
					output.Last.Value.Add(shape1[pos]);
				}
				prev = pos;
				pos = (pos + 1) % length;
			}
		}

		// foreach(List<Vector2> fragment in output) {
		// 	Debug.Log("New fragment: ");
		// 	foreach(Vector2 point in fragment) {
		// 		Debug.Log(point);
		// 	}
		// }
		return output;
	}

	// The function that returns true if 
	// two shapes are intersect
	bool doIntersect(Vector2[] shape1, Vector2[] shape2) {
		Vector2 prev = shape1[shape1.Length - 1];
		foreach(Vector2 curr in shape1) {
			if (doIntersect(prev, curr, shape2) || isInside(shape2, curr))
				return true;
			prev = curr;
		}
		return false;
	}

	// The function that returns true if 
	// line segment 'p1p2' intersect with the shape. 
	bool doIntersect(Vector2 p1, Vector2 p2, Vector2[] shape) {
		Vector2 prev = shape[shape.Length - 1];
		foreach(Vector2 curr in shape) {
			if (doIntersect(p1, p2, prev, curr))
				return true;
			prev = curr;
		}
		return false;
	}

	// COPIED ZONE, DON'T TOUCH
	// Define Infinite (Using INT_MAX 
	// caused overflow problems) 
	int INF = 10000; 
	float EPSILON = 1e-9f;

	// Given three colinear points p, q, r, 
	// the function checks if point q lies 
	// on line segment 'pr' 
	bool onSegment(Vector2 p, Vector2 q, Vector2 r) 
	{ 
		if (q.x <= Mathf.Max(p.x, r.x) && 
			q.x >= Mathf.Min(p.x, r.x) && 
			q.y <= Mathf.Max(p.y, r.y) && 
			q.y >= Mathf.Min(p.y, r.y)) 
		{ 
			return true; 
		} 
		return false; 
	} 

	// To find orientation of ordered triplet (p, q, r). 
	// The function returns following values 
	// 0 --> p, q and r are colinear 
	// 1 --> Clockwise 
	// 2 --> Counterclockwise 
	int orientation(Vector2 p, Vector2 q, Vector2 r) 
	{ 
		float val = (q.y - p.y) * (r.x - q.x) - 
				(q.x - p.x) * (r.y - q.y); 

		if (Mathf.Abs(val) < EPSILON) 
		{ 
			return 0; // colinear 
		} 
		return (val > 0) ? 1 : 2; // clock or counterclock wise 
	} 

	// The function that returns true if 
	// line segment 'p1q1' and 'p2q2' intersect. 
	bool doIntersect(Vector2 p1, Vector2 q1, 
							Vector2 p2, Vector2 q2) 
	{ 
		// Find the four orientations needed for 
		// general and special cases 
		int o1 = orientation(p1, q1, p2); 
		int o2 = orientation(p1, q1, q2); 
		int o3 = orientation(p2, q2, p1); 
		int o4 = orientation(p2, q2, q1); 

		// General case 
		if (o1 != o2 && o3 != o4) 
		{ 
			return true; 
		} 

		// Special Cases 
		// p1, q1 and p2 are colinear and 
		// p2 lies on segment p1q1 
		if (o1 == 0 && onSegment(p1, p2, q1)) 
		{ 
			return true; 
		} 

		// p1, q1 and p2 are colinear and 
		// q2 lies on segment p1q1 
		if (o2 == 0 && onSegment(p1, q2, q1)) 
		{ 
			return true; 
		} 

		// p2, q2 and p1 are colinear and 
		// p1 lies on segment p2q2 
		if (o3 == 0 && onSegment(p2, p1, q2)) 
		{ 
			return true; 
		} 

		// p2, q2 and q1 are colinear and 
		// q1 lies on segment p2q2 
		if (o4 == 0 && onSegment(p2, q1, q2)) 
		{ 
			return true; 
		} 

		// Doesn't fall in any of the above cases 
		return false; 
	} 

	// Returns true if the point p lies 
	// inside the polygon[] with n vertices 
	bool isInside(Vector2 []polygon, Vector2 p) 
	{ 
		int  n = polygon.Length;

		// There must be at least 3 vertices in polygon[] 
		if (n < 3) 
		{ 
			return false; 
		} 

		// Create a point for line segment from p to infinite 
		Vector2 extreme = new Vector2(INF, p.y); 

		// Count intersections of the above line 
		// with sides of polygon 
		int count = 0, i = 0; 
		do
		{ 
			int next = (i + 1) % n; 

			// Check if the line segment from 'p' to 
			// 'extreme' intersects with the line 
			// segment from 'polygon[i]' to 'polygon[next]' 
			if (doIntersect(polygon[i], 
							polygon[next], p, extreme)) 
			{ 
				// If the point 'p' is colinear with line 
				// segment 'i-next', then check if it lies 
				// on segment. If it lies, return true, otherwise false 
				if (orientation(polygon[i], p, polygon[next]) == 0) 
				{ 
					return onSegment(polygon[i], p, 
									polygon[next]); 
				} 
				if (p.y != polygon[i].y) count++; 
			} 
			i = next; 
		} while (i != 0); 

		// Return true if count is odd, false otherwise 
		return (count % 2 == 1); 	// Same as (count%2 == 1) 
	} 
}
