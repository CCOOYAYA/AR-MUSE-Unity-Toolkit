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
        EditorWindow editorWindow = EditorWindow.GetWindow(typeof(XMLGenerator), false, "Config Generator", false);
        editorWindow.Show();
    }

    bool foldout = false;
    bool fileRead = false;
    int selected = -1;
    string[] toolbarString = new string[] { "Museum Info", "Exhibits List" };

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
                if (!fileRead)
                    EditorGUILayout.HelpBox("You can load a previous generated config.", MessageType.Info);
            }
            else
            {
                var path = EditorUtility.OpenFilePanel("Load a Config File", "", "xml");
                if (!(path == null || path == ""))
                {
                    list.Clear();
                    // TO-DO: Read XML
                    XmlDocument xml = new XmlDocument();
                    xml.Load(path);
                    bool isValid = AnalyzeXML(xml);
                    if (isValid)
                    {
                        // Jump to museum bar and flod the load page
                        selected = 0;
                        foldout = false;
                        fileRead = true;
                    }
                    else ShowNotification(new GUIContent("Invalid xml file."));
                }
            }
        }

        // Info
        if (fileRead)
            EditorGUILayout.HelpBox($"Config file loaded.", MessageType.Info);

        // Toolbar
        selected = GUILayout.Toolbar(selected, toolbarString);
        if (selected >= 0)
        {
            switch (toolbarString[selected])
            {
                // Museum toolbar area
                case "Museum Info":
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
                // Exhibits toolbar area
                case "Exhibits List":
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
            if (museumTitle != null && museumSubTitle != null && museumAuthor != null && museumVersion != null && museumDescription != null && museumLink != null)
            {
                string path = EditorUtility.SaveFilePanel("Save config file", "", "", "xml");
                if (!(path == null || path == ""))
                {
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
                            exhibitNode.AppendChild(CreateElementWithInnerText(xml, "status", list[i].status.ToString()));
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
            else ShowNotification(new GUIContent("Complete museum info first."));
        }
    }

    private bool AnalyzeXML(XmlDocument xmlDoc)
    {
        // Get root element
        XmlElement root = xmlDoc.DocumentElement;
        if (root.Name != "museum")
            return false;
        else
        {
            // Hashset for verifying integrity
            ISet<string> museumSet = new HashSet<string>();
            // Get museum config
            XmlNode museumNode = root.SelectSingleNode("museum");
            for (int i = 0; i < museumNode.ChildNodes.Count; i++)
            {
                //Debug.Log($"{museumNode.ChildNodes[i].Name}: {museumNode.ChildNodes[i].InnerText}");
                if (!museumSet.Contains(museumNode.ChildNodes[i].Name))
                    museumSet.Add(museumNode.ChildNodes[i].Name);
            }
            if (!(museumSet.Contains("title") && museumSet.Contains("subtitle") && museumSet.Contains("author") && museumSet.Contains("version") && museumSet.Contains("description") && museumSet.Contains("link") && museumSet.Count == 6))
                return false;
            else
            {
                for (int i = 0; i < museumNode.ChildNodes.Count; i++)
                {
                    switch (museumNode.ChildNodes[i].Name)
                    {
                        case "title":
                            museumTitle = museumNode.ChildNodes[i].InnerText;
                            break;
                        case "subtitle":
                            museumSubTitle = museumNode.ChildNodes[i].InnerText;
                            break;
                        case "author":
                            museumAuthor = museumNode.ChildNodes[i].InnerText;
                            break;
                        case "version":
                            museumVersion = museumNode.ChildNodes[i].InnerText;
                            break;
                        case "description":
                            museumDescription = museumNode.ChildNodes[i].InnerText;
                            break;
                        case "link":
                            museumLink = museumNode.ChildNodes[i].InnerText;
                            break;
                        default:
                            break;
                    }
                }
            }

            // Get others config
            XmlNodeList exhibitNodeList = root.SelectSingleNode("exhibit").ChildNodes;
            for (int i = 0; i < exhibitNodeList.Count; i++)
            {
                ISet<string> exhibitSet = new HashSet<string>();
                for (int j = 0; j < exhibitNodeList[i].ChildNodes.Count; j++)
                {
                    //Debug.Log($"{exhibitNodeList[i].ChildNodes[j].Name}: {exhibitNodeList[i].ChildNodes[j].InnerText}");
                    if (!exhibitSet.Contains(exhibitNodeList[i].ChildNodes[j].Name))
                        exhibitSet.Add(exhibitNodeList[i].ChildNodes[j].Name);
                }
                if (!(exhibitSet.Contains("id") && exhibitSet.Contains("status") && exhibitSet.Contains("title") && exhibitSet.Contains("subtitle") && exhibitSet.Contains("author") && exhibitSet.Contains("description") && exhibitSet.Contains("fileName") && exhibitSet.Count == 7))
                    return false;
                else
                {
                    string i_id = "", i_title = "", i_subtitle = "", i_author = "", i_des = "", i_filename = "";
                    ExhibitStatus i_status = ExhibitStatus.off;
                    for (int j = 0; j < exhibitNodeList[i].ChildNodes.Count; j++)
                    {
                        switch (exhibitNodeList[i].ChildNodes[j].Name)
                        {
                            case "id":
                                i_id = exhibitNodeList[i].ChildNodes[j].InnerText;
                                break;
                            case "status":
                                i_status = exhibitNodeList[i].ChildNodes[j].InnerText == "on" ? ExhibitStatus.on : ExhibitStatus.off;
                                break;
                            case "title":
                                i_title = exhibitNodeList[i].ChildNodes[j].InnerText;
                                break;
                            case "subtitle":
                                i_subtitle = exhibitNodeList[i].ChildNodes[j].InnerText;
                                break;
                            case "author":
                                i_author = exhibitNodeList[i].ChildNodes[j].InnerText;
                                break;
                            case "description":
                                i_des = exhibitNodeList[i].ChildNodes[j].InnerText;
                                break;
                            case "fileName":
                                i_filename = exhibitNodeList[i].ChildNodes[j].InnerText;
                                break;
                            default:
                                break;
                        }
                    }
                    list.Add(new Exhibit(i_id, i_status, i_title, i_subtitle, i_author, i_des, i_filename));
                }
            }
        }
        return true;
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
        public ExhibitStatus status;
        public string title;
        public string subtitle;
        public string author;
        public string description;
        public string filename;

        public Exhibit(string id , ExhibitStatus status, string title, string subtitle, string author, string description, string filename)
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

    public enum ExhibitStatus
    {
        off,
        on,
    }
}
