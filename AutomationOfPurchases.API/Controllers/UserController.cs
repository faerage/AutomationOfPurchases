using AutomationOfPurchases.API.Services;
using AutomationOfPurchases.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AutomationOfPurchases.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUser(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAllUsers()
        {
            var all = await _userService.GetAllAsync();
            return Ok(all);
        }

        [HttpPost]
        public async Task<ActionResult<UserDTO>> CreateUser(UserDTO dto)
        {
            var created = await _userService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetUser), new { id = created.UserId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserDTO dto)
        {
            var success = await _userService.UpdateAsync(id, dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var success = await _userService.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
