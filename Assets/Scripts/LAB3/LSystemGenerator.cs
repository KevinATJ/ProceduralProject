using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class LSystemGenerator : MonoBehaviour
{
    public int iterations = 4;
    public string axiom = "X";
    public Dictionary<char, string> rules;

    void Awake()
    {
        rules = new Dictionary<char, string>
        {
            { 'X', "F[+X][-X]FXL" },
            { 'F', "FF" }
        };
    }

    public string GenerateSentence()
    {
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