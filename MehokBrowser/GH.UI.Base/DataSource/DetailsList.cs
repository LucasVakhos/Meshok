namespace MehokBrowser.Controls
{
    /// <summary>Список детальных источников данных.</summary>
    internal class DetailsList : System.Collections.Generic.List<DataSource>
    {
        internal void RegDataSource(DataSource detail) { Add(detail); }
        internal void UnRegDataSource(DataSource detail) { Remove(detail); }
        internal void ReOpenDetailsByTimer()
        {
            foreach (var ds in this)
                ds.ReOpenByTimer();
        }
    }
}
