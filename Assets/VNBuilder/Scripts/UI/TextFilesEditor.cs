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
using System.Text;

public class TextFilesEditor : EditorWindow
{
    [SerializeField] private VisualTreeAsset m_VisualTreeAsset;
    private TextAsset currentTextFile;

    private VisualElement selectedLine;

    private ScrollView scrollView;

    private List<VisualElement> linesContainers;

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

        Button newLineButton = root.Q<Button>("AddLine");
        newLineButton.RegisterCallback<ClickEvent>(evt => InsertNewLine());
        Button addNameButton = root.Q<Button>("SelectName");
        addNameButton.RegisterCallback<ClickEvent>(evt => AddNewName());
        Button addBGButton = root.Q<Button>("ChangeBG");
        addBGButton.RegisterCallback<ClickEvent>(evt => AddNewBG());
        Button addMusicButton = root.Q<Button>("ChangeOST");
        addMusicButton.RegisterCallback<ClickEvent>(evt => AddNewMusic());
        Button addSoundButton = root.Q<Button>("PlaySound");
        addSoundButton.RegisterCallback<ClickEvent>(evt => AddNewSound());
        Button addCharButton = root.Q<Button>("Character");
        addCharButton.RegisterCallback<ClickEvent>(evt => AddNewChar());
        Button addNarratorButton = root.Q<Button>("SelectNarrator");
        addNarratorButton.RegisterCallback<ClickEvent>(evt => AddNewNarrator());
        Button addVariantButton = root.Q<Button>("ShowVariants");
        addVariantButton.RegisterCallback<ClickEvent>(evt => AddNewVariant());

