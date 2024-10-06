using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace ButikProje.Models
{
    public static class DbUsers
    {
        /// <summary>
        /// Gets the first element with given email.
        /// </summary>
        public static int GetIdWithEmail(string email)
        {
            using (masterEntities db = new masterEntities())
            {
                TblButikKullanicilar entityInstance = db.TblButikKullanicilars.Where(x => x.Email == email).First();

                return entityInstance.Id;
            }
        }

        /// <summary>
        /// Gets the current user with their identity name from the database.
        /// </summary>
        public static TblButikKullanicilar GetCurrentUser(IPrincipal user)
        {
            if (user.Identity.IsAuthenticated)
            {
                using (masterEntities db = new masterEntities())
                {
                    int uid = Convert.ToInt32(user.Identity.Name);
                    return db.TblButikKullanicilars.SingleOrDefault(x => x.Id == uid);
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets all user ids with the given role name.
        /// </summary>
        public static async Task<int[]> GetIdsWithRole(masterEntities db, string roleName)
        {
            try
            {
                switch (roleName)
                {
                    case AdminRoleName:
                        IQueryable<int> adminUserIds = db.TblAdmins.Select(x => x.KullaniciId);
                        int[] adminIds = await db.TblButikKullanicilars
                            .Join(db.TblAdmins,
                                  user => user.Id,
                                  admin => admin.KullaniciId,
                                  (user, admin) => user) // Return the user
                            .Select(x => x.Id)
                            .ToArrayAsync();

                        return adminIds;
                    case UserRoleName:
                        int[] ids = await db.TblButikKullanicilars
                        .GroupJoin(
                            db.TblAdmins,
                            user => user.Id,
                            admin => admin.KullaniciId,
                            (user, admins) => new { User = user, Admins = admins }) // Combine user with matching admins
                        .Where(x => !x.Admins.Any()) // Filter to get users with no matching admins
                        .Select(x => x.User) // Select only the user
                        .Select(x => x.Id)
                        .ToArrayAsync();

                        return ids;
                    default:
                        throw new Exception("No such role.");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);

                int[] emptyArray = Array.Empty<int>();
                return emptyArray;
            }
        }

        /// <summary>
        /// Gets all elements with given role number.
        /// </summary>
        public static async Task<int[]> GetIdsWithRole(masterEntities db, int roleNumber)
        {
            try
            {
                switch (roleNumber)
                {
                    case AdminRoleNumber:
                        IQueryable<int> adminUserIds = db.TblAdmins.Select(x => x.KullaniciId);
                        int[] adminIds = await db.TblButikKullanicilars
                            .Join(db.TblAdmins,
                                  user => user.Id,
                                  admin => admin.KullaniciId,
                                  (user, admin) => user) // Return the user
                            .Select(x => x.Id)
                            .ToArrayAsync();

                        return adminIds;
                    case UserRoleNumber:
                        int[] ids = await db.TblButikKullanicilars
                        .GroupJoin(
                            db.TblAdmins,
                            user => user.Id,
                            admin => admin.KullaniciId,
                            (user, admins) => new { User = user, Admins = admins }) // Combine user with matching admins
                        .Where(x => !x.Admins.Any()) // Filter to get users with no matching admins
                        .Select(x => x.User) // Select only the user
                        .Select(x => x.Id)
                        .ToArrayAsync();

                        return ids;
                    default:
                        throw new Exception("No such role.");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);

                int[] emptyArray = Array.Empty<int>();
                return emptyArray;
            }
        }

        public const int UserRoleNumber = 1;
        public const int AdminRoleNumber = 999;
        public const string UserRoleName = "User";
        public const string AdminRoleName = "Admin";
        public static int[] RoleNumbers = { UserRoleNumber, AdminRoleNumber };

        public static int GetNumOfRole(string roleName)
        {
            int roleNumber = 0;
            string lowerRoleName = roleName.ToLower();

            if (lowerRoleName == "user" || lowerRoleName == "common")
            {
                roleNumber = UserRoleNumber;
            }
            else if (lowerRoleName == "admin" || lowerRoleName == "manager" || lowerRoleName == "yonetici" || lowerRoleName == "yetkili")
            {
                roleNumber = AdminRoleNumber;
            }

            return roleNumber;
        }

        /// <summary>
        /// Will return an empty string if the <paramref name="roleNumber"/> doesn't match with any role names.
        /// </summary>
        public static string GetNameOfRoleNum(int roleNumber)
        {
            if (roleNumber == UserRoleNumber) return UserRoleName;
            else if (roleNumber == AdminRoleNumber) return AdminRoleName;
            else return string.Empty;
        }
    }
}