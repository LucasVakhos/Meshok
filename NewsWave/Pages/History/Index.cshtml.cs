using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsWave.NewsMaker;

namespace NewsWave.Pages.History;

public sealed class IndexModel : PageModel
{
    private readonly NewsMakerHistoryStore _history;

    public IndexModel(NewsMakerHistoryStore history) => _history = history;

    public IReadOnlyList<NewsMakerSnapshot> Runs { get; private set; } = [];

    public void OnGet() => Runs = _history.GetRecent(50);
}
