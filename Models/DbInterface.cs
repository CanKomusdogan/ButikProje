using System;
using System.Collections.Generic;
using System.IO;
using ButikProje.Commons;

namespace ButikProje.Models
{
    public class DbInterface
    {
        public string BannerTitle { get; set; }
        public string BannerAltTitle { get; set; }
        public string FooterText { get; set; }
        public string UserAgreement { get; set; }

        public List<DbProduct> Products { get; set; }
        public List<TblSepet> ProductsInCart { get; set; }
        public List<TblSiparisler> Orders { get; set; }
        public int CartCount { get; set; }
        public List<TblKategoriler> ProductCategories { get; set; }
        public List<TblBedenler> ProductSizes { get; set; }
        public TblIletisim ContactInfo { get; set; }
        public DbInterface()
        {
            Products = new List<DbProduct>();
            ProductsInCart = new List<TblSepet>();
            Orders = new List<TblSiparisler>();
            ProductCategories = new List<TblKategoriler>();
        }
    }


    public class DbProduct
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public double? SalePercentage { get; set; }
        public bool OnSale { get; set; }
        public string Details { get; set; }
        public List<DbProductPhoto> Photos { get; set; }
        public string Category { get; set; }
        public List<TblBedenler> Sizes { get; set; }

        public DbProduct()
        {
            Photos = new List<DbProductPhoto>();
            Sizes = new List<TblBedenler>();
        }
    }

    public partial class DbProductPhoto
    {
        public int ID { get; set; }
        public int ConnectedProductID { get; set; }
        public string PhotoContent { get; set; }

        /// <summary>
        /// Converts the <code>PhotoContent</code> property value to base64.
        /// </summary>
        /// <returns>The base64 string of PhotoContent. <br /> Will return an empty string if PhotoContent is a URL.</returns>
        public string ToBase64()
        {
            if (!DbCommons.IsURL(PhotoContent))
            {
                byte[] imageArray = File.ReadAllBytes(PhotoContent); // Read the image as byte array
                string base64Image = Convert.ToBase64String(imageArray);
                return base64Image;
            }
            else return string.Empty;
        }
    }
}