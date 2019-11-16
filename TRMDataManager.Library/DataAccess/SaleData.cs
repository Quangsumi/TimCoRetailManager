using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRMDataManager.Library.Internal.DataAccess;
using TRMDataManager.Library.Models;

namespace TRMDataManager.Library.DataAccess
{
    public class SaleData
    {
        public void SaveSale(SaleModel sale, string userId)
        {
            List<SaleDetailDBModel> details = new List<SaleDetailDBModel>();
            ProductData products = new ProductData();
            var taxRate = ConfigHelper.GetTaxRate()/100;

            foreach (SaleDetailModel item in sale.SaleDetails)
            {
                SaleDetailDBModel detail = new SaleDetailDBModel() { ProductId = item.ProductId, Quantity = item.Quantity };

                // Get the information about this product
                ProductModel productInfo = products.GetProductById(detail.ProductId);

                if (productInfo == null)
                    throw new ArgumentNullException($"The ProductId of {detail.ProductId} could not be found on the database");

                detail.PurchasePrice = (productInfo.RetailPrice * detail.Quantity);

                if (productInfo.IsTaxable)
                {
                    detail.Tax = (detail.PurchasePrice * taxRate);
                }

                details.Add(detail);
            }

            // Create the Sale model
            SaleDBModel saleDB = new SaleDBModel()
            {
                SubTotal = details.Sum(x => x.PurchasePrice),
                Tax = details.Sum(x => x.Tax),
                CashierId = userId
            };

            saleDB.Total = saleDB.SubTotal + saleDB.Tax;

            // Save the sale model
            SqlDataAccess sql = new SqlDataAccess();
            sql.SaveData<SaleDBModel>("dbo.spSale_Insert", saleDB, "TRMData");

            // Get the ID from the detail model
            saleDB.Id = sql.LoadData<int, dynamic>("dbo.spSale_Lookup", new { saleDB.CashierId, saleDB.SaleDate }, "TRMData").FirstOrDefault();

            // Finish filling in the sale detai models
            foreach (var item in details)
            {
                item.SaleId = saleDB.Id;
                // Save the sale detail models
                sql.SaveData("dbo.spSaleDetail_Insert", item, "TRMData");
            }
        }
    }
}
