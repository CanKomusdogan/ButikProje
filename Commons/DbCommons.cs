using ButikProje.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ButikProje.Commons
{
    public static class DbCommons
    {
        public const int PageSize = 20;

        /// <returns>Custom class.</returns>
        public static async Task<List<DbProduct>> GetProducts(masterEntities db, Filters filters = Filters.Default, int page = 1)
        {
            IOrderedQueryable<TblUrunTanim> products;
            switch (filters)
            {
                case Filters.Default:
                    products = db.TblUrunTanims.Include(x => x.TblUrunFotoes).OrderByDescending(x => x.UrunId);
                    break;
                case Filters.IncreasingPrice:
                    products = db.TblUrunTanims.Include(x => x.TblUrunFotoes).OrderBy(x => x.UrunFiyat);
                    break;
                case Filters.DecreasingPrice:
                    products = db.TblUrunTanims.Include(x => x.TblUrunFotoes).OrderByDescending(x => x.UrunFiyat);
                    break;
                default:
                    products = db.TblUrunTanims.Include(x => x.TblUrunFotoes).OrderByDescending(x => x.UrunId);
                    break;
            }

            IQueryable<TblUrunTanim> pagedProducts = products.Skip((page - 1) * PageSize).Take(PageSize);

            List<TblUrunTanim> fetchedProducts = await pagedProducts.ToListAsync();
            List<DbProduct> dbProducts = fetchedProducts.Select(product =>
            {
                DbProduct dbProduct = new DbProduct
                {
                    ID = product.UrunId,
                    Name = product.UrunIsim,
                    Details = product.UrunAciklama,
                    Price = product.UrunFiyat,
                    OnSale = product.UrunIndirim != 0,
                    Category = product.UrunKategori,
                    OldPrice = product.UrunIndirim != 0 ? product.UrunEskiFiyat : null
                };
                if (dbProduct.OnSale)
                {
                    dbProduct.SalePercentage = CalculateDiscountPercentage(dbProduct.OldPrice.GetValueOrDefault(), dbProduct.Price);
                }

                return dbProduct;
            }).ToList();

            foreach (DbProduct dbProduct in dbProducts)
            {
                dbProduct.Photos = await GetProductPhotos(db, dbProduct.ID);
            }

            return dbProducts;
        }

        /// <returns>Custom class.</returns>
        public static async Task<List<DbProduct>> GetProducts(masterEntities db, Filters filters = Filters.Default)
        {
            IOrderedQueryable<TblUrunTanim> products;
            switch (filters)
            {
                case Filters.Default:
                    products = db.TblUrunTanims.Include(x => x.TblUrunFotoes).OrderByDescending(x => x.UrunId);
                    break;
                case Filters.IncreasingPrice:
                    products = db.TblUrunTanims.Include(x => x.TblUrunFotoes).OrderBy(x => x.UrunFiyat);
                    break;
                case Filters.DecreasingPrice:
                    products = db.TblUrunTanims.Include(x => x.TblUrunFotoes).OrderByDescending(x => x.UrunFiyat);
                    break;
                default:
                    products = db.TblUrunTanims.Include(x => x.TblUrunFotoes).OrderByDescending(x => x.UrunId);
                    break;
            }

            List<TblUrunTanim> fetchedProducts = await products.ToListAsync();
            List<Task<DbProduct>> tasks = fetchedProducts.Select(async product =>
            {
                DbProduct dbProduct = new DbProduct
                {
                    ID = product.UrunId,
                    Name = product.UrunIsim,
                    Details = product.UrunAciklama,
                    Price = product.UrunFiyat,
                    OnSale = product.UrunIndirim != 0,
                    Category = product.UrunKategori,
                    Sizes = await GetSizesOfProduct(product.UrunId),
                    OldPrice = product.UrunIndirim != 0 ? product.UrunEskiFiyat : null
                };

                if (dbProduct.OnSale)
                {
                    dbProduct.SalePercentage = CalculateDiscountPercentage(dbProduct.OldPrice.GetValueOrDefault(), dbProduct.Price);
                }

                return dbProduct;
            }).ToList();

            List<DbProduct> dbProducts = (await Task.WhenAll(tasks)).ToList();


            foreach (DbProduct dbProduct in dbProducts)
            {
                dbProduct.Photos = await GetProductPhotos(db, dbProduct.ID);
            }

            return dbProducts;
        }

        /// <returns>Custom class.</returns>
        public static async Task<List<DbProduct>> GetProducts(masterEntities db, string categoryName, int page = 1)
        {
            IQueryable<TblUrunTanim> products = db.TblUrunTanims.Include(x => x.TblUrunFotoes).OrderByDescending(x => x.UrunId).Where(x => x.UrunKategori == categoryName);

            IQueryable<TblUrunTanim> pagedProducts = products.Skip((page - 1) * PageSize).Take(PageSize);

            List<TblUrunTanim> fetchedProducts = await pagedProducts.ToListAsync();
            List<Task<DbProduct>> tasks = fetchedProducts.Select(async product =>
            {
                DbProduct dbProduct = new DbProduct
                {
                    ID = product.UrunId,
                    Name = product.UrunIsim,
                    Details = product.UrunAciklama,
                    Price = product.UrunFiyat,
                    OnSale = product.UrunIndirim != 0,
                    Category = product.UrunKategori,
                    Sizes = await GetSizesOfProduct(product.UrunId),
                    OldPrice = product.UrunIndirim != 0 ? product.UrunEskiFiyat : null
                };

                if (dbProduct.OnSale)
                {
                    dbProduct.SalePercentage = CalculateDiscountPercentage(dbProduct.OldPrice.GetValueOrDefault(), dbProduct.Price);
                }

                return dbProduct;
            }).ToList();

            List<DbProduct> dbProducts = (await Task.WhenAll(tasks)).ToList();


            foreach (DbProduct dbProduct in dbProducts)
            {
                dbProduct.Photos = await GetProductPhotos(db, dbProduct.ID);
            }
            return dbProducts;
        }

        /// <returns>Custom class.</returns>
        public static async Task<List<DbProduct>> GetProducts(masterEntities db, IEnumerable<int> productIds)
        {
            IQueryable<TblUrunTanim> products = db.TblUrunTanims.Where(x => productIds.Contains(x.UrunId));

            List<TblUrunTanim> fetchedProducts = await products.ToListAsync();
            List<Task<DbProduct>> tasks = fetchedProducts.Select(async product =>
            {
                DbProduct dbProduct = new DbProduct
                {
                    ID = product.UrunId,
                    Name = product.UrunIsim,
                    Details = product.UrunAciklama,
                    Price = product.UrunFiyat,
                    OnSale = product.UrunIndirim != 0,
                    Category = product.UrunKategori,
                    Sizes = await GetSizesOfProduct(product.UrunId),
                    OldPrice = product.UrunIndirim != 0 ? product.UrunEskiFiyat : null
                };

                if (dbProduct.OnSale)
                {
                    dbProduct.SalePercentage = CalculateDiscountPercentage(dbProduct.OldPrice.GetValueOrDefault(), dbProduct.Price);
                }

                return dbProduct;
            }).ToList();

            List<DbProduct> dbProducts = (await Task.WhenAll(tasks)).ToList();


            foreach (DbProduct dbProduct in dbProducts)
            {
                dbProduct.Photos = await GetProductPhotos(db, dbProduct.ID);
            }

            return dbProducts;
        }

        /// <returns>Custom class.</returns>
        public static async Task<DbProduct> GetProduct(masterEntities db, int productId)
        {
            TblUrunTanim product = await db.TblUrunTanims.Include(x => x.TblUrunFotoes).FirstOrDefaultAsync(x => x.UrunId == productId);

            DbProduct productModel = new DbProduct
            {
                ID = product.UrunId,
                Name = product.UrunIsim,
                Details = product.UrunAciklama,
                Category = product.UrunKategori,
                Price = product.UrunFiyat,
                OnSale = product.UrunIndirim != 0,
                Photos = await GetProductPhotos(db, product.UrunId),
                Sizes = await GetSizesOfProduct(product.UrunId),
                OldPrice = product.UrunIndirim != 0 ? product.UrunEskiFiyat : null,
                SalePercentage = product.UrunIndirim != 0 ? CalculateDiscountPercentage(product.UrunEskiFiyat ?? 0, product.UrunFiyat) : (double?)null
            };

            return productModel;
        }

        /// <returns>Custom class.</returns>
        public static async Task<List<DbProductPhoto>> GetProductPhotos(masterEntities db, int productId)
        {
            TblUrunTanim productImgs = await db.TblUrunTanims.Include(x => x.TblUrunFotoes).SingleOrDefaultAsync(x => x.UrunId == productId);
            if (productImgs == null)
            {
                return new List<DbProductPhoto>();
            }

            List<DbProductPhoto> productPhotos = productImgs.TblUrunFotoes
                                                                .Select(photo => new DbProductPhoto
                                                                {
                                                                    ID = photo.FotoId,
                                                                    ConnectedProductID = photo.UrunId,
                                                                    PhotoContent = photo.UrunFoto
                                                                })
                                                                .ToList();

            return productPhotos;
        }
        /// <returns>Custom class.</returns>
        public static List<DbProductPhoto> ToProductPhotos(IEnumerable<TblUrunFoto> tblProductPhotos)
        {
            if (tblProductPhotos == null || !tblProductPhotos.Any())
            {
                return new List<DbProductPhoto>();
            }

            List<DbProductPhoto> dbProductPhotos = tblProductPhotos.Select(photo => new DbProductPhoto
            {
                ID = photo.FotoId,
                ConnectedProductID = photo.UrunId,
                PhotoContent = photo.UrunFoto
            }).ToList();

            return dbProductPhotos;
        }

        public enum DataTransfererType
        {
            Session,
            TempData,
            ViewData,
            ViewBag,

            /// <summary>
            /// For the ClearData function.
            /// </summary>
            All
        }

        public enum Result
        {
            Success, Error,
        }
        public enum OccuredOn
        {
            Login, Register,
            Verification, PasswordChange

        }
        public enum ErrDetails
        {
            InvalidLogin, EmailExists, EmailUnregistered, NotAuthorized,
            ConnectionError, FormatError, AlreadySignedIn,
            EmptyInputs, IncorrectVerificationCode, ProductsInCartRemoved
        }

        public static string GetResultMessage(Result result)
        {
            string resultMessage = result == Result.Success ? "Değişiklikler uygulandı." : "Değişiklikler uygulanamadı.";

            return resultMessage;
        }
        public static string GetResultMessage(Result result, OccuredOn occuredOn)
        {
            string resultMessage;

            switch (occuredOn)
            {
                case OccuredOn.Login:
                    resultMessage = result == Result.Success ? "Giriş başarılı." : "Giriş başarısız.";
                    break;
                case OccuredOn.Register:
                    resultMessage = result == Result.Success ? "Kaydolma başarılı." : "Kaydolma başarısız.";
                    break;
                case OccuredOn.Verification:
                    resultMessage = result == Result.Success ? "Doğrulama başarılı." : "Doğrulama başarısız.";
                    break;
                case OccuredOn.PasswordChange:
                    resultMessage = result == Result.Success ? "Şifre değiştirme başarılı." : "Şifre değiştirme başarısız.";
                    break;
                default:
                    resultMessage = result == Result.Success ? "Başarılı." : "Başarısız";
                    break;
            }

            return resultMessage;
        }


        /// <summary>
        /// Returns result based on the comparison of the params
        /// </summary>
        public static Result GetResult(params IComparable[] comparableObject)
        {
            for (int i = 1; i < comparableObject.Length; i++)
            {
                if (!comparableObject[i].Equals(comparableObject[i - 1]))
                {
                    return Result.Error;
                }
            }
            return Result.Success;
        }

        public static string GetDetailedResultMessage(ErrDetails err) => GetResultMessage(Result.Error) + "\u0020" + GetResultMessageDetails(err: err);
        public static string GetDetailedResultMessage(ErrDetails err, OccuredOn occuredOn) => GetResultMessage(Result.Error, occuredOn) + "\u0020" + GetResultMessageDetails(err: err);

        public static string GetResultMessageDetails(ErrDetails err)
        {
            string resultMessageDetails;
            switch (err)
            {
                case ErrDetails.InvalidLogin:
                    resultMessageDetails = "Giriş bilgileri doğru değildi.";
                    break;
                case ErrDetails.EmailExists:
                    resultMessageDetails = "Bu e-posta sistemde zaten kayıtlı.";
                    break;
                case ErrDetails.EmailUnregistered:
                    resultMessageDetails = "Bu e-posta sistemde kayıtlı değil.";
                    break;
                case ErrDetails.NotAuthorized:
                    resultMessageDetails = "Bu işlemi gerçekleştirmek için yeterli yetkiniz yok.";
                    break;
                case ErrDetails.ConnectionError:
                    resultMessageDetails = "Bağlantı hatası!";
                    break;
                case ErrDetails.FormatError:
                    resultMessageDetails = "Format hatası!";
                    break;
                case ErrDetails.AlreadySignedIn:
                    resultMessageDetails = "Zaten giriş yapıldı.";
                    break;
                case ErrDetails.EmptyInputs:
                    resultMessageDetails = "Kutucuklar boş!";
                    break;
                case ErrDetails.IncorrectVerificationCode:
                    resultMessageDetails = "Kod doğru değildi, lütfen tekrar deneyiniz.";
                    break;
                case ErrDetails.ProductsInCartRemoved:
                    resultMessageDetails = "Sepetinize eklenen ürünlerin bazıları kaldırıldı.";
                    break;
                default:
                    resultMessageDetails = "Bilinmeyen hata";
                    break;

            }

            return resultMessageDetails;
        }

        public static double CalculateDiscountPercentage(decimal oldPrice, decimal newPrice)
        {
            decimal discountAmount = oldPrice - newPrice;
            return (double)((discountAmount / oldPrice) * 100);
        }

        /// <summary>
        /// Gets the cart count.
        /// </summary>
        /// <returns>The cart count of <paramref name="identity"/> if the <paramref name="request"/> is authenticated</returns>
        public static async Task<int> GetCartCount(masterEntities db, HttpRequestBase request, IIdentity identity)
        {
            if (request.IsAuthenticated)
            {
                int uid = Convert.ToInt32(identity.Name);
                return await db.TblSepets.CountAsync(x => x.KullaniciId == uid);
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// Gets the cart count.
        /// </summary>
        /// <returns>The cart count of <paramref name="uid"/></returns>
        public static async Task<int> GetCartCount(masterEntities db, int uid) => await db.TblSepets.CountAsync(x => x.KullaniciId == uid);

        /// <returns>Auto generated database class.</returns>
        public static async Task<List<TblSepet>> GetProductsInCart(masterEntities db, HttpRequestBase request, IIdentity identity)
        {
            if (request.IsAuthenticated)
            {
                int uid = Convert.ToInt32(identity.Name);
                return await db.TblSepets.Where(x => x.KullaniciId == uid).ToListAsync();
            }
            else
            {
                return null;
            }

        }
        /// <returns>Auto generated database class.</returns>
        public static async Task<List<TblSepet>> GetProductsInCart(masterEntities db, int uid) => await db.TblSepets.Where(x => x.KullaniciId == uid).ToListAsync();

        /// <returns>Custom class.</returns>
        public static async Task<List<DbProduct>> GetDbProductsInCart(masterEntities db, HttpRequestBase request, IIdentity identity)
        {
            List<DbProduct> products = new List<DbProduct>();

            if (request.IsAuthenticated)
            {
                products.AddRange(await GetProducts(db, (await GetProductsInCart(db, request, identity)).Select(x => x.UrunId)));
            }

            return products;
        }
        public static async Task<List<DbProduct>> GetDbProductsInCart(masterEntities db, int uid)
        {
            List<DbProduct> products = new List<DbProduct>();

            products.AddRange(await GetProducts(db, (await GetProductsInCart(db, uid)).Select(x => x.UrunId)));

            return products;
        }

        public static async Task<decimal?> GetTotalOfProductsInCart(masterEntities db, HttpRequestBase request, IIdentity identity)
        {
            List<TblSepet> productsInCart = await GetProductsInCart(db, request, identity);
            if (productsInCart == null || productsInCart.Count == 0)
            {
                return null;
            }

            decimal? price = 0;

            int userId = Convert.ToInt32(identity.Name);
            List<int> productIds = await db.TblSepets.Where(x => x.KullaniciId == userId).Select(x => x.UrunId).ToListAsync();

            price = await db.TblUrunTanims.Where(x => productIds.Contains(x.UrunId)).SumAsync(x => x.UrunFiyat);

            return price;
        }
        public static async Task<decimal?> GetTotalOfProductsInCart(masterEntities db, int uid)
        {
            List<TblSepet> productsInCart = await GetProductsInCart(db, uid);
            if (productsInCart == null || !productsInCart.Any())
            {
                return null;
            }

            decimal? price = 0;

            List<int> productIds = await db.TblSepets.Where(x => x.KullaniciId == uid).Select(x => x.UrunId).ToListAsync();

            price = await db.TblUrunTanims.Where(x => productIds.Contains(x.UrunId)).SumAsync(x => x.UrunFiyat);

            return price;
        }

        public enum Filters
        {
            IncreasingPrice, DecreasingPrice, Default
        }

        public static string FilterUserTranslator(Filters filters)
        {
            string userFriendlyFilterText = string.Empty;

            switch (filters)
            {
                case Filters.Default:
                    userFriendlyFilterText = "Yeni eklenenler";
                    break;
                case Filters.IncreasingPrice:
                    userFriendlyFilterText = "Artan fiyat";
                    break;
                case Filters.DecreasingPrice:
                    userFriendlyFilterText = "Azalan fiyat";
                    break;
            }

            return userFriendlyFilterText;
        }

        public static async Task<List<TblBedenler>> GetSizes(masterEntities db) => await db.TblBedenlers.ToListAsync();

        public static async Task<List<TblUrunBedenleri>> GetProductSizes(masterEntities db) => await db.TblUrunBedenleris.ToListAsync();
        public static async Task<List<TblUrunBedenleri>> GetProductSizes(masterEntities db, int productId) => await db.TblUrunBedenleris.Where(x => x.UrunId == productId).ToListAsync();
        public static async Task<List<TblBedenler>> GetSizesOfProduct(int productId)
        {
            using (masterEntities db = new masterEntities())
            {
                return await db.TblUrunBedenleris
                    .Where(ub => ub.UrunId == productId)
                    .Join(db.TblBedenlers,
                        ub => ub.BedenId,
                        b => b.Id,
                        (ub, b) => b)
                    .ToListAsync();
            }
        }
        public static async Task<List<TblUrunBedenleri>> GetAllProductSizes(masterEntities db, IEnumerable<int> productIds) => await db.TblUrunBedenleris.Where(x => productIds.Contains(x.UrunId)).ToListAsync();

        /// <summary>
        /// Don't use it if your view doesn't require all data that is needed on most views.
        /// </summary>
        public static async Task<DbInterface> GetDefaultModel(masterEntities db, HttpRequestBase request, IIdentity identity, Filters filters = Filters.Default)
        {
            TblArayuz setInterface = await db.TblArayuzs.SingleOrDefaultAsync();
            DbInterface dbInterface = new DbInterface
            {
                Products = await GetProducts(db, filters),
                CartCount = await GetCartCount(db, request, identity),
                ProductsInCart = await GetProductsInCart(db, request, identity),
                ProductSizes = await GetSizes(db),
                ProductCategories = await db.TblKategorilers.ToListAsync(),
                ContactInfo = await db.TblIletisims.SingleOrDefaultAsync()
            };

            if (request.IsAuthenticated && Roles.IsUserInRole(identity.Name, DbUsers.AdminRoleName))
            {
                dbInterface.Orders = await db.TblSiparislers.Include(x => x.TblSiparisItemlers).OrderByDescending(x => x.SiparisId).ToListAsync();
            }
            else if (request.IsAuthenticated)
            {
                int uid = Convert.ToInt32(identity.Name);
                dbInterface.Orders = await db.TblSiparislers.Include(x => x.TblSiparisItemlers).Where(x => x.SiparisKullaniciId == uid).OrderByDescending(x => x.SiparisId).ToListAsync();
            }

            if (setInterface != null)
            {
                dbInterface.BannerTitle = setInterface.AfisBaslik;
                dbInterface.BannerAltTitle = setInterface.AfisAltbaslik;
                dbInterface.FooterText = setInterface.Altbilgi;
                dbInterface.UserAgreement = setInterface.UyelikSozlesmesi;
            }

            return dbInterface;
        }
        public static async Task<DbInterface> GetDefaultModel(masterEntities db, int uid, Filters filters = Filters.Default)
        {
            TblArayuz setInterface = await db.TblArayuzs.SingleOrDefaultAsync();
            DbInterface dbInterface = new DbInterface
            {
                Products = await GetProducts(db, filters),
                CartCount = await GetCartCount(db, uid),
                ProductsInCart = await GetProductsInCart(db, uid),
                ProductSizes = await GetSizes(db),
                ProductCategories = await db.TblKategorilers.ToListAsync(),
                ContactInfo = await db.TblIletisims.SingleOrDefaultAsync(),
                Orders = await db.TblSiparislers.Include(x => x.TblSiparisItemlers).Where(x => x.SiparisKullaniciId == uid).OrderByDescending(x => x.SiparisId).ToListAsync()
            };

            if (setInterface != null)
            {
                dbInterface.BannerTitle = setInterface.AfisBaslik;
                dbInterface.BannerAltTitle = setInterface.AfisAltbaslik;
                dbInterface.FooterText = setInterface.Altbilgi;
                dbInterface.UserAgreement = setInterface.UyelikSozlesmesi;
            }

            return dbInterface;
        }
        public static async Task<DbInterface> GetDefaultModel(masterEntities db, HttpRequestBase request, IIdentity identity, int page, Filters filters = Filters.Default)
        {
            TblArayuz setInterface = await db.TblArayuzs.SingleOrDefaultAsync();
            DbInterface dbInterface = new DbInterface
            {
                Products = await GetProducts(db, filters, page),
                CartCount = await GetCartCount(db, request, identity),
                ProductsInCart = await GetProductsInCart(db, request, identity),
                ProductSizes = await GetSizes(db),
                ProductCategories = await db.TblKategorilers.ToListAsync(),
                ContactInfo = await db.TblIletisims.SingleOrDefaultAsync()
            };

            if (request.IsAuthenticated)
            {
                int uid = Convert.ToInt32(identity.Name);
                dbInterface.Orders = await db.TblSiparislers.Include(x => x.TblSiparisItemlers).Where(x => x.SiparisKullaniciId == uid).OrderByDescending(x => x.SiparisId).ToListAsync();
            }

            if (setInterface != null)
            {
                dbInterface.BannerTitle = setInterface.AfisBaslik;
                dbInterface.BannerAltTitle = setInterface.AfisAltbaslik;
                dbInterface.FooterText = setInterface.Altbilgi;
                dbInterface.UserAgreement = setInterface.UyelikSozlesmesi;
            }

            return dbInterface;
        }
        public static async Task<DbInterface> GetDefaultModel(masterEntities db, int uid, int page, Filters filters = Filters.Default)
        {
            TblArayuz setInterface = await db.TblArayuzs.SingleOrDefaultAsync();
            DbInterface dbInterface = new DbInterface
            {
                Products = await GetProducts(db, filters, page),
                CartCount = await GetCartCount(db, uid),
                ProductsInCart = await GetProductsInCart(db, uid),
                ProductCategories = await db.TblKategorilers.ToListAsync(),
                ProductSizes = await GetSizes(db),
                ContactInfo = await db.TblIletisims.SingleOrDefaultAsync(),
                Orders = await db.TblSiparislers.Include(x => x.TblSiparisItemlers).Where(x => x.SiparisKullaniciId == uid).OrderByDescending(x => x.SiparisId).ToListAsync()
            };

            if (setInterface != null)
            {
                dbInterface.BannerTitle = setInterface.AfisBaslik;
                dbInterface.BannerAltTitle = setInterface.AfisAltbaslik;
                dbInterface.FooterText = setInterface.Altbilgi;
                dbInterface.UserAgreement = setInterface.UyelikSozlesmesi;
            }

            return dbInterface;
        }

        /// <summary>
        /// The properties of this enum contains table names that are the most expensive to fetch in terms of performance. <br />
        /// There might be cases where you only need to fetch some of them on certain views.
        /// </summary>
        public enum TablesToFetch
        {
            Products, Cart, Orders
        }

        /// <returns>Minimal data to save performance when all of the data is not needed.</returns>
        public static async Task<DbInterface> GetMinimalModel(masterEntities db, HttpRequestBase request, IIdentity identity)
        {
            TblArayuz setInterface = await db.TblArayuzs.SingleOrDefaultAsync();
            DbInterface dbInterface = new DbInterface
            {
                CartCount = await GetCartCount(db, request, identity),
                ProductCategories = await db.TblKategorilers.ToListAsync(),
                ContactInfo = await db.TblIletisims.SingleOrDefaultAsync()
            };

            if (setInterface != null)
            {
                dbInterface.BannerTitle = setInterface.AfisBaslik;
                dbInterface.BannerAltTitle = setInterface.AfisAltbaslik;
                dbInterface.FooterText = setInterface.Altbilgi;
                dbInterface.UserAgreement = setInterface.UyelikSozlesmesi;
            }

            return dbInterface;
        }

        /// <returns>Minimal data to save performance when all of the data is not needed.</returns>
        public static async Task<DbInterface> GetMinimalModel(masterEntities db, int uid)
        {
            TblArayuz setInterface = await db.TblArayuzs.SingleOrDefaultAsync();
            DbInterface dbInterface = new DbInterface
            {
                CartCount = await GetCartCount(db, uid),
                ProductCategories = await db.TblKategorilers.ToListAsync(),
                ContactInfo = await db.TblIletisims.SingleOrDefaultAsync(),
            };

            if (setInterface != null)
            {
                dbInterface.BannerTitle = setInterface.AfisBaslik;
                dbInterface.BannerAltTitle = setInterface.AfisAltbaslik;
                dbInterface.FooterText = setInterface.Altbilgi;
                dbInterface.UserAgreement = setInterface.UyelikSozlesmesi;
            }

            return dbInterface;
        }

        public static RedirectResult RedirectToActionWithHash(Controller controller, string actionName, string hash)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            ControllerContext controllerContext = controller.ControllerContext;
            string controllerName = controllerContext.RouteData.Values["controller"]?.ToString();

            if (!string.IsNullOrEmpty(actionName) && !string.IsNullOrEmpty(controllerName))
            {
                UrlHelper urlHelper = new UrlHelper(controllerContext.RequestContext);
                string url = urlHelper.Action(actionName, controllerName) + "#" + hash;
                return new RedirectResult(url);
            }

            return new RedirectResult("~/Home/Index");
        }

        public static bool IsURL(string input)
        {
            return Uri.TryCreate(input, UriKind.Absolute, out Uri uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public static bool IsFilePath(string input) => Path.IsPathRooted(input) && !input.Contains(Uri.SchemeDelimiter);

        public static bool TryConvertToInt(object obj, out int result)
        {
            if (obj == null)
            {
                result = 0;
                return false;
            }

            try
            {
                result = Convert.ToInt32(obj);
                return true;
            }
            catch (Exception)
            {
                result = 0;
                return false;
            }
        }

        /// <summary>
        /// Uppercases the first letter of the given string.
        /// </summary>
        /// <returns> <paramref name="str"/> with its first character uppercased.</returns>
        public static string UppercaseFirst(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return string.Empty;
            }

            return char.ToUpper(str[0], CultureInfo.InvariantCulture) + str.Substring(1);
        }
    }
}