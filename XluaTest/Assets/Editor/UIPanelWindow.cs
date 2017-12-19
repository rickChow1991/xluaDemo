using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


/*--------------------------------------------
Describe:The ui windows,display all ui perfabs.
Author  :WangQinXin
Date    :2016 04 20
--------------------------------------------*/
public class UIWindowEditor : EditorWindow
{
    private const string PanelPath = "Assets/Resources/UI/Panels/";
    private static string[] PathFilter = {
          "Resources" + Path.DirectorySeparatorChar + "UI" + Path.DirectorySeparatorChar,
          "BlocksFramework" + Path.DirectorySeparatorChar + "UI" + Path.DirectorySeparatorChar,
    };

    private static string searchTxt;

    public class UIFloder
    {
        public string floderName;
        public bool flodout;
        public int depth;
        public UIFloder parent;
        public Dictionary<string, UIFloder> floders;
        public List<UIFile> files;

        public UIFloder()
        {
            floders = new Dictionary<string, UIFloder>();
            files = new List<UIFile>();
            flodout = true;
        }
        public void SetFloder(List<string> array, bool displayLayer = false)
        {
            if (array[0].EndsWith(".prefab"))
            {
                UIFile f = new UIFile(this, array[0].Substring(0, array[0].Length - 7), displayLayer);
                files.Add(f);
            }
            else
            {
                UIFloder floder = null;
                if (!floders.TryGetValue(array[0], out floder))
                {
                    floder = new UIFloder();
                    floder.floderName = array[0];
                    floder.parent = this;
                    floder.depth = depth + 1;
                    floders.Add(array[0], floder);
                }
                array.RemoveAt(0);
                floder.SetFloder(array, displayLayer);
            }
        }

        public void OnGUI()
        {
            EditorGUI.indentLevel += depth;
            EditorGUILayout.GetControlRect(true, 16f, EditorStyles.foldout);
            Rect foldRect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.MouseUp && foldRect.Contains(Event.current.mousePosition))
            {
                flodout = !flodout;
                GUI.changed = true;
                Event.current.Use();
            }
            flodout = EditorGUI.Foldout(foldRect, flodout, floderName);
            if (flodout)
            {
                foreach (var pair in floders)
                {
                    pair.Value.OnGUI();
                }
                foreach (var file in files)
                {
                    file.OnGUI();
                }
            }
            EditorGUI.indentLevel -= depth;
        }

