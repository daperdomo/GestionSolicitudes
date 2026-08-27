namespace SB.Solicitudes.Api.Authorization;

public static class RoleNames
{
    public static string Administrator => "Administrador";
    public static string Analyst => "Analista";
    public static string Requester => "Solicitante";
}

public static class PolicyNames
{
    public const string ManageRequests = "ManageRequests";
    public const string Administration = "Administration";
}
