using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;
using UserService.Services;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/User")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly IJwtService _jwt;

        public UserController(IUserService service, IJwtService jwt)
        {
            _service = service;
            _jwt = jwt;
        }

        // GET /api/user
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(List<UserDto>), 200)]
        public async Task<ActionResult<List<UserDto>>> GetAll()
        {
            var users = await _service.GetAllAsync();
            return Ok(users);
        }

        // GET /api/user/{id}
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UserDto>> GetById(int id)
        {
            var user = await _service.GetByIdAsync(id);
            return user != null ? Ok(user) : NotFound();
        }

        // POST /api/user
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UserDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<UserDto>> Create(CreateUserDto dto)
        {
            var user = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = user.IdUser }, user);
        }

        // PUT /api/user/{id}
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UserDto>> Update(int id, UpdateUserDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            return updated != null ? Ok(updated) : NotFound();
        }

        // PATCH /api/user/{id}
        [HttpPatch("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UserDto>> Patch(int id, JsonPatchDocument<UpdateUserDto> patch)
        {
            var updated = await _service.PatchAsync(id, patch);
            return updated != null ? Ok(updated) : NotFound();
        }

        // DELETE /api/user/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        // POST /api/user/login
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<AuthDto>> Login(LoginDto dto)
        {
            var authResult = await _service.LoginAsync(dto.Email, dto.Password);

            if (authResult == null)
                return Unauthorized();

            // authResult já contém o token gerado no serviço
            return Ok(authResult);
        }
    }
}