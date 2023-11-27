namespace Wipedir.Menu;
public class Menu
{
    #region - ctor -
    public Menu(string title, List<MenuItem> items)
    {
        ArgumentNullException.ThrowIfNull(title, nameof(title));
        ArgumentNullException.ThrowIfNull(items, nameof(items));
        if (string.IsNullOrEmpty(title) || string.IsNullOrWhiteSpace(title))
            throw new ArgumentException($"'{nameof(title)}' can't be empty or only containing whitespaces.");
        if (items.Count == 0)
            throw new ArgumentException($"'{nameof(items)}' can't be empty.");

        m_CurrentIndex = 0;
        m_Title = title;
        Items = [.. items];
    }
    #endregion

    #region - properties -

    #region [Items]
    public List<MenuItem> Items { get; set; } = new();
    #endregion

    #region - private properties -

    #region [m_Title]
    private string m_Title;
    #endregion

    #region [m_CurrentIndex]
    private uint m_CurrentIndex { get; set; }
    private int m_CurrentIndexInt => (int)m_CurrentIndex;
    #endregion

    #region [m_CurrentPosition]
    private uint m_CurrentPosition => m_CurrentIndex % m_MaxLines;
    #endregion

    #region [m_MaxLines]
    private uint m_MaxLines => (uint)(Console.BufferHeight - 2);
    #endregion

    #region [m_CanMoveDown]
    private bool m_CanMoveDown => m_CurrentIndex == Items.Count - 1;
    #endregion

    #region [m_CanMoveUp]
    private bool m_CanMoveUp => m_CurrentIndex != 0;
    #endregion

    #region [m_CanOpenSubList]
    private bool m_CanOpenSubList => Items[m_CurrentIndexInt].SubItems.Any() && !Items[m_CurrentIndexInt].Open;
    #endregion

    #region [m_CanCloseSubList]
    private bool m_CanCloseSubList => Items[m_CurrentIndexInt].SubItems.Any() && Items[m_CurrentIndexInt].Open;
    #endregion

    #region [m_ItemCount]
    private int m_ItemCount => (Items.Count + Items.Where(x => x.SubItems.Any() && x.Open).Sum(x => x.SubItems.Count));
    #endregion

    #endregion

    #endregion

    #region - methods -

    #region [Execute]
    public void Execute()
    {
        var input = Console.ReadKey();
    }
    #endregion

    #region [__HandleKeyInput]
    private void __HandleKeyInput(ConsoleKey key)
    {
        switch (key)
        {
            case ConsoleKey.Enter:
                Items[m_CurrentIndexInt].Selected = !Items[m_CurrentIndexInt].Selected;
                return;
            case ConsoleKey.UpArrow:
                if (m_CanMoveUp)
                    m_CurrentIndex--;
                return;
            case ConsoleKey.DownArrow: 
                if(m_CanMoveDown)
                    m_CurrentIndex++;
                return;
            case ConsoleKey.LeftArrow:
                if (m_CanOpenSubList)
                    Items[m_CurrentIndexInt].Open = true;
                return;
            case ConsoleKey.RightArrow:
                if (m_CanCloseSubList)
                    Items[m_CurrentIndexInt].Open = false;
                return;
        }
    }
    #endregion

    #endregion
}
