using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public static class AutoBuilder {
	static string APPNAME = "test";
	static string TARGET = "target";
	static string [] CollectScenePaths() {
		string[] scenes = new string[EditorBuildSettings.scenes.Length];
		for (int i = 0; i < scenes.Length; i++) {
			scenes[i] = EditorBuildSettings.scenes[i].path;
		}
		return scenes;
	}
	
	[MenuItem("File/AutoBuilder/iOS")]
	static void PerformiOSBuild() {
        PlayerSettings.productName = "坦克战争";
        PlayerSettings.applicationIdentifier = "com.liyong.xztk";
		PlayerSettings.bundleVersion = "1.0";
		BuildOptions opt = BuildOptions.Development | BuildOptions.SymlinkLibraries ;
		string targetDir = TARGET+"/ios";
		string error = BuildPipeline.BuildPlayer (CollectScenePaths(), targetDir, BuildTarget.iOS, opt ).ToString();
		if (error != null && error.Length > 0) {
			throw new IOException("Build Failed: "+error);
		}

	}

	[MenuItem("File/AutoBuilder/Windows")]
	static void PerformWindowsBuild()
	{
        PlayerSettings.productName = "坦克战争";
        PlayerSettings.applicationIdentifier = "com.liyong.xztk";
		PlayerSettings.bundleVersion = "1.3";
		BuildOptions opt = BuildOptions.Development | BuildOptions.SymlinkLibraries ;
		//string targetDir = TARGET+"/windows";
	    var targetDir = "../../zc/Scripts/windows";
		string error = BuildPipeline.BuildPlayer (CollectScenePaths(), targetDir, BuildTarget.StandaloneWindows64, opt ).ToString();
		if (error != null && error.Length > 0) {
			throw new IOException("Build Failed: "+error);
		}
	}

	[MenuItem("File/AutoBuilder/Android")]
	static void PerformAndroidBuild() {
        PlayerSettings.productName = "坦克战争";
		PlayerSettings.applicationIdentifier = "com.liyong.xztk";
		PlayerSettings.bundleVersion = "1.3";
	    BuildOptions opt = BuildOptions.None;//BuildOptions.Development;//| BuildOptions.SymlinkLibraries ;
		//string targetDir = TARGET+"/android";
	    var targetDir = "../../zc/Scripts/test.apk";
		string error = BuildPipeline.BuildPlayer (CollectScenePaths(), targetDir, BuildTarget.Android, opt ).ToString();
		if (error != null && error.Length > 0) {
			throw new IOException("Build Failed: "+error);
		}
	}

	[MenuItem("File/AutoBuilder/Ubuntu")]
	static void PerformUbuntuBuild() {
        PlayerSettings.productName = "坦克战争";
		PlayerSettings.applicationIdentifier = "com.liyong.xztk";
		PlayerSettings.bundleVersion = "1.3";
		//BuildOptions opt = BuildOptions.Development;//| BuildOptions.SymlinkLibraries ;
	    BuildOptions opt = BuildOptions.EnableHeadlessMode;

		//string targetDir = TARGET+"/android";
        //var targetDir = "../../zc/Scripts/linux";
	    //发布出去的Ubuntu机器人 QueryRoom 还是要一起发出去 到外网服务器上面
        var targetDir = "../../zc/Scripts/windows";
		string error = BuildPipeline.BuildPlayer (CollectScenePaths(), targetDir, BuildTarget.StandaloneLinux64, opt ).ToString();
		if (error != null && error.Length > 0) {
			throw new IOException("Build Failed: "+error);
		}
	}

}
