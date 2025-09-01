using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeBuilder : MonoBehaviour
{
    public GameObject branchPrefab;
    public GameObject leafPrefab;
    public float angle = 40f;
    float branchLength;
    float leafLength;


    private Stack<TransformInfo> transformStack;

    void Awake()
    {
        transformStack = new Stack<TransformInfo>();
        branchLength = branchPrefab.transform.localScale.y;
        leafLength = leafPrefab.transform.localScale.y;
        Debug.Log("Lenght :" + branchLength);
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
                    Vector3 branchPos = initialPos + rotation * Vector3.up * branchLength;
                    GameObject branch = Instantiate(branchPrefab, transform);
                    branch.transform.localPosition = initialPos+branchPos;
                    branch.transform.localRotation = rotation;

                    initialPos = branchPos;
                    break;
                case '+':

                    rotation *= Quaternion.Euler(0, angle, 0) * Quaternion.Euler(0, 0, angle);//2 caracteres más de rotación
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
                    Vector3 leafPos = initialPos + rotation * Vector3.up * leafLength;
                    GameObject leaf = Instantiate(leafPrefab, transform);
                    leaf.transform.localPosition = initialPos+leafPos;
                    leaf.transform.localRotation = rotation;
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
