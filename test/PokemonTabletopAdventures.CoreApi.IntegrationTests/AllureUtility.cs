using System.Text;
using System.Text.Json;
using Allure.Net.Commons;

namespace PokemonTabletopAdventures.CoreApi.IntegrationTests;

public static class AllureUtility
{
    public static void Arrange(string stepTitle, Action step)
    {
        AllureApi.Step($"Arrange: {stepTitle}", step);
    }
    
    public static async Task ArrangeAsync(string stepTitle, Func<Task> step)
    {
        await AllureApi.Step($"Arrange: {stepTitle}", step);
    }

    public static T Arrange<T>(string stepTitle, Func<T> step)
    {
        return AllureApi.Step($"Arrange: {stepTitle}", () => ExpandStep(step));
    }

    public static async Task<T> ArrangeAsync<T>(string stepTitle, Func<Task<T>> step)
    {
        return await AllureApi.Step($"Arrange: {stepTitle}", () => ExpandStep(step));
    }
    
    public static void Act(string stepTitle, Action step)
    {
        AllureApi.Step($"Act: {stepTitle}", step);
    }
    
    public static async Task Act(string stepTitle, Func<Task> step)
    {
        await AllureApi.Step($"Act: {stepTitle}", step);
    }

    public static T Act<T>(string stepTitle, Func<T> step)
    {
        return AllureApi.Step($"Act: {stepTitle}", () => ExpandStep(step));
    }

    public static async Task<T> Act<T>(string stepTitle, Func<Task<T>> step)
    {
        return await AllureApi.Step($"Act: {stepTitle}", () => ExpandStep(step));
    }
    
    public static void Assert(string stepTitle, Action step)
    {
        AllureApi.Step($"Assert: {stepTitle}", step);
    }
    
    public static async Task Assert(string stepTitle, Func<Task> step)
    {
        await AllureApi.Step($"Assert: {stepTitle}", step);
    }

    private static T ExpandStep<T>(Func<T> step)
    {
        var result = step();
        var json = JsonSerializer.Serialize(result);
        var bytes = Encoding.ASCII.GetBytes(json);
        var typeName = typeof(T).Name;
        var generics = typeof(T).GetGenericArguments().Select(x => x.Name).ToList();
        var fileName = generics.Count == 0 ? typeName : $"{typeName}<{string.Join(", ", generics)}>";
        AllureApi.AddAttachment(fileName, "application/json", bytes, ".json");
        return result;
    }

    private static async Task<T> ExpandStep<T>(Func<Task<T>> step)
    {
        var result = await step();
        var json = JsonSerializer.Serialize(result);
        var bytes = Encoding.ASCII.GetBytes(json);
        AllureApi.AddAttachment(nameof(T), "application/json", bytes, ".json");
        return result;
    }
}