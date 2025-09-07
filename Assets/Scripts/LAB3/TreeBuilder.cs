using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeBuilder : MonoBehaviour
{
    public GameObject branchPrefab;
    public GameObject leafPrefab;
    public float angle = 28f;
    public float branchScaleY = 1f;

    float branchLength;
    float leafLength;

    private Stack<TransformInfo> transformStack;

    public void Awake()
    {
        transformStack = new Stack<TransformInfo>();
        branchLength = branchPrefab.transform.localScale.y * branchScaleY;
        Debug.Log("Lenght :" + branchLength);
    }

    private void Start()
    {
       
    }

    public void DrawTree(string lSystemSentence)
    {
        Vector3 initialPos = Vector3.zero;
        Quaternion rotation = Quaternion.identity;
        Debug.Log("L-System Sentence: " + lSystemSentence);
        foreach (char c in lSystemSentence)
        {
            switch (c)
            {
                case 'F':
                    Vector3 branchPos = initialPos + rotation * Vector3.up * branchLength * 2f;
                    GameObject branch = Instantiate(branchPrefab, transform);
                    branch.transform.localPosition = initialPos;
                    branch.transform.localRotation = rotation;
                    branch.transform.localScale = new Vector3(
                        branch.transform.localScale.x,
                        branchPrefab.transform.localScale.y * branchScaleY,
                        branch.transform.localScale.z
                    );

                    initialPos = branchPos;
                    break;
                //Rotacion derecha e izquierda
                case '+':
                    rotation *= Quaternion.Euler(0, 0, angle);
                    break;
                case '-':
                    rotation *= Quaternion.Euler(0, 0, -angle);
                    break;
                //Rotacion en el suelo
                case '*':
                    rotation *= Quaternion.Euler(0, angle, 0);
                    break;
                case '/':
                    rotation *= Quaternion.Euler(0, -angle, 0);
                    break;
                //Rotacion adelante y atras
                case '(':
                    rotation *= Quaternion.Euler(angle, 0, 0);
                    break;
                case ')':
                    rotation *= Quaternion.Euler(-angle, 0, 0);
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
                    leaf.transform.localPosition = leafPos;
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
