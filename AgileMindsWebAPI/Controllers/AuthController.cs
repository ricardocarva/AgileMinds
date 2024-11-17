using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using AgileMinds.Shared.Models;

using AgileMindsWebAPI.Data;

using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] Login loginModel)
    {
        var user = _context.Users.FirstOrDefault(u => u.Username == loginModel.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(loginModel.Password, user.Password))
        {
            return Unauthorized("Invalid credentials");
        }

        var token = GenerateJwtToken(user);
        return Ok(new { Token = token });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel registerModel)
    {
        if (_context.Users.Any(u => u.Username == registerModel.Username))
        {
            return BadRequest("Username already exists.");
        }

        if (registerModel.Password != registerModel.ConfirmPassword)
        {
            return BadRequest("Passwords do not match.");
        }

        // Hash the password before storing it
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerModel.Password);
        var user = new User
        {
            FirstName = registerModel.FirstName,
            LastName = registerModel.LastName,
            Username = registerModel.Username,
            Password = hashedPassword,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        return Ok(new { Token = token });
    }


    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] {
                new Claim("username", user.Username),
                new Claim("userid", user.Id.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
