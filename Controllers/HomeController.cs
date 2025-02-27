﻿using ButikProje.Models;
using FirebaseAdmin.Messaging;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI.WebControls;
using static ButikProje.Commons.DbCommons;
using BCryptNet = BCrypt.Net;

namespace ButikProje.Controllers
{
    public class HomeController : Controller
    {
        private readonly bool WebsiteActive;
        private const string ErrorMessage = "ErrorMessage";
        private const string UnexpectedErrorMessage = "Beklenmedik bir hata oluştu. Eğer hata ile tekrar karşılaşırsanız website sahibi ile iletişime geçiniz. \n";
        private const int DefaultWorkFactor = 10;

        private readonly string EnvironmentBasedEmail;
        private readonly string EnvironmentBasedEmailPassword;
        private readonly string EnvironmentBasedSmtpHost;
        private readonly int EnvironmentBasedSmtpPort;

        public HomeController()
        {
#if DEBUG
            EnvironmentBasedEmail = "himmetcangibisiyok@gmail.com";
            EnvironmentBasedEmailPassword = "tdes sshe huwt vjqo";
            EnvironmentBasedSmtpHost = "smtp.gmail.com";
            EnvironmentBasedSmtpPort = 587;
#else
                EnvironmentBasedEmail = "postmaster@pdbutik.com";
                EnvironmentBasedEmailPassword = "Banyemicem12*";
                EnvironmentBasedSmtpHost = "mail5019.site4now.net";
                EnvironmentBasedSmtpPort = 465;
#endif

            using (masterEntities db = new masterEntities())
            {
                TblButikAyarlar settings = db.TblButikAyarlars.FirstOrDefault();
                if (settings != null && settings.SiteAktif is int websiteActive)
                {
                    WebsiteActive = Convert.ToBoolean(websiteActive);
                }
                else
                {
                    WebsiteActive = true;
                }
            }

            if (!WebsiteActive)
            {
                throw new HttpException(403, "Erişim reddedildi. Site bu an devre dışı, başka bir zaman tekrar deneyiniz.");
            }

            List<string> roles = new List<string>
            {
                "User", "Admin"
            };

            foreach (string role in roles.Where(role => !Roles.RoleExists(role)))
            {
                Roles.CreateRole(role);
            }
        }

        private async Task<DbInterface> GetModel(bool minimal = false, Filters filters = Filters.Default)
        {
            bool isTempUser = TryConvertToInt(Session["TempUID"], out int uid);

            using (masterEntities db = new masterEntities())
            {
                if (!minimal)
                {
                    return isTempUser ? await GetDefaultModel(db, uid, filters) : await GetDefaultModel(db, Request, User.Identity, filters);
                }
                else
                {
                    return isTempUser ? await GetMinimalModel(db, uid) : await GetMinimalModel(db, Request, User.Identity);
                }
            }
        }



        public async Task<ActionResult> Index(Filters filters = Filters.Default, int page = 1)
        {
            SetFilterViewData(filters);
            await SetPaginationViewData(page, "Index");



            return View(await GetModel(filters: filters));
        }

        public async Task SetPaginationViewData(int pageIndex, string currentAction)
        {
            ViewBag.CurrentPage = pageIndex;

            int productCount = 0;

            using (masterEntities db = new masterEntities())
            {
                productCount = await db.TblUrunTanims.CountAsync();
            }

            int totalPages = (int)Math.Ceiling(productCount / (double)PageSize);

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentAction = currentAction;
        }
        public async Task SetPaginationViewData(masterEntities db, int pageIndex, string currentAction)
        {
            ViewBag.CurrentPage = pageIndex;

            int productCount;

            productCount = await db.TblUrunTanims.CountAsync();

            int totalPages = (int)Math.Ceiling(productCount / (double)PageSize);

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentAction = currentAction;
        }
        public void SetFilterViewData(Filters filters)
        {
            ViewBag.TranslatedFilter = FilterUserTranslator(filters);
            ViewBag.CurrentFilter = filters;
        }

        public async Task<ActionResult> About()
        {
            ViewBag.Message = "Your application description page.";
            return View(await GetModel(true));
        }

        public async Task<ActionResult> Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View(await GetModel(true));
        }

        public async Task<ActionResult> Login(string ReturnUrl)
        {
            ViewBag.ErrorMessage = TempData[ErrorMessage] ?? string.Empty;
            ViewBag.VerificationStarted = TempData["VerificationStarted"] ?? false;
            ViewBag.PasswordChangeVerificationStarted = TempData["PasswordChangeVerificationStarted"] ?? false;
            if (!string.IsNullOrWhiteSpace(ReturnUrl))
            {
                ViewBag.ReturnUrl = ReturnUrl;
            }

            return View(await GetModel(true));
        }

        private static bool ValidateEmailFormat(string email) => !string.IsNullOrWhiteSpace(email) && email.Length <= 320 && Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$");

        private static bool ValidatePasswordFormat(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length > 72) return false;

