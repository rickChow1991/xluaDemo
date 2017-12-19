using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class ScrollRectSnap : ScrollRect
{

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);

    }

}
