using Microsoft.AspNetCore.Mvc;
using BeanScene.Web.Models;
using BeanScene.Web.Models.DataStructures;



public class DoublyLinkedListController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var model = new DoublyLinkedListViewModel{
            NumbersInput = string.Empty,
            ForwardResult = string.Empty,
            BackwardResult = string.Empty
        };
        return View();
    }

    [HttpPost]
    public IActionResult Index(string NumbersInput)
    {
        if (string.IsNullOrWhiteSpace(NumbersInput))
            return View(new DoublyLinkedListViewModel());

        // Parse user input
        var numbers = NumbersInput
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(n => int.Parse(n.Trim()))
            .ToList();

        // Build the linked list
        var dll = new DoublyLinkedList();
        foreach (var num in numbers)
            dll.AddLast(num);

        // Build forward + backward results
        string forward = dll.DisplayForward();
        string backward = dll.DisplayBackward();


        var model = new DoublyLinkedListViewModel
        {
            NumbersInput = NumbersInput,
            ForwardResult = forward,
            BackwardResult = backward
        };

        return View(model);
    }
}
