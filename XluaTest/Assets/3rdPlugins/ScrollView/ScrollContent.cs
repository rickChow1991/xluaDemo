using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;

[ExecuteInEditMode]
public class ScrollContent : UIBehaviour
{
    /// <summary>
    /// Return the count of elements this scrollview will display;
    /// </summary>
    public delegate int GetScrollCellSize();

    /// <summary>
    /// The callback method while the index update
    /// </summary>
    /// <param name="index"></param>
    /// <param name="element"></param>
    public delegate void UpdateScrollView(int index, RectTransform element);

    public enum HCorner { Center = 0, Top = 1, Bottom = 2 }
    public enum VCorner { Center = 0, Left = 1, Right = 2 }

    public enum Direction { LeftToRight = 0, RightToLeft = 1, TopToBottom = 2, BottomToTop = 3 };

    [SerializeField]
    private ScrollRect m_ScrollRect;
    public ScrollRect scrollRect { get { return m_ScrollRect; } set { m_ScrollRect = value; } }

    [SerializeField]
    private RectTransform m_Sample;//Sample use for instance
    public RectTransform sample { get { return m_Sample; } set { m_Sample = value; } }

    [SerializeField]
    private RectOffset m_Padding;
    public RectOffset padding { get { return m_Padding; } set { m_Padding = value; } }

    [SerializeField]
    private Vector2 m_Spacing;
    public Vector2 spacing { get { return m_Spacing; } set { m_Spacing = value; } }

    [SerializeField]
    private Direction m_Direction = Direction.LeftToRight; //Layout Direction
    public Direction direction { get { return m_Direction; } set { m_Direction = value; } }

    [SerializeField]
    private VCorner m_VCorner;
    public VCorner vCorner { get { return m_VCorner; } set { m_VCorner = value; } }

    [SerializeField]
    private HCorner m_HCorner;
    public HCorner hCorner { get { return m_HCorner; } set { m_HCorner = value; } }

    [SerializeField]
    private int m_RawChildCount = 1;
    public int rawChildCount { get { return m_RawChildCount; } set { m_RawChildCount = (value > 0 ? value : 1); } }

    private List<RectTransform> m_Children;      //elements array
    private RectTransform m_ViewPort;
    private RectTransform m_Content;
    private Bounds m_ViewBounds;
    private Bounds m_ContenBounds;
    private Vector2 m_CellSize;
    private int m_CellCount;
    private int m_RealCount;

    /// <summary>
    /// Element 出现间隔
    /// </summary>
    private float m_Interval;
    public float interval
    {
        get { return m_Interval; }
        set { m_Interval = value; }
    }

    [SerializeField]
    private GetScrollCellSize m_OnGetElementCount;
    public GetScrollCellSize onGetElementCount { get { return m_OnGetElementCount; } set { m_OnGetElementCount = value; } }


    [SerializeField]
    private UpdateScrollView m_OnUpdateScrollView;
    /// <summary>
    /// Method (int index, RectTransform element)
    /// </summary>
    public UpdateScrollView onUpdateScrollView { get { return m_OnUpdateScrollView; } set { m_OnUpdateScrollView = value; } }

    private int m_LastFocus;
    private int m_Head;
    private int m_Trail;
    private float m_Space;

    private DrivenRectTransformTracker m_Tracker = new DrivenRectTransformTracker();

    /// <summary>
    /// 提示!不要在 Awake 里调用，刚开始UI没有初始化好，RectTransform的 rect zie都为0，会引发错误，建议使用coroutine来停等一帧
    /// </summary>
    public void Show()
    {
        TrackRectTransform(Vector2.zero);
        Clear();
        StartCoroutine(IShow());
    }

    IEnumerator IShow()
    {
        yield return new WaitForEndOfFrame();
        if (interval > 0) yield return LayoutOriElements(interval);
        else LayoutOriElements();
    }


    public void RefreshView()
    {
        var idx = 3 * m_LastFocus;
        for (int i = 0; i < m_Children.Count; i++)
        {
            if (idx < m_CellCount)
            {
                onUpdateScrollView(idx, m_Children[idx % m_RealCount]);
                idx++;
            }
        }
    }

    public RectTransform GetChild(int index)
    {
        if (index < 0 || index > onGetElementCount()) return null;
        var idx = index % m_RealCount;
        return m_Children[idx];
    }

