using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class LSystemGenerator : MonoBehaviour
{
    public int iterations = 4;
    public string axiom = "X";
    public Dictionary<char, string> rules;
    private List<Dictionary<char, string>> treeRulesList;

    void Awake()
    {
        Dictionary<char, string> rules1 = new Dictionary<char, string>
        {
            { 'X', "F[+X][-X]FXL" },
            { 'F', "FF" }
        };

        Dictionary<char, string> rules2 = new Dictionary<char, string>
        {
            { 'X', "FF[+X]F[-X]FFXL" },
            { 'F', "F" }
        };

        Dictionary<char, string> rules3 = new Dictionary<char, string>
        {
            { 'X', "F[+X]FXL" },
            { 'F', "F-F" }
        };

        treeRulesList = new List<Dictionary<char, string>>
        {
            rules1,
            rules2,
            rules3
        };
    }

    public string GenerateSentence()
    {
        int randomIndex = Random.Range(0, treeRulesList.Count);
        rules = treeRulesList[randomIndex];

        string current = axiom;
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < iterations; i++)
        {
            sb.Clear();
            foreach (char c in current)
            {
                sb.Append(rules.ContainsKey(c) ? rules[c] : c.ToString());
            }
            current = sb.ToString();
        }
        return current;
    }
}