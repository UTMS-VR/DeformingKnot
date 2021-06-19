using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

// https://note.com/graffity/n/n9afca9154c50

public static class DockerBuild {
    [MenuItem("Build/ApplicationBuild/Android")]
    public static void BuildAndroid() {
        // Android に Switch Platform
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

        var sceneNameArray = CreateBuildTargetScenes().ToArray();
        PlayerSettings.applicationIdentifier = "com.ncompany.deformingknot";
        PlayerSettings.productName = "curve_writing";
        PlayerSettings.companyName = "DefaultCompany";

        // AppBundle は使用しない
        EditorUserBuildSettings.buildAppBundle = false;

        BuildPipeline.BuildPlayer(sceneNameArray, "DeformingKnot.apk", BuildTarget.Android, BuildOptions.Development);
    }

    #region Util

    private static IEnumerable<string> CreateBuildTargetScenes() {
        foreach (var scene in EditorBuildSettings.scenes) {
            if (scene.enabled)
                yield return scene.path;
        }
    }

    #endregion
}
