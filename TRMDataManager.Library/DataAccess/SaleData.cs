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

            using(SqlDataAccess sql = new SqlDataAccess())
            {
                try
                {
                    sql.StartTransaction("TRMData");

                    // Save the sale model
                    sql.SaveDataInTransaction<SaleDBModel>("dbo.spSale_Insert", saleDB);

                    // Get the ID from the detail model
                    saleDB.Id = sql.LoadDataInTransaction<int, dynamic>("dbo.spSale_Lookup", new { saleDB.CashierId, saleDB.SaleDate }).FirstOrDefault();

                    // Finish filling in the sale detai models
                    foreach (var item in details)
                    {
                        item.SaleId = saleDB.Id;
                        // Save the sale detail models
                        sql.SaveDataInTransaction("dbo.spSaleDetail_Insert", item);
                    }

                    sql.CommitTransation();
                }
                catch
                {
                    sql.RollbackTransaction();
                    throw;
                }
            }
        }

        public List<SaleReportModel> GetSaleReports()
        {
            SqlDataAccess sql = new SqlDataAccess();

            var output = sql.LoadData<SaleReportModel, dynamic>("dbo.spSale_SaleReport", new { }, "TRMData");

            return output;
        }
    }
}
