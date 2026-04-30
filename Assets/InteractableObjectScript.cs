using UnityEngine;

public class InteractableObjectScript : MonoBehaviour
{
    private Reader readerScript;
    private VNVariables variablesScript;

    private void Start()
    {
        readerScript = Camera.main.GetComponent<Reader>();
        variablesScript = Camera.main.GetComponent<VNVariables>();
    }
    public void DestroyMyself()
    {
        Destroy(gameObject);
    }
    public void StartScenario(string fileName)
    {
        readerScript.ChangeScenario(fileName);
        readerScript.DoLine();
    }
    public void SetVariables(string variablePlusOperation)
    {
        string[] temp = new string[2];
        string operation = "=";
        if (variablePlusOperation.Contains("="))
        {
            operation = "=";
        }
        else if (variablePlusOperation.Contains("+")) 
        {
            operation = "+";
        }
        else if (variablePlusOperation.Contains("-"))
        {
            operation = "-";
        }
        else if (variablePlusOperation.Contains("/"))
        {
            operation = "/";
        }
        else if (variablePlusOperation.Contains("*"))
        {
            operation = "*";
        }

        temp[0] = variablePlusOperation.Substring(0, variablePlusOperation.IndexOf(operation));
        temp[1] = variablePlusOperation.Substring(variablePlusOperation.IndexOf(operation));

        variablesScript.ChangeVariable(temp[0], temp[1]);
    }
}
