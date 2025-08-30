using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Ssera.Client.Infra.Components;

public partial class MultiSelectCheckBoxList<T>
{
    private readonly string _componentId = Guid.NewGuid().ToString();

    [Parameter, EditorRequired]
    public IEnumerable<KeyValuePair<string, T>> Data { get; set; } = null!;

    [Parameter, EditorRequired]
    public ISet<T> Values { get; set; } = null!;

    [Parameter]
    public EventCallback<ISet<T>> ValuesChanged { get; set; }

    [Parameter, EditorRequired]
    public int ColumnCount { get; set; }

    [Parameter]
    public string Style { get; set; } = "";

    [Parameter]
    public RenderFragment<RenderFragment>? Template { get; set; }

    private async Task OnCheckboxValueChanged(bool state, T value)
    {
        _ = state ? Values.Add(value) : Values.Remove(value);
        await ValuesChanged.InvokeAsync(Values);
    }
}
