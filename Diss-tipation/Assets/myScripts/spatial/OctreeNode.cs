using UnityEngine;
using System.Collections.Generic;

public class OctreeNode<T> where T : class
{

    // generic class for objects held by the octree
    class OctreeObject
    {
        public T Obj;
        public Vector3 Pos;
        public float objRadius;
        public int objID;
    }

    internal Vector3 Centre { get; set; }
    internal float sideLength { get; set; }

    float minSize;
    Bounds bounds = default(Bounds);
    Bounds[] childBounds;
    int maxObjectsAllowed = 8;

    Vector3 actualBoundsSize;

    //list of the objects the node holds
    List<OctreeObject> objects = new List<OctreeObject>();
    OctreeNode<T>[] children = null;

    // construct the octree
    public OctreeNode(float nodeLength, float minNodeSize, Vector3 nodeCentre)
    {
        setValues(nodeLength, minSize, nodeCentre);
    }

    // set the values for the node
    void setValues(float _nodeLength, float _minNodeSize, Vector3 _nodeCentre)
    {
        sideLength = _nodeLength;
        minSize = _minNodeSize;
        Centre = _nodeCentre;

        // create the bounds size (will be uniform)
        actualBoundsSize = new Vector3(sideLength, sideLength, sideLength);
        //create the bounding box
        bounds = new Bounds(Centre, actualBoundsSize);

        // calculate the required values of each bounding box
        float quarter = sideLength / 4.0f;
        float actualChildLength = sideLength / 2.0f;
        Vector3 actualChildSize = new Vector3(actualChildLength, actualChildLength, actualChildLength);

        // initialise the bounding box array
        childBounds = new Bounds[8];
        // assign the new child bounds
        childBounds[0] = new Bounds(Centre + new Vector3(-quarter, quarter, -quarter), actualChildSize);
        childBounds[1] = new Bounds(Centre + new Vector3(quarter, quarter, -quarter), actualChildSize);
        childBounds[2] = new Bounds(Centre + new Vector3(-quarter, quarter, quarter), actualChildSize);
        childBounds[3] = new Bounds(Centre + new Vector3(quarter, quarter, quarter), actualChildSize);
        childBounds[4] = new Bounds(Centre + new Vector3(-quarter, -quarter, -quarter), actualChildSize);
        childBounds[5] = new Bounds(Centre + new Vector3(-quarter, -quarter, quarter), actualChildSize);
        childBounds[6] = new Bounds(Centre + new Vector3(quarter, -quarter, -quarter), actualChildSize);
        childBounds[7] = new Bounds(Centre + new Vector3(quarter, -quarter, -quarter), actualChildSize);
    }

    public bool Add(T obj, Vector3 objPos, float objRadius, int objID)
    {
        if (!doesItFit(bounds, objPos))
            return false;
        internalAdd(obj, objPos, objRadius, objID);
        return true;
    }

    void internalAdd(T obj, Vector3 objPos, float objRadius, int objID)
    {
        // if there are few enough objects in the node/if the node is infertile
        if (objects.Count < maxObjectsAllowed || (sideLength / 2) < minSize)
        {
            OctreeObject newObject = new OctreeObject();
            newObject.Obj = obj;
            newObject.Pos = objPos;
            newObject.objRadius = objRadius;
            newObject.objID = objID;

            objects.Add(newObject);
        }
        // there are too many objects + the node is too small to have children
        else
        {
            int bestFitChild;
            // if the node has no children
            if (children == null)
            {
                // split the node
                splitNode();

                // ioterate through the objects in this node
                for (int i = objects.Count - 1; i >= 0; --i)
                {
                    OctreeObject existingObject = objects[i];
                    // for each object establish which node it would fit in
                    bestFitChild = BestFitChild(existingObject.Pos);
                    //add the object to the node
                    children[bestFitChild].internalAdd(existingObject.Obj, existingObject.Pos, existingObject.objRadius, objID);
                    //remove the object from this node
                    objects.Remove(existingObject);
                }
            }
            // recursively call the function to place this object
            bestFitChild = BestFitChild(objPos);
            children[bestFitChild].internalAdd(obj, objPos, objRadius, objID);
        }
    }

