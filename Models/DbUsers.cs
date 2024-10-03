using System;
using System.Linq;
using System.Security.Principal;

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
        /// Gets all elements with given role name.
        /// </summary>
        public static int[] GetIdsWithRole(masterEntities db, string roleName)
        {
            try
            {
                int numOfRoleName = GetNumOfRole(roleName);

                if (!RoleNumbers.Any(x => x == numOfRoleName))
                    throw new Exception("No such role, returning an empty array.");
                else
                {
                    IQueryable<TblButikKullanicilar> entityInstances = db.TblButikKullanicilars.Where(x => x.Rol == numOfRoleName);
                    int[] ids = entityInstances.Select(x => x.Id).ToArray();

                    return ids;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);

                int[] emptyArray = new int[0];
                return emptyArray;
            }
        }

        /// <summary>
        /// Gets all elements with given role number.
        /// </summary>
        public static int[] GetIdsWithRole(int roleNumber)
        {
            using (masterEntities db = new masterEntities())
            {
                try
                {
                    if (!RoleNumbers.Any(x => x == roleNumber))
                        throw new Exception("No such role, returning an empty array.");
                    else
                    {
                        var entityInstances = db.TblButikKullanicilars.Where(x => x.Rol == roleNumber);
                        int[] ids = entityInstances.Select(x => x.Id).ToArray();

                        return ids;
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);

                    int[] emptyArray = new int[0];
                    return emptyArray;
                }
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
        public static string GetNameOfRoleNum(int roleNumber)
        {
            if (roleNumber == UserRoleNumber)
                return UserRoleName;
            else if (roleNumber == AdminRoleNumber)
                return AdminRoleName;
            else
                return string.Empty;
        }
    }
}