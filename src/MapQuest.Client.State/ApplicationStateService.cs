using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace MapQuest.Client.State;

public class ApplicationStateService
{
    const string applicationStateKey = "applicationState";

    public event EventHandler<ApplicationState>? ApplicationStateChanged;
    public ApplicationState State { get; private set; } = new ApplicationState();

    private IJSRuntime JSRuntime = null!;

    public async Task SetupAsync(IServiceProvider serviceProvider)
    {
        JSRuntime = serviceProvider.GetRequiredService<IJSRuntime>()!;
        var state = await GetItem<ApplicationState>(applicationStateKey);
        if (state != null)
        {
            State = state;
        }
        else
        {
            await SaveAsync();
        }
    }

    public async Task SaveAsync()
    {
        bool hasChanges = false;
        var previousState = await GetItem<ApplicationState>(applicationStateKey);
        hasChanges = (previousState != null && string.Compare(System.Text.Json.JsonSerializer.Serialize(previousState), System.Text.Json.JsonSerializer.Serialize(State)) != 0);
        await SetItem(applicationStateKey, State);
        if (hasChanges)
        {
            ApplicationStateChanged?.Invoke(this, State);
        }
    }

    public async Task<T?> GetItem<T>(string key)
    {
        var stringValue = await JSRuntime.InvokeAsync<string?>("localStorage.getItem", key);
        var result = stringValue switch
        {
            null => default(T),
            _ => System.Text.Json.JsonSerializer.Deserialize<T>(stringValue)
        };
        return result;
    }

    public async Task SetItem<T>(string key, T? value)
    {
        if (value == null)
        {
            await RemoveItem(key);
            return;
        }
        var stringValue = System.Text.Json.JsonSerializer.Serialize(value);
        await JSRuntime.InvokeVoidAsync("localStorage.setItem", key, stringValue);
    }

    public async Task RemoveItem(string key)
    {
        await JSRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }

    public async Task Clear()
    {
        await JSRuntime.InvokeVoidAsync("localStorage.clear");
    }
}