    public bool remove(T obj, int objID)
    {
        // set the object removed to be false
        bool objectRemoved = false;

        //iterate through the objects
        for (int i = 0; i < objects.Count; ++i)
        {
            // if the object matches the object passed in
            if (objID == objects[i].objID)
            {
                // remove the object and set the bool to be true
                objectRemoved = objects.Remove(objects[i]);
                break;
            }
        }
        // if the object was not removed and the node has children
        if (!objectRemoved && children != null)
        {
            // iterate through the children nodes
            for (int i = 0; i < 8; ++i)
            {
                // call the remove function from inside the child node
                objectRemoved = children[i].remove(obj, objID);
                if (objectRemoved)
                    break;
            }
        }
        // if the child has been removed
        if (objectRemoved && children != null)
        {
            // check if the node should merge with another
            if (shouldMerge())
            {
                // then merge
                Merge();
            }
        }
        // confirm that the object has been removed
        return objectRemoved;
    }

    void splitNode()
    {
        float quarter = sideLength / 4f;
        float newLength = sideLength / 2;
        children = new OctreeNode<T>[8];
        children[0] = new OctreeNode<T>(newLength, minSize, Centre + new Vector3(-quarter, quarter, -quarter));
        children[1] = new OctreeNode<T>(newLength, minSize, Centre + new Vector3(quarter, quarter, -quarter));
        children[2] = new OctreeNode<T>(newLength, minSize, Centre + new Vector3(-quarter, quarter, quarter));
        children[3] = new OctreeNode<T>(newLength, minSize, Centre + new Vector3(quarter, quarter, quarter));
        children[4] = new OctreeNode<T>(newLength, minSize, Centre + new Vector3(-quarter, -quarter, -quarter));
        children[5] = new OctreeNode<T>(newLength, minSize, Centre + new Vector3(quarter, -quarter, -quarter));
        children[6] = new OctreeNode<T>(newLength, minSize, Centre + new Vector3(-quarter, -quarter, quarter));
        children[7] = new OctreeNode<T>(newLength, minSize, Centre + new Vector3(quarter, -quarter, quarter));
    }

    bool shouldMerge()
    {
        int totalObjects = objects.Count;
        if (children != null)
        {
            // iterate throgh the children
            foreach (OctreeNode<T> child in children)
            {
                // if the child node has it's own children
                if (child.children != null)
                {
                    // not possible to merge
                    return false;
                }
                totalObjects += child.objects.Count;
            }
        }
        // if the total objects exceeds the max then return false
        return totalObjects <= maxObjectsAllowed;
    }

    void Merge()
    {
        // for each child
        for(int i = 0; i < 8; ++i)
        {
            OctreeNode<T> currentChild = children[i];
            int numObjects = currentChild.objects.Count;
            // for each object
            for (int j = numObjects - 1; j >= 0; --j)
            {
                // add each object to this node
                OctreeObject currentObject = currentChild.objects[j];
                objects.Add(currentObject);
            }
        }
        // kill the children
        children = null;
    }

    public OctreeNode<T> shrinkIfPossible(float minLength)
    {
        // the the node is too big to shrink
        if(sideLength < (2 * minLength))
        {
            return this;
        }
        // this node contains no children/objects
        if (objects.Count == 0 && children.Length == 0)
        {
            return this;
        }

        int bestFit = -1;
        //check the objects in the node
        for (int i = 0; i < objects.Count; ++i)
        {
            OctreeObject currentObject = objects[i];
            int newBestFit = BestFitChild(currentObject.Pos);
            // if the node has more than 1 child 
            if (i == 0 || newBestFit == bestFit)
            {
                if (bestFit < 0)
                    bestFit = newBestFit;
            }
            else // it can't be shrunk
                return this;
        }

        if(children != null)
        {
            bool childHadObjects = false;
            for(int i = 0; i < children.Length; ++i)
            {
                // if the children have objects
                if(children[i].HasObjects())
                {
                    if (childHadObjects)
                        // another child had objects
                        return this;
                    if (bestFit >= 0 && bestFit != i)
                        return this; 
                    childHadObjects = true;
                    bestFit = i;
                }
            }
        }
        // can shrink
        if(children == null)
        {
            setValues(sideLength / 2, minSize, childBounds[bestFit].center);
            return this;
        }
        // return the appropriate child as new root
        return children[bestFit];
    }

