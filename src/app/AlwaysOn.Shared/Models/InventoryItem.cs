using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AlwaysOn.Shared.Models
{
    public class InventoryItem
    {
        public Guid? Id { get; set; }

        [JsonPropertyName("Product_ID")]
        public string ProductId { get; set; }

        [JsonPropertyName("Product_Category_ID")]
        public string CategoryId { get; set; }

        [JsonPropertyName("Product_Category_Name")]
        public string CategoryName { get; set; }

        [JsonPropertyName("Product_Category_Desc")]
        public string CategoryDesc { get; set; }

        [JsonPropertyName("ProductCategoryRule")]
        public string CategoryRule { get; set; }

        [JsonPropertyName("Product_Name")]
        public string Name { get; set; }

        [JsonPropertyName("Product_Desc")]
        public string Desc { get; set; }

        [JsonPropertyName("Product_Details")]
        public string Details { get; set; }

        [JsonPropertyName("Product_Price")]
        public decimal Price { get; set; }

        [JsonPropertyName("Product_Qty_Beginning")]
        public int QtyBeginning { get; set; }

        [JsonPropertyName("Product_Qty_On_Hand")]
        public int QtyOnHand { get; set; }

        [JsonPropertyName("Product_Qty_Reorder_Threshold")]
        public int QtyReorderThreshold { get; set; }

        [JsonPropertyName("Product_Status")]
        public string Status { get; set; }

        [JsonPropertyName("Product_Sold_Rule")]
        public string SoldRule { get; set; }

        [JsonPropertyName("Supplier_ID")]
        public string SupplierId { get; set; }

        [JsonPropertyName("Product_Tags")]
        public List<ProductTag> Tags { get; set; }

        /// <summary>
        /// Time to live in Cosmos DB. In Seconds
        /// </summary>
        [JsonPropertyName("ttl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int? TimeToLive { get; set; }
    }

    public class ProductTag
    {
        [JsonPropertyName("Product_Tag_ID")]
        public string TagId { get; set; }

        [JsonPropertyName("Product_Tag_Name")]
        public string TagName { get; set; }
    }
}
