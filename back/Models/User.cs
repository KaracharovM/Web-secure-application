using System;
using System.ComponentModel.DataAnnotations;

public class User
{
    public int Id { get; set; }
    
    [Required]
    public string Username { get; set; }
    
    [Required]
    public string PasswordHash { get; set; }
    
    [Required]
    public string Salt { get; set; }
    
    public string RefreshToken { get; set; } = string.Empty;
    
    public DateTime RefreshTokenExpiryTime { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? LastLoginAt { get; set; } // Добавляем поле для отслеживания последнего входа
    
    public bool IsLocked { get; set; }
    
    public int LoginAttempts { get; set; }
}