    protected override void Awake()
    {
        m_Children = new List<RectTransform>();
        //m_Children.Add(m_Sample);
        m_ViewPort = transform.parent as RectTransform;
        m_Content = transform as RectTransform;
        m_CellSize = m_Sample.GetComponent<RectTransform>().rect.size;
        m_Sample.gameObject.SetActive(false);
        //scrollRect.onLayoutCompleted.AddListener(LayoutCompleted);
    }

    void TrackRectTransform(Vector2 delta)
    {
        m_Tracker.Clear();
        m_Tracker.Add(this, m_Content,
            DrivenTransformProperties.Anchors |
            DrivenTransformProperties.SizeDelta |
                    DrivenTransformProperties.AnchoredPosition |
                    DrivenTransformProperties.Pivot);

        Vector2 v = GetAnchorByDirection();

        m_Content.anchoredPosition = Vector2.zero;
        m_Content.anchorMin = v;
        m_Content.anchorMax = v;
        m_Content.pivot = v;
        if (delta == Vector2.zero)
            m_Content.sizeDelta = m_ViewPort.rect.size;
        else
            m_Content.sizeDelta = delta;
        UpdateBounds();
    }

    public void Clear()
    {
        for (int i = 0; i < m_Children.Count; i++)
        {
            m_Children[i].gameObject.SetActive(false);
        }
    }

    public void ShowElements()
    {
        var count = onGetElementCount.Invoke();
        for (int i = 0; i < count; i++)
        {
            m_Children[i].gameObject.SetActive(true);
        }
    }

    public void ClearLaout()
    {
        foreach (var c in m_Children)
        {
            if (c && c != m_Sample)
                DestroyImmediate(c.gameObject);
        }
        m_Children.Clear();
        m_Children.Add(m_Sample);
    }

    public void LayoutCompleted()
    {
        m_ViewPort = transform.parent as RectTransform;
        m_Content = transform as RectTransform;
        m_CellSize = m_Sample.GetComponent<RectTransform>().rect.size;
        m_OnGetElementCount += () => { return 10; };
        m_OnUpdateScrollView += (int index, RectTransform element) => { };
        TrackRectTransform(Vector2.zero);
        LayoutOriElements();
    }

