#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;
using System.IO;
using System.Xml;

public class BuildPostProcessor
{
    [PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.iOS)
        {
            ProcessiOSBuild(pathToBuiltProject);
        }
        else if (target == BuildTarget.Android)
        {
            ProcessCustomAndroidManifest();
        }
    }

    private static void ProcessiOSBuild(string path)
    {
        string plistPath = Path.Combine(path, "Info.plist");
        PlistDocument plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        PlistElementDict rootDict = plist.root;

        rootDict.SetString("NSLocalNetworkUsageDescription", "Esta aplicación necesita descubrir dispositivos en la red local.");
        rootDict.SetString("NSCameraUsageDescription", "Esta aplicación necesita acceso a la cámara para usar realidad aumentada.");

        PlistElementArray services = rootDict.CreateArray("NSBonjourServices");
        services.AddString("_udp.");

        plist.WriteToFile(plistPath);
        Debug.Log("Info.plist actualizado con soporte de red local, Bonjour y acceso a cámara para iOS.");
    }

    private static void ProcessCustomAndroidManifest()
    {
        string manifestPath = Path.Combine(Application.dataPath, "Plugins/Android/AndroidManifest.xml");

        if (!File.Exists(manifestPath))
        {
            Debug.LogWarning("No se encontró AndroidManifest.xml en Assets/Plugins/Android/");
            return;
        }

        XmlDocument manifestDoc = new XmlDocument();
        manifestDoc.Load(manifestPath);

        XmlNamespaceManager nsManager = new XmlNamespaceManager(manifestDoc.NameTable);
        nsManager.AddNamespace("android", "http://schemas.android.com/apk/res/android");

        XmlNode manifestNode = manifestDoc.SelectSingleNode("/manifest");

        AddPermission(manifestDoc, manifestNode, "android.permission.INTERNET");
        AddPermission(manifestDoc, manifestNode, "android.permission.ACCESS_WIFI_STATE");
        AddPermission(manifestDoc, manifestNode, "android.permission.CHANGE_WIFI_MULTICAST_STATE");
        AddPermission(manifestDoc, manifestNode, "android.permission.ACCESS_NETWORK_STATE");

        manifestDoc.Save(manifestPath);
        Debug.Log("AndroidManifest.xml personalizado actualizado con permisos de red.");
    }

    private static void AddPermission(XmlDocument doc, XmlNode manifestNode, string permission)
    {
        foreach (XmlNode node in manifestNode.ChildNodes)
        {
            if (node.Name == "uses-permission" &&
                node.Attributes["android:name"]?.Value == permission)
                return; // Ya existe
        }

        XmlElement elem = doc.CreateElement("uses-permission");
        elem.SetAttribute("name", "http://schemas.android.com/apk/res/android", permission);
        manifestNode.AppendChild(elem);
    }
}
#endif