        public string GetRootName()
        {
            if (parent == null)
                return floderName;
            else
                return parent.GetRootName();
        }
        public string GetPath()
        {
            var path = "";
            if (depth == 0) return path;
            if (parent != null)
                path = parent.GetPath();
            path += floderName + Path.DirectorySeparatorChar;
            return path;
        }
    }

    public class UIFile
    {
        public string fileName;
        public string displayName;
        public string fullPath;
        public UIFloder parent;
        public bool selected;
        private GameObject m_Instance;
        public GameObject instance
        {
            get { return m_Instance; }
            set
            {
                m_Instance = value;
                modifications = PrefabUtility.GetPropertyModifications(m_Instance);
            }
        }
        public PropertyModification[] modifications;

        public UIFile(UIFloder _parent, string _fileName, bool displayLayer)
        {
            fileName = _fileName;
            displayName = fileName;
            parent = _parent;
            //查找当前 Hierhary 中 同名Gameobject
            var objects = FindObjectsOfType<GameObject>();
            List<GameObject> objs = new List<GameObject>();
            foreach (var o in objects)
            {
                if (o.name != fileName) continue;
                var parent = PrefabUtility.GetPrefabParent(o);
                var parentPath = AssetDatabase.GetAssetPath(parent);
                //追溯 GameObjet 的路径，进行比较
                parentPath = parentPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                selected = parentPath.Contains(GetPath());
                if (selected)
                {
                    instance = o;
                    break;
                }
            }

            if (!displayLayer) return;
            var path = GetPath();
            var prefab = Resources.Load<GameObject>(path);
            if (prefab == null) return;
            var canvas = prefab.GetComponent<Canvas>();
            if (canvas == null) return;
            displayName += "(" + canvas.sortingLayerName + " " + canvas.sortingOrder + ")";
        }

        public string GetPath()
        {
            return "UI" + Path.DirectorySeparatorChar + parent.GetPath() + fileName;
        }

        public string GetFullPath()
        {
            return "Assets" + Path.DirectorySeparatorChar + parent.GetRootName() + Path.DirectorySeparatorChar + GetPath() + ".prefab";
        }

        public void OnGUI()
        {
            var path = GetFullPath();
            if (!string.IsNullOrEmpty(searchTxt) && path.IndexOf(searchTxt, System.StringComparison.CurrentCultureIgnoreCase) <= 0) return;
            EditorGUI.indentLevel++;
            bool ori = selected;
            selected = EditorGUILayout.ToggleLeft(displayName, selected);
            if (ori != selected)
            {
                if (selected)
                    Selected();
                else
                    DisSelected();
            }
            EditorGUI.indentLevel--;
        }

        void Selected()
        {
            var path = GetPath();
            Debug.Log("Load " + path);
            if (instance == null)
            {
                var prefab = Resources.Load(path);
                if (prefab == null)
                {
                    prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GetFullPath());
                    if (prefab == null)
                    {
                        Debug.LogWarning("Can not find this prefab :" + path);
                        return;
                    }
                }
                instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                if (instance == null) return;
                Selection.activeGameObject = instance;
                var rect = instance.GetComponent<RectTransform>();
                if (rect != null)
                {
                    var canvas = GameObject.Find("Canvas");
                    if (canvas != null)
                        rect.SetParent(canvas.transform, false);
                }
            }

        }

        void DisSelected()
        {
            var path = GetPath();
            Debug.Log("UnLoad " + path);
            if (instance != null)
            {
                if (modifications != null)
                {
                    var modis = PrefabUtility.GetPropertyModifications(instance);
                    if (modis.Length == modifications.Length)
                    {

                        for (int i = 0; i < modis.Length; i++)
                        {
                            var a = modifications[i];
                            var b = modis[i];
                            if (a.target != b.target) break;
                            if (a.propertyPath != b.propertyPath) break;
                            if (a.value != b.value) break;
                            if (a.objectReference != b.objectReference) break;
                        }
                        DestroyImmediate(instance);
                        return;
                    }
                }
                Debug.Log("Replace Prefab :" + fileName);
                var prefab = PrefabUtility.GetPrefabParent(instance);
                PrefabUtility.ReplacePrefab(instance, prefab, ReplacePrefabOptions.ConnectToPrefab);
                DestroyImmediate(instance);
            }
        }
    }

    Dictionary<string, UIFloder> dic;
    Vector2 scrollViewPos;
    bool[] flodout_game;

    [MenuItem("Window/Custom/UIPanel %q")]
    public static void OpenWindow()
    {
        var w = GetWindow<UIWindowEditor>("UI");
        w.autoRepaintOnSceneChange = true;
        w.position = new Rect(500, 100, 300, 800);
        w.minSize = new Vector2(200, 500);
        w.Init();
        w.Show();
    }

    public void Init(bool displayLayer = false)
    {
        var files = Directory.GetFiles("Assets", "*.prefab", SearchOption.AllDirectories);
        dic = new Dictionary<string, UIFloder>();

        foreach (var f in files)
        {
            bool skip = true;
            for (int a = 0; a < PathFilter.Length; a++)
            {
                var p = PathFilter[a];
                if (f.Contains(p)) skip = false;
            }
            if (skip) continue;

            var sps = f.Split(Path.DirectorySeparatorChar);
            var key = sps[1];
            var i = f.IndexOf("UI") + 3;
            var value = f.Substring(i);
            var floders = value.Split(Path.DirectorySeparatorChar);
            var list = new List<string>();
            list.AddRange(floders);
            if (!dic.ContainsKey(key))
            {
                UIFloder floder = new UIFloder();
                floder.floderName = key;
                dic.Add(key, floder);
            }
            dic[key].SetFloder(list, displayLayer);
        }
    }

    void CreatenNewUI()
    {
        var root = GameObject.Find("Canvas");
        if (root == null)
        {
            // Root for the UI
            root = new GameObject("Canvas");
            Canvas canvas = root.AddComponent<Canvas>();
            root.layer = LayerMask.NameToLayer("UI");
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            root.AddComponent<CanvasScaler>();
            root.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(root, "Create " + root.name);

            // if there is no event system add one...
            var esys = Object.FindObjectOfType<EventSystem>();
            if (esys == null)
            {
                var eventSystem = new GameObject("EventSystem");
                GameObjectUtility.SetParentAndAlign(eventSystem, null);
                esys = eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
                eventSystem.AddComponent<TouchInputModule>();

                Undo.RegisterCreatedObjectUndo(eventSystem, "Create " + eventSystem.name);
            }

        }

        int index = 0;
        string newFileName = "UINewView";
        while (true)
        {
            newFileName = "UINewView" + (index == 0 ? "" : index.ToString());
            string newFilePath = PanelPath + newFileName + ".prefab";
            if (File.Exists(newFilePath))
                index++;
            else
                break;
        }


        var newUI = new GameObject(newFileName);
        newUI.AddComponent<RectTransform>();
        GameObjectUtility.SetParentAndAlign(newUI, root);
        if (newUI.transform.parent as RectTransform)
        {
            RectTransform rect = newUI.transform as RectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }

        var path = PanelPath + newFileName + ".prefab";
        PrefabUtility.CreatePrefab(path, newUI, ReplacePrefabOptions.ConnectToPrefab);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeGameObject = newUI;
        Init();
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
        searchTxt = EditorGUILayout.TextField(searchTxt, GUI.skin.FindStyle("ToolbarSeachTextField"));
        if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
        {
            searchTxt = "";
            GUI.FocusControl(null);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh", GUILayout.MinWidth(60)))
        {
            Init();
        }
        if (GUILayout.Button("New", GUILayout.MinWidth(60)))
        {
            CreatenNewUI();
        }
        EditorGUILayout.EndHorizontal();
        if (dic == null) return;
        scrollViewPos = EditorGUILayout.BeginScrollView(scrollViewPos);
        EditorGUILayout.BeginVertical();
        foreach (var f in dic)
        {
            f.Value.OnGUI();
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }
}
