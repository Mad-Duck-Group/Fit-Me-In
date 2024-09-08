using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class ColliderCombiner : MonoBehaviour
{
    void Start()
    {
        //CombineChildCollidersIntoPolygon();
    }

    [Button("Combine Child Colliders")]
    void CombineChildCollidersIntoPolygon()
    {
        // Get all BoxCollider2D components in the children
        BoxCollider2D[] childColliders = transform.GetChild(0).GetComponentsInChildren<BoxCollider2D>();

        if (childColliders.Length == 0)
        {
            Debug.LogWarning("No child BoxCollider2D found to combine.");
            return;
        }

        // Create a list to store the vertices for the polygon
        List<Vector2> polygonPoints = new List<Vector2>();

        foreach (BoxCollider2D boxCollider2D in childColliders)
        {
            // Get the world corners of each BoxCollider2D
            Vector2[] corners = GetColliderCorners(boxCollider2D);
            foreach (Vector2 corner in corners)
            {
                // Convert each corner point to the parent's local space
                Vector2 localCorner = transform.InverseTransformPoint(corner);
                polygonPoints.Add(localCorner);
            }

            // Optionally destroy the child collider after processing
            Destroy(boxCollider2D);
        }

        // Create a new PolygonCollider2D on the parent
        PolygonCollider2D parentPolygonCollider = gameObject.AddComponent<PolygonCollider2D>();
        parentPolygonCollider.SetPath(0, polygonPoints.ToArray());

        Debug.Log("Combined colliders into a single PolygonCollider2D.");
    }

    Vector2[] GetColliderCorners(BoxCollider2D boxCollider2D)
    {
        // Get the center and extents of the BoxCollider2D in world space
        Vector2 colliderCenter = boxCollider2D.transform.TransformPoint(boxCollider2D.offset);
        Vector2 size = boxCollider2D.size * 0.5f;

        Vector2 topLeft = colliderCenter + new Vector2(-size.x, size.y);
        Vector2 topRight = colliderCenter + new Vector2(size.x, size.y);
        Vector2 bottomLeft = colliderCenter + new Vector2(-size.x, -size.y);
        Vector2 bottomRight = colliderCenter + new Vector2(size.x, -size.y);

        return new Vector2[] { topLeft, topRight, bottomRight, bottomLeft };
    }
}
