namespace SGCM.Domain.Constants
{
    public static class AppRoles
    {
        public const string Patient = "Patient";
        public const string Doctor = "Doctor";
        public const string Admin = "Admin";

        public static readonly string[] All = { Patient, Doctor, Admin };
        public static readonly string[] SelfRegisterable = { Patient, Doctor };
    }
}
