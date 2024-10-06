using ButikProje.Models;
using FirebaseAdmin.Messaging;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using static ButikProje.Commons.DbCommons;


namespace ButikProje.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {

        private const string Result = "Result";
        private const string PhotosDirectory = "/Content/img/UrunFotolari/";

        public async Task<ActionResult> Index(bool notifRedirect = false)
        {
            ViewBag.Result = TempData[Result];
            ViewBag.ShowSeeResultButton = TempData["ShowSRB"];
            ViewBag.NotifRedirect = notifRedirect;

            using (masterEntities db = new masterEntities())
            {
                return View("Index", await GetDefaultModel(db, Request, User.Identity));
            }
        }

        [HttpPost]
        public async Task<ActionResult> SaveDeviceToken(string token)
        {
            string result;
            try
            {
                using (masterEntities db = new masterEntities())
                {
                    int uid = Convert.ToInt32(User.Identity.Name);
                    if (!await db.TblCihazlars.AnyAsync(x => x.DeviceToken == token && x.UserId == uid))
                    {
                        TblCihazlar device = db.TblCihazlars.Create();
                        device.UserId = uid;
                        device.DeviceToken = token;

                        db.TblCihazlars.Add(device);

                        await db.SaveChangesAsync();

                        result = "Saved device token.";
                    }
                    else if (!await db.TblCihazlars.AnyAsync(x => x.DeviceToken == token))
                    {
                        IQueryable<TblCihazlar> oldTokens = db.TblCihazlars.Where(x => x.UserId == uid && x.DeviceToken != token);
                        db.TblCihazlars.RemoveRange(oldTokens);

                        TblCihazlar device = db.TblCihazlars.FirstOrDefault(x => x.UserId == uid);
                        if (device != null)
                        {
                            device.DeviceToken = token;
                        }

                        await db.SaveChangesAsync();

                        result = "Saved device token.";
                    }
                    else
                    {
                        result = "Device token already saved.";
                    }
                }
            }
            catch (Exception ex)
            {
                result = "An error occurred while saving device token: " + ex.Message;
            }

            return Json(new { result });
        }

        public async Task<ActionResult> SetSettings(bool websiteActive)
        {
            using (masterEntities db = new masterEntities())
            {
                TblButikAyarlar settings = await db.TblButikAyarlars.FirstOrDefaultAsync();
                settings.SiteAktif = Convert.ToInt32(websiteActive);

                string result = GetResultMessage(GetResult(await db.SaveChangesAsync() != 0, true));
                TempData[Result] = result;
            }

            return RedirectToActionWithHash(this, "Index", "4");
        }

