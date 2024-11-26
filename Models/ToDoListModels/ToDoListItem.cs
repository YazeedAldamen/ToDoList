using System.ComponentModel.DataAnnotations;

namespace ToDoList.Models.ToDoListModels;

public class ToDoListItem
{
    public int Id { get; set; }
    [MaxLength(100)]
    public string Title { get; set; }
    public bool IsCompleted { get; set; } = false;
}
