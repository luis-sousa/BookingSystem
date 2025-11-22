using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;
using UserService.Services;

[ApiController]
[Route("api/v{version:apiVersion}/User")]
[ApiVersion("2.0")]
public class UserController2 : ControllerBase
{
    private readonly IUserService _userService;

    public UserController2(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPatch("{id}/password")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdatePassword(int id, [FromBody] UpdatePasswordDto dto)
    {
        await _userService.UpdatePasswordAsync(id, dto);
        return NoContent();
    }
}