#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Search;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;

public class TextFilesEditor : EditorWindow
{
    [SerializeField] private VisualTreeAsset m_VisualTreeAsset;
    private TextAsset currentTextFile;

    [MenuItem("Window/Scenario Editor")]
    public static void ShowExample()
    {
        var wnd = GetWindow<TextFilesEditor>();
        wnd.titleContent = new GUIContent("ScenarioEditor");
    }

    public void CreateGUI()
    {
        if (m_VisualTreeAsset == null)
        {
            Debug.LogError("VisualTreeAsset не назначен! Пожалуйста, укажите UXML файл в инспекторе.");
            return;
        }

        VisualElement root = rootVisualElement;
        VisualTreeAsset asset = m_VisualTreeAsset;
        ObjectField textFileField = new();
        asset.CloneTree(root);

        VisualElement selector = root.Q<VisualElement>("ScenarioSelector");
        textFileField.objectType = typeof(TextAsset);
        textFileField.style.height = 40;
        selector.Add(textFileField);

        ScrollView scrollView = root.Q<ScrollView>("lines");
        if (scrollView == null)
        {
            Debug.LogError("ScrollView с именем 'LinesScrollView' не найден в UXML.");
            return;
        }

        textFileField.RegisterValueChangedCallback(evt =>
        {
            currentTextFile = evt.newValue as TextAsset;
            RefreshScrollView(scrollView);
        });
    }

