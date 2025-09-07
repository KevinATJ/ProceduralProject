using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeBuilder : MonoBehaviour
{
    public GameObject branchPrefab;
    public GameObject leafPrefab;
    [HideInInspector] public float angle;
    [HideInInspector] public float branchScaleY;

    float branchLength;
    float leafLength;

    private Stack<TransformInfo> transformStack;

    public Material normalTrunkMaterial;
    public Material normalLeafMaterial;
    public Material volcanicTrunkMaterial;
    public Material volcanicLeafMaterial;

    private Material currentTrunkMaterial;
    private Material currentLeafMaterial;

    public void Awake()
    {
        transformStack = new Stack<TransformInfo>();
        branchLength = branchPrefab.transform.localScale.y * branchScaleY;
        Debug.Log("Lenght :" + branchLength);
    }

    private void Start()
    {

    }
    public void SetMaterials(DS_Terrain.TerrainType terrainType)
    {
        if (terrainType == DS_Terrain.TerrainType.Normal)
        {
            currentTrunkMaterial = normalTrunkMaterial;
            currentLeafMaterial = normalLeafMaterial;
        }
        else
        {
            currentTrunkMaterial = volcanicTrunkMaterial;
            currentLeafMaterial = volcanicLeafMaterial;
        }
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
                    var branchRenderer = branch.GetComponent<MeshRenderer>();
                    if (branchRenderer != null && currentTrunkMaterial != null)
                        branchRenderer.material = currentTrunkMaterial;

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
                    var leafRenderer = leaf.GetComponent<MeshRenderer>();
                    if (leafRenderer != null && currentLeafMaterial != null)
                        leafRenderer.material = currentLeafMaterial;
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
