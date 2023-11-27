namespace Wipedir.Menu;
public record MenuItem
{
    #region - properties -

    #region [ItemName]
    public string ItemName { get; init; } = string.Empty;
    #endregion

    #region [Selected]
    public bool Selected { get; set; } = false;
    #endregion

    #region [Open]
    public bool Open { get; set; } = false;
    #endregion

    #region [SubItems]
    public List<MenuItem> SubItems { get; init; } = new();
    #endregion

    #endregion
}
