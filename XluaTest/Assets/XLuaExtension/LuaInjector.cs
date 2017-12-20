using System;
using UnityEngine;
using XLua;
using UnityEngine.UI;
namespace XLuaExtension
{
    public class LuaInjector : MonoBehaviour
    {
        public static void Inject(GameObject go, LuaTable self)
        {
            var childs = go.transform.GetComponentsInChildren<RectTransform>();
            for (int i = 0; i < childs.Length; i++)
            {
                var c = childs[i];
                if (c.name.StartsWith("g_"))
                {
                    self.Set(c.name, c.gameObject);
                }else if (c.name.StartsWith("txt_"))
                {
                    self.Set(c.name, c.GetComponent<Text>());
                }
                else if (c.name.StartsWith("img_"))
                {
                    self.Set(c.name, c.GetComponent<Image>());
                }
                else if (c.name.StartsWith("btn_"))
                {
                    self.Set(c.name, c.GetComponent<Button>());
                }
                else if (c.name.StartsWith("sr_"))
                {
                    self.Set(c.name, c.GetComponent<ScrollRect>());
                }
                else if(c.name.StartsWith("sc_"))
                {
                    self.Set(c.name, c.GetComponent<ScrollContent>());
                }
            }
        }
    }
}