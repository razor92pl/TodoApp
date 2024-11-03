using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using TodoApp.Data;
using TodoApp.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace TodoApp.Controllers
{
    public class TasksController : Controller
    {
        private readonly AppDbContext _context;

        public TasksController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? selectedDate, string filter = "all")
        {
            if (!selectedDate.HasValue)
            {
                selectedDate = DateTime.Today;
            }

            ViewBag.SelectedDate = selectedDate.Value.ToString("yyyy-MM-dd");
            ViewBag.Filter = filter;
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var tasks = from t in _context.Tasks select t;
            tasks = tasks.Where(t => t.DueDate.Date == selectedDate.Value.Date);

            switch (filter)
            {
                case "completed":
                    tasks = tasks.Where(t => t.IsCompleted);
                    break;
                case "pending":
                    tasks = tasks.Where(t => !t.IsCompleted);
                    break;
                case "all":
                default:
                    break;
            }

            // Powiadomienia o niewykonanych zadaniach z poprzednich dni
            var incompleteTasks = await _context.Tasks
                .Where(t => !t.IsCompleted && t.DueDate.Date < today)
                .GroupBy(t => t.DueDate.Date)
                .Select(group => new { Date = group.Key, Count = group.Count() })
                .ToListAsync();

            if (incompleteTasks.Any())
            {
                var message = "Masz niewykonane zadania z dni: ";
                foreach (var task in incompleteTasks)
                {
                    message += $"{task.Date.ToShortDateString()} ({task.Count} zadań), ";
                }
                ViewBag.Notification = message.TrimEnd(',', ' ');
            }

            // Powiadomienia o zadaniach do wykonania jutro
            var tomorrowTasks = await _context.Tasks
                .Where(t => !t.IsCompleted && t.DueDate.Date == tomorrow)
                .ToListAsync();

            if (tomorrowTasks.Any())
            {
                var message = $"Jutro masz {tomorrowTasks.Count} zadań do wykonania.";
                ViewBag.TomorrowNotification = message;
            }

            return View(await tasks.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,DueDate,IsCompleted")] TodoTask task)
        {
            if (ModelState.IsValid)
            {
                _context.Add(task);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { selectedDate = task.DueDate.ToString("yyyy-MM-dd") });
            }
            return View(task);
        }

        public async Task<IActionResult> Edit(int? id, string selectedDate)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            ViewBag.SelectedDate = selectedDate;
            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,DueDate,IsCompleted")] TodoTask task, string selectedDate)
        {
            if (id != task.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(task);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskExists(task.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { selectedDate = selectedDate });
            }
            ViewBag.SelectedDate = selectedDate;
            return View(task);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFilteredTasks(DateTime selectedDate, string filter)
        {
            var tasks = from t in _context.Tasks select t;
            tasks = tasks.Where(t => t.DueDate.Date == selectedDate.Date);

            switch (filter)
            {
                case "completed":
                    tasks = tasks.Where(t => t.IsCompleted);
                    break;
                case "pending":
                    tasks = tasks.Where(t => !t.IsCompleted);
                    break;
                case "all":
                default:
                    break;
            }

            _context.Tasks.RemoveRange(tasks);
            await _context.SaveChangesAsync();
            return Ok();
        }

        private bool TaskExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
    }
}
