namespace TownOfUs.Roles;

public static class TouRoleGroups
{
    public static RoleOptionsGroup Crewmate { get; } = new("Crewmate Roles", RoleColors.Crewmate);
    public static RoleOptionsGroup Neutral { get; } = new("Neutral Roles", RoleColors.Neutral);
    public static RoleOptionsGroup Impostor { get; } = new("Impostor Roles", RoleColors.Impostor);

    /*public static RoleOptionsGroup VI { get; } = new("Crewmate Investigative Roles", RoleColors.Crewmate);
    public static RoleOptionsGroup VK { get; } = new("Crewmate Killing Roles", RoleColors.Crewmate);
    public static RoleOptionsGroup VP { get; } = new("Crewmate Protective Roles", RoleColors.Crewmate);
    public static RoleOptionsGroup VS { get; } = new("Crewmate Support Roles", RoleColors.Crewmate);
    public static RoleOptionsGroup VU { get; } = new("Crewmate Utility Roles", RoleColors.Crewmate);

    public static RoleOptionsGroup IE { get; } = new("Neutral Evil Roles", RoleColors.Neutral);

    public static RoleOptionsGroup MD { get; } = new("Impostor Deception Roles", RoleColors.Impostor);
    public static RoleOptionsGroup MK { get; } = new("Impostor Killing Roles", RoleColors.Impostor);
    public static RoleOptionsGroup MS { get; } = new("Impostor Support Roles", RoleColors.Impostor);
    public static RoleOptionsGroup MU { get; } = new("Impostor Utility Roles", RoleColors.Impostor);*/
}