using System;
using System.ComponentModel.DataAnnotations;

public class LoginUserRequest
{
    [Required]
    public string username { set; get; }

    [Required]
    public string password { set; get; }

}
