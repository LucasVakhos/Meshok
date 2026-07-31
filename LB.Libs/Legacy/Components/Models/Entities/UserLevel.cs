using System.ComponentModel.DataAnnotations;
namespace LB.Libs;

public enum UserLevel
{
    [Display(Name = "Администратор базы"), Role("DB_ADMIN")]
    Admin = 1,
    [Display(Name = "Менеджер базы"), Role("DB_MANAGER")]
    Manager,
    [Display(Name = "Курьер"), Role("USER")]
    Courier
}
