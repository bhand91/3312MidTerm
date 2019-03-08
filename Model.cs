using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Hand.MidTerm
{

 public class AppDbContext : DbContext
 {
     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
     {
         optionsBuilder.UseSqlite("Data Source=database.db");
     }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CartProduct>()
            .HasKey(c => new {c.CartId, c.ProductId});
    }
    public DbSet<Customer> Customers {get; set;}
    public DbSet<Product> Products {get; set;}
    public DbSet<Cart> Carts {get; set;}

    public DbSet<CartProduct> CartProducts {get; set;}
 }

 public class Customer
 {
     public int CustomerId {get; set;} //PK

     public string FirstName {get; set;}

     public string LastName {get; set;}

     public string Email {get; set;}

     //public Cart Cart {get; set;} //Navigation Property. Each customer has one cart.

     public int CartId {get; set;} //FK
 }

 public class Product
 {
     public int ProductId {get; set;}

     public string Description {get; set;}

     public decimal Price {get; set;}

     public List<CartProduct> CartProducts {get; set;}

     public override string ToString()
       {
           return $"{ProductId} {Description} {Price}";
       }
 }

 public class Cart
 {
     public int CartId {get; set;} //PK

     public int CustomerId {get; set;} //FK

     public decimal Total {get; set;}

     public Customer Customer {get; set;} //Navigation Property. Each cart belongs to one customer

     public List<CartProduct> CartProducts {get; set;} //Navigation Property. Each cart can have many CartProducts

 }

 public class CartProduct
 {
     public int CartId {get; set;} //PK1/FK1

     public int ProductId {get; set;} //PK2/FK2

     public Cart Cart {get; set;}

     public Product Products {get; set;}

     public int Quantity {get; set;}

     public decimal ProductTotal {get; set;}

     public override string ToString()
       {
           return $"{ProductId} {Cart.CustomerId} {Products.Description} ${Products.Price}";
       }


 }

}
