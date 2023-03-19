using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AssetBundleBrowser;

public class AssetBundlePacker : EditorWindow
{
    [MenuItem ("MUSE/AR Toolkit/Asset Bundle Packer")]
    public static void ShowWindow()
    {
        var editorWindow = EditorWindow.GetWindow(typeof(AssetBundleBrowser.AssetBundleBrowserMain), false, "Asset Bundle Packer", true);
        editorWindow.Show();
    }
}
