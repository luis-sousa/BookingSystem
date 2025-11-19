using AutoMapper;
using FluentValidation;
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
        private readonly IMapper _mapper;
        private readonly IValidator<CreateUserDto> _createValidator;
        private readonly IValidator<UpdateUserDto> _updateValidator;
        private readonly IValidator<JsonPatchDocument<UpdateUserDto>> _patchValidator;
        private readonly IValidator<LoginDto> _loginValidator;

        public UserController(
            IUserService service,
            IJwtService jwt,
            IMapper mapper,
            IValidator<CreateUserDto> createValidator,
            IValidator<UpdateUserDto> updateValidator,
            IValidator<JsonPatchDocument<UpdateUserDto>> patchValidator,
            IValidator<LoginDto> loginValidator)
        {
            _service = service;
            _jwt = jwt;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _patchValidator = patchValidator;
            _loginValidator = loginValidator;
        }

        // ----------------------------
        // GET /api/user
        // ----------------------------
        [HttpGet]
        [Authorize] // qualquer usuário autenticado
        [ProducesResponseType(typeof(List<UserDto>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var users = await _service.GetAllAsync();
            return Ok(users);
        }

        // ----------------------------
        // GET /api/user/{id}
        // ----------------------------
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _service.GetByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // ----------------------------
        // POST /api/user
        // ----------------------------
        [HttpPost]
        [AllowAnonymous] // qualquer pessoa pode criar/login
        [ProducesResponseType(typeof(UserDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            var validation = await _createValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return BadRequest(validation.Errors);

            if (await _service.EmailExistsAsync(dto.Email))
                return BadRequest("Email já existe");

            var user = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = user.IdUser }, user);
        }

        // ----------------------------
        // PUT /api/user/{id}
        // ----------------------------
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
        {
            var validation = await _updateValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return BadRequest(validation.Errors);

            var updated = await _service.UpdateAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // ----------------------------
        // PATCH /api/user/{id}
        // ----------------------------
        [HttpPatch("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Patch(int id, [FromBody] JsonPatchDocument<UpdateUserDto> patchDoc)
        {
            var validation = await _patchValidator.ValidateAsync(patchDoc);
            if (!validation.IsValid) return BadRequest(validation.Errors);

            var user = await _service.GetByIdAsync(id);
            if (user == null) return NotFound();

            var userToPatch = _mapper.Map<UpdateUserDto>(user);
            patchDoc.ApplyTo(userToPatch, ModelState);

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updated = await _service.PatchAsync(id, userToPatch);
            return Ok(updated);
        }

        // ----------------------------
        // DELETE /api/user/{id}
        // ----------------------------
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // só admin pode deletar
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent(); // 204
        }

        // ----------------------------
        // POST /api/user/login
        // ----------------------------
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(string), 200)] // JWT token
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var validation = await _loginValidator.ValidateAsync(dto);
            if (!validation.IsValid) return BadRequest(validation.Errors);

            var userEntity = await _service.LoginAsync(dto.Email, dto.Password);
            if (userEntity == null) return Unauthorized("Credenciais inválidas");

            var userDto = _mapper.Map<UserDto>(userEntity);
            var token = _jwt.GenerateToken(userDto, "User");

            return Ok(token);
        }
    }
    }



