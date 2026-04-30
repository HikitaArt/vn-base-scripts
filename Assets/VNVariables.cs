using System;
using System.Collections.Generic;
using UnityEngine;

public class VNVariables : MonoBehaviour
{
    public Dictionary<string, object> variables;
    void Start()
    {
        variables = new Dictionary<string, object>()
        {
            ["money"] = 30,
            ["health"] = 80
        };
    }
    public void ChangeVariable(string varName, string operation)
    {
        if (operation.Substring(0,1) == "+")
        {
            variables[varName] = Convert.ToInt64(variables[varName]) + Convert.ToInt64(operation.Substring(1));
        }
        else if (operation.Substring(0, 1) == "-")
        {
            variables[varName] = Convert.ToInt64(variables[varName]) - Convert.ToInt64(operation.Substring(1));
        }
        else if (operation.Substring(0, 1) == "/")
        {
            variables[varName] = Convert.ToInt64(variables[varName]) / Convert.ToInt64(operation.Substring(1));
        }
        else if (operation.Substring(0, 1) == "*")
        {
            variables[varName] = Convert.ToInt64(variables[varName]) * Convert.ToInt64(operation.Substring(1));
        }
        else if (operation.Substring(0, 1) == "=")
        {
            variables[varName] = Convert.ToInt64(operation.Substring(1));
        }
        Debug.Log(varName + ": " + variables[varName]);
    }
    public object GetVariable(string varName)
    {
        if (variables.ContainsKey(varName))
        {
            return variables[varName];
        }
        return false;
    }
}
