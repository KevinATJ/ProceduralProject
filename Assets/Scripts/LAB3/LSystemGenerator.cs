using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[System.Serializable]
public class LRule
{
    public char symbol;
    public string replacement;
}

[System.Serializable]
public class LSystemRuleSet
{
    public string name = "Tipo de árbol";
    public string axiom = "X";
    public int iterations = 4;
    public float angle = 28f;
    public float branchScaleY = 1f;
    public List<LRule> rulesList = new List<LRule>();
}

public class LSystemGenerator : MonoBehaviour
{
    public List<LSystemRuleSet> treeTypes = new List<LSystemRuleSet>();

    public string GenerateSentence(int treeTypeIndex)
    {
        if (treeTypeIndex < 0 || treeTypeIndex >= treeTypes.Count)
            treeTypeIndex = 0;

        var ruleSet = treeTypes[treeTypeIndex];
        var rules = new Dictionary<char, string>();
        foreach (var rule in ruleSet.rulesList)
            rules[rule.symbol] = rule.replacement;

        string current = ruleSet.axiom;
        var sb = new StringBuilder();

        for (int i = 0; i < ruleSet.iterations; i++)
        {
            sb.Clear();
            foreach (char c in current)
                sb.Append(rules.ContainsKey(c) ? rules[c] : c.ToString());
            current = sb.ToString();
        }
        return current;
    }
}

