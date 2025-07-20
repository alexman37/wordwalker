using System.IO;
using UnityEditor;
using UnityEngine;

public class ExportAssetBundles
{
    [MenuItem("Assets/Build AssetBundle")]
    static void ExportResource()
    {
        string folderName = "AssetBundles";
        string filePath = Path.Combine(Application.streamingAssetsPath, folderName);

        // Set to "None" for a fast build of new asset bundles. Set to ForceRebuild to build everything from scratch
        //BuildAssetBundleOptions opts = BuildAssetBundleOptions.None;
        BuildAssetBundleOptions opts = BuildAssetBundleOptions.ForceRebuildAssetBundle;

        //BuildPipeline.BuildAssetBundles(filePath, opts, EditorUserBuildSettings.activeBuildTarget);
        BuildPipeline.BuildAssetBundles(filePath, opts, BuildTarget.StandaloneWindows);
        //BuildPipeline.BuildAssetBundles(filePath, opts, BuildTarget.StandaloneWindows64);
        //BuildPipeline.BuildAssetBundles(filePath, opts, BuildTarget.StandaloneOSX); //mac
        //BuildPipeline.BuildAssetBundles(filePath, opts, BuildTarget.iOS);
        //BuildPipeline.BuildAssetBundles(filePath, opts, BuildTarget.Android);
        //BuildPipeline.BuildAssetBundles(filePath, opts, BuildTarget.WebGL);

        //Refresh the Project folder
        AssetDatabase.Refresh();
    }
}