    void UpdateBounds()
    {
        m_ContenBounds = new Bounds(m_Content.rect.center, m_Content.rect.size);
        m_ViewBounds = new Bounds(m_ViewPort.rect.center, m_ViewPort.rect.size);
    }
    /// <summary>
    /// Generate Original elemets that will show in the viewport at first.
    /// </summary>
    void LayoutOriElements()
    {
        if (m_OnGetElementCount == null) return;
        if (m_OnUpdateScrollView == null) return;
        m_CellCount = onGetElementCount();
        if (m_CellCount == 0) return;
        UpdateBounds();

        if ((int)direction > 1)
        {
            //Vertical Layout
            bool top = true;
            if (m_Direction == Direction.TopToBottom)
                top = true;
            else if (m_Direction == Direction.BottomToTop)
                top = false;
            m_Space = top ? padding.top : padding.bottom;
            var viewHeight = m_ViewBounds.size.y - m_Space;
            var maxCount = m_RawChildCount * Mathf.CeilToInt(viewHeight / (m_CellSize.y + spacing.y)) + m_RawChildCount;
            m_RealCount = Mathf.Min(m_CellCount, maxCount);
            var dir = top ? 1 : -1;
            var height = dir * (spacing.y + m_CellSize.y - m_Space);
            var pos = Vector2.zero;
            if (vCorner == VCorner.Center)
                pos.x = (m_ContenBounds.min.x + m_ContenBounds.max.x) * 0.5f;
            else if (vCorner == VCorner.Left)
                pos.x = m_ContenBounds.min.x + padding.left + m_CellSize.x * 0.5f;
            else if (vCorner == VCorner.Right)
                pos.x = m_ContenBounds.max.x - padding.right - m_CellSize.x * 0.5f;
            var anchor = GetAnchorByDirection();
            for (int i = 0; i < m_RealCount; i++)
            {
                RectTransform e = null;
                if (i < m_Children.Count)
                {
                    e = m_Children[i];
                }
                else
                {
                    e = Instantiate<RectTransform>(m_Sample);
                    e.SetParent(transform, false);
                    m_Children.Add(e);
                }
                e.gameObject.SetActive(true);
                e.anchorMin = anchor;
                e.anchorMax = anchor;
                e.pivot = anchor;
                pos.x = 0 - (m_RawChildCount - 1) * 0.5f * (m_CellSize.x + spacing.x);
                if (i % m_RawChildCount == 0)
                    height -= dir * (m_CellSize.y + spacing.y);
                else
                    pos.x += (i % m_RawChildCount) * (m_CellSize.x + spacing.x);
                pos.y = height;
                e.anchoredPosition = pos;
                onUpdateScrollView(i, e);
            }
            height = Mathf.CeilToInt(m_CellCount/ m_RawChildCount) * m_CellSize.y + spacing.y * (Mathf.CeilToInt(m_CellCount/ m_RawChildCount) - 1 < 0 ? 0 : Mathf.CeilToInt(m_CellCount/ m_RawChildCount) - 1);
            height += padding.top + padding.bottom;
            var delta = m_Content.sizeDelta;
            delta.y = height;
            delta.x = m_Content.sizeDelta.x * 2 + 20f;
            TrackRectTransform(delta);
            m_Trail = m_RealCount - m_RawChildCount;
        }
        else
        {
            //Horizontal Layout
            bool left = true;
            if (m_Direction == Direction.LeftToRight)
                left = false;
            else if (m_Direction == Direction.RightToLeft)
                left = true;
            //Calculate the count of elements that could full of viewport
            m_Space = left ? padding.left : padding.right;
            var viewWith = m_ViewBounds.size.x - m_Space;
            var maxCount = Mathf.CeilToInt(viewWith / (m_CellSize.x + spacing.x)) + 1;
            m_RealCount = Mathf.Min(m_CellCount, maxCount);

            var dir = left ? 1 : -1;
            var width = dir * (spacing.x + m_CellSize.x - m_Space);
            //Calculate Y value of element's value
            var pos = Vector2.zero;
            if (hCorner == HCorner.Center)
                pos.y = (m_ContenBounds.min.y + m_ContenBounds.max.y) * 0.5f;
            else if (hCorner == HCorner.Bottom)
                pos.y = m_ContenBounds.min.y + padding.bottom + m_CellSize.y * 0.5f;
            else if (hCorner == HCorner.Top)
                pos.y = m_ContenBounds.max.y - padding.top - m_CellSize.y * 0.5f;
            var anchor = GetAnchorByDirection();
            for (int i = 0; i < m_RealCount; i++)
            {
                RectTransform e = null;
                if (i < m_Children.Count)
                {
                    e = m_Children[i];
                }
                else
                {
                    e = Instantiate<RectTransform>(m_Sample);
                    e.SetParent(transform, false);
                    m_Children.Add(e);
                }
                e.anchorMin = anchor;
                e.anchorMax = anchor;
                e.pivot = anchor;
                width -= dir * (m_CellSize.x + spacing.x);
                pos.x = width;
                e.anchoredPosition = pos;
                e.gameObject.SetActive(true);
                onUpdateScrollView(i, e);
            }
            width = m_CellCount * m_CellSize.x + spacing.x * (m_CellCount - 1 < 0 ? 0 : m_CellCount - 1);
            width += padding.left + padding.right;
            var delta = m_Content.sizeDelta;
            delta.x = width;
            TrackRectTransform(delta);
            m_Trail = m_RealCount - 1;
        }
        m_Head = 0;
        m_LastFocus = 0;
    }

