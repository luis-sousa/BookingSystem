using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using FluentValidation;
using UserService.DTOs;
using UserService.Middleware;
using UserService.Models;
using UserService.Repositories;
using UserService.Services;

public class UserAuthService : IUserService
{
    private readonly ILogger<UserAuthService> _logger;
    private readonly IUserRepository _repo;
    private readonly IJwtService _jwt;
    private readonly IMapper _mapper;

    private readonly IValidator<CreateUserDto> _createValidator;
    private readonly IValidator<UpdateUserDto> _updateValidator;
    private readonly IValidator<UpdatePasswordDto> _passwordValidator;
    private readonly IValidator<LoginDto> _loginValidator;

    public UserAuthService(
        IUserRepository repo,
        IJwtService jwt,
        IMapper mapper,
        IValidator<CreateUserDto> createValidator,
        IValidator<UpdateUserDto> updateValidator,
        IValidator<UpdatePasswordDto> passwordValidator,
        IValidator<LoginDto> loginValidator,
        ILogger<UserAuthService> logger
    )
    {
        _repo = repo;
        _jwt = jwt;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _passwordValidator = passwordValidator;
        _loginValidator = loginValidator;
        _logger = logger;
    }

    // -------------------- MÉTODOS AUXILIARES --------------------

    private async Task<User> GetUserOrThrowAsync(int id)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null)
        {
            _logger.LogInformation("User Not Found {Username}", user?.Username);
            throw new BusinessException(404, 4001, "Utilizador não encontrado");
        }
        return user;
    }

    private async Task ValidateDtoAsync<T>(IValidator<T> validator, T dto)
    {
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var errors = string.Join("; ", result.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
            throw new BusinessException(400,4000, errors);
        }
    }

    // -------------------- USER CRUD --------------------

    public async Task<List<UserDto>> GetAllAsync()
    {
        _logger.LogInformation("A obter todos os utilizadores...");
        var users = await _repo.GetAllAsync();
        return _mapper.Map<List<UserDto>>(users);
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await GetUserOrThrowAsync(id);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        await ValidateDtoAsync(_createValidator, dto);

        if (await _repo.EmailExistsAsync(dto.Email))
        {
            _logger.LogWarning("Email {Email} já utilizado", dto.Email);
            throw new BusinessException(409, 4002, "Email já registado");
        }

        var user = _mapper.Map<User>(dto);
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        await _repo.AddAsync(user);
        await _repo.SaveChangesAsync();

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto?> UpdateAsync(int id, UpdateUserDto dto)
    {
        var user = await GetUserOrThrowAsync(id);
        await ValidateDtoAsync(_updateValidator, dto);

        _mapper.Map(dto, user);
        await _repo.SaveChangesAsync();

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto?> PatchAsync(int id, JsonPatchDocument<UpdateUserDto> patch)
    {
        var user = await GetUserOrThrowAsync(id);
        var dto = _mapper.Map<UpdateUserDto>(user);

        patch.ApplyTo(dto);
        await ValidateDtoAsync(_updateValidator, dto);

        _mapper.Map(dto, user);
        await _repo.SaveChangesAsync();

        return _mapper.Map<UserDto>(user);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await GetUserOrThrowAsync(id);
        await _repo.DeleteAsync(user);
        await _repo.SaveChangesAsync();
        return true;
    }

    // -------------------- AUTENTICAÇÃO --------------------

    public async Task<AuthDto?> LoginAsync(string email, string password)
    {
        _logger.LogInformation("A iniciar o login com {Email} : {Password}", email, password);

        var dto = new LoginDto { Email = email, Password = password };
        await ValidateDtoAsync(_loginValidator, dto);

        var user = await _repo.GetByEmailAsync(email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            _logger.LogWarning("Login inválido");
            throw new BusinessException(404, 4011, "Login inválido");
        }

        var userdto = _mapper.Map<UserDto>(user);

        // Geração do JWT
        var token = _jwt.GenerateToken(userdto, user.Role);

        _logger.LogInformation("Token gerado com sucesso {Email} : {Password} - {Token} ", email, password, token);

        return new AuthDto
        {
            User = _mapper.Map<UserDto>(user),
            Token = token
        };
    }

    public async Task<bool> UpdatePasswordAsync(int id, UpdatePasswordDto dto)
    {
        await ValidateDtoAsync(_passwordValidator, dto);

        var user = await GetUserOrThrowAsync(id);

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
        {
            _logger.LogWarning("Password atual incorreta");
            throw new BusinessException(400, 4004, "Password atual incorreta");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _repo.SaveChangesAsync();

        return true;
    }
}
