using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCryptNet = BCrypt.Net.BCrypt;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly IEventJuryRepository _eventJuryRepo;

        public AuthService(
            IUserRepository userRepository,
            IDepartmentRepository departmentRepository,
            IRoleRepository roleRepository,
            IUserRoleRepository userRoleRepository,
            IConfiguration configuration,
            ILogger<AuthService> logger,
            IEventJuryRepository eventJuryRepo)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            _userRoleRepository = userRoleRepository ?? throw new ArgumentNullException(nameof(userRoleRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventJuryRepo = eventJuryRepo ?? throw new ArgumentNullException(nameof(eventJuryRepo));
        }

        public async Task<bool> RegisterAsync(RegisterDto dto, Guid? createdBy = null)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "Registration data cannot be null.");

            if (await _userRepository.ExistsByEmailAsync(dto.Email))
            {
                _logger.LogWarning("Registration failed. Email already exists: {Email}", dto.Email);
                return false;
            }

            var department = await _departmentRepository.GetByNameAsync(dto.DepartmentName);
            var hashedPassword = BCryptNet.HashPassword(dto.Password);

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = hashedPassword,
                DepartmentId = department?.DepartmentId,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
                CreatedBy = createdBy,
                UpdatedBy = createdBy
            };

            await _userRepository.AddAsync(user);

            if (createdBy == null)
            {
                user.CreatedBy = user.UserId;
                user.UpdatedBy = user.UserId;
                await _userRepository.UpdateAsync(user);
            }
            var contributorRole = await _roleRepository.GetByNameAsync("Contributor");
            if (contributorRole == null)
                throw new InvalidOperationException(
                    "Registration failed. Default role 'Contributor' is not configured in the system.");

            var userRole = new UserRole
            {
                UserId = user.UserId,
                RoleId = contributorRole.RoleId,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
                CreatedBy = createdBy ?? user.UserId,
                UpdatedBy = createdBy ?? user.UserId
            };

            await _userRoleRepository.AddAsync(userRole);

            _logger.LogInformation("User registered successfully: {Email}", dto.Email);
            return true;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "Login data cannot be null.");

            _logger.LogInformation("Login attempt for user: {Email}", dto.Email);

            var user = await _userRepository.GetUserWithRolesByEmailAsync(dto.Email);

            if (user == null || !BCryptNet.Verify(dto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed due to invalid credentials: {Email}", dto.Email);
                return null;
            }

            var eventIds = await _eventJuryRepo.GetEventIdsForUserAsync(user.UserId);
            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET");

            if (string.IsNullOrWhiteSpace(secretKey))
                throw new InvalidOperationException("JWT_SECRET environment variable is missing in AuthService.");

            var issuer = _configuration["JwtSettings:Issuer"];
            var audience = _configuration["JwtSettings:Audience"];
            var expiresMinutes = int.Parse(_configuration["JwtSettings:ExpiresInMinutes"]);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);



            // Base identity claims
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.Name)
            };

            // Role claims
            foreach (var role in user.UserRoleUsers.Select(ur => ur.Role.RoleName))
                claims.Add(new Claim(ClaimTypes.Role, role));

            // Event jury claims
            foreach (var eventId in eventIds)
                claims.Add(new Claim("event_jury", eventId.ToString()));

            // Create JWT token
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            _logger.LogInformation("Login successful for user: {Email}", dto.Email);

            return new AuthResponseDto
            {
                Token = tokenString,
                Name = user.Name,
                Email = user.Email,
                Roles = user.UserRoleUsers.Select(ur => ur.Role.RoleName).ToList(),
                UserId = user.UserId,
                EventJuryIds = eventIds
            };
        }
    }
}
