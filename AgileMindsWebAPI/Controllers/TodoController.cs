using AgileMinds.Shared.Models;
using AgileMindsWebAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace AgileMindsWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TodosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/todoitems
        [HttpPost]
        public async Task<IActionResult> CreateTodoItem([FromBody] Todo todoItem)
        {
            if (todoItem == null)
            {
                return BadRequest("Todo item data is null");
            }

            _context.Todos.Add(todoItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTodoItemById), new { id = todoItem.Id }, todoItem);
        }

        // GET: api/todoitems/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserTodos(int userId)
        {
            var todos = await _context.Todos
                .Where(t => t.UserID == userId)
                .ToListAsync();

            return Ok(todos);
        }

        // GET: api/todoitems/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTodoItemById(int id)
        {
            var todoItem = await _context.Todos.FindAsync(id);

            if (todoItem == null)
            {
                return NotFound("Todo item not found");
            }

            return Ok(todoItem);
        }

        // PUT: api/todoitems/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodoItem(int id, [FromBody] Todo updatedTodoItem)
        {
            if (id != updatedTodoItem.Id)
            {
                return BadRequest("Todo item ID mismatch");
            }

            var todoItem = await _context.Todos.FindAsync(id);
            if (todoItem == null)
            {
                return NotFound("Todo item not found");
            }

            // Update fields
            todoItem.Text = updatedTodoItem.Text;
            todoItem.IsCompleted = updatedTodoItem.IsCompleted;
            todoItem.Date = updatedTodoItem.Date;

            await _context.SaveChangesAsync();

            return Ok(todoItem);
        }

        // DELETE: api/todoitems/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodoItem(int id)
        {
            var todoItem = await _context.Todos.FindAsync(id);
            if (todoItem == null)
            {
                return NotFound("Todo item not found");
            }

            _context.Todos.Remove(todoItem);
            await _context.SaveChangesAsync();

            return Ok("Todo item deleted successfully");
        }
    }
}