    private void RefreshScrollView(ScrollView scrollView)
    {
        scrollView.Clear();

        if (currentTextFile == null)
        {
            var infoLabel = new Label("Выберите текстовый файл");
            infoLabel.style.color = Color.gray;
            scrollView.Add(infoLabel);
            return;
        }

        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        if (lines.Length == 0 || (lines.Length == 1 && string.IsNullOrEmpty(lines[0])))
        {
            var infoLabel = new Label("Файл пуст");
            infoLabel.style.color = Color.gray;
            return;
        }

        for (int i = 0; i < lines.Length; i++)
        {
            var container = new VisualElement();
            container.AddToClassList("main-container-line");

            var delButton = new Button();
            delButton.text = "❌";
            delButton.AddToClassList("del-button");
            container.Add(delButton);
            var index = i;
            delButton.RegisterCallback<ClickEvent>(evt => DeleteLine(index, container));

            CheckAllTags(container, lines[i], i);

            scrollView.Add(container);
        }
    }
    private void CheckAllTags(VisualElement visualContainer, string curLine, int lineIndex)
    {
        CheckName(visualContainer, curLine, lineIndex);
        CheckMusic(visualContainer, curLine, lineIndex);
        CheckSound(visualContainer, curLine, lineIndex);
        CheckChar(visualContainer, curLine, lineIndex);
        CheckText(visualContainer, curLine, lineIndex);
        CheckVariants(visualContainer, curLine);
    }
    private void CheckMusic(VisualElement visualContainer, string curLine, int lineIndex)
    {
        Match match = Regex.Match(curLine, @"<music=.+?>");
        if (match.Success)
        {
            var container = new VisualElement();
            container.style.marginTop = 2.5f;
            container.style.marginBottom = 2.5f;
            container.style.paddingRight = 5f;
            container.style.paddingBottom = 6f;
            container.style.paddingTop = 6f;
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignContent = Align.Center;
            container.style.backgroundColor = new Color(140f / 255f, 140f / 255f, 255f / 255f);
            container.style.width = 450;

            var delButton = new Button();
            delButton.AddToClassList("del-button");
            delButton.text = "❌";
            container.Add(delButton);
            delButton.RegisterCallback<ClickEvent>(evt => DelMusicField(lineIndex, container));

            string musicName = match.Value.Substring(match.Value.IndexOf("\"") + 1);
            musicName = musicName.Substring(0, musicName.IndexOf("\""));

            var musicField = new ObjectField();
            musicField.style.width = 115;
            musicField.style.alignSelf = Align.Center;
            musicField.value = Resources.Load<AudioClip>("OST/" + musicName);
            container.Add(musicField);
            musicField.RegisterValueChangedCallback(evt => ChangeMusicField(lineIndex, evt.newValue.name));

            visualContainer.Add(container);
        }
    }
    private void CheckSound(VisualElement visualContainer, string curLine, int lineIndex)
    {
        Match match = Regex.Match(curLine, @"<sound=.+?>");
        if (match.Success)
        {
            var container = new VisualElement();
            container.style.marginTop = 2.5f;
            container.style.marginBottom = 2.5f;
            container.style.paddingRight = 5f;
            container.style.paddingBottom = 6f;
            container.style.paddingTop = 6f;
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignContent = Align.Center;
            container.style.backgroundColor = new Color(234f / 255f, 255f / 255f, 128f / 255f);
            container.style.width = 450;

            var delButton = new Button();
            delButton.AddToClassList("del-button");
            delButton.text = "❌";
            container.Add(delButton);
            delButton.RegisterCallback<ClickEvent>(evt => DelSoundField(lineIndex, container));

            string musicName = match.Value.Substring(match.Value.IndexOf("\"") + 1);
            musicName = musicName.Substring(0, musicName.IndexOf("\""));

            var musicField = new ObjectField();
            musicField.style.width = 115;
            musicField.style.alignSelf = Align.Center;
            musicField.value = Resources.Load<AudioClip>("sounds/" + musicName);
            container.Add(musicField);
            musicField.RegisterValueChangedCallback(evt => ChangeSoundField(lineIndex, evt.newValue.name));

            visualContainer.Add(container);
        }
    }
    private void CheckChar(VisualElement visualContainer, string curLine, int lineIndex)
    {
        Match match = Regex.Match(curLine, @"<char=.+?>");
        if (match.Success)
        {
            var container = new VisualElement();
            container.style.marginTop = 2.5f;
            container.style.marginBottom = 2.5f;
            container.style.paddingRight = 5f;
            container.style.paddingBottom = 6f;
            container.style.paddingTop = 6f;
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignContent = Align.Center;
            container.style.backgroundColor = new Color(199f/255f, 32f/255f, 133f/255f);
            container.style.width = 450;

            var delButton = new Button();
            delButton.AddToClassList("del-button");
            delButton.text = "❌";
            container.Add(delButton);

            string name = match.Value.Substring(match.Value.IndexOf("\"")+1);
            name = name.Substring(0, name.IndexOf("\""));

            string anim = match.Value.Substring(match.Value.IndexOf("anim=\"") + 6);
            anim = anim.Substring(0, anim.IndexOf("\""));

            var character = new ObjectField();
            character.style.width = 115;
            character.style.alignSelf = Align.Center;
            character.value = Resources.Load<GameObject>("Characters/" + name);
            container.Add(character);
            character.RegisterValueChangedCallback(evt => ChangeCharField(lineIndex, evt.newValue.name));

            var animLabel = new Label();
            animLabel.text = "anim";
            animLabel.style.fontSize = 12;
            animLabel.style.alignSelf = Align.Center;
            container.Add(animLabel);

            var animValue = new ObjectField();
            animValue.style.width = 115;
            animValue.value = Resources.Load<GameObject>("Animation/" + anim);
            animValue.style.alignSelf = Align.Center;
            container.Add(animValue);

            var xLabel = new Label();
            xLabel.text = "x";
            xLabel.style.fontSize = 12;
            xLabel.style.alignSelf = Align.Center;
            container.Add(xLabel);

            var xValue = new FloatField();
            xValue.style.width = 50;
            xValue.style.alignSelf = Align.Center;
            container.Add(xValue);

            var yLabel = new Label();
            yLabel.text = "y";
            yLabel.style.fontSize = 12;
            yLabel.style.alignSelf = Align.Center;
            container.Add(yLabel);

            var yValue = new FloatField();
            yValue.style.width = 50;
            yValue.style.alignSelf = Align.Center;
            container.Add(yValue);

            visualContainer.Add(container);
        }
    }
    private void CheckName(VisualElement visualContainer, string curLine, int lineIndex)
    {
        Match match = Regex.Match(curLine, @"<name=.+?>");
        if (match.Success)
        {
            var container = new VisualElement();
            container.AddToClassList("name-container");

            var delButton = new Button();
            delButton.AddToClassList("del-button");
            delButton.text = "❌";
            container.Add(delButton);
            delButton.RegisterCallback<ClickEvent>(evt => DelNameField(lineIndex, container));

            var textField = new TextField();
            textField.AddToClassList("text-field");
            textField.value = match.ToString().Substring(match.ToString().IndexOf("=") + 2, match.ToString().Length - 9);
            container.Add(textField);
            visualContainer.Add(container);
            textField.RegisterValueChangedCallback(evt => ChangeNameField(lineIndex, evt.newValue));
        }
    }
    public void ChangeNameField(int index, string newName)
    {
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string editingLine = lines[index];
        string firstPart = editingLine.Substring(0, editingLine.IndexOf("<name=\"") + 7);
        editingLine = editingLine.Substring(editingLine.IndexOf("\"")+1);
        editingLine = editingLine.Substring(editingLine.IndexOf("\""));
        lines[index] = firstPart+newName+editingLine;


        RewriteTextFile(lines);
    }
    public void DelNameField(int index, VisualElement element)
    {
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string editingLine = lines[index];
        string firstPart = editingLine.Substring(0, editingLine.IndexOf("<name=\""));
        editingLine = editingLine.Substring(editingLine.IndexOf("<name=\"") + 7);
        editingLine = editingLine.Substring(editingLine.IndexOf("\">") + 2);
        lines[index] = firstPart + editingLine;

        element.RemoveFromHierarchy();

        RewriteTextFile(lines);
    }
    public void RewriteTextFile(string[] linesArray)
    {
        string path = AssetDatabase.GetAssetPath(currentTextFile);
        File.WriteAllText(path, "");
        using (StreamWriter writer = new StreamWriter(path))
        {
            foreach (string line in linesArray)
            {
                writer.WriteLine(line);
            }
        }
        AssetDatabase.Refresh();
    }
    public void ChangeLineField(int index, string newName)
    {
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string editingLine = lines[index];
        string firstPart = editingLine.Substring(0, editingLine.IndexOf("<line>") + 6);
        editingLine = editingLine.Substring(editingLine.IndexOf("</line>"));
        lines[index] = firstPart + newName + editingLine;

        RewriteTextFile(lines);
    }
    /// переделать изменение персонажа, ведь их может быть много, а изменяется первый попавшийся. И добавить смену анимации и позиции
    public void ChangeCharField(int index, string newName)
    {
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string editingLine = lines[index];
        string firstPart = editingLine.Substring(0, editingLine.IndexOf("<char=\"") + 7);
        editingLine = editingLine.Substring(editingLine.IndexOf("<char=\"") + 7);
        editingLine = editingLine.Substring(editingLine.IndexOf("\""));
        lines[index] = firstPart + newName + editingLine;

        RewriteTextFile(lines);
    }
    public void ChangeMusicField(int index, string newName)
    {
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string editingLine = lines[index];
        string firstPart = editingLine.Substring(0, editingLine.IndexOf("<music=\"") + 8);
        editingLine = editingLine.Substring(editingLine.IndexOf("<music=\"") + 8);
        editingLine = editingLine.Substring(editingLine.IndexOf("\""));
        lines[index] = firstPart + newName + editingLine;

        RewriteTextFile(lines);
    }
    public void DelMusicField(int index, VisualElement element)
    {
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string editingLine = lines[index];
        string firstPart = editingLine.Substring(0, editingLine.IndexOf("<music=\""));
        editingLine = editingLine.Substring(editingLine.IndexOf("<music=\"") + 8);
        editingLine = editingLine.Substring(editingLine.IndexOf("\">") + 2);
        lines[index] = firstPart + editingLine;

        element.RemoveFromHierarchy();

        RewriteTextFile(lines);
    }
    public void ChangeSoundField(int index, string newName)
    {
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string editingLine = lines[index];
        string firstPart = editingLine.Substring(0, editingLine.IndexOf("<sound=\"") + 8);
        editingLine = editingLine.Substring(editingLine.IndexOf("<sound=\"") + 8);
        editingLine = editingLine.Substring(editingLine.IndexOf("\""));
        lines[index] = firstPart + newName + editingLine;

        RewriteTextFile(lines);
    }
    public void DelSoundField(int index, VisualElement element)
    {
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string editingLine = lines[index];
        string firstPart = editingLine.Substring(0, editingLine.IndexOf("<sound=\""));
        editingLine = editingLine.Substring(editingLine.IndexOf("<sound=\"") + 8);
        editingLine = editingLine.Substring(editingLine.IndexOf("\">") + 2);
        lines[index] = firstPart + editingLine;

        element.RemoveFromHierarchy();

        RewriteTextFile(lines);
    }
    public void DeleteLine(int index, VisualElement element)
    {
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        List<string> linesList = lines.ToList();
        linesList.RemoveAt(index);
        lines = linesList.ToArray();
        element.RemoveFromHierarchy();

        RewriteTextFile(lines); 
    }
    private void CheckText(VisualElement visualContainer, string curLine, int lineIndex)
    {
        Match match = Regex.Match(curLine, @"<line>.+?</line>");
        if (match.Success)
        {
            string str = match.Value.Substring(6, match.Value.IndexOf("</line>") - 6); 
            var container = new VisualElement();
            container.AddToClassList("text-container");

            var textField = new TextField();
            textField.value = str;
            visualContainer.Add(container);
            container.Add(textField);
            textField.RegisterValueChangedCallback(evt => ChangeLineField(lineIndex, evt.newValue));
        }
    }
    private void CheckVariants(VisualElement visualContainer, string curLine)
    {
        MatchCollection matches = Regex.Matches(curLine, @"<variant.*?>.*?</variant>");
        if (matches.Count > 0)
        {
            string[] variants = matches.Cast<Match>().Select(m => m.Value).ToArray();

            foreach ( var variant in variants )
            {
                var container = new VisualElement();
                container.style.marginTop = 2.5f;
                container.style.marginBottom = 2.5f;
                container.style.paddingRight = 5f;
                container.style.paddingBottom = 6f;
                container.style.paddingTop = 6f;
                container.style.flexDirection = FlexDirection.Row;
                container.style.alignContent = Align.Center;
                container.style.backgroundColor = new Color(255f / 255f, 140f / 255f, 140f / 255f);
                container.style.width = 450;

                var delButton = new Button();
                delButton.AddToClassList("del-button");
                delButton.text = "❌";
                container.Add(delButton);

                string variantText = variant.ToString().Substring(variant.ToString().IndexOf(">")+1);
                variantText = variantText.Substring(0, variantText.IndexOf("<"));
                var variantField = new TextField();
                variantField.style.width = 115;
                variantField.style.alignSelf = Align.Center;
                variantField.value = variantText;
                container.Add(variantField);
                
                var nextScenarioField = new ObjectField();
                nextScenarioField.style.width = 115;
                nextScenarioField.style.alignSelf = Align.Center;
                if (variant.ToString().Contains("next-scenario"))
                {
                    string nextScenario = variant.ToString().Substring(variant.ToString().IndexOf("\"")+1);
                    nextScenario = nextScenario.Substring(0, nextScenario.IndexOf("\""));
                    Debug.Log(nextScenario);
                    nextScenarioField.value = Resources.Load<TextAsset>("Scenarios/" + nextScenario);
                }
                container.Add(nextScenarioField);
                visualContainer.Add(container);
            }
        }
    }
}
#endif