    public void getNearby(int objID, ref Ray ray, ref float maxDistance, List<T> result)
    {
        // Does the ray hit this node at all?
        // Note: Expanding the bounds is not exactly the same as a real distance check, but it's fast.
        // TODO: Does someone have a fast AND accurate formula to do this check?
        bounds.Expand(new Vector3(maxDistance * 2, maxDistance * 2, maxDistance * 2));
        bool intersected = bounds.IntersectRay(ray);
        bounds.size = actualBoundsSize;
        if (!intersected)
        {
            return;
        }

        // Check against any objects in this node
        for (int i = 0; i < objects.Count; i++)
        {
            if (DistanceToRay(ray, objects[i].Pos) <= maxDistance)
            {
                if (objects[i].objID != objID)
                {
                    result.Add(objects[i].Obj);
                }
            }
        }

        // Check children
        if (children != null)
        {
            for (int i = 0; i < 8; i++)
            {
                children[i].getNearby(objID, ref ray, ref maxDistance, result);
            }
        }
    }
        //public void getNearby(Vector3 pos, float radius, List<T> result)
        //{
        //    // expand the bounding box
        //    bounds.Expand(new Vector3(maxDistance * 2, maxDistance * 2, maxDistance * 2));
        //    //check if the ray intersects with the bounding box
        //    bool intersected = bounds.IntersectRay(ray);
        //    // reset teh bounds size
        //    bounds.size = actualBoundsSize;
        //    if (!intersected)
        //        return;



        //    // check the ray against the objects
        //    for (int i = 0; i < objects.Count; ++i)
        //    {
        //        OctreeObject Obj = objects[i];
        //        for (int j = 0; j < objects.Count; ++j)
        //        {
        //            if (i != j)
        //            {
        //                OctreeObject neObj = objects[j];
        //                float distance = (((int)(Obj.Pos.x - neObj.Pos.x) ^ 2 + ((int)(Obj.Pos.y - neObj.Pos.y) ^ 2 + ((int)(Obj.Pos.z - neObj.Pos.z) ^ 2))));
        //                float radiusDistance = (int)(Obj.objRadius + neObj.objRadius) ^ 2;

        //                if (distance < radiusDistance)
        //                {
        //                    // add the object to the results
        //                    result.Add(objects[i].Obj);
        //                }
        //            }
        //        }

        //    }

        //    // if it has children
        //    if (children != null)
        //    {
        //        // for each child
        //        for(int i = 0; i < 8; ++i)
        //        {
        //            // recursively call
        //            children[i].getNearby(pos, radius, result);
        //        }
        //    }
        //}

        #region helper functions
        bool HasObjects()
    {
        if (objects.Count > 0) return true;

        if (children != null)
        {
            for (int i = 0; i < 8; i++)
            {
                if (children[i].HasObjects()) return true;
            }
        }

        return false;
    }

    int BestFitChild(Vector3 objPos)
    {
        return (objPos.x <= Centre.x ? 0 : 1) + (objPos.y >= Centre.y ? 0 : 4) + (objPos.z <= Centre.z ? 0 : 2);
    }

    static bool doesItFit(Bounds outerBounds, Vector3 point)
    {
        if (point.x < outerBounds.max.x && point.x > outerBounds.min.x)
        {
            if (point.y < outerBounds.max.y && point.y > outerBounds.min.y)
            {
                if (point.z < outerBounds.max.z && point.z > outerBounds.min.z)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static float DistanceToRay(Ray ray, Vector3 point)
    {
        return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
    }

    public void SetChildren(OctreeNode<T>[] childOctrees)
    {
        if (childOctrees.Length != 8)
        {
            Debug.LogError("Child octree array must be length 8. Was length: " + childOctrees.Length);
            return;
        }

        children = childOctrees;
    }

    public void DrawAllBounds(float depth = 0)
    {
        float tintVal = depth / 7; // Will eventually get values > 1. Color rounds to 1 automatically
        Gizmos.color = new Color(tintVal, 0, 1.0f - tintVal);

        Bounds thisBounds = new Bounds(Centre, new Vector3(sideLength, sideLength, sideLength));
        Gizmos.DrawWireCube(thisBounds.center, thisBounds.size);

        if (children != null)
        {
            depth++;
            for (int i = 0; i < 8; i++)
            {
                children[i].DrawAllBounds(depth);
            }
        }
        Gizmos.color = Color.white;
    }

    #endregion
}
