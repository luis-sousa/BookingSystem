using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;
using UserService.Models;
using UserService.Repositories;

[ApiController]
[Route("api/v{version:apiVersion}/User")]
[ApiVersion("2.0")]
public class UserController2 : ControllerBase
{
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserController2(IUserRepository repo, IPasswordHasher<User> passwordHasher)
    {
        _repo = repo;
        _passwordHasher = passwordHasher;
    }

    [HttpPatch("{id}/password")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdatePassword(int id, [FromBody] UpdatePasswordDto dto)
    {
        // find user
        var user = await _repo.GetByIdAsync(id);
        if (user == null)
            return NotFound();

        // check atual password
        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.CurrentPassword);
        if (result == PasswordVerificationResult.Failed)
            return BadRequest("Current password is incorrect.");

        // update password
        user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
        await _repo.SaveChangesAsync();

        // return 204 NoContent     
        return NoContent();
    }
}