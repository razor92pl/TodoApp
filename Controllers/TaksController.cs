using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TodoApp.Models;

namespace TodoApp.Controllers
{
    public class TasksController : Controller
    {
        public IActionResult Index()
        {
            var tasks = new List<TodoTask>(); // Pusta lista dla testów
            return View(tasks);
        }
    }
}