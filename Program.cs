using System;
using NLog.Web;
using System.IO;
using System.Linq;
using NorthwindConsole2.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace NorthwindConsole2
{
    class Program
    {
        // create static instance of Logger
        private static NLog.Logger logger = NLogBuilder.ConfigureNLog(Directory.GetCurrentDirectory() + "\\nlog.config").GetCurrentClassLogger();
        static void Main(string[] args)
        {
            logger.Info("Program started");

               try
            {
                string choice;
                do
                {
                    //Products
                    Console.WriteLine("1) Add New Records to Products Table");
                    Console.WriteLine("2) Edit Record from Products Table");
                    Console.WriteLine("3) Display all records in the Products table");
                    Console.WriteLine("4) Display a specific Product");
                    //Category
                    Console.WriteLine("5) Add New Records to Categories Table");
                    Console.WriteLine("6) Edit Record from Categories Table");
                    Console.WriteLine("7) Display all categories in the Categories table");
                    Console.WriteLine("8) Display all Categories and their related active product data");
                    Console.WriteLine("9) Display a specific Category");
                    Console.WriteLine("10) Delete a Category");
                    Console.WriteLine("11) Delete a Product");
                    Console.WriteLine("\"q\" to quit");
                    choice = Console.ReadLine();
                    Console.Clear();
                    logger.Info($"Option {choice} selected");
                     
                    if (choice == "1")
                    {
                        //add new records to products table
                        Products product = new Products();
                        Console.WriteLine("Enter Product Name:");
                        product.ProductName = Console.ReadLine();
                        Console.WriteLine("Enter SupplierID:");
                        product.SupplierId = Convert.ToInt16(Console.ReadLine());
                        Console.WriteLine("Enter CategoryID:");
                        product.CategoryId = Convert.ToInt16(Console.ReadLine());
                        Console.WriteLine("Enter Quantity Per Unit:");
                        product.QuantityPerUnit = Console.ReadLine();
                        Console.WriteLine("Enter Unit Price:");
                        product.UnitPrice = Convert.ToDecimal(Console.ReadLine());
                        Console.WriteLine("Enter Units in Stock:");
                        product.UnitsInStock = Convert.ToInt16(Console.ReadLine());
                        Console.WriteLine("Enter Units on Order:");
                        product.UnitsOnOrder = Convert.ToInt16(Console.ReadLine());
                        Console.WriteLine("Enter Reorder Level:");
                        product.ReorderLevel = Convert.ToInt16(Console.ReadLine());
                        product.Discontinued = false;

                        ValidationContext context = new ValidationContext(product, null, null);
                        List<ValidationResult> results = new List<ValidationResult>();

                        var isValid = Validator.TryValidateObject(product, context, results, true);
                        if (isValid)
                        {
                            var db = new NWConsole_96_MLBContext();
                            //check for unique name
                            if (db.Products.Any(p => p.ProductName == product.ProductName))
                            {
                                //generate validation error
                                isValid = false;
                                results.Add(new ValidationResult("Name exists", new string[] { "Product Name" }));
                            }
                            else
                            {
                                logger.Info("Product Name Validation Passed");
                                db.AddProduct(product);
                                logger.Info("Product added - {name}", product.ProductName);
                            }
                        }
                        if (!isValid)
                        {
                            foreach (var result in results)
                            {
                                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                            }
                        }
                    }
                    else if (choice == "2")
                    {
                        //edit records from products table

                        Console.WriteLine("Choose the product to edit:");
                        var db = new NWConsole_96_MLBContext();
                        var product = GetProduct(db);
                        if (product != null)
                        {
                            // input product
                            Products UpdatedProduct = InputProduct(db);
                            if (UpdatedProduct != null)
                            {
                                UpdatedProduct.ProductId = product.ProductId;
                                db.EditProduct(UpdatedProduct);
                                logger.Info($"Product (id: {product.ProductId}) updated");
                            }
                        }
                    }

                    else if (choice == "3")
                    {
                        //Display all records in the Products table (ProductName only) - user decides if they want to see 
                        //all products, discontinued products, or active (not discontinued) products. Discontinued products 
                        //should be distinguished from active products.
                        Console.WriteLine("Would you like to display:\n1) Display All Products\n2) Display Discontinued Products\n3) Display Active Products\n*enter any other key to go back to the menu*");
                        string userResponse = Console.ReadLine();
                        var db = new NWConsole_96_MLBContext();

                        if (userResponse == "1")
                        {
                            logger.Info("Displaying all products");
                            //display all products
                            var products = db.Products.OrderBy(p => p.ProductName);
                            foreach (Products p in products)
                            {
                                Console.WriteLine($"{p.ProductName}");
                            }
                        }
                        else if (userResponse == "2")
                        {
                            logger.Info("Displaying discontinued products");
                            var discontinuedProducts = db.Products.Where(p => p.Discontinued == true).OrderBy(p => p.ProductId);
                            foreach (Products p in discontinuedProducts)
                            {
                                Console.WriteLine($"{p.ProductName}");
                            }
                        }
                        else if (userResponse == "3")
                        {
                            logger.Info("Displaying active products");
                            var activeProducts = db.Products.Where(p => p.Discontinued == false).OrderBy(p => p.ProductName);
                            foreach (Products p in activeProducts)
                            {
                                Console.WriteLine($"{p.ProductName}");
                            }
                        }
                    }
                    else if (choice == "4")
                    {
                        //Display a specific Product (all product fields should be displayed)
                        Console.WriteLine("Choose the product to display:");
                        var db = new NWConsole_96_MLBContext();
                        //display all products
                        var products = db.Products.OrderBy(p => p.ProductId);
                        foreach (Products p in products)
                        {
                            Console.WriteLine($"{p.ProductId}: {p.ProductName}");
                        }
                        int product = Convert.ToInt32(Console.ReadLine());
                        // display product
                        var specificProduct = db.Products.Where(p => p.ProductId == product);
                        logger.Info($"Displaying Product ID: {product}");
                        foreach (Products p in specificProduct)
                        {
                            Console.WriteLine($"Product ID: {p.ProductId}");
                            Console.WriteLine($"Product Name: {p.ProductName}");
                            Console.WriteLine($"Supplier ID: {p.SupplierId}");
                            Console.WriteLine($"Category ID: {p.CategoryId}");
                            Console.WriteLine($"Quantity Per Unit: {p.QuantityPerUnit}");
                            Console.WriteLine($"Unit Price: {p.UnitPrice}");
                            Console.WriteLine($"Units In Stock: {p.UnitsInStock}");
                            Console.WriteLine($"Units On Order: {p.UnitsOnOrder}");
                            Console.WriteLine($"Reorder Level: {p.ReorderLevel}");
                            Console.WriteLine($"Discontinued: {p.Discontinued}");
                        }
                    }
                    else if (choice == "5")
                    {
                        //Add new records to the Categories table
                        Categories category = new Categories();
                        Console.WriteLine("Enter Category Name: ");
                        category.CategoryName = Console.ReadLine();
                        Console.WriteLine("Enter Category Description: ");
                        category.Description = Console.ReadLine();

                        ValidationContext context = new ValidationContext(category, null, null);
                        List<ValidationResult> results = new List<ValidationResult>();

                        var isValid = Validator.TryValidateObject(category, context, results, true);
                        if (isValid)
                        {
                            var db = new NWConsole_96_MLBContext();
                            //check for unique name
                            if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
                            {
                                //generate validation error
                                isValid = false;
                                results.Add(new ValidationResult("Name Exists", new string[] { "Category Name" }));
                            }
                            else
                            {
                                logger.Info("Category Name Validation Passed");
                                db.AddCategory(category);
                                logger.Info("Category Added = {name}", category.CategoryName);
                            }
                        }
                        if (!isValid)
                        {
                            foreach (var result in results)
                            {
                                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                            }
                        }
                    }
                    else if (choice == "6")
                    {
                        //Edit a specified record from the Categories table
                        Console.WriteLine("Choose the category to edit: ");
                        var db = new NWConsole_96_MLBContext();
                        var category = GetCategory(db);
                        if (category != null)
                        {
                            //input category
                            Categories UpdatedCategory = InputCategory(db);
                            if (UpdatedCategory != null)
                            {
                                UpdatedCategory.CategoryId = category.CategoryId;
                                db.EditCategory(UpdatedCategory);
                                logger.Info($"Category id: {category.CategoryId} updated");
                            }
                        }
                    }
                    else if (choice == "7")
                    {
                        //Display all Categories in the Categories table (CategoryName and Description)
                        var db = new NWConsole_96_MLBContext();
                        var categories = db.Categories.OrderBy(c => c.CategoryId);
                        logger.Info("Displaying all categories");
                        foreach (Categories c in categories)
                        {
                            Console.WriteLine($"{c.CategoryName} - {c.Description}");
                        }
                    }
                    else if (choice == "8")
                    {
                        //Display all Categories and their related active (not discontinued) product data (CategoryName, ProductName)
                        var db = new NWConsole_96_MLBContext();
                        var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
                        logger.Info("Displaying all categories and their active products");
                        foreach (var category in query)
                        {
                            Console.WriteLine($"{category.CategoryName}");
                            foreach (Products p in category.Products)
                            {
                                if (p.Discontinued == false)
                                {
                                    Console.WriteLine($"\t{p.ProductName}");
                                }
                            }
                        }
                    }

                    else if (choice == "9")
                    {
                        //Display a specific Category and its related active product data (CategoryName, ProductName)
                        Console.WriteLine("Choose the category to display: ");
                        var db = new NWConsole_96_MLBContext();
                        //display all categories
                        var categories = db.Categories.OrderBy(c => c.CategoryId);
                        foreach(Categories c in categories)
                        {
                            Console.WriteLine($"{c.CategoryId}: {c.CategoryName}");
                        }
                        int category = Convert.ToInt32(Console.ReadLine());
                        //display category and products
                        var specificCategory = db.Categories.Include("Products").Where(c => c.CategoryId == category);
                        logger.Info($"Displaying category id: {category} and it's active products");
                        foreach(var item in specificCategory)
                        {
                            Console.WriteLine($"{item.CategoryName}");
                            foreach(Products p in item.Products)
                            {
                                if(p.Discontinued == false)
                                {
                                    Console.WriteLine($"\t{p.ProductName}");
                                }
                            }
                        }
                    } 
                   
                    else if (choice == "10")
                    {
                        var db = new NWConsole_96_MLBContext();
                        var query = db.Categories.OrderBy(p => p.CategoryId);

                        Console.WriteLine("Select the category ID you want to delete:");
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
                        }
                        int id = int.Parse(Console.ReadLine());
                        Console.Clear();
                        logger.Info($"CategoryId {id} selected");
                        Categories categories = db.Categories.FirstOrDefault(c => c.CategoryId == id);

                        db.Categories.Remove(categories);
                        db.SaveChanges();
                        logger.Info($"Category Id {id} deleted");
                    }
                    else if(choice == "11")
                    {
                        var db = new NWConsole_96_MLBContext();
                        var query = db.Products.OrderBy(p => p.ProductId);

                        Console.WriteLine("Select the product ID you want to delete:");
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.ProductId}) {item.ProductName}");
                        }
                        int id = int.Parse(Console.ReadLine());
                        Console.Clear();
                        logger.Info($"CategoryId {id} selected");
                        Products products = db.Products.FirstOrDefault(p => p.ProductId == id);
                        db.Products.Remove(products);
                        db.SaveChanges();
                        logger.Info($"Product Id {id} deleted");
                    }
                    
                    Console.WriteLine();
                   

                } while (choice.ToLower() != "q");
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }


            logger.Info("Program ended");
        }
        public static Products GetProduct(NWConsole_96_MLBContext db)
        {
            //display all products
            var products = db.Products.OrderBy(p => p.ProductName);
            foreach (Products p in products)
            {
                Console.WriteLine($"{p.ProductId}: {p.ProductName}");
            }
            if (int.TryParse(Console.ReadLine(), out int ProductId))
            {
                Products product = db.Products.FirstOrDefault(p => p.ProductId == ProductId);
                if (product != null)
                {
                    return product;
                }
            }
            logger.Error("Invalid Product ID");
            return null;
        }

        public static Products InputProduct(NWConsole_96_MLBContext db)
        {
            Products product = new Products();
            Console.WriteLine("Enter Product Name:");
            product.ProductName = Console.ReadLine();
            Console.WriteLine("Enter Quantity Per Unit:");
            product.QuantityPerUnit = Console.ReadLine();
            Console.WriteLine("Enter Unit Price:");
            product.UnitPrice = Convert.ToDecimal(Console.ReadLine());
            Console.WriteLine("Enter Units in Stock:");
            product.UnitsInStock = Convert.ToInt16(Console.ReadLine());
            Console.WriteLine("Enter Units on Order:");
            product.UnitsOnOrder = Convert.ToInt16(Console.ReadLine());
            Console.WriteLine("Enter Reorder Level:");
            product.ReorderLevel = Convert.ToInt16(Console.ReadLine());
            Console.WriteLine($"Enter true if product is Discontinued or false if it is Active");
            product.Discontinued = Convert.ToBoolean(Console.ReadLine());

            ValidationContext context = new ValidationContext(product, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(product, context, results, true);
            if (isValid)
            {
                var db1 = new NWConsole_96_MLBContext();
                //check for unique name
                if (db1.Products.Any(p => p.ProductName == product.ProductName))
                {
                    //generate validation error
                    isValid = false;
                    results.Add(new ValidationResult("Name exists", new string[] { "Product Name" }));
                }
                else
                {
                    logger.Info("Product Name Validation Passed");
                }
            }
            if (!isValid)
            {
                foreach (var result in results)
                {
                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                }
                return null;
            }

            return product;
        }
          public static Categories GetCategory(NWConsole_96_MLBContext db)
        {
            //display all categories
            var categories = db.Categories.OrderBy(c => c.CategoryName);
            foreach (Categories c in categories)
            {
                Console.WriteLine($"{c.CategoryId}: {c.CategoryName}");
            }
            if (int.TryParse(Console.ReadLine(), out int CategoryId))
            {
                Categories category = db.Categories.FirstOrDefault(c => c.CategoryId == CategoryId);
                if (category != null)
                {
                    return category;
                }
            }
            logger.Error("Invalid Category ID");
            return null;
        }

        public static Categories InputCategory(NWConsole_96_MLBContext db)
        {
            Categories category = new Categories();
            Console.WriteLine("What are you editing?\nEnter n for name & description\nEnter d for just description)");
            string choice = Console.ReadLine();
            if (choice == "n")
            {
                Console.WriteLine("Enter Category Name: ");
                category.CategoryName = Console.ReadLine();
                Console.WriteLine("Enter Category Description: ");
                category.Description = Console.ReadLine();

                ValidationContext context = new ValidationContext(category, null, null);
                List<ValidationResult> results = new List<ValidationResult>();

                var isValid = Validator.TryValidateObject(category, context, results, true);
                if (isValid)
                {
                    var db1 = new NWConsole_96_MLBContext();
                    //check for unique name
                    if (db1.Categories.Any(c => c.CategoryName == category.CategoryName))
                    {
                        //generate validation error
                        isValid = false;
                        results.Add(new ValidationResult("Name exists", new string[] { "Category Name" }));
                    }
                    else
                    {
                        logger.Info("Category Name Validation Passed");
                    }
                }
                if (!isValid)
                {
                    foreach (var result in results)
                    {
                        logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                    }
                    return null;
                }

                return category;
            }
            else
            {
                Console.WriteLine("Enter Category Name that you are editing: ");
                category.CategoryName = Console.ReadLine();
                Console.WriteLine("Enter Category Description: ");
                category.Description = Console.ReadLine();
                return category;
            }
        }
         
    }
}