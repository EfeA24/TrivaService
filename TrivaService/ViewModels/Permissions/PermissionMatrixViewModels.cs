namespace TrivaService.ViewModels.Permissions
{
    public class RolePropertyPermissionInputViewModel
    {
        public string PropertyName { get; set; } = string.Empty;
        public bool CanRead { get; set; } = true;
        public bool CanWrite { get; set; } = true;
    }

    public class RoleEntityPermissionInputViewModel
    {
        public string EntityName { get; set; } = string.Empty;
        public bool CanRead { get; set; } = true;
        public bool CanCreate { get; set; } = true;
        public bool CanUpdate { get; set; } = true;
        public bool CanDelete { get; set; } = true;
        public List<RolePropertyPermissionInputViewModel> Properties { get; set; } = new List<RolePropertyPermissionInputViewModel>();
    }

    public class RoleEditViewModel
    {
        public int Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? RoleDescription { get; set; }
        public bool IsActive { get; set; } = true;
        public List<RoleEntityPermissionInputViewModel> Permissions { get; set; } = new List<RoleEntityPermissionInputViewModel>();
    }
}
