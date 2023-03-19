using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Xml;
using System.IO;
using System.Text;

public class XMLGenerator : EditorWindow
{
    [MenuItem("MUSE/AR Toolkit/Config Generator")]
    public static void ShowWindow()
    {
        EditorWindow editorWindow = EditorWindow.GetWindow(typeof(XMLGenerator), true, "Config Generator", false);
        editorWindow.Show();
    }

    bool foldout = false;
    bool fileRead = false;
    int selected = -1;
    string[] toolbarString = new string[] { "Museum", "Exhibits" };

    // Museum
    string museumTitle;
    string museumSubTitle;
    string museumAuthor;
    string museumVersion;
    string museumDescription;
    string museumLink;

    // Exhibit
    [SerializeField]
    private List<Exhibit> list = new List<Exhibit>();
    private SerializedObject serObj;
    private SerializedProperty serPty;
    Vector2 scrollPos;
    string exhibitId;
    string exhibitStatus;
    string exhibitTitle;
    string exhibitSubTitle;
    string exhibitAuthor;
    string exhibitDescription;
    string exhibitFileName;

    private void OnEnable()
    {
        serObj = new SerializedObject(this);
        serPty = serObj.FindProperty("list");
    }

    private void OnGUI()
    {
        // Foldout
        foldout = EditorGUILayout.Foldout(foldout, "Load Config");
        if (foldout)
        {
            // Button for load pre config
            if (!GUILayout.Button("Load Config(xml file)", GUILayout.Height(40)))
            {
                EditorGUILayout.HelpBox("You can load a previous config.", MessageType.Info);
            }
            else
            {
                var path = EditorUtility.OpenFilePanel("Load a Config File", "", "xml");
                // TO-DO: Read XML
                XmlDocument xml = new XmlDocument();
                xml.Load(path);


                foldout = false;
                fileRead = true;
            }
        }

        if (fileRead)
            EditorGUILayout.HelpBox($"Config file loaded.", MessageType.Info);

        // Toolbar
        selected = GUILayout.Toolbar(selected, toolbarString);
        if (selected >= 0)
        {
            switch (toolbarString[selected])
            {
                case "Museum":
                    scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                    museumTitle = EditorGUILayout.TextField("Title", museumTitle);
                    museumSubTitle = EditorGUILayout.TextField("Subtitle", museumSubTitle);
                    museumAuthor = EditorGUILayout.TextField("Author", museumAuthor);
                    museumVersion = EditorGUILayout.TextField("Version", museumVersion);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Description", GUILayout.MaxWidth(147));
                    museumDescription = EditorGUILayout.TextArea(museumDescription, GUILayout.MaxHeight(100));
                    GUILayout.EndHorizontal();
                    museumLink = EditorGUILayout.TextField("Server Root Link", museumLink);
                    EditorGUILayout.EndScrollView();
                    break;
                case "Exhibits":
                    scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                    serObj.Update();
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(serPty, true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serObj.ApplyModifiedProperties();
                    }
                    EditorGUILayout.EndScrollView();
                    break;
                default:
                    break;
            }
        }

        // Button for save file
        if (GUILayout.Button("Save", GUILayout.Height(40)))
        {
            string path = EditorUtility.SaveFilePanel("Save config file", "", "","xml");
            XmlDocument xml = new XmlDocument();
            // Set declaration
            // XmlDeclaration xmlDecl = xml.CreateXmlDeclaration("1.0", "UTF-8", "");
            // xml.AppendChild(xmlDecl);
            // root of this xml file
            XmlElement root = xml.CreateElement("museum");
            // museum node
            XmlElement museumNode = xml.CreateElement("museum");
            museumNode.AppendChild(CreateElementWithInnerText(xml, "title", museumTitle));
            museumNode.AppendChild(CreateElementWithInnerText(xml, "subtitle", museumSubTitle));
            museumNode.AppendChild(CreateElementWithInnerText(xml, "author", museumAuthor));
            museumNode.AppendChild(CreateElementWithInnerText(xml, "version", museumVersion));
            museumNode.AppendChild(CreateElementWithInnerText(xml, "description", museumDescription));
            museumNode.AppendChild(CreateElementWithInnerText(xml, "link", museumLink));
            root.AppendChild(museumNode);

            if (list.Count > 0)
            {
                // exhibit node
                XmlElement exhibitRoot = xml.CreateElement("exhibit");
                for (int i = 0; i < list.Count; i++)
                {
                    XmlElement exhibitNode = xml.CreateElement("exhibit");
                    exhibitNode.AppendChild(CreateElementWithInnerText(xml, "id", list[i].id));
                    exhibitNode.AppendChild(CreateElementWithInnerText(xml, "status", list[i].status));
                    exhibitNode.AppendChild(CreateElementWithInnerText(xml, "title", list[i].title));
                    exhibitNode.AppendChild(CreateElementWithInnerText(xml, "subtitle", list[i].subtitle));
                    exhibitNode.AppendChild(CreateElementWithInnerText(xml, "author", list[i].author));
                    exhibitNode.AppendChild(CreateElementWithInnerText(xml, "description", list[i].description));
                    exhibitNode.AppendChild(CreateElementWithInnerText(xml, "fileName", list[i].filename));

                    exhibitRoot.AppendChild(exhibitNode);
                }
                root.AppendChild(exhibitRoot);
            }
            // append root and then file write
            xml.AppendChild(root);
            xml.Save(path);
        }
    }

    private XmlElement CreateElementWithInnerText(XmlDocument xmlDoc, string elementName, string innerText)
    {
        XmlElement element = xmlDoc.CreateElement(elementName);
        element.InnerText = innerText;
        return element;
    }

    public class Museum
    {
        public string title;
        public string subtitle;
        public string author;
        public string version;
        public string description;
        public string link;
    }

    [Serializable]
    public class Exhibit
    {
        public string id;
        public string status;
        public string title;
        public string subtitle;
        public string author;
        public string description;
        public string filename;

        public Exhibit(string id, string status, string title, string subtitle, string author, string description, string filename)
        {
            this.id = id;
            this.status = status;
            this.title = title;
            this.subtitle = subtitle;
            this.author = author;
            this.description = description;
            this.filename = filename;
        }
    }


}
