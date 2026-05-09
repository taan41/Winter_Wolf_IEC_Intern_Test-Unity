using UnityEditor;

public class CustomMenu
{
	[MenuItem("CustomMenu/Reload And Save")]
	private static void ReloadAndSave()
	{
		EditorApplication.ExitPlaymode();
		EditorUtility.RequestScriptReload();
		AssetDatabase.Refresh();
		UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
	}
}