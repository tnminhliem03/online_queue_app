namespace OnlineQueueAPI.Permission
{
    public static class ApiPermissions
    {
        public static readonly Dictionary<(string Method, string Path), List<string>> Permissions = new()
        {
            { ("GET", "/api/users"), new List<string> { "Admin" } },
            { ("DELETE", "/api/users/{id}"), new List<string> { "Admin" } },
            { ("POST", "/api/fields"), new List<string> { "Admin" } },
            { ("PUT", "/api/fields/{id}"), new List<string> { "Admin" } },
            { ("DELETE", "/api/fields/{id}"), new List<string> { "Admin" } },
            { ("PUT", "/api/organizations/{id}"), new List<string> { "Admin", "Manager" } },
            { ("PUT", "/api/organizations/{id}/update-status"), new List<string> { "Admin", "Manager" } },
            { ("DELETE", "/api/organizations/{id}"), new List<string> { "Admin", "Manager" } },
            { ("POST", "/api/services"), new List<string> { "Admin", "Manager" } },
            { ("PUT", "/api/services/{id}"), new List<string> { "Admin", "Manager" } },
            { ("DELETE", "/api/services/{id}"), new List<string> { "Admin", "Manager" } },
            { ("POST", "/api/queues"), new List<string> { "Admin", "Manager" } },
            { ("PUT", "/api/queues/{id}"), new List<string> { "Admin", "Manager" } },
            { ("DELETE", "/api/queues/{id}"), new List<string> { "Admin", "Manager" } },
            { ("PUT", "/api/appointments/{id}/check-in"), new List<string> { "Customer", "Manager" } },
            { ("PUT", "/api/appointments/{id}/turn"), new List<string> { "Manager" } },
            { ("PUT", "/api/appointments/{id}/in-progress"), new List<string> { "Manager" } },
            { ("PUT", "/api/appointments/{id}/complete"), new List<string> { "Manager" } },
            { ("PUT", "/api/appointments/{id}/skip"), new List<string> { "Manager" } },
            { ("GET", "/api/appointments/statistics"), new List<string> { "Admin", "Manager" } },
            { ("GET", "/api/roles/partner"), new List<string> { "Admin", "Manager"} },
            { ("POST", "/api/roles/add-staff"), new List<string> { "Admin", "Manager"} },
            { ("DELETE", "/api/roles/"), new List<string> { "Admin", "Manager"} },
        };
    }
}
