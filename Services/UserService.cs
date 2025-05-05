using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using WisVestAPI.Models;

namespace WisVestAPI.Services
{
 public class UserService
 {
 private readonly string _filePath = "users.json";
 private readonly PasswordHasher<User> _passwordHasher;

 public UserService()
 {
  _passwordHasher = new PasswordHasher<User>();
 }

 // Load all users from JSON
 public List<User> GetAllUsers()
 {
  try
  {
  if (!File.Exists(_filePath))
  {
   return new List<User>();
  }

  var json = File.ReadAllText(_filePath);
  return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
  }
  catch (Exception ex)
  {
  Console.WriteLine($"Error loading users: {ex.Message}");
  return new List<User>();
  }
 }

 // Save all users to JSON
 public void SaveAllUsers(List<User> users)
 {
  try
  {
  var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
  File.WriteAllText(_filePath, json);
  }
  catch (Exception ex)
  {
  Console.WriteLine($"Error saving users: {ex.Message}");
  }
 }

 public bool UserExists(string email)
 {
  var users = GetAllUsers();
  return users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
 }

 public User? GetUserByEmail(string email)
 {
  var users = GetAllUsers();
  return users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
 }

public void AddUser(User user)
{
var users = GetAllUsers();
 user.Id = users.Count > 0 ? users.Max(u => u.Id) + 1 : 1;
 users.Add(user);
 SaveAllUsers(users);
}


 public void UpdateUser(User user)
 {
  var users = GetAllUsers();
  var existingUser = users.FirstOrDefault(u => u.Id == user.Id);
  if (existingUser != null)
  {
  existingUser.PasswordHash = user.PasswordHash;
  // Update other fields if needed (e.g., Name, Email, etc.)
  SaveAllUsers(users);
  Console.WriteLine("User updated successfully.");
  }
 }

 public bool ValidateUserLogin(string email, string password)
 {
  var user = GetUserByEmail(email);
  if (user != null)
  {
  var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
  return result == PasswordVerificationResult.Success;
  }
  return false;
 }
 }
}
