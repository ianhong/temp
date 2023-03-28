using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AlwaysOn.Shared.Models
{
    public class OrderItem
    {
        public Guid? Id { get; set; }

        [JsonPropertyName("Product_Category_ID")]
        public string CustomerId { get; set; }

        [JsonPropertyName("Customer_Data_Type")]
        public string CustomerDataType { get; set; }

        [JsonPropertyName("Customer_Order_Date")]
        public string OrderDate { get; set; }

        [JsonPropertyName("Customer_Order_Total")]
        public string OrderTotal { get; set; }

        [JsonPropertyName("Customer_Order_Details")]
        public List<OrderDetails> OrderDetails { get; set; } = new List<OrderDetails>();

        [JsonPropertyName("Customer_Order_Count")]
        public string OrderCount { get; set; }

        [JsonPropertyName("Customer_Order_Payments")]
        public List<OrderPayments> OrderPayments { get; set; } = new List<OrderPayments>();

        [JsonPropertyName("Customer_Payments")]
        public List<Payments> Payments { get; set; } = new List<Payments>();

        [JsonPropertyName("Customer_Addresses")]
        public List<CustomerAddress> CustomerAddresses { get; set; } = new List<CustomerAddress>();
    }

    public class OrderPayments
    {
        [JsonPropertyName("Customer_Payment_ID")]
        public string Id { get; set; }

        [JsonPropertyName("Customer_Payment_Amt")]
        public decimal Amountt { get; set; }
    }

    public class Payments
    {
        [JsonPropertyName("Customer_Payment_ID")]
        public string Id { get; set; }

        [JsonPropertyName("Customer_Payment_Source")]
        public string Source { get; set; }

        [JsonPropertyName("Customer_Payment_Carrier")]
        public string Carrier { get; set; }

        [JsonPropertyName("Customer_Payment_Is_Default")]
        public bool IsDefault { get; set; }

        [JsonPropertyName("Customer_Payment_Number")]
        public string PaymentNumber { get; set; }

        [JsonPropertyName("Customer_Payment_Exp_Date")]
        public decimal PaymentExpDate { get; set; }

        [JsonPropertyName("Customer_Payment_CVV_Code")]
        public int CvvCode { get; set; }
    }

    public class OrderDetails
    {
        [JsonPropertyName("Product_ID")]
        public string Id { get; set; }

        [JsonPropertyName("Product_Name")]
        public string Name { get; set; }

        [JsonPropertyName("Product_Price")]
        public decimal Price { get; set; }

        [JsonPropertyName("Product_Qty_Ordered")]
        public int QtyOrdered { get; set; }

        [JsonPropertyName("Product_Discount")]
        public decimal Discount { get; set; }

        [JsonPropertyName("Product_Tax")]
        public decimal Tax { get; set; }

        [JsonPropertyName("Total_Cost")]
        public decimal Cost { get; set; }
    }

    public class CustomerAddress
    {
        [JsonPropertyName("Customer_Address_Type")]
        public string AddressType { get; set; }

        [JsonPropertyName("Customer_Address_Line1")]
        public string AddressLine1 { get; set; }

        [JsonPropertyName("Customer_Address_Line2")]
        public string AddressLine2 { get; set; }

        [JsonPropertyName("Customer_Address_Unit")]
        public string AddressUnit { get; set; }

        [JsonPropertyName("Customer_City")]
        public string City { get; set; }

        [JsonPropertyName("Customer_State")]
        public string State { get; set; }

        [JsonPropertyName("Customer_Zip")]
        public string Zip { get; set; }

        [JsonPropertyName("Customer_Zip_Extn")]
        public string ZipExtn { get; set; }
    }
}
