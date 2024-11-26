using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using ToDoList.Models.ToDoListModels;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace ToDoList.Controllers;

public class ToDoListController : Controller
{
    private readonly IMemoryCache _memoryCache;
    private static int id = 0;
    private const string filePath = "Controllers\\Data\\items.json";
    public ToDoListController(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    [HttpGet]
    public async Task<IActionResult> ToDoList()
    {
        try
        {
            if (!_memoryCache.TryGetValue("data", out List<ToDoListItem> items))
            {
                if (!System.IO.File.Exists(filePath))
                {
                    System.IO.File.Create(filePath);
                }
                else
                {
                    string fileData = System.IO.File.ReadAllText(filePath);
                    items = !string.IsNullOrEmpty(fileData) ? JsonSerializer.Deserialize<List<ToDoListItem>>(fileData) : items;
                }

                _memoryCache.Set("data", items);
            }

            return View(items != null ? items : new List<ToDoListItem>());
        }
        catch (Exception ex)
        {
            return View(new List<ToDoListItem>());
        }
    }

    public async Task<IActionResult> ToggleCompletion([FromBody] int id)
    {
        try
        {
            string fileData = System.IO.File.ReadAllText(filePath);
            var items = !string.IsNullOrEmpty(fileData) ? JsonSerializer.Deserialize<List<ToDoListItem>>(fileData) : new List<ToDoListItem>();

            var item = items.FirstOrDefault(x => x.Id == id);

            if (item != null)
            {
                item.IsCompleted = !item.IsCompleted;
                await System.IO.File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(items));
                _memoryCache.Set("data", items);
            }

            return PartialView("_ToDoListPartial", items);
        }
        catch (Exception ex)
        {
            return RedirectToAction(nameof(ToDoList));
        }
    }

    public async Task<IActionResult> AddTask([FromBody] string title)
    {
        try
        {
            string fileData = System.IO.File.ReadAllText(filePath);
            var items = !string.IsNullOrEmpty(fileData) ? JsonSerializer.Deserialize<List<ToDoListItem>>(fileData) : new List<ToDoListItem>();

            int id = items.Any() ? items.Max(x => x.Id) : 0;

            items.Add(new ToDoListItem
            {
                Id = id + 1,
                Title = title,
            });

            await System.IO.File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(items));
            _memoryCache.Set("data", items);

            return PartialView("_ToDoListPartial", items); // Return the updated partial view
        }
        catch (Exception ex)
        {
            return RedirectToAction(nameof(ToDoList));
        }
    }


    public async Task<IActionResult> DeleteTask([FromBody] int id)
    {
        try
        {
            string fileData = System.IO.File.ReadAllText(filePath);
            var items = !string.IsNullOrEmpty(fileData) ? JsonSerializer.Deserialize<List<ToDoListItem>>(fileData) : new List<ToDoListItem>();

            items.Remove(items.FirstOrDefault(x => x.Id == id));

            if (items.Any())
            {
                await System.IO.File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(items));
                _memoryCache.Set("data", items);
            }
            else
            {
                await System.IO.File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(new List<ToDoListItem>()));
                _memoryCache.Remove("data");
            }

            return PartialView("_ToDoListPartial", items);
        }
        catch (Exception ex)
        {
            return RedirectToAction(nameof(ToDoList));

        }
    }
}
