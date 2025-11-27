using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace queryHelp.DTOs
{
    public record UserCreateDto([Required] string Username, [Required] string Email, [Required] string Password);
    public record UserReadDto(int Id, string Username, string Email, string Role, System.DateTime CreatedAt);
    public record UserUpdateDto(string? Email, string? Role);
}