using System.Text;
using System.Text.Json;
using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;

namespace PokemonTabletopAdventures.CoreApi.IntegrationTests;

[AllureNUnit]
[AllureEpic("Pokemon Tabletop Adventures Api")]
public abstract class BaseTest
{
    protected void AllureArrange(string stepTitle, Action step)
    {
        AllureApi.Step($"Arrange: {stepTitle}", step);
    }
    
    protected async Task AllureArrangeAsync(string stepTitle, Func<Task> step)
    {
        await AllureApi.Step($"Arrange: {stepTitle}", step);
    }

    protected T AllureArrange<T>(string stepTitle, Func<T> step)
    {
        return AllureApi.Step($"Arrange: {stepTitle}", () => ExpandStep(step));
    }

    protected async Task<T> AllureArrangeAsync<T>(string stepTitle, Func<Task<T>> step)
    {
        return await AllureApi.Step($"Arrange: {stepTitle}", () => ExpandStep(step));
    }
    
    protected void AllureAct(string stepTitle, Action step)
    {
        AllureApi.Step($"Act: {stepTitle}", step);
    }
    
    protected async Task AllureAct(string stepTitle, Func<Task> step)
    {
        await AllureApi.Step($"Act: {stepTitle}", step);
    }

    protected T AllureAct<T>(string stepTitle, Func<T> step)
    {
        return AllureApi.Step($"Act: {stepTitle}", () => ExpandStep(step));
    }

    protected async Task<T> AllureAct<T>(string stepTitle, Func<Task<T>> step)
    {
        return await AllureApi.Step($"Act: {stepTitle}", () => ExpandStep(step));
    }
    
    protected void AllureAssert(string stepTitle, Action step)
    {
        AllureApi.Step($"Assert: {stepTitle}", step);
    }
    
    protected async Task AllureAssert(string stepTitle, Func<Task> step)
    {
        await AllureApi.Step($"Assert: {stepTitle}", step);
    }

    private T ExpandStep<T>(Func<T> step)
    {
        var result = step();
        var json = JsonSerializer.Serialize(result);
        var bytes = Encoding.ASCII.GetBytes(json);
        AllureApi.AddAttachment(nameof(T), "application/json", bytes, ".json");
        return result;
    }

    private async Task<T> ExpandStep<T>(Func<Task<T>> step)
    {
        var result = await step();
        var json = JsonSerializer.Serialize(result);
        var bytes = Encoding.ASCII.GetBytes(json);
        AllureApi.AddAttachment(nameof(T), "application/json", bytes, ".json");
        return result;
    }
}