        scrollView = root.Q<ScrollView>("lines");
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
        linesContainers = new List<VisualElement>();
        for (int i = 0; i < lines.Length; i++)
        {
            var container = new VisualElement();
            container.AddToClassList("main-container-line");
            container.RegisterCallback<ClickEvent>(evt => SelectLine(container));

            var delButton = new Button();
            delButton.text = "❌";
            delButton.AddToClassList("del-button");
            container.Add(delButton);
            var index = i;
            delButton.RegisterCallback<ClickEvent>(evt => DeleteLine(container));

            CheckAllTags(container, lines[i], i);

            scrollView.Add(container);

            linesContainers.Add(container);
        }
    }
    private void SelectLine(VisualElement elem)
    {
        if (selectedLine != null)
        {
            selectedLine.style.backgroundColor = new Color(230f / 255f, 230f / 255f, 230f / 255f);
        }
        selectedLine = elem;
        elem.style.backgroundColor = Color.white;
    }
    private void InsertNewLine()
    {
        var container = new VisualElement();
        container.AddToClassList("main-container-line");
        container.RegisterCallback<ClickEvent>(evt => SelectLine(container));

        var delButton = new Button();
        delButton.text = "❌";
        delButton.AddToClassList("del-button");
        container.Add(delButton);
        delButton.RegisterCallback<ClickEvent>(evt => DeleteLine(container));

        var textContainer = new VisualElement();
        textContainer.AddToClassList("text-container");

        var textField = new TextField();
        
        container.Add(textContainer);
        textContainer.Add(textField);
        textField.RegisterValueChangedCallback(evt => ChangeLineField(container, evt.newValue));

        if (selectedLine != null)
        {
            int indexOfLine = scrollView.IndexOf(selectedLine);
            scrollView.Insert(indexOfLine+1, container);
            linesContainers.Insert(indexOfLine+1, container);

            string text = currentTextFile.text;
            string[] linesArray = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            List<string> linesList = linesArray.ToList();
            linesList.Insert(indexOfLine + 1, "<line></line>");

            string path = AssetDatabase.GetAssetPath(currentTextFile);
            File.WriteAllText(path, "");
            using (StreamWriter writer = new StreamWriter(path))
            {
                foreach (string line in linesList)
                {
                    writer.WriteLine(line);
                }
            }
            AssetDatabase.Refresh();
        }
        else
        {
            scrollView.Insert(scrollView.childCount, container);
            linesContainers.Insert(linesContainers.Count, container);

            string text = currentTextFile.text;
            string[] linesArray = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            List<string> linesList = linesArray.ToList();
            linesList.Insert(linesArray.Length, "<line></line>");

            string path = AssetDatabase.GetAssetPath(currentTextFile);
            File.WriteAllText(path, "");
            using (StreamWriter writer = new StreamWriter(path))
            {
                foreach (string line in linesList)
                {
                    writer.WriteLine(line);
                }
            }
            AssetDatabase.Refresh();
        }
        SelectLine(container);
    }
    private void AddNewName()
    {
        int index = linesContainers.IndexOf(selectedLine);
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        if (!lines[index].Contains("<name"))
        {
            if (lines[index].Contains("<narrator>"))
            {
                DelNarratorField(linesContainers[index].Query(className: "narrator-container").ToList()[0]);
            }

            AddTagInText("<name=\"\">", index);

            var nameContainer = new VisualElement();
            nameContainer.AddToClassList("name-container");

            var delButton = new Button();
            delButton.AddToClassList("del-button");
            delButton.text = "❌";
            nameContainer.Add(delButton);
            delButton.RegisterCallback<ClickEvent>(evt => DelNameField(nameContainer));
            var textField = new TextField();
            textField.AddToClassList("text-field");
            textField.value = "";
            nameContainer.Add(textField);
            selectedLine.Insert(1, nameContainer);
            textField.RegisterValueChangedCallback(evt => ChangeNameField(linesContainers.IndexOf(selectedLine), evt.newValue));
        }
    }
    private void AddNewNarrator()
    {
        int index = linesContainers.IndexOf(selectedLine);
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        if (!lines[index].Contains("<narrator>"))
        {
            if (lines[index].Contains("<name=\""))
            {
                DelNameField(linesContainers[index].Query(className: "name-container").ToList()[0]);
            }

            AddTagInText("<narrator>", index);

            var container = new VisualElement();
            container.AddToClassList("narrator-container");

            var delButton = new Button();
            delButton.AddToClassList("del-button");
            delButton.text = "❌";
            container.Add(delButton);
            delButton.RegisterCallback<ClickEvent>(evt => DelNarratorField(container));
            var label = new Label();
            label.text = "narrator";
            label.style.fontSize = 12;
            label.style.alignSelf = Align.Center;
            container.Add(label);

            selectedLine.Insert(1, container);
        }
    }
    private void AddNewMusic()
    {
        int index = linesContainers.IndexOf(selectedLine);
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        if (!lines[index].Contains("<music"))
        {
            AddTagInText("<music=\"\">", index);

            var container = new VisualElement();
            container.AddToClassList("music-container");

            var delButton = new Button();
            delButton.AddToClassList("del-button");
            delButton.text = "❌";
            container.Add(delButton);
            delButton.RegisterCallback<ClickEvent>(evt => DelMusicField(container));

            var musicField = new ObjectField();
            musicField.objectType = typeof(AudioClip);
            musicField.style.width = 115;
            musicField.style.alignSelf = Align.Center;
            container.Add(musicField);
            musicField.RegisterValueChangedCallback(evt => ChangeMusicField(linesContainers.IndexOf(selectedLine), evt.newValue.name));

            selectedLine.Insert(1, container);
        }
    }
    private void AddNewSound()
    {
        int index = linesContainers.IndexOf(selectedLine);
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        if (!lines[index].Contains("<sound"))
        {
            AddTagInText("<sound=\"\">", index);

            var container = new VisualElement();
            container.AddToClassList("sound-container");

            var delButton = new Button();
            delButton.AddToClassList("del-button");
            delButton.text = "❌";
            container.Add(delButton);
            delButton.RegisterCallback<ClickEvent>(evt => DelSoundField(container));

            var musicField = new ObjectField();
            musicField.objectType = typeof(AudioClip);
            musicField.style.width = 115;
            musicField.style.alignSelf = Align.Center;
            container.Add(musicField);
            musicField.RegisterValueChangedCallback(evt => ChangeSoundField(linesContainers.IndexOf(selectedLine), evt.newValue.name));

            selectedLine.Insert(1, container);
        }
    }
    private void AddNewChar()
    {
        int index = linesContainers.IndexOf(selectedLine);
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        //if (!lines[index].Contains("<"))
        //{
            AddTagInText("<char=\"\", anim=\"\", x=\"0\", y=\"0\">", index);

            var container = new VisualElement();
            container.AddToClassList("char-container");

            var delButton = new Button();
            delButton.AddToClassList("del-button");
            delButton.text = "❌";
            container.Add(delButton);

            var character = new ObjectField();
            character.objectType = typeof(GameObject);
            character.style.width = 115;
            character.style.alignSelf = Align.Center;
            container.Add(character);
            EventCallback<ChangeEvent<UnityEngine.Object>> eventCallback = evt => { };
            eventCallback = evt => ChangeCharField(linesContainers.IndexOf(selectedLine), evt.newValue.name, "", character, eventCallback);
            character.RegisterValueChangedCallback(eventCallback);

            delButton.RegisterCallback<ClickEvent>(evt => DelCharField(container, character.value.name));

            var animLabel = new Label();
            animLabel.text = "anim";
            animLabel.style.fontSize = 12;
            animLabel.style.alignSelf = Align.Center;
            container.Add(animLabel);

            var animValue = new ObjectField();
            animValue.objectType = typeof(AnimationClip);
            animValue.style.width = 115;
            animValue.style.alignSelf = Align.Center;
            animValue.RegisterValueChangedCallback(evt => ChangeCharAnimField(linesContainers.IndexOf(selectedLine), character.value.name, evt.newValue.name));
            container.Add(animValue);

            var xLabel = new Label();
            xLabel.text = "x";
            xLabel.style.fontSize = 12;
            xLabel.style.alignSelf = Align.Center;
            container.Add(xLabel);

            var xValue = new FloatField();
            xValue.style.width = 50;
            xValue.style.alignSelf = Align.Center;
            xValue.RegisterValueChangedCallback(evt => ChangeCharX(linesContainers.IndexOf(selectedLine), character.value.name, evt.newValue.ToString()));
            container.Add(xValue);

            var yLabel = new Label();
            yLabel.text = "y";
            yLabel.style.fontSize = 12;
            yLabel.style.alignSelf = Align.Center;
            container.Add(yLabel);

            var yValue = new FloatField();
            yValue.style.width = 50;
            yValue.style.alignSelf = Align.Center;
            yValue.RegisterValueChangedCallback(evt => ChangeCharY(linesContainers.IndexOf(selectedLine), character.value.name, evt.newValue.ToString()));
            container.Add(yValue);

            selectedLine.Insert(1, container);
        //}
    }
    private void AddNewVariant()
    {
        int index = linesContainers.IndexOf(selectedLine);
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        //if (!lines[index].Contains("<music"))
        //{
            AddTagInText("<variant, next-scenario=\"\"></variant>", index);

            var container = new VisualElement();
            container.AddToClassList("variant-container");

            var delButton = new Button();
            delButton.AddToClassList("del-button");
            delButton.text = "❌";
            container.Add(delButton);

        var variant = new ObjectField();
        variant.objectType = typeof(TextAsset);
        variant.style.width = 115;
        variant.style.alignSelf = Align.Center;
        container.Add(variant);
        

        delButton.RegisterCallback<ClickEvent>(evt => DelVariantField(container, variant.value.name));

            var textLabel = new Label();
            textLabel.text = "text";
            textLabel.style.fontSize = 12;
            textLabel.style.alignSelf = Align.Center;
            container.Add(textLabel);

            var textValue = new TextField();
        textValue.style.width = 115;
        textValue.style.alignSelf = Align.Center;
        EventCallback<ChangeEvent<string>> eventCallback2 = evt => { };
        eventCallback2 = evt => ChangeVariantTextField(linesContainers.IndexOf(selectedLine), evt.newValue, "", textValue, eventCallback2);
        textValue.RegisterValueChangedCallback(eventCallback2);
            container.Add(textValue);

        EventCallback<ChangeEvent<UnityEngine.Object>> eventCallback = evt => { };
        eventCallback = evt => ChangeVariantField(linesContainers.IndexOf(selectedLine), evt.newValue.name, textValue.value);
        variant.RegisterValueChangedCallback(eventCallback);

        selectedLine.Insert(1, container);
        //}
    }
    private void AddNewBG()
    {
        int index = linesContainers.IndexOf(selectedLine);
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        if (!lines[index].Contains("<bg"))
        {
            AddTagInText("<bg=\"\", anim=\"\">", index);

            var bgContainer = new VisualElement();
            bgContainer.AddToClassList("bg-container");

            var delButton = new Button();
            delButton.AddToClassList("del-button");
            delButton.text = "❌";
            bgContainer.Add(delButton);
            delButton.RegisterCallback<ClickEvent>(evt => DelBGField( linesContainers[index], bgContainer));

            var bgField = new ObjectField();
            bgField.objectType = typeof(Texture2D);
            bgField.style.width = 115;
            bgField.style.alignSelf = Align.Center;
            bgContainer.Add(bgField);
            bgField.RegisterValueChangedCallback(evt => ChangeBGField(linesContainers[index], evt.newValue.name));

            var animLabel = new Label();
            animLabel.text = "transition";
            animLabel.style.fontSize = 12;
            animLabel.style.alignSelf = Align.Center;
            bgContainer.Add(animLabel);

            var options = new List<string> { "none", "transparent", "black" };
            DropdownField animField = new DropdownField("transition", new List<string> { "none", "transparent", "black" }, 0);
            animField.labelElement.RemoveFromHierarchy();
            animField.RegisterValueChangedCallback(evt => ChangeBGAnimField(linesContainers[index], evt.newValue));
            bgContainer.Add(animField);
            
            selectedLine.Insert(1, bgContainer);
        }
    }

    private void AddTagInText(string tag, int indexLine)
    {
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        lines[indexLine] = tag + lines[indexLine];

        RewriteTextFile(lines);
    }

    private void CheckAllTags(VisualElement visualContainer, string curLine, int lineIndex)
    {
        CheckName(visualContainer, curLine);
        CheckNarrator(visualContainer, curLine);
        CheckMusic(visualContainer, curLine);
        CheckSound(visualContainer, curLine);
        CheckChar(visualContainer, curLine);
        CheckBG(visualContainer, curLine);
        CheckText(visualContainer, curLine);
        CheckVariants(visualContainer, curLine);
    }

    private void CheckBG(VisualElement visualContainer, string curLine)
    {
        Match match = Regex.Match(curLine, @"<bg=.+?>");
        if (match.Success)
        {
            var container = new VisualElement();
            container.AddToClassList("bg-container");

            var delButton = new Button();
            delButton.AddToClassList("del-button");
            delButton.text = "❌";
            container.Add(delButton);
            delButton.RegisterCallback<ClickEvent>(evt => DelBGField(visualContainer, container));

            string bgName = match.Value.Substring(match.Value.IndexOf("\"") + 1);
            bgName = bgName.Substring(0, bgName.IndexOf("\""));

            var bgField = new ObjectField();
            bgField.objectType = typeof(Texture2D);
            bgField.style.width = 115;
            bgField.style.alignSelf = Align.Center;
            bgField.value = Resources.Load<Texture2D>("BGs/" + bgName);
            container.Add(bgField);
            bgField.RegisterValueChangedCallback(evt => ChangeBGField(visualContainer, evt.newValue.name));

            var animLabel = new Label();
            animLabel.text = "transition";
            animLabel.style.fontSize = 12;
            animLabel.style.alignSelf = Align.Center;
            container.Add(animLabel);

            var animValue = match.Value.Substring(match.Value.IndexOf("anim=\"")+6);
            animValue = animValue.Substring(0, animValue.IndexOf("\""));

            var options = new List<string> { "none", "transparent", "black" };
            DropdownField animField = new DropdownField("transition", new List<string> { "none", "transparent", "black" }, 0);
            animField.labelElement.RemoveFromHierarchy();
            animField.value = animValue;
            animField.RegisterValueChangedCallback(evt => ChangeBGAnimField(visualContainer, evt.newValue));
            container.Add(animField);

            visualContainer.Add(container);
        }
    }
    private void CheckMusic(VisualElement visualContainer, string curLine)
    {
        Match match = Regex.Match(curLine, @"<music=.+?>");
        if (match.Success)
        {
            var container = new VisualElement();
            container.AddToClassList("music-container");

            var delButton = new Button();
            delButton.AddToClassList("del-button");
            delButton.text = "❌";
            container.Add(delButton);
            delButton.RegisterCallback<ClickEvent>(evt => DelMusicField(container));

            string musicName = match.Value.Substring(match.Value.IndexOf("\"") + 1);
            musicName = musicName.Substring(0, musicName.IndexOf("\""));

            var musicField = new ObjectField();
            musicField.objectType = typeof(AudioClip);
            musicField.style.width = 115;
            musicField.style.alignSelf = Align.Center;
            musicField.value = Resources.Load<AudioClip>("OST/" + musicName);
            container.Add(musicField);
            musicField.RegisterValueChangedCallback(evt => ChangeMusicField(linesContainers.IndexOf(visualContainer), evt.newValue.name));

            visualContainer.Add(container);
        }
    }
    private void CheckSound(VisualElement visualContainer, string curLine)
    {
        Match match = Regex.Match(curLine, @"<sound=.+?>");
        if (match.Success)
        {
            var container = new VisualElement();
            container.AddToClassList("sound-container");

            var delButton = new Button();
            delButton.AddToClassList("del-button");
            delButton.text = "❌";
            container.Add(delButton);
            delButton.RegisterCallback<ClickEvent>(evt => DelSoundField(container));

            string musicName = match.Value.Substring(match.Value.IndexOf("\"") + 1);
            musicName = musicName.Substring(0, musicName.IndexOf("\""));

            var musicField = new ObjectField();
            musicField.objectType = typeof(AudioClip);
            musicField.style.width = 115;
            musicField.style.alignSelf = Align.Center;
            musicField.value = Resources.Load<AudioClip>("sounds/" + musicName);
            container.Add(musicField);
            musicField.RegisterValueChangedCallback(evt => ChangeSoundField(linesContainers.IndexOf(visualContainer), evt.newValue.name));

            visualContainer.Add(container);
        }
    }
    private void CheckChar(VisualElement visualContainer, string curLine)
    {
        MatchCollection matches = Regex.Matches(curLine, @"<char=.+?>");
        if (matches.Count > 0)
        {
            string[] matchStrings = matches.Cast<Match>().Select(m => m.Value).ToArray();
            foreach (string match in matchStrings)
            {
                var container = new VisualElement();
                container.AddToClassList("char-container");

                var delButton = new Button();
                delButton.AddToClassList("del-button");
                delButton.text = "❌";
                
                container.Add(delButton);

                string name = match.Substring(match.IndexOf("\"") + 1);
                name = name.Substring(0, name.IndexOf("\""));

                string anim = match.Substring(match.IndexOf("anim=\"") + 6);
                anim = anim.Substring(0, anim.IndexOf("\""));

                var character = new ObjectField();
                character.objectType = typeof(GameObject);
                character.style.width = 115;
                character.style.alignSelf = Align.Center;
                character.value = Resources.Load<GameObject>("Characters/" + name);
                container.Add(character);
                EventCallback<ChangeEvent<UnityEngine.Object>> eventCallback = evt => { };
                eventCallback = evt => ChangeCharField(linesContainers.IndexOf(visualContainer), evt.newValue.name, name, character, eventCallback);
                character.RegisterValueChangedCallback(eventCallback);

                delButton.RegisterCallback<ClickEvent>(evt => DelCharField(container, character.value.name));

                var animLabel = new Label();
                animLabel.text = "anim";
                animLabel.style.fontSize = 12;
                animLabel.style.alignSelf = Align.Center;
                container.Add(animLabel);

                var animValue = new ObjectField();
                animValue.objectType = typeof(AnimationClip);
                animValue.style.width = 115;
                animValue.value = Resources.Load<AnimationClip>("Animations/" + anim);
                animValue.style.alignSelf = Align.Center;
                animValue.RegisterValueChangedCallback(evt => ChangeCharAnimField(linesContainers.IndexOf(visualContainer), character.value.name, evt.newValue.name));
                container.Add(animValue);

                var xLabel = new Label();
                xLabel.text = "x";
                xLabel.style.fontSize = 12;
                xLabel.style.alignSelf = Align.Center;
                container.Add(xLabel);

                var xValue = new FloatField();
                xValue.style.width = 50;
                xValue.style.alignSelf = Align.Center;
                xValue.RegisterValueChangedCallback(evt => ChangeCharX(linesContainers.IndexOf(visualContainer), character.value.name, evt.newValue.ToString()));
                container.Add(xValue);

                var yLabel = new Label();
                yLabel.text = "y";
                yLabel.style.fontSize = 12;
                yLabel.style.alignSelf = Align.Center;
                container.Add(yLabel);

                var yValue = new FloatField();
                yValue.style.width = 50;
                yValue.style.alignSelf = Align.Center;
                yValue.RegisterValueChangedCallback(evt => ChangeCharY(linesContainers.IndexOf(visualContainer), character.value.name, evt.newValue.ToString()));
                container.Add(yValue);

                visualContainer.Add(container);
            }
        }
    }
    private void CheckName(VisualElement visualContainer, string curLine)
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
            delButton.RegisterCallback<ClickEvent>(evt => DelNameField(container));

            var textField = new TextField();
            textField.AddToClassList("text-field");
            textField.value = match.ToString().Substring(match.ToString().IndexOf("=") + 2, match.ToString().Length - 9);
            container.Add(textField);
            visualContainer.Add(container);
            textField.RegisterValueChangedCallback(evt => ChangeNameField(linesContainers.IndexOf(visualContainer), evt.newValue));
        }
    }
    private void CheckNarrator(VisualElement visualContainer, string curLine)
    {
        Match match = Regex.Match(curLine, @"<narrator>");
        if (match.Success)
        {
            var container = new VisualElement();
            container.AddToClassList("narrator-container");

            var delButton = new Button();
            delButton.AddToClassList("del-button");
            delButton.text = "❌";
            container.Add(delButton);
            delButton.RegisterCallback<ClickEvent>(evt => DelNarratorField(container));

            var label = new Label();
            label.text = "narrator";
            label.style.fontSize = 12;
            label.style.alignSelf = Align.Center;
            container.Add(label);
            visualContainer.Add(container);
        }
    }
    private void CheckText(VisualElement visualContainer, string curLine)
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
            textField.RegisterValueChangedCallback(evt => ChangeLineField(visualContainer, evt.newValue));
        }
    }
    private void CheckVariants(VisualElement visualContainer, string curLine)
    {
        MatchCollection matches = Regex.Matches(curLine, @"<variant.*?>.*?</variant>");
        if (matches.Count > 0)
        {
            string[] variants = matches.Cast<Match>().Select(m => m.Value).ToArray();

            foreach (var variant in variants)
            {
                var container = new VisualElement();
                container.AddToClassList("variant-container");

                var delButton = new Button();
                delButton.AddToClassList("del-button");
                delButton.text = "❌";
                container.Add(delButton);



                var nextScenarioField = new ObjectField();
                nextScenarioField.objectType = typeof(TextAsset);
                nextScenarioField.style.width = 115;
                nextScenarioField.style.alignSelf = Align.Center;
                container.Add(nextScenarioField);
                if (variant.ToString().Contains("next-scenario"))
                {
                    string nextScenario = variant.ToString().Substring(variant.ToString().IndexOf("\"") + 1);
                    nextScenario = nextScenario.Substring(0, nextScenario.IndexOf("\""));
                    nextScenarioField.value = Resources.Load<TextAsset>("Scenarios/" + nextScenario);
                }
                container.Add(nextScenarioField);

                delButton.RegisterCallback<ClickEvent>(evt => DelVariantField(container, nextScenarioField.value.name));

                var textLabel = new Label();
                textLabel.text = "text";
                textLabel.style.fontSize = 12;
                textLabel.style.alignSelf = Align.Center;
                container.Add(textLabel);

                string variantText = variant.ToString().Substring(variant.ToString().IndexOf(">") + 1);
                variantText = variantText.Substring(0, variantText.IndexOf("<"));
                var textValue = new TextField();
                textValue.style.width = 115;
                textValue.style.alignSelf = Align.Center;
                EventCallback<ChangeEvent<string>> eventCallback2 = evt => { };
                eventCallback2 = evt => ChangeVariantTextField(linesContainers.IndexOf(visualContainer), evt.newValue, "", textValue, eventCallback2);
                textValue.RegisterValueChangedCallback(eventCallback2);
                textValue.value = variantText;
                container.Add(textValue);

                EventCallback<ChangeEvent<UnityEngine.Object>> eventCallback = evt => { };
                eventCallback = evt => ChangeVariantField(linesContainers.IndexOf(visualContainer), evt.newValue.name, textValue.value);
                nextScenarioField.RegisterValueChangedCallback(eventCallback);

                visualContainer.Add(container);
            }
        }
    }

    public void RewriteTextFile(string[] linesArray)
    {
        string path = AssetDatabase.GetAssetPath(currentTextFile);
        string aaa = linesArray[0];
        for (int i = 1; i < linesArray.Length; i++)
        {
            aaa += "\n" + linesArray[i];
        }
        File.WriteAllText(path, aaa, Encoding.Unicode);
        AssetDatabase.Refresh();
    }

    public void ChangeNameField(int index, string newName)
    {
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string editingLine = lines[index];
        string firstPart = editingLine.Substring(0, editingLine.IndexOf("<name=\"") + 7);
        editingLine = editingLine.Substring(editingLine.IndexOf("\"") + 1);
        editingLine = editingLine.Substring(editingLine.IndexOf("\""));
        lines[index] = firstPart + newName + editingLine;


        RewriteTextFile(lines);
    }
    public void ChangeBGField(VisualElement container, string newName)
    {
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string editingLine = lines[linesContainers.IndexOf(container)];
        string firstPart = editingLine.Substring(0, editingLine.IndexOf("<bg=\"") + 5);
        editingLine = editingLine.Substring(editingLine.IndexOf("\"") + 1);
        editingLine = editingLine.Substring(editingLine.IndexOf("\""));
        lines[linesContainers.IndexOf(container)] = firstPart + newName + editingLine;


        RewriteTextFile(lines);
    }
    public void ChangeBGAnimField(VisualElement container, string newAnim)
    {
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string editingLine = lines[linesContainers.IndexOf(container)];
        string firstPart = editingLine.Substring(0, editingLine.IndexOf("<bg=\""));
        string secPart = editingLine.Substring(editingLine.IndexOf("<bg=\""));
        editingLine = secPart.Substring(secPart.IndexOf("anim=\"")+6);
        editingLine = editingLine.Substring(editingLine.IndexOf("\""));
        secPart = secPart.Substring(0, secPart.IndexOf("anim=\"") + 6);
        lines[linesContainers.IndexOf(container)] = firstPart + secPart + newAnim + editingLine;
        RewriteTextFile(lines);
    }
    public void ChangeLineField(VisualElement container, string newName)
    {
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string editingLine = lines[linesContainers.IndexOf(container)];
        string firstPart = editingLine.Substring(0, editingLine.IndexOf("<line>") + 6);
        editingLine = editingLine.Substring(editingLine.IndexOf("</line>"));
        lines[linesContainers.IndexOf(container)] = firstPart + newName + editingLine;

        RewriteTextFile(lines);
    }
    public void ChangeCharField(int index, string newName, string oldName, ObjectField field, EventCallback<ChangeEvent<UnityEngine.Object>> eventCallback)
    {
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string editingLine = lines[index];
        string firstPart = editingLine.Substring(0, editingLine.IndexOf("<char=\""+oldName) + 7);
        editingLine = editingLine.Substring(editingLine.IndexOf("<char=\""+oldName) + 7);
        editingLine = editingLine.Substring(editingLine.IndexOf("\""));
        lines[index] = firstPart + newName + editingLine;
        field.UnregisterValueChangedCallback(eventCallback);
        EventCallback<ChangeEvent<UnityEngine.Object>> newEvent = evt => { };
        newEvent = evt => ChangeCharField(index, evt.newValue.name, newName, field, newEvent);
        field.RegisterValueChangedCallback(newEvent);
        

        RewriteTextFile(lines);
    }
    public void ChangeVariantTextField(int index, string newText, string oldText, TextField field, EventCallback<ChangeEvent<string>> eventCallback)
    {
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string editingLine = lines[index];
        MatchCollection matches = Regex.Matches(editingLine, @"<variant.*?>.*?</variant>");
        string[] variants = matches.Cast<Match>().Select(m => m.Value).ToArray();
        int tagIndex = 0;
        for (int i = 0; i < variants.Length; i++)
        {
            if (variants[i].Contains(oldText))
            {
                tagIndex = i;
                break;
            }
        }
        string firstPart = editingLine.Substring(0, editingLine.IndexOf(variants[tagIndex]));
        string tag = editingLine.Substring(editingLine.IndexOf(variants[tagIndex]));
        string secPart = tag.Substring(tag.IndexOf("</variant>")+10);
        tag = tag.Substring(0, tag.IndexOf("</variant>") + 10);
        tag = tag.Substring(0, tag.IndexOf(">")+1) + newText + tag.Substring(tag.IndexOf("</variant>"));
        lines[index] = firstPart + tag + secPart;
        field.UnregisterValueChangedCallback(eventCallback);
        EventCallback<ChangeEvent<string>> newEvent = evt => { };
        newEvent = evt => ChangeVariantTextField(index, evt.newValue, newText, field, newEvent);
        field.RegisterValueChangedCallback(newEvent);

        RewriteTextFile(lines);
    }
    public void ChangeVariantField(int index, string newScenario, string oldText)
    {
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string editingLine = lines[index];
        MatchCollection matches = Regex.Matches(editingLine, @"<variant.*?>.*?</variant>");
        string[] variants = matches.Cast<Match>().Select(m => m.Value).ToArray();
        int tagIndex = 0;
        for (int i = 0; i < variants.Length; i++)
        {
            if (variants[i].Contains(oldText))
            {
                tagIndex = i;
                break;
            }
        }
        string firstPart = editingLine.Substring(0, editingLine.IndexOf(variants[tagIndex]));
        string tag = editingLine.Substring(editingLine.IndexOf(variants[tagIndex]));
        string secPart = tag.Substring(tag.IndexOf("</variant>") + 10);
        tag = tag.Substring(0, tag.IndexOf("</variant>") + 10);
        
        tag = tag.Substring(0, tag.IndexOf("next-scenario=\"") + 15) + newScenario + tag.Substring(tag.IndexOf("\">"));
        lines[index] = firstPart + tag + secPart;

        RewriteTextFile(lines);
    }
    public void ChangeCharAnimField(int index, string charName, string newAnim)
    {
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string editingLine = lines[index];
        string firstPart = editingLine.Substring(0, editingLine.IndexOf("<char=\"" + charName));
        string secPart = editingLine.Substring(editingLine.IndexOf("<char=\"" + charName));
        string thirdPart = secPart.Substring(secPart.IndexOf("anim=\"") + 6);
        thirdPart = thirdPart.Substring(thirdPart.IndexOf("\""));
        secPart = secPart.Substring(0, secPart.IndexOf("anim=\"") + 6);
        lines[index] = firstPart + secPart + newAnim + thirdPart;


        RewriteTextFile(lines);
    }
    public void ChangeCharX(int index, string charName, string newAnim)
    {
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string editingLine = lines[index];
        string firstPart = editingLine.Substring(0, editingLine.IndexOf("<char=\"" + charName));
        string secPart = editingLine.Substring(editingLine.IndexOf("<char=\"" + charName));
        string thirdPart = secPart.Substring(secPart.IndexOf("x=\"") + 3);
        thirdPart = thirdPart.Substring(thirdPart.IndexOf("\""));
        secPart = secPart.Substring(0, secPart.IndexOf("x=\"") + 3);
        lines[index] = firstPart + secPart + newAnim + thirdPart;


        RewriteTextFile(lines);
    }
    public void ChangeCharY(int index, string charName, string newAnim)
    {
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string editingLine = lines[index];
        string firstPart = editingLine.Substring(0, editingLine.IndexOf("<char=\"" + charName));
        string secPart = editingLine.Substring(editingLine.IndexOf("<char=\"" + charName));
        string thirdPart = secPart.Substring(secPart.IndexOf("y=\"") + 3);
        thirdPart = thirdPart.Substring(thirdPart.IndexOf("\""));
        secPart = secPart.Substring(0, secPart.IndexOf("y=\"") + 3);
        lines[index] = firstPart + secPart + newAnim + thirdPart;


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

    public void DelCharField(VisualElement element, string nameChar)
    {
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string editingLine = lines[linesContainers.IndexOf(element.parent)];
        string tag = editingLine.Substring(editingLine.IndexOf("<char=\"" + nameChar));
        editingLine = editingLine.Substring(0, editingLine.IndexOf("<char=\"" + nameChar));
        string secPart = tag.Substring(tag.IndexOf(">")+1);
        lines[linesContainers.IndexOf(element.parent)] = editingLine + secPart;
        RewriteTextFile(lines);

        element.RemoveFromHierarchy();

    }
    public void DelVariantField(VisualElement element, string nextScenario)
    {
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string editingLine = lines[linesContainers.IndexOf(element.parent)];
        string tag = editingLine.Substring(editingLine.IndexOf("<variant, next-scenario=\"" + nextScenario));
        editingLine = editingLine.Substring(0, editingLine.IndexOf("<variant, next-scenario=\"" + nextScenario));
        string secPart = tag.Substring(tag.IndexOf("t>") + 2);
        lines[linesContainers.IndexOf(element.parent)] = editingLine + secPart;
        RewriteTextFile(lines);

        element.RemoveFromHierarchy();

    }
    public void DelMusicField(VisualElement element)
    {
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string editingLine = lines[linesContainers.IndexOf(element.parent)];
        string firstPart = editingLine.Substring(0, editingLine.IndexOf("<music=\""));
        editingLine = editingLine.Substring(editingLine.IndexOf("<music=\"") + 8);
        editingLine = editingLine.Substring(editingLine.IndexOf("\">") + 2);
        lines[linesContainers.IndexOf(element.parent)] = firstPart + editingLine;

        element.RemoveFromHierarchy();

        RewriteTextFile(lines);
    }
    public void DelSoundField(VisualElement element)
    {
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string editingLine = lines[linesContainers.IndexOf(element.parent)];
        string firstPart = editingLine.Substring(0, editingLine.IndexOf("<sound=\""));
        editingLine = editingLine.Substring(editingLine.IndexOf("<sound=\"") + 8);
        editingLine = editingLine.Substring(editingLine.IndexOf("\">") + 2);
        lines[linesContainers.IndexOf(element.parent)] = firstPart + editingLine;

        element.RemoveFromHierarchy();

        RewriteTextFile(lines);
    }
    public void DelNameField(VisualElement element)
    {
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string editingLine = lines[linesContainers.IndexOf(element.parent)];
        string firstPart = editingLine.Substring(0, editingLine.IndexOf("<name=\""));
        editingLine = editingLine.Substring(editingLine.IndexOf("<name=\"") + 7);
        editingLine = editingLine.Substring(editingLine.IndexOf("\">") + 2);
        lines[linesContainers.IndexOf(element.parent)] = firstPart + editingLine;

        element.RemoveFromHierarchy();

        RewriteTextFile(lines);
    }
    public void DelNarratorField(VisualElement element)
    {
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string editingLine = lines[linesContainers.IndexOf(element.parent)];
        string firstPart = editingLine.Substring(0, editingLine.IndexOf("<narrator>"));
        editingLine = editingLine.Substring(editingLine.IndexOf("<narrator>") + 10);
        lines[linesContainers.IndexOf(element.parent)] = firstPart + editingLine;

        element.RemoveFromHierarchy();

        RewriteTextFile(lines);
    }
    public void DelBGField(VisualElement element, VisualElement bgElem)
    {
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string editingLine = lines[linesContainers.IndexOf(element)];
        string firstPart = editingLine.Substring(0, editingLine.IndexOf("<bg=\""));
        editingLine = editingLine.Substring(editingLine.IndexOf("<bg=\"") + 4);
        editingLine = editingLine.Substring(editingLine.IndexOf("\">") + 2);
        lines[linesContainers.IndexOf(element)] = firstPart + editingLine;

        bgElem.RemoveFromHierarchy();

        RewriteTextFile(lines);
    }
    public void DeleteLine(VisualElement element)
    {
        string text = currentTextFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        List<string> linesList = lines.ToList();
        linesList.RemoveAt(linesContainers.IndexOf(element));
        linesContainers.Remove(element);
        lines = linesList.ToArray();
        element.RemoveFromHierarchy();

        RewriteTextFile(lines); 
    }
}
#endif