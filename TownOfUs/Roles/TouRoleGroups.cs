namespace TownOfUs.Roles;

public static class TouRoleGroups
{
    public static RoleOptionsGroup VI { get; } = new("Village Investigative Roles", RoleColors.Village);
    public static RoleOptionsGroup VK { get; } = new("Village Killing Roles", RoleColors.Village);
    public static RoleOptionsGroup VP { get; } = new("Village Protective Roles", RoleColors.Village);
    public static RoleOptionsGroup VS { get; } = new("Village Support Roles", RoleColors.Village);
    public static RoleOptionsGroup VU { get; } = new("Village Utility Roles", RoleColors.Village);

    public static RoleOptionsGroup IE { get; } = new("Independent Evil Roles", RoleColors.Independent);

    public static RoleOptionsGroup MD { get; } = new("Mafia Deception Roles", RoleColors.Mafia);
    public static RoleOptionsGroup MK { get; } = new("Mafia Killing Roles", RoleColors.Mafia);
    public static RoleOptionsGroup MS { get; } = new("Mafia Support Roles", RoleColors.Mafia);
    public static RoleOptionsGroup MU { get; } = new("Mafia Utility Roles", RoleColors.Mafia);
}