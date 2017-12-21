using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;

public class ImportProcess : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
    {
        StringBuilder sb = new StringBuilder(1024);
        foreach (var file in importedAssets)
        {
            //if (file.EndsWith(".fnt")) { OnPostprocessAllAssets_Font(file); continue; }
            //if (file.EndsWith(".xlsx")) { OnPostprocessAllAssets_Excel(file); continue; }
            if (file.EndsWith(".bytes"))
            {
                var path = file;
                var fileName = Path.GetFileName(path);
                if (file.Contains("Assets/Resources/"))
                {
                    path = file.Remove(0, 17);
                    path = path.Remove(path.LastIndexOf('.'));
                    sb.Append(path + ";");
                }
            }
        }
        if (sb.Length > 0)
            OnPostProtoFile(sb);
    }

    static void OnPostProtoFile(StringBuilder sb)
    {
        var savePath = "Assets/Resources/";
        savePath = Path.GetDirectoryName(savePath);
        var saveFilePath = savePath + "/protoPath.txt";
        File.WriteAllText(saveFilePath, sb.ToString(), Encoding.UTF8);
    }

    static void OnPostprocessAllAssets_Font(string file)
    {
        var fontName = Path.GetFileNameWithoutExtension(file);
        var dir = Path.GetDirectoryName(file) + '/';
        var txt = File.ReadAllText(file);
        var match = Regex.Match(txt, "file=\"(.*)\"");
        var texPath = dir + match.Groups[1].Value;

        match = Regex.Match(txt, "chars count=(\\d*)");
        var charCount = System.Convert.ToInt32(match.Groups[1].Value);


        match = Regex.Match(txt, "spacing=(\\d*),(\\d*)");
        var spacingX = System.Convert.ToInt32(match.Groups[1].Value);
        var spacingY = System.Convert.ToInt32(match.Groups[2].Value);

        match = Regex.Match(txt, "common lineHeight=(\\d*.*)base=(\\d*.*)scaleW=(\\d*.*)scaleH=(\\d*.*)pages=(\\d*.*)packed=(\\d*.*)");
        var scaleW = System.Convert.ToInt32(match.Groups[3].Value);
        var scaleH = System.Convert.ToInt32(match.Groups[4].Value);

        var mChars = Regex.Matches(txt, "char id=(\\d*.*) x=(\\d*.*) y=(\\d*.*) width=(\\d*.*) height=(\\d*.*) xoffset=(\\d*.*) yoffset=(\\d*.*) xadvance=(\\d*.*) page=(\\d*.*) chnl=(\\d*.*)");
        // var mChars = Regex.Matches(txt, "char id=(\\d*.*)x=(\\d*.*)y=(\\d*.*)width=(\\d*.*)height=(\\d*.*)xoffset=(\\d*.*)yoffset=(\\d*.*)xadvance=(\\d*.*)page=(\\d*.*)chnl=(\\d*.*).*\"");
        //var mChars = Regex.Matches(txt, "char id=(\\d.*)x=(\\d)");

        var matPath = dir + fontName + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (!mat)
        {
            mat = new Material(Shader.Find("Sprites/Default"));
            mat.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            Debug.LogWarning(mat.mainTexture.name);
            AssetDatabase.CreateAsset(mat, matPath);
        }
        var charInfos = new List<CharacterInfo>();
        foreach (Match m in mChars)
        {
            var info = new CharacterInfo();
            var x = System.Convert.ToInt32(m.Groups[2].Value);
            var y = System.Convert.ToInt32(m.Groups[3].Value);
            var w = System.Convert.ToInt32(m.Groups[4].Value);
            var h = System.Convert.ToInt32(m.Groups[5].Value);
            var xOff = System.Convert.ToInt32(m.Groups[6].Value);
            var yOff = System.Convert.ToInt32(m.Groups[7].Value);
            var xAdvance = System.Convert.ToInt32(m.Groups[8].Value);

            var fx = (float)x / (float)scaleW;
            var fy = 1 - (float)y / (float)scaleH;
            var fw = (float)(w /*+ spacingX*/) / (float)scaleW;
            var fh = (float)(h /*+ spacingY*/) / (float)scaleH;
            info.index = System.Convert.ToInt32(m.Groups[1].Value);
            info.uvBottomLeft = new Vector2(fx, fy - fh);
            info.uvBottomRight = new Vector2(fx + fw, fy - fh);
            info.uvTopLeft = new Vector2(fx, fy);
            info.uvTopRight = new Vector2(fx + fw, fy);
            info.maxX = xOff + w;
            info.maxY = -yOff;
            info.minX = info.maxX - w;
            info.minY = info.maxY - h;
            info.advance = xAdvance;
            charInfos.Add(info);
            //Debug.Log(string.Format("id={0}", m.Groups[1].Value));
        }
        var fontPath = dir + fontName + ".fontsettings";
        var font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
        if (!font)
        {
            font = new Font(fontName);
            font.material = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            AssetDatabase.CreateAsset(font, fontPath);
        }
        font.characterInfo = charInfos.ToArray();
        EditorUtility.SetDirty(font);
        AssetDatabase.SaveAssets();

        if (charCount != charInfos.Count)
        {
            Debug.LogWarning(string.Format("Not all characters in font {0} was imported. Imported:{1}, All:{2}", fontName, charInfos.Count, charCount));
        }
        Debug.Log(string.Format("Generate custom font [{0}] with {1} characters.", fontName, charInfos.Count));
    }
    /*
    /// <summary>
    /// Preprocess Model
    /// </summary>
    void OnPreprocessModel()
    {
        if (!assetPath.Contains("ArtExport")) return;
        var importer = assetImporter as ModelImporter;
        var fileName = Path.GetFileNameWithoutExtension(assetPath);
        var folder = Path.GetDirectoryName(assetPath);
        var path = folder + "/" + fileName + ".prefab";
        path = path.Replace("ArtExport", "Resources/ArtAssets");
        var ams = importer.clipAnimations;
        foreach (var a in ams)
        {
            if (a.name.EndsWith("Loop", System.StringComparison.OrdinalIgnoreCase))
                a.loopTime = true;
        }
        EditorUtils.CreateFloderIfNotExist(path);
        var asset = AssetDatabase.LoadAssetAtPath(path + "_M", typeof(GameObject));
        if (asset != null) return;
        var tempMode = (GameObject)AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject));
        var p = new GameObject(fileName);
        p.AddComponent<RectTransform>();
        tempMode = GameObject.Instantiate(tempMode);

        //Modify Position
        tempMode.transform.position = Vector3.zero;
        //Modify Scale
        tempMode.transform.localScale = Vector3.one;
        //Modify Rotate
        var rotate = tempMode.transform.rotation.eulerAngles;
        rotate = new Vector3(rotate.x, rotate.y + 180, rotate.z);
        tempMode.transform.rotation = Quaternion.Euler(rotate);
        //Modify Materaial
        var shader = Shader.Find("Mobile/Diffuse");
        var render = tempMode.GetComponent<Renderer>();
        if (render != null)
        {
            for (int i = 0; i < render.sharedMaterials.Length; i++)
            {
                render.sharedMaterials[i].shader = shader;
            }
        }
        tempMode.transform.SetParent(p.transform, false);
        var prefab = PrefabUtility.CreatePrefab(path, p, ReplacePrefabOptions.ConnectToPrefab);
        GameObject.DestroyImmediate(p);
    }
    

    static void OnPostprocessAllAssets_Excel(string path)
    {
        var fileName = Path.GetFileName(path);
        if (fileName.StartsWith("~")) return;
        var result = ExcelReader.ConvertCsvFromExcel(path);
        if (fileName.StartsWith("Loc")) OnPostLocFile(result, path);
    }

    static void OnPostLocFile(DataSet dataSet, string path)
    {
        var savePath = path.Replace("Editor Default Resources/", "");
        var fileName = Path.GetFileNameWithoutExtension(path);
        savePath = Path.GetDirectoryName(savePath) + "/Resources/" + fileName + "/";
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);
        var table = dataSet.Tables[0];
        var columeCount = table.Columns.Count;
        var firstRow = table.Rows[0];
        for (int i = 0; i < columeCount; i++)
        {
            var colName = firstRow[i].ToString();
            var rowCount = table.Rows.Count;
            var saveFilePath = savePath + colName + ".csv";
            StringBuilder sb = new StringBuilder(1024);
            for (int j = 1; j < rowCount; j++)
            {
                sb.AppendLine(table.Rows[j][i].ToString());
            }
            File.WriteAllText(saveFilePath, sb.ToString(), Encoding.UTF8);
            Debug.LogFormat("Generate csv file : {0} ,path :{1}", colName, saveFilePath);
        }
    }

    /// <summary>
    /// Preprocess Audio
    /// </summary>
    void OnPreprocessAudio()
    {
        var importer = assetImporter as AudioImporter;

    }

    /// <summary>
    /// Postprocess Audio
    /// </summary>
    void OnPostprocessAudio(AudioClip clip)
    {
        if (!assetPath.Contains("AudioExport")) return;
            var c = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
        var sc = ScriptableObject.CreateInstance<QYAudioClip>();
        sc.clip = c;
        if (c.name.EndsWith("Loop", System.StringComparison.OrdinalIgnoreCase) ||
            c.name.EndsWith("Lp", System.StringComparison.OrdinalIgnoreCase))
            sc.loop = true;
        var path = assetPath.Replace("AudioExport", "Resources/Audio");
        path = Path.GetDirectoryName(path);
        path += "/" + c.name + ".asset";
        EditorUtils.CreateFloderIfNotExist(path);
        AssetDatabase.CreateAsset(sc, path);
        EditorUtility.SetDirty(sc);
    }


    void OnPreprocessTexture()
    {
        if (!assetPath.Contains("Assets/Textures"))
            return;
        bool isTrueColor = false;
        var pathArray = assetPath.Split('/');
        var fileFullName = pathArray[pathArray.Length - 1];
        var fs = fileFullName.Split('.');
        var fileName = fs[0];
        //if (fileName.EndsWith("_i")) return;
        var tag = Path.GetFileName(Path.GetDirectoryName(assetPath));
        
        TextureImporter ti = (TextureImporter)assetImporter;

        TextureImporterPlatformSettings setting = new TextureImporterPlatformSettings();

        ti.assetBundleName = null;
        if (fileName.EndsWith("_i"))
        {
            isTrueColor = true;
            //ti.textureType = TextureImporterType.Sprite;
            //ti.spritePackingTag = null;
        }
        else
        if (!tag.EndsWith("_i"))
        {
            isTrueColor = false;
            ti.textureType = TextureImporterType.Sprite;
            ti.spritePackingTag = tag;
            //AssetBundle
            if (assetPath.Contains("Textures/AssetBundle/"))
            {
                ti.assetBundleName = tag;
            }
        }
        ti.npotScale = TextureImporterNPOTScale.None;
        ti.filterMode = FilterMode.Bilinear;

        TextureImporterSettings st = new TextureImporterSettings();
        ti.ReadTextureSettings(st);
        st.spriteMeshType = SpriteMeshType.FullRect;
        ti.SetTextureSettings(st);
        ti.SetPlatformTextureSettings(ImporterPlatformSetting.GetTextureIPSettings("Android", tag, isTrueColor));
        ti.SetPlatformTextureSettings(ImporterPlatformSetting.GetTextureIPSettings("iPhone", tag, isTrueColor));
    }
    */
}
