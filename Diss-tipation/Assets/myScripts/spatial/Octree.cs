using UnityEngine;
using System.Collections.Generic;

namespace fluidClasses
{
    public class Octree<T> where T : class
    {
        int objCount = 0;
        float initialSize;
        float minNodeSize;

        OctreeNode<T> rootNode;

        public Octree(float _initialSize, Vector3 _initialPosition, float _minNodeSize)
        {
            initialSize = _initialSize;
            minNodeSize = _minNodeSize;
            Debug.Log("IT KINDA WORKS");

            rootNode = new OctreeNode<T>(initialSize, minNodeSize, _initialPosition);
        }
        // add the particle to the list
        public void Add(T obj, Vector3 objPos, float objRadius, int objID)
        {
            int count = 0;
            while (!rootNode.Add(obj, objPos, objRadius, objID))
            {
                grow(objPos - rootNode.Centre);
                if (++count > 20)
                    return;
            }
            ++objCount;
        }
        //remove particle from the list
        public bool Remove(T obj, int objID)
        {
            bool removed = rootNode.remove(obj, objID);

            if (removed)
            {
                --objCount;
                rootNode = rootNode.shrinkIfPossible(initialSize);
            }

            return removed;
        }

        public void moveParticle(T obj, Vector3 objPos, float objRadius, int ObjID)
        {
            Remove(obj, ObjID);

            Add(obj, objPos, objRadius, ObjID);
        }


        //find the neighbouring particles
        public T[] getNearby(int objID, Ray ray, float radius)
        {
            List<T> collidingWith = new List<T>();
            rootNode.getNearby(objID, ref ray, ref radius, collidingWith);
            return collidingWith.ToArray();
        }

        void grow(Vector3 direction)
        {
            int xDirection = direction.x >= 0 ? 1 : -1;
            int yDirection = direction.y >= 0 ? 1 : -1;
            int zDirection = direction.z >= 0 ? 1 : -1;

            // save the old root
            OctreeNode<T> oldRoot = rootNode;

            // calculate variables for new root
            float half = rootNode.sideLength / 2;
            float newLength = rootNode.sideLength * 2;
            Vector3 newCentre = rootNode.Centre + new Vector3(xDirection * half, yDirection * half, xDirection * half);

            // create new root
            rootNode = new OctreeNode<T>(newLength, minNodeSize, newCentre);

            // assign root position
            int rootPos = getRootPosIndex(xDirection, yDirection, zDirection);
            OctreeNode<T>[] children = new OctreeNode<T>[8];

            for (int i = 0; i < 8; ++i)
            {
                if (i == rootPos)
                {
                    children[i] = oldRoot;
                }
                else
                {
                    // adjust directions for each node
                    xDirection = i % 2 == 0 ? -1 : 1;
                    yDirection = i > 3 ? -1 : 1;
                    zDirection = (i < 2 || (i > 3 && i < 6)) ? -1 : 1;
                    children[i] = new OctreeNode<T>(rootNode.sideLength, minNodeSize, newCentre + new Vector3(xDirection * half, yDirection * half, zDirection * half));
                }
            }
        }


        static int getRootPosIndex(int xDir, int yDir, int zDir)
        {
            int result = xDir > 0 ? 1 : 0;
            if (yDir < 0) result += 4;
            if (zDir > 0) result += 2;
            return result;
        }

        public void DrawAllObjects()
        {
            rootNode.DrawAllBounds();
        }

    }
}