        [HttpPost]
        public async Task<JsonResult> ListProductDetails(int selectedProductId)
        {
            try
            {
                List<string> selectedProductPhotos = new List<string>();

                using (masterEntities db = new masterEntities())
                {
                    TblUrunTanim selectedProduct = await db.TblUrunTanims.Include(x => x.TblUrunFotoes).FirstOrDefaultAsync(x => x.UrunId == selectedProductId);

                    string selectedProductName = selectedProduct.UrunIsim;
                    string selectedProductInfo = selectedProduct.UrunAciklama;
                    decimal selectedProductPrice = selectedProduct.UrunFiyat;

                    bool onSale = Convert.ToBoolean(selectedProduct.UrunIndirim);

                    decimal selectedProductOldPrice = 0;
                    string selectedProductSalePercentage = string.Empty;
                    if (onSale)
                    {
                        selectedProductOldPrice = selectedProduct.UrunEskiFiyat.GetValueOrDefault();
                        selectedProductSalePercentage = CalculateDiscountPercentage(selectedProductOldPrice, selectedProductPrice).ToString("0");
                    }


                    selectedProductPhotos = selectedProduct.TblUrunFotoes.OrderByDescending(x => x.FotoId).Select(photo => Url.Content(photo.UrunFoto)).ToList();

                    TblKategoriler selectedCategory = db.TblKategorilers.FirstOrDefault(x => x.KategoriIsim == selectedProduct.UrunKategori);
                    int selectedProductCategory = 0;
                    if (selectedCategory != null)
                    {
                        selectedProductCategory = selectedCategory.Id;
                    }

                    if (onSale)
                    {
                        return Json(new { selectedProductName, selectedProductInfo, selectedProductPrice, selectedProductPhotos, onSale, selectedProductOldPrice, selectedProductSalePercentage, selectedProductCategory });
                    }

                    return Json(new { selectedProductName, selectedProductInfo, selectedProductPrice, selectedProductPhotos, onSale, selectedProductCategory });
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return Json(new { result = ex.Message });
            }
        }

        private void SavePhotos(IEnumerable<HttpPostedFileBase> newFilePhotos, List<string> photoServerPaths)
        {
            foreach (HttpPostedFileBase inputProductPhoto in newFilePhotos)
            {
                if (inputProductPhoto != null && inputProductPhoto.ContentLength > 0)
                {
                    string inputPhotoName = Path.GetFileName(inputProductPhoto.FileName);
                    string photoServerPath = Path.Combine(PhotosDirectory, inputPhotoName);

                    while (System.IO.File.Exists(Server.MapPath(photoServerPath)))
                    {
                        inputPhotoName = Guid.NewGuid().ToString("N") + "_" + inputPhotoName;
                        photoServerPath = Path.Combine(PhotosDirectory, inputPhotoName);
                    }

                    inputProductPhoto.SaveAs(Server.MapPath(photoServerPath));
                    photoServerPaths.Add(photoServerPath);
                }
            }
        }

        private void DeleteOldPhotos(masterEntities db, int productId)
        {
            IQueryable<TblUrunFoto> oldPhotos = db.TblUrunFotoes.Where(x => x.UrunId == productId);
            db.TblUrunFotoes.RemoveRange(oldPhotos);
        }

        private void AddNewPhotos(masterEntities db, IEnumerable<string> photoServerPaths, int productId)
        {
            if (photoServerPaths == null || !photoServerPaths.Any()) return;

            IEnumerable<TblUrunFoto> newProductPhotos = photoServerPaths.Select(photoServerPath => new TblUrunFoto
            {
                UrunFoto = photoServerPath,
                UrunId = productId
            });

            db.TblUrunFotoes.AddRange(newProductPhotos);
        }

        private void SetProductPricing(TblUrunTanim product, string newPrice, bool onSale, string newSalePrice)
        {
            if (!string.IsNullOrWhiteSpace(newPrice))
            {
                product.UrunFiyat = decimal.Parse(newPrice, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
            }

            product.UrunIndirim = Convert.ToInt32(onSale);
            product.UrunEskiFiyat = onSale ? (decimal?)decimal.Parse(newSalePrice, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture) : null;
        }

        private async Task SetProductCategory(masterEntities db, TblUrunTanim product, int? newCategory)
        {
            if (newCategory != null)
            {
                TblKategoriler category = await db.TblKategorilers.FirstOrDefaultAsync(x => x.Id == newCategory);
                product.UrunKategori = category?.KategoriIsim;
            }
        }

        public async Task<ActionResult> EditSelectedProduct(int selectedProductId, string newName, string newPrice, string newDescription, bool onSale,
            IEnumerable<HttpPostedFileBase> newFilePhotos, string newUrlPhotos, int? newCategory, string newSalePrice = "")
        {
            try
            {
                using (masterEntities db = new masterEntities())
                {
                    TblUrunTanim selectedProduct = await db.TblUrunTanims.Include(x => x.TblUrunFotoes).FirstOrDefaultAsync(x => x.UrunId == selectedProductId);

                    if (!string.IsNullOrWhiteSpace(newName))
                    {
                        selectedProduct.UrunIsim = newName;
                    }

                    if (!string.IsNullOrWhiteSpace(newDescription))
                    {
                        selectedProduct.UrunAciklama = newDescription;
                    }

                    SetProductPricing(selectedProduct, newPrice, onSale, newSalePrice);
                    await SetProductCategory(db, selectedProduct, newCategory);
                    await db.SaveChangesAsync();

                    List<string> photoServerPaths = new List<string>();

                    if (newFilePhotos != null && newFilePhotos.Any())
                    {
                        SavePhotos(newFilePhotos, photoServerPaths);
                        DeleteOldPhotos(db, selectedProductId);
                        AddNewPhotos(db, photoServerPaths, selectedProductId);
                    }

                    if (!string.IsNullOrWhiteSpace(newUrlPhotos))
                    {
                        DeleteOldPhotos(db, selectedProductId);
                        string[] photoUrls = newUrlPhotos.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        AddNewPhotos(db, photoUrls, selectedProductId);
                    }

                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                TempData[Result] = ex.Message;
            }

            return RedirectToActionWithHash(this, "Index", "2");
        }

        public async Task<ActionResult> RemoveSelectedProduct(int selectedProductId)
        {
            try
            {
                using (masterEntities db = new masterEntities())
                {
                    TblUrunTanim selectedProduct = await db.TblUrunTanims.Include(x => x.TblUrunFotoes).FirstOrDefaultAsync(x => x.UrunId == selectedProductId);

                    DeleteOldPhotos(db, selectedProductId);
                    db.TblUrunTanims.Remove(selectedProduct);

                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                TempData[Result] = ex.Message;
            }

            return RedirectToActionWithHash(this, "Index", "2");
        }

        [HttpPost]
        public async Task<ActionResult> AddProduct(IEnumerable<HttpPostedFileBase> inputProductPhotos, string inputName, string inputDescription, string inputPrice, bool inputOnSale,
            string inputPhotoUrls, int inputCategory, string inputOldPrice = "0")
        {
            List<string> photoServerPaths = new List<string>();

            try
            {
                if (inputProductPhotos != null && inputProductPhotos.Any())
                {
                    SavePhotos(inputProductPhotos, photoServerPaths);
                }

                using (masterEntities db = new masterEntities())
                {
                    if (await db.TblUrunTanims.AnyAsync(x => x.UrunIsim == inputName))
                    {
                        throw new Exception("Bu isimle başka bir ürün zaten var, çakışma önlemek için lütfen farklı bir isim seçiniz.");
                    }

                    TblUrunTanim dbProductDisplay = db.TblUrunTanims.Create();
                    dbProductDisplay.UrunIsim = inputName;
                    dbProductDisplay.UrunAciklama = inputDescription;

                    SetProductPricing(dbProductDisplay, inputPrice, inputOnSale, inputOldPrice);
                    await SetProductCategory(db, dbProductDisplay, inputCategory);

                    db.TblUrunTanims.Add(dbProductDisplay);
                    await db.SaveChangesAsync();

                    AddNewPhotos(db, photoServerPaths, dbProductDisplay.UrunId);

                    if (!string.IsNullOrWhiteSpace(inputPhotoUrls))
                    {
                        string[] photoUrls = inputPhotoUrls.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        AddNewPhotos(db, photoUrls, dbProductDisplay.UrunId);
                    }

                    string result = GetResultMessage(GetResult(await db.SaveChangesAsync() != 0, true));
                    TempData[Result] = result;
                }
            }
            catch (DbUpdateException dux)
            {
                DeleteAllFiles(inputProductPhotos.Select(x => x.FileName));
                Console.Error.WriteLine(dux.Message);
                TempData[Result] = dux.Message;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                TempData[Result] = ex.Message;
            }

            return RedirectToActionWithHash(this, "Index", "2");
        }


        private void DeleteAllFiles(IEnumerable<string> fileNames)
        {
            foreach (string fileName in fileNames)
            {
                if (!IsURL(fileName))
                {
                    System.IO.File.Delete(Server.MapPath(fileName));
                }
            }
        }

        public async Task<ActionResult> DoesProductNameExist(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                try
                {
                    bool productNameExists = false;

                    using (masterEntities db = new masterEntities())
                    {
                        productNameExists = await db.TblUrunTanims.AnyAsync(x => x.UrunIsim == name);
                    }

                    return Json(new { productNameExists });
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    return Json(new { message = ex.Message });
                }
            }
            else
            {
                return Json(new { message = "İsim boş bırakılamaz." });
            }
        }

        public async Task<ActionResult> ChangeBannerContent(string AfisBaslikEdit, string AfisAltBaslikEdit)
        {
            try
            {
                using (masterEntities db = new masterEntities())
                {
                    TblArayuz entityInstance = await db.TblArayuzs.SingleAsync();

                    if (!string.IsNullOrEmpty(AfisBaslikEdit))
                    {
                        entityInstance.AfisBaslik = AfisBaslikEdit;
                    }

                    if (!string.IsNullOrEmpty(AfisAltBaslikEdit))
                    {
                        entityInstance.AfisAltbaslik = AfisAltBaslikEdit;
                    }
                    else if (string.IsNullOrEmpty(AfisBaslikEdit) && string.IsNullOrEmpty(AfisAltBaslikEdit))
                    {
                        throw new Exception(GetDetailedResultMessage(ErrDetails.EmptyInputs));
                    }

                    string result = GetResultMessage(GetResult(await db.SaveChangesAsync() != 0, true));

                    TempData[Result] = result;
                    TempData["ShowSRB"] = true;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                TempData[Result] = ex.Message;
            }

            return RedirectToActionWithHash(this, "Index", "1");
        }
        public PartialViewResult GetBannerContentForm()
        {
            return PartialView("TempFormCBC");
        }

        public async Task<ActionResult> ChangeFooterContent(string AltbilgiEdit)
        {
            try
            {
                using (masterEntities db = new masterEntities())
                {
                    TblArayuz entityInstance = await db.TblArayuzs.SingleAsync();

                    if (!string.IsNullOrEmpty(AltbilgiEdit))
                    {
                        entityInstance.Altbilgi = AltbilgiEdit;
                    }
                    else
                    {
                        throw new Exception(GetDetailedResultMessage(ErrDetails.EmptyInputs));
                    }

                    string result = GetResultMessage(GetResult(await db.SaveChangesAsync() != 0, true));
                    TempData[Result] = result;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                TempData[Result] = ex.Message;
            }

            return RedirectToActionWithHash(this, "Index", "1");
        }
        public PartialViewResult GetFooterContentForm()
        {
            return PartialView("TempFormCFC");
        }

        private async Task<string> RemoveUser(masterEntities db, int uid)
        {
            TblButikKullanicilar user = await db.TblButikKullanicilars.FirstOrDefaultAsync(x => x.Id == uid) ?? throw new Exception("Kullanıcı bulunamadı. Bu hata ile tekrar karşılaşırsanız yazılımcı ile iletişime geçiniz.");
            bool userIsAdmin = await db.TblAdmins.AnyAsync(x => x.KullaniciId == uid);
            string userRoleName = !userIsAdmin ? "User" : "Admin";

            db.TblButikKullanicilars.Remove(user);

            string uidName = uid.ToString();
            if (Roles.IsUserInRole(uidName, userRoleName))
            {
                Roles.RemoveUserFromRole(uidName, userRoleName);

                if (userIsAdmin && await db.TblAdmins.FirstOrDefaultAsync(x => x.KullaniciId == uid) is TblAdmin adminRef)
                {
                    db.TblAdmins.Remove(adminRef);
                }
            }

            string result = GetResultMessage(GetResult(await db.SaveChangesAsync() != 0, true));
            return result;
        } 

        public async Task<ActionResult> DeleteUser(int userId)
        {
            try
            {
                using (masterEntities db = new masterEntities())
                {
                    TempData[Result] = await RemoveUser(db, userId);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                TempData[Result] = ex.Message;
            }

            return RedirectToActionWithHash(this, "Index", "3");
        }
        public async Task<ActionResult> ChangeRole(int userId, int currentRole)
        {
            try
            {
                using (masterEntities db = new masterEntities())
                {
                    TblButikKullanicilar user = await db.TblButikKullanicilars.SingleOrDefaultAsync(x => x.Id == userId);

                    if (currentRole == DbUsers.AdminRoleNumber)
                    {
                        db.TblAdmins.Remove(await db.TblAdmins.SingleOrDefaultAsync(x => x.KullaniciId == userId));

                        string userIdName = userId.ToString();
                        if (Roles.IsUserInRole(userIdName, DbUsers.AdminRoleName))
                        {
                            Roles.RemoveUserFromRole(userIdName, "Admin");
                        }

                        if (!Roles.IsUserInRole(userIdName, DbUsers.UserRoleName))
                        {
                            Roles.AddUserToRole(userIdName, "User");
                        }

                    }
                    else if (currentRole == DbUsers.UserRoleNumber)
                    {
                        TblAdmin tblAdmins = new TblAdmin
                        {
                            KullaniciId = userId
                        };
                        db.TblAdmins.Add(tblAdmins);

                        string userIdName = userId.ToString();
                        if (Roles.IsUserInRole(userIdName, DbUsers.UserRoleName))
                        {
                            Roles.RemoveUserFromRole(userIdName, "User");
                        }

                        if (!Roles.IsUserInRole(userIdName, DbUsers.AdminRoleName))
                        {
                            Roles.AddUserToRole(userIdName, "Admin");
                        }
                    }


                    string result = GetResultMessage(GetResult(await db.SaveChangesAsync() != 0, true));
                    TempData[Result] = result;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                TempData[Result] = ex.Message;

            }

            return RedirectToActionWithHash(this, "Index", "3");
        }

        public async Task<ActionResult> CreateCategory(string inputCategoryName)
        {
            try
            {
                using (masterEntities db = new masterEntities())
                {
                    TblKategoriler category = new TblKategoriler
                    {
                        KategoriIsim = inputCategoryName
                    };
                    db.TblKategorilers.Add(category);

                    string result = GetResultMessage(GetResult(await db.SaveChangesAsync() != 0, true));
                    TempData[Result] = result;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                TempData[Result] = ex.Message;
            }

            return RedirectToActionWithHash(this, "Index", "2");
        }

        public async Task<ActionResult> EditCategory(int selectedCategoryIdEditForm, string newCategoryName)
        {
            try
            {
                using (masterEntities db = new masterEntities())
                {
                    TblKategoriler category = await db.TblKategorilers.FirstOrDefaultAsync(x => x.Id == selectedCategoryIdEditForm);
                    category.KategoriIsim = newCategoryName;

                    string result = GetResultMessage(GetResult(await db.SaveChangesAsync() != 0, true));
                    TempData[Result] = result;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                TempData[Result] = ex.Message;
            }

            return RedirectToActionWithHash(this, "Index", "2");
        }

        public async Task<ActionResult> DeleteCategory(int selectedCategoryIdConfirmationForm)
        {
            try
            {
                using (masterEntities db = new masterEntities())
                {
                    TblKategoriler category = await db.TblKategorilers.FirstOrDefaultAsync(x => x.Id == selectedCategoryIdConfirmationForm);
                    db.TblKategorilers.Remove(category);

                    string result = GetResultMessage(GetResult(await db.SaveChangesAsync() != 0, true));
                    TempData[Result] = result;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                TempData[Result] = ex.Message;
            }

            return RedirectToActionWithHash(this, "Index", "2");
        }

        public async Task<ActionResult> ChangeProductCategories(List<int> selectedProductsIds, int newCategoryId)
        {
            try
            {
                using (masterEntities db = new masterEntities())
                {
                    TblKategoriler newCategory = await db.TblKategorilers.FirstOrDefaultAsync(x => x.Id == newCategoryId);
                    List<TblUrunTanim> selectedProducts = await db.TblUrunTanims
                                                                .Where(x => selectedProductsIds.Contains(x.UrunId))
                                                                .ToListAsync();

                    foreach (TblUrunTanim selectedProduct in selectedProducts)
                    {
                        selectedProduct.UrunKategori = newCategory.KategoriIsim;
                    }

                    string result = GetResultMessage(GetResult(await db.SaveChangesAsync() != 0, true));
                    TempData[Result] = result;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                TempData[Result] = ex.Message;
            }

            return RedirectToActionWithHash(this, "Index", "2");
        }

        public async Task<ActionResult> EditContactInfo(string email, string phone, string instagram, string youtube)
        {
            try
            {
                using (masterEntities db = new masterEntities())
                {
                    TblIletisim contactInfoTbl = await db.TblIletisims.FirstOrDefaultAsync();

                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        contactInfoTbl.Mail = email;
                    }
                    if (!string.IsNullOrWhiteSpace(phone))
                    {
                        contactInfoTbl.TelNo = phone;
                    }
                    if (!string.IsNullOrWhiteSpace(instagram))
                    {
                        contactInfoTbl.Instagram = instagram;
                    }
                    if (!string.IsNullOrWhiteSpace(youtube))
                    {
                        contactInfoTbl.Youtube = youtube;
                    }

                    string result = GetResultMessage(GetResult(await db.SaveChangesAsync() != 0, true));
                    TempData[Result] = result;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                TempData[Result] = ex.Message;
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<ActionResult> SetUserAgreement(string content)
        {
            try
            {
                using (masterEntities db = new masterEntities())
                {
                    TblArayuz tblInterface = await db.TblArayuzs.SingleAsync();

                    tblInterface.UyelikSozlesmesi = content;

                    string result = GetResultMessage(GetResult(await db.SaveChangesAsync() != 0, true));
                    TempData[Result] = result;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                TempData[Result] = ex.Message;
            }

            return RedirectToActionWithHash(this, "Index", "1");
        }

        public async Task<ActionResult> RemoveOrder(int orderId)
        {
            try
            {
                using (masterEntities db = new masterEntities())
                {
                    IQueryable<TblSiparisItemler> orderItems = db.TblSiparisItemlers.Where(x => x.SiparisId == orderId);
                    db.TblSiparisItemlers.RemoveRange(orderItems);

                    TblSiparisler order = await db.TblSiparislers.FirstOrDefaultAsync(x => x.SiparisId == orderId);
                    db.TblSiparislers.Remove(order);

                    string result = GetResultMessage(GetResult(await db.SaveChangesAsync() != 0, true));
                    TempData[Result] = result;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                TempData[Result] = ex.Message;
            }

            return RedirectToAction("Index", "Admin");
        }

        private string Base64Encode(string plainText)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}