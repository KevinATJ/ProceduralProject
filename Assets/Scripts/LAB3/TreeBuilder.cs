using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeBuilder : MonoBehaviour
{
    public GameObject branchPrefab;
    public GameObject leafPrefab;
    public float angle = 25f;
    float length;


    private Stack<TransformInfo> transformStack;

    void Awake()
    {
        transformStack = new Stack<TransformInfo>();
        length = branchPrefab.transform.localScale.y;
        Debug.Log("Lenght :" + length);
    }

    private void Start()
    {
       
    }

    public void DrawTree(string lSystemSentence)
    {
        Vector3 initialPos = Vector3.zero;
        Quaternion rotation = Quaternion.identity;

        foreach (char c in lSystemSentence)
        {
            switch (c)
            {
                case 'F':
                    Vector3 newPos = initialPos + rotation * Vector3.up * length;
                    GameObject branch = Instantiate(branchPrefab, transform);
                    branch.transform.localPosition = (initialPos + newPos) / 2f;
                    branch.transform.localRotation = rotation;

                    initialPos = newPos;
                    break;
                case '+':

                    rotation *= Quaternion.Euler(0, angle, 0) * Quaternion.Euler(0, 0, angle);
                    break;
                case '-':
                    rotation *= Quaternion.Euler(0, -angle, 0) * Quaternion.Euler(0, 0, -angle);
                    break;
                case '[':
                    transformStack.Push(new TransformInfo { position = initialPos, rotation = rotation });
                    break;
                case ']':
                    TransformInfo ti = transformStack.Pop();
                    initialPos = ti.position;
                    rotation = ti.rotation;
                    break;
                case 'L':
                    GameObject leaf = Instantiate(leafPrefab, transform);
                    leaf.transform.localPosition = initialPos;
                    break;
            }
        }
    }

    private struct TransformInfo
    {
        public Vector3 position;
        public Quaternion rotation;
    }
}