    IEnumerator LayoutOriElements(float wait)
    {
        if (m_OnGetElementCount == null) yield break;
        if (m_OnUpdateScrollView == null) yield break;
        m_CellCount = onGetElementCount();
        if (m_CellCount == 0) yield break;
        UpdateBounds();

        if ((int)direction > 1)
        {
            //Vertical Layout
            bool top = true;
            if (m_Direction == Direction.TopToBottom)
                top = true;
            else if (m_Direction == Direction.BottomToTop)
                top = false;
            m_Space = top ? padding.top : padding.bottom;
            var viewHeight = m_ViewBounds.size.y - m_Space;
            var maxCount = Mathf.CeilToInt(viewHeight / (m_CellSize.y + spacing.y)) + 1;
            m_RealCount = Mathf.Min(m_CellCount, maxCount);
            var dir = top ? 1 : -1;
            var height = dir * (spacing.y + m_CellSize.y - m_Space);
            var pos = Vector2.zero;
            if (vCorner == VCorner.Center)
                pos.x = (m_ContenBounds.min.x + m_ContenBounds.max.x) * 0.5f;
            else if (vCorner == VCorner.Left)
                pos.x = m_ContenBounds.min.x + padding.left + m_CellSize.x * 0.5f;
            else if (vCorner == VCorner.Right)
                pos.x = m_ContenBounds.max.x - padding.right - m_CellSize.x * 0.5f;
            var anchor = GetAnchorByDirection();
            for (int i = 0; i < m_RealCount; i++)
            {
                RectTransform e = null;
                if (i < m_Children.Count)
                {
                    e = m_Children[i];
                }
                else
                {
                    e = Instantiate<RectTransform>(m_Sample);
                    e.SetParent(transform, false);
                    m_Children.Add(e);
                }
                e.gameObject.SetActive(true);
                e.anchorMin = anchor;
                e.anchorMax = anchor;
                e.pivot = anchor;
                height -= dir * (m_CellSize.y + spacing.y);
                pos.y = height;
                e.anchoredPosition = pos;
                onUpdateScrollView(i, e);
                yield return new WaitForSeconds(wait);
            }
            height = m_CellCount * m_CellSize.y + spacing.y * (m_CellCount - 1 < 0 ? 0 : m_CellCount - 1);
            height += padding.top + padding.bottom;
            var delta = m_Content.sizeDelta;
            delta.y = height;
            TrackRectTransform(delta);
        }
        else
        {
            //Horizontal Layout
            bool left = true;
            if (m_Direction == Direction.LeftToRight)
                left = false;
            else if (m_Direction == Direction.RightToLeft)
                left = true;
            //Calculate the count of elements that could full of viewport
            m_Space = left ? padding.left : padding.right;
            var viewWith = m_ViewBounds.size.x - m_Space;
            var maxCount = Mathf.CeilToInt(viewWith / (m_CellSize.x + spacing.x)) + 1;
            m_RealCount = Mathf.Min(m_CellCount, maxCount);

            var dir = left ? 1 : -1;
            var width = dir * (spacing.x + m_CellSize.x - m_Space);
            //Calculate Y value of element's value
            var pos = Vector2.zero;
            if (hCorner == HCorner.Center)
                pos.y = (m_ContenBounds.min.y + m_ContenBounds.max.y) * 0.5f;
            else if (hCorner == HCorner.Bottom)
                pos.y = m_ContenBounds.min.y + padding.bottom + m_CellSize.y * 0.5f;
            else if (hCorner == HCorner.Top)
                pos.y = m_ContenBounds.max.y - padding.top - m_CellSize.y * 0.5f;
            var anchor = GetAnchorByDirection();
            for (int i = 0; i < m_RealCount; i++)
            {
                RectTransform e = null;
                if (i < m_Children.Count)
                {
                    e = m_Children[i];
                }
                else
                {
                    e = Instantiate<RectTransform>(m_Sample);
                    e.SetParent(transform, false);
                    m_Children.Add(e);
                }
                e.anchorMin = anchor;
                e.anchorMax = anchor;
                e.pivot = anchor;
                width -= dir * (m_CellSize.x + spacing.x);
                pos.x = width;
                e.anchoredPosition = pos;
                e.gameObject.SetActive(true);
                onUpdateScrollView(i, e);
                yield return new WaitForSeconds(wait);
            }
            width = m_CellCount * m_CellSize.x + spacing.x * (m_CellCount - 1 < 0 ? 0 : m_CellCount - 1);
            width += padding.left + padding.right;
            var delta = m_Content.sizeDelta;
            delta.x = width;
            TrackRectTransform(delta);
        }
        m_Head = 0;
        m_Trail = m_RealCount - 1;
        m_LastFocus = 0;
    }

    public void SetDirection(Direction dir)
    {
        direction = dir;
        TrackRectTransform(Vector2.zero);
    }

    protected override void OnEnable()
    {
#if UNITY_EDITOR
        TrackRectTransform(Vector2.zero);
#endif
    }

    protected override void OnDisable()
    {
        m_Tracker.Clear();
    }