            // Minimum length 8, at least one uppercase, one lowercase and one digit
            return Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$");

        }

        private (bool valid, string msg) ValidateForm(string email, string password)
        {
            bool isEmailValid = ValidateEmailFormat(email);
            bool isPasswordValid = ValidatePasswordFormat(password);

            if (!isEmailValid && !isPasswordValid)
            {
                return (false, "Email ve parola formatı yanlış.");
            }
            else if (!isEmailValid)
            {
                return (false, "Email formatı yanlış.");
            }
            else if (!isPasswordValid)
            {
                return (false, "Parola formatı yanlış.");
            }

            return (true, string.Empty);
        }

        private string HashPassword(string password) => BCryptNet.BCrypt.HashPassword(password, DefaultWorkFactor);

        private async Task<TblButikKullanicilar> SaveUser(masterEntities db, string name, string surname, string email, string pwdHash, int role = DbUsers.UserRoleNumber)
        {
            TblButikKullanicilar user = new TblButikKullanicilar
            {
                Ad = name,
                Soyad = surname,
                Email = email,
                ParolaHash = pwdHash
            };
            db.TblButikKullanicilars.Add(user);
            await db.SaveChangesAsync();

            return user;
        }

        private bool VerifyUserPassword(TblButikKullanicilar user, string password) => VerifyPasswordHash(password, user.ParolaHash);

        private bool VerifyPasswordHash(string password, string storedHash) => BCryptNet.BCrypt.Verify(password, storedHash);


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SignIn(string GirisEmail, string GirisParola, bool OnuHatirla, string returnUrl)
        {
            try
            {
                (bool valid, string msg) = ValidateForm(GirisEmail, GirisParola);
                if (!valid) throw new SecurityException(msg);

                if (!Request.IsAuthenticated)
                {
                    string redirectingWhere = string.Empty;

                    using (masterEntities db = new masterEntities())
                    {
                        TblButikKullanicilar userEfInstance = await db.TblButikKullanicilars.SingleOrDefaultAsync(x => x.Email == GirisEmail) ?? throw new SecurityException("E-mail veya parola yanlış.");
                        if (VerifyUserPassword(userEfInstance, GirisParola))
                        {
                            string userName = userEfInstance.Id.ToString();

                            string role = string.Empty;

                            bool isAdmin = await db.TblAdmins.AnyAsync(x => x.KullaniciId == userEfInstance.Id);

                            role = !isAdmin ? DbUsers.UserRoleName : DbUsers.AdminRoleName;

                            if (!Roles.IsUserInRole(userName, role))
                            {
                                Roles.AddUserToRole(userName, role);
                            }

                            Session.Abandon();
                            FormsAuthentication.SetAuthCookie(userName, OnuHatirla);

                            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                            {
                                return Redirect(returnUrl);
                            }

                            if (role.Equals("Admin"))
                            {
                                return RedirectToAction("Index", "Admin");
                            }

                            return RedirectToAction("Index", "Home");

                        }
                        else
                        {
                            throw new SecurityException(GetDetailedResultMessage(ErrDetails.InvalidLogin, OccuredOn.Login));
                        }
                    }
                }
                else
                {
                    throw new Exception(GetDetailedResultMessage(ErrDetails.AlreadySignedIn, OccuredOn.Login));
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                TempData[ErrorMessage] = ex.Message;

                return RedirectToAction("Login", "Home");
            }
        }

        public string GenerateSHA256(char[] input)
        {
            using (SHA256 sHA256 = SHA256.Create())
            {
                byte[] hash = sHA256.ComputeHash(Encoding.UTF8.GetBytes(input));

                StringBuilder hashString = new StringBuilder();

                for (int i = 0; i < hash.Length; i++)
                {
                    hashString.Append(hash[i].ToString("X2"));
                }

                return hashString.ToString();
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult SignOut()
        {
            try
            {
                FormsAuthentication.SignOut();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Sets the temporary user id if the user is not authenticated or the temp uid hasn't been set and returns the uid. <br />
        /// If it has been set, the temp uid is returned as is.<br />
        /// It's more appropriate to use the returned value only once when the temp uid is being set since it's more fitting for the design pattern.
        /// </summary>
        /// <returns>The User ID</returns>
        private int SetTempUIDWrapper()
        {
            int userId;
            if (!Request.IsAuthenticated)
            {
                if (TryConvertToInt(Session["TempUID"], out int uid))
                {
                    userId = uid;
                }
                else
                {
                    SetTemporaryUID();
                    userId = Convert.ToInt32(Session["TempUID"]);
                }
            }
            else
            {
                userId = Convert.ToInt32(User.Identity.Name);
            }

            return userId;
        }

        private async Task SendEmail(MailMessage mailMessage)
        {
            using (SmtpClient smtpClient = new SmtpClient(EnvironmentBasedSmtpHost, EnvironmentBasedSmtpPort))
            {
                smtpClient.Credentials = new NetworkCredential(EnvironmentBasedEmail, EnvironmentBasedEmailPassword);
                smtpClient.Timeout = 300000;
                smtpClient.EnableSsl = true;

                await smtpClient.SendMailAsync(mailMessage);
            }
        }

        private async Task SendVerificationEmail(masterEntities db, string receiverEmail)
        {
            int userId = SetTempUIDWrapper();

            byte[] protectedCode = ProtectedData.Protect(BitConverter.GetBytes(GenerateVerificationCode()), null, DataProtectionScope.LocalMachine);
            string encryptedBase64 = Convert.ToBase64String(protectedCode);

            TblGizliKodlar secretCode = new TblGizliKodlar
            {
                Userid = userId,
                Verificationcode = encryptedBase64,
            };

            db.TblGizliKodlars.Add(secretCode);
            await db.SaveChangesAsync();

            using (MailMessage mailMessage = new MailMessage())
            {
                mailMessage.From = new MailAddress(EnvironmentBasedEmail);
                mailMessage.Subject = "P&D Boutique Doğrulama kodunuz";
                mailMessage.IsBodyHtml = true;
                mailMessage.Body = "<h3>Doğrulama kodu: " + BitConverter.ToInt32(ProtectedData.Unprotect(protectedCode, null, DataProtectionScope.LocalMachine), 0) + "</h3>";
                mailMessage.To.Add(receiverEmail);

                await SendEmail(mailMessage: mailMessage);
            }
        }

        private int GenerateVerificationCode()
        {
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] data = new byte[4];
                rng.GetBytes(data);

                // Ensuring the code is 6 digits 
                return (BitConverter.ToInt32(data, 0) % 900000 + 900000) % 900000 + 100000; // Avoid negative value
            }
        }

        private byte[] UnprotectData(string encryptedBase64)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedBase64);
            byte[] decryptedBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.LocalMachine);

            return decryptedBytes;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(string KaydolAd, string KaydolSoyad, string KaydolEmail, string KaydolParola)
        {
            (bool valid, string msg) = ValidateForm(KaydolEmail, KaydolParola);
            if (!valid) throw new SecurityException(msg);

            if (!ModelState.IsValid)
            {
                TempData[ErrorMessage] = "Girilen bilgiler boş veya format yanlış.";
                return RedirectToAction("Login", "Home");
            }

            try
            {
                using (masterEntities db = new masterEntities())
                {
                    if (!await db.TblButikKullanicilars.AnyAsync(x => x.Email == KaydolEmail))
                    {
                        await SendVerificationEmail(db, KaydolEmail);

                        TempData["RegisterName"] = KaydolAd; TempData["RegisterSurname"] = KaydolSoyad; TempData["RegisterEmail"] = KaydolEmail; TempData["RegisterPasswordHash"] = HashPassword(KaydolParola);
                        TempData["VerificationStarted"] = true;
                    }
                    else
                    {
                        throw new Exception(GetDetailedResultMessage(ErrDetails.EmailExists, OccuredOn.Register));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);

                TempData[ErrorMessage] = ex.Message;
            }

            return RedirectToAction("Login", "Home");
        }

        private async Task<byte[]> GetProtectedCode(masterEntities db)
        {
            int uid = SetTempUIDWrapper();

            TblGizliKodlar secretCode = await db.TblGizliKodlars.FirstOrDefaultAsync(c => c.Userid == uid);
            return UnprotectData(secretCode.Verificationcode);
        }

        /// <returns><see langword="true"/> if the protected code was removed; otherwise <see langword="false"/></returns>
        private async Task<bool> DestroyProtectedCode(masterEntities db)
        {
            int uid = SetTempUIDWrapper();

            IQueryable<TblGizliKodlar> secretCodes = db.TblGizliKodlars.Where(c => c.Userid == uid);
            if (secretCodes != null && await secretCodes.AnyAsync())
            {
                db.TblGizliKodlars.RemoveRange(secretCodes);

                if (await db.SaveChangesAsync() != 0)
                {
                    return true;
                }
                else return false;
            }
            else return false;
        }

        private void HandleDbValidationErrors(DbEntityValidationException evx)
        {
            StringBuilder errMsgs = new StringBuilder();

            foreach (DbEntityValidationResult validationErrors in evx.EntityValidationErrors)
            {
                foreach (DbValidationError validationError in validationErrors.ValidationErrors)
                {
                    string errMsg = $"Property: {validationError.PropertyName}, Error: {validationError.ErrorMessage}";
                    errMsgs.Append(errMsg).AppendLine();
                }
            }

            TempData[ErrorMessage] = errMsgs.ToString();
        }

        [HttpPost]
        public async Task<ActionResult> VerifyEmail(int inputVerifyCode)
        {
            using (masterEntities db = new masterEntities())
            {
                try
                {
                    int verifyCode = BitConverter.ToInt32(await GetProtectedCode(db), 0);
                    if (verifyCode == inputVerifyCode)
                    {
                        TblButikKullanicilar dbUsersInstance = new TblButikKullanicilar
                        {
                            Ad = (string)TempData["RegisterName"],
                            Soyad = (string)TempData["RegisterSurname"],

                            Email = (string)TempData["RegisterEmail"]
                        };

                        if (TempData["RegisterPasswordHash"] is string encryptedPwd)
                        {
                            dbUsersInstance.ParolaHash = encryptedPwd;
                        }
                        else throw new SecurityException("Güvenlik hatası belirdi. Eğer hata ile tekrar karşılaşırsanız website sahibi ile iletişimie geçiniz.");

                        db.TblButikKullanicilars.Add(dbUsersInstance);

                        int saveResult = await db.SaveChangesAsync();
                        if (GetResult(saveResult != 0, true) == Result.Error)
                        {
                            throw new Exception(GetResultMessage(Result.Error, OccuredOn.Register));
                        }

                        TempData["VerificationStarted"] = false;

                        return Json(new { message = GetResultMessage(Result.Success, OccuredOn.Verification), errOcurred = false });
                    }
                    else
                    {
                        return Json(new { message = GetDetailedResultMessage(ErrDetails.IncorrectVerificationCode, OccuredOn.Register), errOcurred = true });
                    }
                }
                catch (SmtpException ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    if (ex.StatusCode == SmtpStatusCode.GeneralFailure && ex.InnerException is System.IO.IOException ioEx
                        && ioEx.InnerException is System.Net.Sockets.SocketException sockEx && sockEx.SocketErrorCode == System.Net.Sockets.SocketError.TimedOut)
                    {
                        TempData[ErrorMessage] = "Doğrulama zaman aşımına uğradı.";
                    }
                    else
                    {
                        TempData[ErrorMessage] = "E-Mail gönderilemedi: " + ex.Message;
                    }

                    return RedirectToAction("Login", "Home");
                }
                catch (DbEntityValidationException evx)
                {
                    HandleDbValidationErrors(evx);

                    return RedirectToAction("Login", "Home");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    TempData[ErrorMessage] = UnexpectedErrorMessage + ex.Message;

                    return RedirectToAction("Login", "Home");
                }
                finally
                {
                    await DestroyProtectedCode(db);
                }
            }
        }

        [HttpPost]
        public async Task<ActionResult> ChangePassword(string email)
        {
            try
            {
                string emailTrimmed = email.Trim();

                using (masterEntities db = new masterEntities())
                {
                    if (await db.TblButikKullanicilars.AnyAsync(x => x.Email == emailTrimmed))
                    {
                        await SendVerificationEmail(db, emailTrimmed);

                        TempData["PasswordChangeEmail"] = emailTrimmed;
                        TempData["PasswordChangeVerificationStarted"] = true;
                    }
                    else
                        throw new Exception(GetDetailedResultMessage(ErrDetails.EmailUnregistered, OccuredOn.PasswordChange));
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                TempData[ErrorMessage] = ex.Message;

                TempData.Remove("PasswordChangeEmail");
                TempData.Remove("PasswordChangeVerificationStarted");
            }

            return RedirectToAction("Login", "Home");
        }

        [HttpPost]
        public async Task<ActionResult> VerifyPasswordChangeCode(int inputVerifyCode, string newPassword)
        {
            try
            {
                using (masterEntities db = new masterEntities())
                {
                    if (BitConverter.ToInt32(await GetProtectedCode(db), 0) == inputVerifyCode)
                    {
                        string passwordChangeEmail = (string)TempData["PasswordChangeEmail"];

                        TblButikKullanicilar user = await db.TblButikKullanicilars.FirstOrDefaultAsync(x => x.Email == passwordChangeEmail)
                                                    ?? throw new Exception("Kullanıcı bulanamadı, eğer bu hata ile tekrar karşılaşırsanız website sahibi ile iletişime geçiniz.");

                        string passwordHash = HashPassword(newPassword);
                        user.ParolaHash = passwordHash;


                        if (GetResult(await db.SaveChangesAsync() != 0, true) == Result.Error)
                        {
                            throw new Exception(GetResultMessage(Result.Error, OccuredOn.PasswordChange));
                        }
                        else
                        {
                            TempData["VerificationStarted"] = false;

                            return Json(new { message = GetResultMessage(Result.Success, OccuredOn.Verification), errOcurred = false });
                        }
                    }
                    else
                    {
                        return Json(new { message = GetDetailedResultMessage(ErrDetails.IncorrectVerificationCode, OccuredOn.PasswordChange), errOcurred = true });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                TempData[ErrorMessage] = ex.Message;

                return RedirectToAction("Login", "Home");
            }
        }

        public void ClearData(DataTransfererType dataTransfererType)
        {
            switch (dataTransfererType)
            {
                case DataTransfererType.Session:
                    Session.Clear();
                    break;
                case DataTransfererType.TempData:
                    TempData.Clear();
                    break;
                case DataTransfererType.ViewData:
                    ViewData.Clear();
                    break;
                case DataTransfererType.ViewBag:
                    ViewData.Clear();
                    break;
                case DataTransfererType.All:
                    Session.Clear();
                    TempData.Clear();
                    ViewData.Clear();
                    break;
            }
        }


        public async Task<ActionResult> Siparisler() => View(await GetModel());

        public async Task<ActionResult> Product(string name)
        {
            using (masterEntities db = new masterEntities())
            {
                TblUrunTanim product = await db.TblUrunTanims.FirstOrDefaultAsync(x => x.UrunIsim.Equals(name));

                if (product == null)
                {
                    return HttpNotFound();
                }

                DbInterface dbInterface = await GetModel(true);

                dbInterface.Products.Add(await GetProduct(db, product.UrunId));

                if (TempData["AddedToCart"] != null)
                {
                    ViewBag.AddedToCart = TempData["AddedToCart"];
                }

                return View(dbInterface);
            }
        }

        public async Task<ActionResult> Search(string q, Filters filters = Filters.Default, int page = 1)
        {
            string qProper = q.ToLower().Trim();

            DbInterface dbInterface = await GetModel(filters: filters);
            List<DbProduct> products = dbInterface.Products.Where(x => x.Name.ToLower().Trim().Contains(qProper)).ToList();

            products = products.Skip((page - 1) * PageSize).Take(PageSize).ToList();

            if (products != null && products.Count > 0)
            {
                dbInterface.Products = products;
                ViewBag.Result = '"' + q + '"' + " için sonuçlar:";
            }
            else
            {
                dbInterface.Products.Clear();
                ViewBag.Result = '"' + q + '"' + " için sonuç yok.";
            }
            ViewBag.Query = q;
            SetFilterViewData(filters);
            await SetPaginationViewData(page, "Search");

            return View(dbInterface);
        }

        public async Task<ActionResult> Category(string categoryName, Filters filters = Filters.Default, int page = 1)
        {
            using (masterEntities db = new masterEntities())
            {
                TblKategoriler category = await db.TblKategorilers.FirstOrDefaultAsync(x => x.KategoriIsim == categoryName);

                if (category == null)
                {
                    return HttpNotFound();
                }

                DbInterface dbInterface = await GetModel(true);

                dbInterface.Products = await GetProducts(db, categoryName, page);

                ViewBag.CategoryName = categoryName;
                SetFilterViewData(filters);
                await SetPaginationViewData(page, "Category");

                return View(dbInterface);
            }
        }

        private void SetTemporaryUID()
        {
            if (Session["TempUID"] == null)
            {
                int rnd = new Random().Next(5000, 50000);

                Session["TempUID"] = rnd;
            }
        }

        [HttpGet]
        public async Task<ActionResult> AddToCart(int productId, bool isAjaxRequest = false)
        {
            using (masterEntities db = new masterEntities())
            {
                try
                {
                    if (!Request.IsAuthenticated)
                    {
                        SetTemporaryUID();
                    }

                    int userId = TryConvertToInt(Session["TempUID"], out int uid) ? uid : Convert.ToInt32(User.Identity.Name);

                    TblSepet cartTable = new TblSepet
                    {
                        UrunId = productId,
                        KullaniciId = userId
                    };

                    db.TblSepets.Add(cartTable);
                    if (await db.SaveChangesAsync() == 0)
                    {
                        throw new Exception(GetResultMessage(Result.Error));
                    }

                    if (!isAjaxRequest)
                    {
                        int cartCount = await GetCartCount(db, userId);
                        return Json(new { cartCount }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        TblUrunTanim product = await db.TblUrunTanims.FirstOrDefaultAsync(x => x.UrunId == productId);
                        string productName = product.UrunIsim;

                        TempData["AddedToCart"] = true;

                        return RedirectToAction("Product", "Home", new { name = productName });
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    TempData[ErrorMessage] = ex.Message;

                    return Json(new { message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> RemoveFromCart(int productId)
        {
            using (masterEntities db = new masterEntities())
            {
                try
                {
                    TblSepet cart = await db.TblSepets.FirstOrDefaultAsync(x => x.UrunId == productId) ?? throw new Exception("Ürün bulunamadı, Silinmiş olabilir.");

                    db.TblSepets.Remove(cart);

                    if (await db.SaveChangesAsync() == 0)
                    {
                        throw new Exception(GetResultMessage(Result.Error));
                    }
                    int cartCount = TryConvertToInt(Session["TempUID"], out int uid) ? await GetCartCount(db, uid) : await GetCartCount(db, Request, User.Identity);
                    return Json(new { cartCount });
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    TempData[ErrorMessage] = ex.Message;

                    return Json(new { message = ex.Message });
                }
            }
        }

        [HttpPost]
        public ActionResult AuthenticationCheck() => Json(new { isAuthenticated = Request.IsAuthenticated });

        public async Task<ActionResult> Cart()
        {
            using (masterEntities db = new masterEntities())
            {
                List<TblSepet> cart = TryConvertToInt(Session["TempUID"], out int uid) ? await GetProductsInCart(db, uid) : await GetProductsInCart(db, Request, User.Identity);

                if (cart == null || !cart.Any())
                {
                    return View(await GetModel());
                }


                // Removes all products from the users cart that were deleted
                foreach (TblSepet cartItem in cart)
                {
                    TblUrunTanim matchingItem = await db.TblUrunTanims.FirstOrDefaultAsync(x => x.UrunId == cartItem.UrunId);

                    if (matchingItem == null)
                    {
                        db.TblSepets.Attach(cartItem);
                        await db.SaveChangesAsync();
                        db.TblSepets.Remove(cartItem);
                    }
                }

                if (await db.SaveChangesAsync() != 0)
                {
                    ViewBag.ErrMsg = GetResultMessageDetails(ErrDetails.ProductsInCartRemoved);
                }
            }

            return View(await GetModel());
        }

        [HttpPost]
        public async Task<ActionResult> RemoveCartItem(int itemId)
        {
            try
            {
                bool quantityOver1 = false;
                decimal? newTotalPrice = 0;

                using (masterEntities db = new masterEntities())
                {
                    int userId = TryConvertToInt(Session["TempUID"], out int uid) ? uid : Convert.ToInt32(User.Identity.Name);

                    quantityOver1 = await db.TblSepets.Where(x => x.UrunId == itemId && x.KullaniciId == userId).CountAsync() > 1;

                    TblSepet cartItem = await db.TblSepets.FirstOrDefaultAsync(x => x.UrunId == itemId && x.KullaniciId == userId);

                    if (cartItem == null)
                    {
                        return Json(new { success = false, message = "Sepette ürün bulunamadı." });
                    }

                    db.TblSepets.Remove(cartItem);

                    if (await db.SaveChangesAsync() == 0)
                    {
                        throw new Exception(GetResultMessage(Result.Error));
                    }

                    List<int> productIds = await db.TblSepets.Where(x => x.KullaniciId == userId).Select(x => x.UrunId).ToListAsync();

                    newTotalPrice = await db.TblUrunTanims.Where(x => productIds.Contains(x.UrunId)).SumAsync(x => (decimal?)x.UrunFiyat) ?? 0;
                }

                return Json(new { success = true, removedItemId = itemId, quantityOver1, newTotalPrice });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }

        public async Task<ActionResult> BuyNow(int productId)
        {
            using (masterEntities db = new masterEntities())
            {
                TblUrunTanim product = await db.TblUrunTanims.FirstAsync(x => x.UrunId == productId);

                if (product != null)
                {
                    if (Request.IsAuthenticated)
                    {
                        TempData["SinglePurchaseItemID"] = productId;
                    }
                    else
                    {
                        Session["SinglePurchaseItemID"] = productId;
                        SetTemporaryUID();
                    }
                    Session["CurrentlySinglePurchasing"] = true;
                    return RedirectToAction("Checkout", "Home", new { purchasingNow = true });
                }
                else
                {
                    return Json("Ürün Bulunamadı.");
                }
            }
        }

        public async Task<ActionResult> Checkout(bool purchasingNow = false)
        {
            bool tempUser = TryConvertToInt(Session["TempUID"], out int uid);

            DbInterface dbInterface = await GetModel();

            if (purchasingNow)
            {
                int? singleItemId = (TempData["SinglePurchaseItemID"] as int?) ?? (Session["SinglePurchaseItemID"] as int?) ?? null;

                if (singleItemId == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                Session.Remove("SinglePurchaseItemID");

                dbInterface.ProductsInCart.Clear();

                using (masterEntities db = new masterEntities())
                {
                    TblSepet tblSepet = new TblSepet
                    {
                        UrunId = singleItemId.Value,
                        KullaniciId = tempUser ? uid : Convert.ToInt32(User.Identity.Name)
                    };

                    dbInterface.ProductsInCart.Add(tblSepet);
                }

                ViewBag.SingleItemPurchase = true;
                ViewBag.SingleItemID = singleItemId;
            }
            else if (!dbInterface.ProductsInCart.Any())
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ErrMsg = TempData[ErrorMessage];

            return View(dbInterface);
        }

        public async Task<ActionResult> Purchase(string buyerName, string buyerSurname, string contactInfo, string address, string postalCode, string city, string country, string telno, int size, string promoCode, bool saveInfo)
        {
            Options options = new Options
            {
                ApiKey = "sandbox-JlLOssMkFfdcZAK52n2UFp78r7LueQLI",
                SecretKey = "sandbox-pYtaoJfUg1BVTmSb6IA5V6pSavHzPQnU",
                BaseUrl = "https://sandbox-api.iyzipay.com"
            };
            bool tempUser = TryConvertToInt(Session["TempUID"], out int uid);
            decimal? cartTotalProduct = null;
            using (masterEntities db = new masterEntities())
            {
                cartTotalProduct = tempUser ? await GetTotalOfProductsInCart(db, uid) : await GetTotalOfProductsInCart(db, Request, User.Identity);
            }
            string totalPrice = cartTotalProduct.Value.ToString(new CultureInfo("en-US"));

            CreateCheckoutFormInitializeRequest request = new CreateCheckoutFormInitializeRequest
            {
                Locale = Locale.TR.ToString(),
                Price = totalPrice,
                ConversationId = new Random().Next(100000000, 999999999).ToString(),
                PaidPrice = totalPrice,
                Currency = Currency.TRY.ToString(),
                PaymentGroup = PaymentGroup.PRODUCT.ToString()
            };
            request.CallbackUrl = $"{Request.Url.Scheme}://{Request.Url.Host}:{Request.Url.Port}{Url.Action("PurchaseCallback", "Home", new { id = Convert.ToInt32(request.ConversationId) })}";

            List<int> enabledInstallments = new List<int>
            {
                2,
                3,
                6,
                9
            };
            request.EnabledInstallments = enabledInstallments;

            Buyer buyer = new Buyer
            {
                Id = tempUser ? uid.ToString() : User.Identity.Name,
                Name = buyerName,
                Surname = buyerSurname,
                GsmNumber = telno,
                Email = contactInfo,
                IdentityNumber = "12345678911",
                RegistrationAddress = address,
                Ip = Request.UserHostAddress,
                City = city,
                Country = country,
                ZipCode = postalCode
            };
            request.Buyer = buyer;
            Address shippingAddress = new Address
            {
                ContactName = buyerName + " " + buyerSurname,
                City = city,
                Country = country,
                Description = address,
                ZipCode = postalCode
            };
            request.ShippingAddress = shippingAddress;
            Address billingAddress = new Address
            {
                ContactName = buyerName + " " + buyerSurname,
                City = city,
                Country = country,
                Description = address,
                ZipCode = postalCode
            };
            request.BillingAddress = billingAddress;

            List<BasketItem> basketItems = new List<BasketItem>();

            using (masterEntities db = new masterEntities())
            {
                basketItems = (tempUser ? await GetDbProductsInCart(db, uid) : await GetDbProductsInCart(db, Request, User.Identity)).Select(product => new BasketItem
                {
                    Id = product.ID.ToString(),
                    Name = product.Name,
                    Category1 = product.Category,
                    Category2 = product.Category + "2",
                    Price = product.Price.ToString(new CultureInfo("en-US")),
                    ItemType = BasketItemType.PHYSICAL.ToString()
                }).ToList();
            }

            request.BasketItems = basketItems;
            CheckoutFormInitialize checkoutFormInitialize = CheckoutFormInitialize.Create(request: request, options: options);
            using (masterEntities db = new masterEntities())
            {
                try
                {
                    TblPayment tblPayment = new TblPayment();

                    int paymentId = Convert.ToInt32(request.ConversationId);
                    tblPayment.PaymentId = paymentId;
                    Session["PaymentID"] = paymentId;

                    tblPayment.Token = checkoutFormInitialize.Token;

                    db.TblPayments.Add(tblPayment);

                    await db.SaveChangesAsync();
                }
                catch (DbEntityValidationException evx)
                {
                    Console.Error.WriteLine(evx.Message);
                    if (!string.IsNullOrWhiteSpace(checkoutFormInitialize.ErrorMessage))
                    {
                        TempData[ErrorMessage] = UppercaseFirst(checkoutFormInitialize.ErrorMessage);
                    }
                    else
                    {
                        foreach (DbEntityValidationResult validationErrors in evx.EntityValidationErrors)
                        {
                            foreach (DbValidationError validationError in validationErrors.ValidationErrors)
                            {
                                TempData[ErrorMessage] = $"Property: {validationError.PropertyName}, Error: {validationError.ErrorMessage}";
                            }
                        }
                    }

                    return RedirectToAction("Checkout", "Home");
                }
            }

            TblSiparisler order = new TblSiparisler();
            using (masterEntities db = new masterEntities())
            {
                order = db.TblSiparislers.Create();

                order.SiparisKullaniciId = tempUser ? uid : Convert.ToInt32(User.Identity.Name);

                order.SiparisTarih = DateTime.Now;
                order.SiparisAdres = $"Ülke: {country} | Şehir: {city} | Adres: {address} | Postal Code: {postalCode} | Tel no: {telno} |";

                int conversationId = Convert.ToInt32(request.ConversationId);

                order.TblSiparisItemlers = (tempUser ? await GetProductsInCart(db, uid) : await GetProductsInCart(db, Request, User.Identity)).Select(cartItem => new TblSiparisItemler
                {
                    UrunId = cartItem.UrunId,
                    SiparisId = conversationId,
                    UrunBeden = size
                }).ToList();
            }
            Session["Order"] = order;

            ViewBag.Iyzico = checkoutFormInitialize.CheckoutFormContent;

            return View(await GetModel(true));
        }

        public async Task<ActionResult> PurchaseItem(string buyerName, string buyerSurname, string contactInfo, string address, string postalCode, string city, string country, int size, string telno, string promoCode, bool saveInfo)
        {
            Options options = new Options
            {
                ApiKey = "sandbox-JlLOssMkFfdcZAK52n2UFp78r7LueQLI",
                SecretKey = "sandbox-pYtaoJfUg1BVTmSb6IA5V6pSavHzPQnU",
                BaseUrl = "https://sandbox-api.iyzipay.com"
            };
            bool tempUser = TryConvertToInt(Session["TempUID"], out int uid);

            int productId = (int)TempData["SingleItemID"];
            string productName = string.Empty;
            string productCategory = string.Empty;
            string productPrice = string.Empty;
            using (masterEntities db = new masterEntities())
            {
                TblUrunTanim product = await db.TblUrunTanims.FirstOrDefaultAsync(x => x.UrunId == productId);
                if (product == null) return RedirectToAction("Checkout");

                productName = product.UrunIsim;
                productCategory = product.UrunKategori;
                productPrice = product.UrunFiyat.ToString(new CultureInfo("en-US"));
            }
            CreateCheckoutFormInitializeRequest request = new CreateCheckoutFormInitializeRequest
            {
                Locale = Locale.TR.ToString(),
                Price = productPrice,
                ConversationId = new Random().Next(100000000, 999999999).ToString(),
                PaidPrice = productPrice,
                Currency = Currency.TRY.ToString(),
                PaymentGroup = PaymentGroup.PRODUCT.ToString()
            };
            request.CallbackUrl = $"{Request.Url.Scheme}://{Request.Url.Host}:{Request.Url.Port}{Url.Action("PurchaseCallback", "Home", new { id = Convert.ToInt32(request.ConversationId) })}";

            List<int> enabledInstallments = new List<int>
            {
                2,
                3,
                6,
                9
            };
            request.EnabledInstallments = enabledInstallments;

            Buyer buyer = new Buyer
            {
                Id = tempUser ? uid.ToString() : User.Identity.Name,
                Name = buyerName,
                Surname = buyerSurname,
                GsmNumber = telno,
                Email = contactInfo,
                IdentityNumber = "12345678911",
                RegistrationAddress = address,
                Ip = Request.UserHostAddress,
                City = city,
                Country = country,
                ZipCode = postalCode
            };
            request.Buyer = buyer;
            Address shippingAddress = new Address
            {
                ContactName = buyerName + " " + buyerSurname,
                City = city,
                Country = country,
                Description = address,
                ZipCode = postalCode
            };
            request.ShippingAddress = shippingAddress;
            Address billingAddress = new Address
            {
                ContactName = buyerName + " " + buyerSurname,
                City = city,
                Country = country,
                Description = address,
                ZipCode = postalCode
            };
            request.BillingAddress = billingAddress;

            List<BasketItem> basketItems = new List<BasketItem>
            {
                new BasketItem
                {
                    Id = productId.ToString(),
                    Name = productName,
                    Category1 = productCategory,
                    Category2 = productCategory + "2",
                    Price = productPrice,
                    ItemType = BasketItemType.PHYSICAL.ToString()
                }
            };

            request.BasketItems = basketItems;
            CheckoutFormInitialize checkoutFormInitialize = CheckoutFormInitialize.Create(request: request, options: options);
            using (masterEntities db = new masterEntities())
            {
                try
                {
                    TblPayment tblPayment = new TblPayment();

                    int paymentId = Convert.ToInt32(request.ConversationId);
                    tblPayment.PaymentId = paymentId;
                    Session["PaymentID"] = paymentId;

                    tblPayment.Token = checkoutFormInitialize.Token;

                    db.TblPayments.Add(tblPayment);

                    await db.SaveChangesAsync();
                }
                catch (DbEntityValidationException evx)
                {
                    Console.Error.WriteLine(evx.Message);
                    if (!string.IsNullOrWhiteSpace(checkoutFormInitialize.ErrorMessage))
                    {
                        TempData[ErrorMessage] = UppercaseFirst(checkoutFormInitialize.ErrorMessage);
                    }
                    else
                    {
                        foreach (DbEntityValidationResult validationErrors in evx.EntityValidationErrors)
                        {
                            foreach (DbValidationError validationError in validationErrors.ValidationErrors)
                            {
                                TempData[ErrorMessage] = $"Property: {validationError.PropertyName}, Error: {validationError.ErrorMessage}";
                            }
                        }
                    }

                    return RedirectToAction("Checkout", "Home");
                }
            }

            TblSiparisler order = new TblSiparisler();
            using (masterEntities db = new masterEntities())
            {
                order = db.TblSiparislers.Create();

                order.SiparisKullaniciId = tempUser ? uid : Convert.ToInt32(User.Identity.Name);

                order.SiparisTarih = DateTime.Now;
                order.SiparisAdres = $"Ülke: {country} | Şehir: {city} | Adres: {address} | Postal Code: {postalCode} | Tel no: {telno} |";

                TblSiparisItemler orderItem = new TblSiparisItemler
                {
                    UrunId = productId,
                    SiparisId = Convert.ToInt32(request.ConversationId),
                    UrunBeden = size
                };

                order.TblSiparisItemlers.Add(orderItem);
            }
            Session["Order"] = order;

            ViewBag.Iyzico = checkoutFormInitialize.CheckoutFormContent;

            return View(await GetModel(true));
        }

        private string Base64Encode(string plainText) => Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));

        private void RemovePaymentSessions()
        {
            Session.Remove("PaymentID");
            Session.Remove("Order");
            Session.Remove("CurrentlySinglePurchasing");
        }

        private async Task SendNewOrderNotifs(masterEntities db, string[] deviceTokens)
        {
            List<Task> tasks = new List<Task>();

            foreach (string token in deviceTokens)
            {
                Message messageToSend = new Message
                {
                    Token = token,
                    Notification = new Notification
                    {
                        Title = "P&D Boutique",
                        Body = "Yeni Siparişler Var!"
                    }
                };

                FirebaseMessaging messaging = FirebaseMessaging.GetMessaging(Startup.FirebaseApp);
                Task task = messaging.SendAsync(messageToSend)
                    .ContinueWith(async (sendTask) =>
                    {
                        if (sendTask.Exception != null)
                        {
                            if (sendTask.Exception.InnerException is FirebaseMessagingException fmex)
                            {
                                await HandleFmex(db, fmex, token);
                            }
                        }
                    });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        private async Task HandleFmex(masterEntities db, FirebaseMessagingException fmex, string deviceToken)
        {
            if (fmex.MessagingErrorCode == MessagingErrorCode.Unregistered || fmex.MessagingErrorCode == MessagingErrorCode.InvalidArgument)
            {
                await RemoveExpiredToken(db, deviceToken);
            }
        }

        private async Task RemoveExpiredToken(masterEntities db, string deviceToken)
        {
            TblCihazlar device = await db.TblCihazlars.FirstOrDefaultAsync(x => x.DeviceToken == deviceToken);
            if (device == null) return;

            db.TblCihazlars.Remove(device);
            await db.SaveChangesAsync();
        }

        private enum ActionType
        {
            View, Redirector, Ajax
        }

        public async Task<ActionResult> Orders()
        {
            bool tempUser = TryConvertToInt(Session["TempUID"], out int uid);
            try
            {
                if (!tempUser && !Request.IsAuthenticated)
                {
                    return RedirectToAction("Login", "Home", new { returnUrl = Url.Encode(Url.Action("Orders", "Home")) });
                }

                if (Session["PaymentID"] != null)
                {
                    int paymentId = (int)Session["PaymentID"];
                    TblSiparisler order = (TblSiparisler)Session["Order"];
                    using (masterEntities db = new masterEntities())
                    {
                        TblPaymentStatu paymentStatus = await db.TblPaymentStatus.FirstOrDefaultAsync(x => x.PaymentId == paymentId);
                        if (paymentStatus != null && paymentStatus.Success == Convert.ToInt32(true) && order != null)
                        {
                            db.TblSiparislers.Add(order);

                            ViewBag.PaymentSucceeded = true;
                            db.TblPaymentStatus.Remove(paymentStatus);

                            if (Session["CurrentlySinglePurchasing"] == null || !(bool)Session["CurrentlySinglePurchasing"])
                            {
                                int userId = tempUser ? uid : Convert.ToInt32(User.Identity.Name);
                                db.TblSepets.RemoveRange(db.TblSepets.Where(x => x.KullaniciId == userId));
                            }

                            await db.SaveChangesAsync();

                            int[] adminIds = await DbUsers.GetIdsWithRole(db, "Admin");
                            string[] tokens = await db.TblCihazlars.Where(x => adminIds.Contains(x.UserId)).Select(x => x.DeviceToken).ToArrayAsync();
                            await SendNewOrderNotifs(db, tokens);
                        }
                        else
                        {
                            ViewBag.PaymentSucceeded = false;
                            db.TblPaymentStatus.Remove(paymentStatus);

                            await db.SaveChangesAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                ViewBag.ErrMsg = ex.Message;
            }
            finally
            {
                if (tempUser)
                {
                    ViewBag.UID = uid;
                }

                RemovePaymentSessions();
            }

            return View(await GetModel());
        }

        public async Task<ActionResult> PurchaseCallback(int id)
        {
            if (!Request.IsSecureConnection)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            if (!ModelState.IsValid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Guvenli degil");
            }

            Options options = new Options
            {
                ApiKey = "sandbox-JlLOssMkFfdcZAK52n2UFp78r7LueQLI",
                SecretKey = "sandbox-pYtaoJfUg1BVTmSb6IA5V6pSavHzPQnU",
                BaseUrl = "https://sandbox-api.iyzipay.com"
            };

            string token = string.Empty;
            using (masterEntities db = new masterEntities())
            {
                TblPayment tblPayment = await db.TblPayments.FirstAsync(x => x.PaymentId == id);

                token = tblPayment.Token;
                db.TblPayments.Remove(tblPayment);

                await db.SaveChangesAsync();
            }

            RetrieveCheckoutFormRequest request = new RetrieveCheckoutFormRequest
            {
                Locale = Locale.TR.ToString(),
                Token = token
            };

            CheckoutForm checkoutForm = CheckoutForm.Retrieve(request, options);

            if (checkoutForm.PaymentStatus == "SUCCESS")
            {
                using (masterEntities db = new masterEntities())
                {
                    TblPaymentStatu paymentStatus = new TblPaymentStatu
                    {
                        PaymentId = id,
                        Success = Convert.ToInt32(true)
                    };
                   
                    db.TblPaymentStatus.Add(paymentStatus);

                    await db.SaveChangesAsync();
                }

                return RedirectToAction("Orders", "Home");
            }
            else
            {
                using (masterEntities db = new masterEntities())
                {
                    TblPaymentStatu paymentStatus = new TblPaymentStatu
                    {
                        PaymentId = id,
                        Success = Convert.ToInt32(false)
                    };

                    db.TblPaymentStatus.Add(paymentStatus);

                    await db.SaveChangesAsync();
                }

                return RedirectToAction("Orders", "Home");
            }
        }

        public async Task<ActionResult> UyelikSozlesmesi() => View(await GetModel(true));
    }
}