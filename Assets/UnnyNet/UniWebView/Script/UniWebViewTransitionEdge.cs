/// <summary>
/// An enum to identify transition edge from or to when the UniWebView
/// transition happens. You can specify an edge in Show() or Hide() methods of web view.
/// </summary>
public enum UniWebViewTransitionEdge
{
    /// <summary>
    /// No transition when showing or hiding.
    /// </summary>
    None = 0,
    /// <summary>
    /// Transit the web view from/to top.
    /// </summary>
    Top,
    /// <summary>
    /// Transit the web view from/to left.
    /// </summary>
    Left,
    /// <summary>
    /// Transit the web view from/to bottom.
    /// </summary>
    Bottom,
    /// <summary>
    /// Transit the web view from/to right.
    /// </summary>
    Right
}
