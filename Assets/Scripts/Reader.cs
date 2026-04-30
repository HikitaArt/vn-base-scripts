using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Reader : MonoBehaviour
{
    public bool animatedText;
    public float textDelay;

    private int curLineIndex;
    private string[] lines;
    
    public TextAsset startScenario;

    public TextMeshProUGUI textPanelText;
    public TextMeshProUGUI namePanelText;
    private GameObject parentUIGO;

    public GameObject variantsHolder;
    public GameObject variantPrefab;

    private VNVariables variablesScript;

    public GameObject uiCanvas;
    public GameObject bgCanvas;
    public Image bgGO;

    public GameObject interactiveParent;

    public AudioSource musicSource;
    public AudioSource soundSource;

    private string curTextLine;

    public bool pointerUnderUI = false;
    public bool uIhidden = false;
    public bool nowChoice = false;
    public bool inDialogue = true;

    void Start()
    {
        variablesScript = gameObject.GetComponent<VNVariables>();
        parentUIGO = namePanelText.transform.parent.parent.gameObject;
        ReadTextFromFile(startScenario);
        DoLine();
    }
    private void ReadTextFromFile(TextAsset txtFile)
    {
        lines = txtFile.text.Split("\n");
        curLineIndex = 0;
        inDialogue = true;
    }
    public TextAsset GetTextFileByName(string fileName)
    {
        return Resources.Load<TextAsset>("Scenarios/"+fileName);
    }
    public void DoLine()
    {
        if (curLineIndex < lines.Length && inDialogue)
        {
            if (!nowChoice)
            {
                if (curLineIndex > 0)
                {
                    if (textPanelText.text != curTextLine)
                    {
                        StopAllCoroutines();
                        textPanelText.text = curTextLine;
                        CheckVariants();
                    }
                    else
                    {
                        CheckAll();
                        curLineIndex++;
                    }
                }
                else
                {
                    CheckAll();
                    curLineIndex++;
                }
            }
        }
        else
        {
            inDialogue = false;
            ChangeUIVisibility(false);
        }
    }
    private void CheckAll()
    {
        CheckIf();
        CheckNameTag();
        CheckNarrator();
        CheckChar();
        CheckBG();
        CheckCharDisappear();
        CheckMusic();
        CheckSound();
        CheckChangeVars();
        CheckText();
        
    }
    private void CheckNameTag()
    {
        string line = lines[curLineIndex];
        Match match = Regex.Match(line, @"<name=.+?>");
        if (match.Success)
        {
            namePanelText.transform.parent.gameObject.SetActive(true);

            string tag = match.Value;

            tag = tag.Substring(tag.IndexOf("=") + 2, tag.Length - 7);
            string name = tag.Substring(0, tag.IndexOf("\"")); 
            namePanelText.text = name;
        }
    }
    private void CheckNarrator()
    {
        string line = lines[curLineIndex];
        Match match = Regex.Match(line, @"<narrator>");
        
        if (match.Success)
        {
            namePanelText.transform.parent.gameObject.SetActive(false);
        }
    }
    private void CheckChar()
    {
        string line = lines[curLineIndex];
        MatchCollection matches = Regex.Matches(line, @"<char=.*?"">");
        if (matches.Count > 0)
        {
            GameObject newChar;
            string[] tags = matches.Cast<Match>().Select(m => m.Value).ToArray();
            string temp;
            foreach (string tag in tags)
            {
                temp = tag.Substring(tag.IndexOf("=") + 2, tag.Length - 7);
                string charact = temp.Substring(0, temp.IndexOf("\""));

                newChar = FindCharacter(charact);
                if (newChar == null)
                {
                    newChar = Instantiate(Resources.Load<GameObject>("Characters/" + charact), uiCanvas.transform);
                    newChar.transform.SetAsFirstSibling();
                }
                if (temp.IndexOf("pos=") != -1)
                {
                    string str = temp.Substring(temp.IndexOf("pos=") + 5);
                    str = str.Substring(0, str.IndexOf("\""));
                    string[] pos = str.Split(" ");
                    newChar.transform.localPosition = new Vector2(Convert.ToInt64(pos[0]), Convert.ToInt64(pos[1]));
                }

                if (temp.IndexOf("anim=") != -1)
                {
                    temp = temp.Substring(temp.IndexOf("anim=") + 6, temp.Length - (temp.IndexOf("anim=") + 6));
                    string anim = temp.Substring(0, temp.IndexOf("\""));
                    newChar.GetComponent<Animator>().Play(anim);
                }
            }
            
        }
    }
    private void CheckBG()
    {
        string line = lines[curLineIndex];
        Match match = Regex.Match(line, @"<bg=.+?>");
        if (match.Success)
        {
            DestroyAllInteractables();
            string tag = match.Value;

            tag = tag.Substring(tag.IndexOf("=") + 2, tag.Length - 5);
            string bg = tag.Substring(0, tag.IndexOf("\""));

            if (tag.Contains("interactable"))
            {
                MatchCollection matches = Regex.Matches(tag, @"interactable="".*?""");
                string[] interactables = matches.Cast<Match>().Select(m => m.Value).ToArray();
                string temp;
                foreach (string interactable in interactables)
                {
                    temp = interactable.Substring(interactable.IndexOf("\"")+1);
                    
                    temp = temp.Substring(0, temp.IndexOf("\""));
                    Instantiate(Resources.Load<GameObject>("Interactive/" + temp), interactiveParent.transform);
                }
            }

            tag = tag.Substring(tag.IndexOf("anim=") + 6, tag.Length - (tag.IndexOf("anim=") + 6));
            string anim = tag.Substring(0, tag.IndexOf("\""));
            if (anim == "black")
            {
                bgCanvas.GetComponent<BGScript>().BlackTranstion(Resources.Load<Sprite>("BGs/" + bg));
            }
            else if (anim == "transparent")
            {
                bgCanvas.GetComponent<BGScript>().TransparentTransition(Resources.Load<Sprite>("BGs/" + bg));
            }
            else
            {
                bgGO.sprite = Resources.Load<Sprite>("BGs/" + bg);
            }

            tag = tag.Substring(tag.IndexOf(">") + 1, tag.Length - (tag.IndexOf(">") + 1));
        }
    }
    private void DestroyAllInteractables()
    {
        if (interactiveParent.transform.childCount > 0)
        {
            for (int i = 0; i < interactiveParent.transform.childCount; i++)
            {
                Destroy(interactiveParent.transform.GetChild(i).gameObject);
            }
        }
    }
    private void CheckCharDisappear()
    {
        string line = lines[curLineIndex];
        Match match = Regex.Match(line, @"<disappear=.+ anim=.+>");
        if (match.Success)
        {
            GameObject deleteChar;
            string tag = match.Value;

            tag = tag.Substring(tag.IndexOf("=") + 2, tag.Length - 12);
            string charact = tag.Substring(0, tag.IndexOf("\""));
            deleteChar = FindCharacter(charact);

            tag = tag.Substring(tag.IndexOf("anim=") + 6, tag.Length - (tag.IndexOf("anim=") + 6));
            string anim = tag.Substring(0, tag.IndexOf("\""));
            if (anim.Length > 0)
            {
                deleteChar.GetComponent<Animator>().Play(anim);
                StartCoroutine(DestroyAfterAnimation(deleteChar));
            }
            else
            {
                Destroy(deleteChar);
            }
        }
    }
    private GameObject FindCharacter(string charName)
    {
        int countOfChilds = uiCanvas.transform.childCount;
        GameObject child;
        for (int i = 0; i<countOfChilds; i++)
        {
            child = uiCanvas.transform.GetChild(i).gameObject;
            try {
                if (child.name.Substring(0, charName.Length) == charName)
                {
                    return child;
                }
            }
            catch (Exception e)
            {
                continue;
            }
        }
        return null;
    }
    IEnumerator DestroyAfterAnimation(GameObject go)
    {
        float duration = go.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0).Length;
        yield return new WaitForSeconds(duration);
        Destroy(go);
    }
    private void CheckText()
    {
        string line = lines[curLineIndex];
        Match match = Regex.Match(line, @"<line>.+?</line>");
        if (match.Success)
        {
            line = match.Value;
            line = line.Substring(6, line.IndexOf("</line>") - 6);
            StopAllCoroutines();
            textPanelText.text = "";
            if (animatedText)
            {
                StartCoroutine(AnimatedText(line));
            }
            else
            {
                textPanelText.text = line;
            }
            curTextLine = line;
        }
    }
    private void CheckVariants()
    {
        DestroyVariants();
        string line = lines[curLineIndex-1];
        MatchCollection matches = Regex.Matches(line, @"<variant.*?>.*?</variant>");

        if (matches.Count > 0)
        {
            nowChoice = true;

            string[] variants = matches.Cast<Match>().Select(m => m.Value).ToArray();
            string txt;
            foreach (string variant in variants)
            {
                
                txt = variant.Substring(variant.IndexOf(">")+1, variant.IndexOf("</variant>") - variant.IndexOf(">") - 1);
                GameObject variantGO = Instantiate(variantPrefab, variantsHolder.transform);
                variantGO.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = txt;
                if (variant.Substring(0, variant.IndexOf(">")).Contains("next-scenario"))
                {
                    string fileName = variant.Substring(0, variant.IndexOf(">"));
                    fileName = fileName.Substring(fileName.IndexOf("next-scenario=")+14);
                    fileName = fileName.Replace(fileName.Substring(fileName.IndexOf(" ")), "");

                    variantGO.GetComponent<Button>().onClick.AddListener(() => ChangeScenario(fileName));
                }
                variantGO.GetComponent<Button>().onClick.AddListener(() => { nowChoice = false; DoLine(); DestroyVariants(); });
            }
        }
    }
    public void ChangeScenario(string nameOfFile)
    {
        DestroyVariants();
        ReadTextFromFile(GetTextFileByName(nameOfFile));
        ChangeUIVisibility(true);
        
    }
    private void DestroyVariants()
    {
        pointerUnderUI = false;
        if (variantsHolder.transform.childCount > 0)
        {
            for (int i = 0; i < variantsHolder.transform.childCount; i++)
            {
                Destroy(variantsHolder.transform.GetChild(i).gameObject);
            }
        }
    }
    IEnumerator AnimatedText(string text)
    {
        yield return new WaitForSeconds(textDelay);
        textPanelText.text += text[0];
        if (text.Length > 1)
        {
            StartCoroutine(AnimatedText(text.Substring(1, text.Length - 1)));
        }
        else
        {
            CheckVariants();
        }
    }
    private void CheckMusic()
    {
        string line = lines[curLineIndex];
        Match match = Regex.Match(line, @"<music=.+>");
        if (match.Success)
        {
            string tag = match.Value;

            tag = tag.Substring(tag.IndexOf("=") + 2, tag.Length - 8);
            string name = tag.Substring(0, tag.IndexOf("\""));

            musicSource.clip = Resources.Load<AudioClip>("OST/" + name);
            musicSource.Play();
        }
    }
    private void CheckSound()
    {
        string line = lines[curLineIndex];
        MatchCollection matches = Regex.Matches(line, @"<sound=.*?"">");
        if (matches.Count > 0)
        {
            string[] tags = matches.Cast<Match>().Select(m => m.Value).ToArray();
            string temp;
            foreach (string tag in tags)
            {
                temp = tag.Substring(tag.IndexOf("\"")+1);
                temp = temp.Substring(0, temp.IndexOf("\""));
                soundSource.PlayOneShot(Resources.Load<AudioClip>("Sounds/" + temp));
            }
        }
    }
    private void CheckChangeVars()
    {
        string line = lines[curLineIndex];
        MatchCollection matches = Regex.Matches(line, @"<var=.+?>");
        string[] matchesText = matches.Cast<Match>().Select(m => m.Value).ToArray();
        if (matchesText.Length > 0)
        {
            string temp = "";
            string[] varAndChanges = { "", "" };
            foreach (string text in matchesText)
            {
                temp = text.Substring(text.IndexOf("\"") + 1);
                temp = temp.Substring(0, temp.IndexOf("\""));
                varAndChanges = temp.Split(":");
                variablesScript.ChangeVariable(varAndChanges[0], varAndChanges[1]);
            }
        }
    }
    private void CheckIf()
    {
        string line = lines[curLineIndex];
        MatchCollection matches = Regex.Matches(line, @"<if:.+?(?<!')>");
        string[] matchesText = matches.Cast<Match>().Select(m => m.Value).ToArray();
        string temp = "";
        foreach (string text in matchesText)
        {
            temp = text.Substring(text.IndexOf("\"")+1);
            string nextScenarioText = temp.Substring(temp.IndexOf("next-scenario=\"") + "next-scenario=\"".Length);
            nextScenarioText = nextScenarioText.Substring(0, nextScenarioText.IndexOf("\""));
            
            temp = temp.Substring(0, temp.IndexOf("\""));
            string boolText = temp;
            if (StringToBool(boolText))
            {
                ChangeScenario(nextScenarioText);
            }

        }
    }
    private bool StringToBool(string str)
    {
        bool boolValue = false;
        string[] params1 = {"","" };
        string operation = ">";
        if (str.Contains("'>='"))
        {
            params1 = str.Split("'>='");
            operation = ">=";
        }
        else if (str.Contains("'<='"))
        {
            params1 = str.Split("'<='");
            operation = "<=";
        }
        else if (str.Contains("'>'"))
        {
            params1 = str.Split("'>'");
            operation = ">";
        }
        else if (str.Contains("'<'"))
        {
            params1 = str.Split("'<'");
            operation = "<";
        }
        else if (str.Contains("'=='"))
        {
            params1 = str.Split("'=='");
            operation = "==";
        }
        int var1 = 0;
        int var2 = 0;
        Match match = Regex.Match(params1[0], @"\p{L}+");
        if (match.Success)
        {
            var1 = Convert.ToInt32(variablesScript.GetVariable(params1[0]));
        }
        else { var1 = Convert.ToInt32(params1[0]); }
        match = Regex.Match(params1[1], @"\p{L}+");
        if (match.Success)
        {
            var2 = Convert.ToInt32(variablesScript.GetVariable(params1[1]));
        }
        else { var2 = Convert.ToInt32(params1[1]); }
        
        if (operation == ">=")
        {
            boolValue = var1 >= var2;
        }
        else if (operation == "<=")
        {
            boolValue = var1 <= var2;
        }
        else if (operation == ">")
        {
            boolValue = var1 > var2;
        }
        else if (operation == "<")
        {
            boolValue = var1 < var2;
        }
        else if (operation == "==")
        {
            boolValue = var1 == var2;
        }
        return boolValue;
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !pointerUnderUI && !uIhidden)
        {
            DoLine();
        }
        else if (Input.GetMouseButtonDown(0) && uIhidden && inDialogue)
        {
            ChangeUIVisibility(true);
        }
    }
    public void ChangeUIVisibility(bool state)
    {
        parentUIGO.SetActive(state);
        uIhidden = !state;
        if (!state) { pointerUnderUI = false; }
    }
}