    Vector2 GetAnchorByDirection()
    {
        switch (direction)
        {
            case Direction.LeftToRight:
                return new Vector2(0, 0.5f);
            case Direction.RightToLeft:
                return new Vector2(1, 0.5f);
            case Direction.TopToBottom:
                return new Vector2(0.5f, 1);
            case Direction.BottomToTop:
                return new Vector2(0.5f, 0);
            default:
                return Vector2.zero;
        }
    }

    void LateUpdate()
    {
        var p = m_Content.anchoredPosition;
        if ((int)direction <= 1)
        {
            var x = Mathf.Abs(p.x) - m_Space;
            var f = Mathf.FloorToInt(x / (m_CellSize.x + spacing.x));
            if (p.x < 0 && direction == Direction.RightToLeft) return;
            if (p.x > 0 && direction == Direction.LeftToRight) return;
            if (f == m_LastFocus || f < 0) return;
            var delta = m_LastFocus - f;
            if (delta < 0 && m_Trail >= m_CellCount - 1) return;
            if (delta > 0 && m_Head <= 0) return;
            var dir = 1;
            if (direction == Direction.RightToLeft)
                dir = -1;
            else if (direction == Direction.LeftToRight)
                dir = 1;
            m_LastFocus = f;
            var c = Mathf.Abs(delta);
            for (int i = 0; i < c; i++)
            {
                if (delta < 0)
                {
                    var hi = m_Head % m_RealCount;
                    var h = m_Children[hi];
                    var pos = h.anchoredPosition;
                    var target = m_Trail + 1;
                    pos.x = dir * (m_Space + target * (m_CellSize.x + spacing.x));
                    h.anchoredPosition = pos;
                    onUpdateScrollView.Invoke(target, h);
                    m_Head++;
                    m_Trail++;
                }
                else
                {
                    var ti = m_Trail % m_RealCount;
                    var t = m_Children[ti];
                    var pos = t.anchoredPosition;
                    var target = m_Head - 1;
                    pos.x = dir * (m_Space + target * (m_CellSize.x + spacing.x));
                    t.anchoredPosition = pos;
                    onUpdateScrollView.Invoke(target, t);
                    m_Head--;
                    m_Trail--;
                }
            }
        }
        else
        {
            var y = Mathf.Abs(p.y) - m_Space;
            var f = Mathf.FloorToInt(y / (m_CellSize.y + spacing.y));
            if (p.y < 0 && direction == Direction.TopToBottom) return;
            if (p.y > 0 && direction == Direction.BottomToTop) return;
            if (f == m_LastFocus || f < 0) return;
            var delta = m_LastFocus - f;
            if (delta < 0 && m_Trail >= m_CellCount - 1) return;
            if (delta > 0 && m_Head <= 0) return;
            var dir = 1;
            if (direction == Direction.TopToBottom)
                dir = -1;
            else if (direction == Direction.BottomToTop)
                dir = 1;
            m_LastFocus = f;
            var c = Mathf.Abs(delta);
            for (int i = 0; i < c; i++)
            {
                if (delta < 0)
                {
                    var hi = m_Head % m_RealCount;
                    for (int j = 0; j < m_RawChildCount; j++)
                    {
                        var h = m_Children[hi + j];
                        var pos = h.anchoredPosition;
                        var target = m_Trail + m_RawChildCount + j;
                        if (target < m_CellCount)
                        {
                            pos.y = dir * (m_Space + (Mathf.CeilToInt(target / m_RawChildCount)) * (m_CellSize.y + spacing.y));
                            h.anchoredPosition = pos;
                            onUpdateScrollView.Invoke(target, h);
                        }
                    }
                    m_Head += m_RawChildCount;
                    m_Trail += m_RawChildCount;
                }
                else
                {
                    var ti = m_Trail % m_RealCount;
                    for (int j = 0; j < m_RawChildCount; j++)
                    {
                        var t = m_Children[ti + j];
                        var pos = t.anchoredPosition;
                        var target = m_Head - m_RawChildCount + j;
                        pos.y = dir * (m_Space + (Mathf.CeilToInt(target / m_RawChildCount)) * (m_CellSize.y + spacing.y));
                        t.anchoredPosition = pos;
                        onUpdateScrollView.Invoke(target, t);
                    }
                    m_Head -= m_RawChildCount;
                    m_Trail -= m_RawChildCount;
                }
            }
        }
    }
}
