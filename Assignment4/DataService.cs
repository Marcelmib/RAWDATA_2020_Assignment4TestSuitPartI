using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ExtensionMethods;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Assignment4
{
    public class DataService
    {
        public string connectionString { get; set; }

        public NpgsqlConnection conn { get; set; }

        public DataService()
        {
            connectionString = "host=localhost;db=Northwind;uid=postgres;pwd=jqe69wxn";
            conn = new NpgsqlConnection(connectionString);
            // conn.Open();

        }

        public List<Category> GetCategories()
        {
            NpgsqlCommand cmd = OpenSqlConnection();
            cmd.CommandText = "select * from categories";
            var reader = cmd.ExecuteReader();
            var categories = new List<Category>();
            while (reader.Read())
            {
                Category category = new Category(reader.GetInt32(0), reader.GetString(1), reader.GetString(2));
                categories.Add(category);
            }

            return categories;
        }
        public Category GetCategory(int num)
        {
            NpgsqlCommand cmd = OpenSqlConnection();
            var category = new Category();
            try
            {
                //succeds and return a category obj, with the appropriate info.
                cmd.CommandText = $"select * from categories Where categoryid = '{num}'";
                var reader = cmd.ExecuteReader();
                reader.Read();
                category = new Category(reader.GetInt32(0), reader.GetString(1), reader.GetString(2));
                cmd.Connection.Close();
                return category;
            }
            catch
            {
                //fails and return a category as null 
                cmd.Connection.Close();
                return null;
            }

        }

        private NpgsqlCommand OpenSqlConnection()
        {
            var cmd = new NpgsqlCommand();
            cmd.Connection = conn;
            cmd.Connection.Open();
            return cmd;
        }

        public Category CreateCategory(string name, string description)
        {
            NpgsqlCommand cmd = OpenSqlConnection();
            cmd.CommandText = $"select count(*) from categories";
            var reader = cmd.ExecuteReader();
            reader.Read();
            var id = reader.GetInt32(0) + 1;
            cmd.Connection.Close();
            cmd.Connection = conn;
            cmd.Connection.Open();
            cmd.CommandText = $"INSERT INTO categories (categoryid, categoryname, description) VALUES('{id}', '{name}', '{description}'); ";
            cmd.ExecuteReader();

            var createdCategory = new Category(id, name, description);
            cmd.Connection.Close();

            return createdCategory;
        }

        public bool DeleteCategory(int categoryID)
        {
            NpgsqlCommand cmd = OpenSqlConnection();
            try
            {
                cmd.CommandText = $" DELETE FROM categories WHERE categoryid = {categoryID}; ";
                int numberOfRecords = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
                if (numberOfRecords == 0) return false;
                else return true;
            }
            catch
            {
                cmd.Connection.Close();
                return false;
            }
        }
        public bool UpdateCategory(int id, string name, string description)
        {

            NpgsqlCommand cmd = OpenSqlConnection();
            cmd.CommandText = $"UPDATE categories SET categoryname = '{name}', description = '{description}'" +
                              $" WHERE categoryid = '{id}';";

            int numberOfRecords = cmd.ExecuteNonQuery();
            cmd.Connection.Close();
            if (numberOfRecords == 0) return false;
            else return true;
        }
        public Product GetProduct(int id)
        {
            NpgsqlCommand cmd = OpenSqlConnection();
            var newProduct = new Product();
            var category = new Category();

            try
            {
                //succeds and return a category obj, with the appropriate info.
                cmd.CommandText = $"select * from products Where productid = '{id}'";
                var reader = cmd.ExecuteReader();
                reader.Read();

                newProduct = new Product(reader.GetInt32(0),
                                         reader.GetString(1),
                                         reader.GetInt32(2),
                                         reader.GetInt32(3),
                                         reader.GetString(4),
                                         reader.GetInt32(5),
                                         reader.GetInt32(6));

                cmd.Connection.Close();
                newProduct.Category = GetCategory(newProduct.CategoryID);
                return newProduct;
            }
            catch
            {
                //fails and return a category as null 
                cmd.Connection.Close();
                return null;
            }
        }
        public List<Product> GetProductByCategory(int categoryid)
        {
            NpgsqlCommand cmd = OpenSqlConnection();
            cmd.CommandText = $"select * from products WHERE categoryid = {categoryid} ";

            var reader = cmd.ExecuteReader();

            var products = new List<Product>();
            while (reader.Read())
            {
                Product product = new Product(reader.GetInt32(0),
                                         reader.GetString(1),
                                         reader.GetInt32(2),
                                         reader.GetInt32(3),
                                         reader.GetString(4),
                                         reader.GetInt32(5),
                                         reader.GetInt32(6));


                products.Add(product);
            }
            cmd.Connection.Close();

            foreach (Product c in products)
            {
                c.Category = GetCategory(c.CategoryID);
            }
            return products;

        }
        public List<Product> GetProductByName(string nameContains)
        {
            NpgsqlCommand cmd = OpenSqlConnection();
            cmd.CommandText = $"select * from products WHERE productname like '%{nameContains}%' ";

            var reader = cmd.ExecuteReader();

            var products = new List<Product>();
            while (reader.Read())
            {
                Product product = new Product(reader.GetInt32(0),
                                         reader.GetString(1),
                                         reader.GetInt32(2),
                                         reader.GetInt32(3),
                                         reader.GetString(4),
                                         reader.GetInt32(5),
                                         reader.GetInt32(6));


                products.Add(product);
            }
            cmd.Connection.Close();

            foreach (Product c in products)
            {
                c.Category = GetCategory(c.CategoryID);
            }
            return products;

        }

        public Order GetOrder(int id)
        {
            NpgsqlCommand cmd = OpenSqlConnection();
            cmd.CommandText = $"select * from orders Where orderid = '{id}'";
            var reader = cmd.ExecuteReader();
            reader.Read();
            var order = new Order();// reader.GetInt32(0), reader.GetString(1), reader.GetString(2));
            order.Id = reader.GetInt32(0);
            order.CustomerId = reader.GetChar(1);
            order.EmployeId = reader.GetInt32(2);
            order.Date = reader.GetDateTime(3);
            order.Required = reader.GetDateTime(4);
            try { order.ShippedDate = reader.GetDateTime(5); }
            catch { }
            order.Freight = reader.GetInt32(6);
            order.ShipName = reader.GetString(7);
            order.ShipAddress = reader.GetString(8);
            order.ShipCity = reader.GetString(9);
            try { order.ShipPostalCode = reader.GetString(10); }
            catch { order.ShipPostalCode = null; }
            order.ShipCountry = reader.GetString(11);

            cmd.Connection.Close();
            order.OrderDetails = GetOrderDetails(id);
            return order;
        }
        public List<Order> GetOrders()
        {
            NpgsqlCommand cmd = OpenSqlConnection();
            var Orderlist = new List<Order>();
            
            cmd.CommandText = $"select * from orders";
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                //creates a new order. 
                var order = new Order();// reader.GetInt32(0), reader.GetString(1), reader.GetString(2));
                order.Id = reader.GetInt32(0);
                order.CustomerId = reader.GetChar(1);
                order.EmployeId = reader.GetInt32(2);
                order.Date = reader.GetDateTime(3);
                order.Required = reader.GetDateTime(4);
                try{order.ShippedDate = reader.GetDateTime(5);}
                catch{}
                order.Freight = reader.GetInt32(6);
                order.ShipName = reader.GetString(7);
                order.ShipAddress = reader.GetString(8);
                order.ShipCity = reader.GetString(9);
                try { order.ShipPostalCode = reader.GetString(10); }
                catch { order.ShipPostalCode = null; }
                order.ShipCountry = reader.GetString(11);
                    
                //Adds orders to order list
                Orderlist.Add(order);
            }
            cmd.Connection.Close();
            foreach (Order x in Orderlist) 
            { 
            x.OrderDetails = GetOrderDetails(x.Id);

            }
            //succeds and return a orderdetails obj, with the appropriate info.
            return Orderlist;
            
           



        }
        public List<OrderDetails> GetOrderDetails(int id)
        {
            NpgsqlCommand cmd = OpenSqlConnection();
            var orderDetailList = new List<OrderDetails>();
            try
            {
                cmd.CommandText = $"select * from orderdetails Where orderid = '{id}'";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var orderDetails = new OrderDetails();
                    orderDetails.OrderId = reader.GetInt32(0);
                    orderDetails.ProductId = reader.GetInt32(1);
                    orderDetails.UnitPrice = reader.GetInt32(2);
                    orderDetails.Quantity = reader.GetInt32(3);
                    orderDetails.Discount = reader.GetInt32(4);


                    orderDetailList.Add(orderDetails);
                }
                cmd.Connection.Close();
                foreach(OrderDetails c in orderDetailList)
                {
                    c.Product = GetProduct(c.ProductId);
                    
                }


                //succeds and return a orderdetails obj, with the appropriate info.
                return orderDetailList;
            }
            catch
            {
                //fails and return a orderdetails as null 
                cmd.Connection.Close();
                return null;
            }

        }
        public List<OrderDetails> GetOrderDetailsByOrderId(int id)
        {
            NpgsqlCommand cmd = OpenSqlConnection();
            var orderDetailList = new List<OrderDetails>();
            try
            {
                cmd.CommandText = $"select * from orderdetails Where orderid = '{id}'";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var orderDetails = new OrderDetails();
                    orderDetails.OrderId = reader.GetInt32(0);
                    orderDetails.ProductId = reader.GetInt32(1);
                    orderDetails.UnitPrice = reader.GetInt32(2);
                    orderDetails.Quantity = reader.GetInt32(3);
                    orderDetails.Discount = reader.GetInt32(4);


                    orderDetailList.Add(orderDetails);
                }
                cmd.Connection.Close();
                foreach (OrderDetails c in orderDetailList)
                {
                    c.Product = GetProduct(c.ProductId);

                }


                //succeds and return a orderdetails obj, with the appropriate info.
                return orderDetailList;
            }
            catch
            {
                //fails and return a orderdetails as null 
                cmd.Connection.Close();
                return null;
            }

        }
        public List<OrderDetails> GetOrderDetailsByProductId(int id)
        {
            NpgsqlCommand cmd = OpenSqlConnection();
            var orderDetailList = new List<OrderDetails>();
            
            //Since the testsheet searched for a wierd date as the first entry, I had to look how he got that date. and it looks
            //like he order by unitprice and then orderid. 
            cmd.CommandText = $"SELECT * FROM orderdetails where productid = {id} order by unitprice desc, orderid asc;";
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var orderDetails = new OrderDetails();
                orderDetails.OrderId = reader.GetInt32(0);
                orderDetails.ProductId = reader.GetInt32(1);
                orderDetails.UnitPrice = reader.GetInt32(2);
                orderDetails.Quantity = reader.GetInt32(3);
                orderDetails.Discount = reader.GetInt32(4);


                orderDetailList.Add(orderDetails);
            }
            cmd.Connection.Close();
            foreach (OrderDetails c in orderDetailList)
            {
                c.Product = GetProduct(c.ProductId);
                c.Order = GetOrder(c.OrderId);
            }


            //succeds and return a orderdetails obj, with the appropriate info.
            return orderDetailList;
            

        }


    }
    public class Category
    {

        public int Id { get; set; } // property
        public string Name { get; set; } // property
        public string Description { get; set; }
        public Category()
        {
            Id = 0;
        }
        public Category(Int32 SqlId, String SqlName)
        {
            Id = SqlId;
            Name = SqlName;
        }
        public Category(Int32 SqlId, String SqlName, string description)
        {
            Id = SqlId;
            Name = SqlName;
            Description = description;
        }

    }

    public class Product
    {

        public int Id { get; set; } // property
        public string Name { get; set; } // property
        public int SupplierId { get; set; }
        public Category Category { get; set; }
        public int CategoryID { get; set; }
        public string QuantityPerUnit { get; set; }
        public int UnitPrice { get; set; }
        public int UnitsInStock { get; set; }

        public Product()
        {
            Id = 0;
            UnitPrice = 0;
            UnitsInStock = 0;
        }
        public Product(int id, string name, int supplierId, int categoryid, string quantityPerUnit, int unitPrice, int unitsInStock)
        {
            Id = id;
            Name = name;
            SupplierId = supplierId;
            QuantityPerUnit = quantityPerUnit;
            UnitPrice = unitPrice;
            UnitsInStock = unitsInStock;
            CategoryID = categoryid;

        }

    }
    public class Order
    {
        public int Id { get; set; } // property
        public int CustomerId { get; set; } // property
        public int EmployeId { get; set; } // property
        public DateTime Date { get; set; } // property
        public DateTime Required { get; set; } // property
        public DateTime ShippedDate { get; set; } // property
        public int Freight { get; set; } // property
        public string ShipName { get; set; } // property
        public string ShipAddress { get; set; } // property
        public string ShipCity { get; set; } // property
        public string ShipPostalCode { get; set; } // property
        public string ShipCountry { get; set; } // property
        public List<OrderDetails> OrderDetails { get; set; } // property

        public Order()
        {
            Id = 0;
            Date = new DateTime();
            Required = new DateTime();
        }
    }
    public class OrderDetails
    {
        public int OrderId { get; set; } // property
        public int ProductId { get; set; } // property
        public int UnitPrice { get; set; } // property
        public int Quantity { get; set; } // property
        public int Discount { get; set; } // property
        public Product Product { get; set; }
        public Order Order { get; set; }
    }
}

// helper methods

namespace ExtensionMethods
{
    public static class MyExtensions
    {
        public static bool IsString(this String str)
        {
            string teststring; 
            try
            {
                teststring = str; 
                return true;
            }
            catch
            {
                return false;
            }

        }
    }
}
