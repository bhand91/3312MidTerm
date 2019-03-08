using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Hand.MidTerm
{
    class Program
    {

        public static int userId;
        public static int cartId;
        public static bool logedIn;

        //login allows users to login in and access program. This will check to see if users exists. If they do not, it will direct them to create an account.
        public static void Login()
        {
            bool endLoop = false;
            while(endLoop == false) //set to enable exit of method
            {
                using (var db = new AppDbContext())
                {
                    try
                    {
                        string userEmail;
                        Console.WriteLine("Useremail: ");
                        userEmail = Console.ReadLine();
                        //Searches for user email
                        var findUser = db.Customers.Where( c => c.Email == userEmail).First();
                        var selId = db.Customers.Where(c => c.Email == userEmail).Select(c => c.CustomerId).First();
                        userId = selId; 
                        var findCart = db.Carts.Where(cart => cart.CustomerId == userId).Select(cart => cart.CartId).First();
                        cartId = findCart;   
                                  
                        
                        logedIn = true;
                        endLoop = true;
                    }
                    catch
                    {
                        Console.WriteLine();
                        Console.WriteLine("Useremail not found.");
                        Console.WriteLine();
                        Console.WriteLine("Create account? (yes/no)");
                        string create = Console.ReadLine();
                        logedIn = false;
                        Console.WriteLine();

                        if(create == "yes") //creates account
                        {
                            while(logedIn == false)
                            {
                            Console.WriteLine("------Create New User------ ");
                            Console.WriteLine();
                        
                            Console.WriteLine("Enter new useremail: ");
                            string newUser = Console.ReadLine();
                            Console.WriteLine();

                            Console.WriteLine("Enter user first name: ");
                            string firstName = Console.ReadLine();
                            Console.WriteLine();
                            
                            Console.WriteLine("Enter user last name: ");
                            string lastName = Console.ReadLine();
                            Console.WriteLine();

                            Customer newby = new Customer {Email = newUser, FirstName = firstName, LastName = lastName};
                            
                            
                            Cart createCart = new Cart {Total = 0M, CustomerId = userId};
        
                            
                            db.Add(createCart);                       
                            db.Add(newby);
                            db.SaveChanges();
                            userId = newby.CustomerId;
                            cartId = createCart.CartId;
                            //Console.WriteLine($"new user - {userId} CartID - {cartId}");
                            logedIn = true;
                            endLoop = true;
                        
                            Console.WriteLine();

                        }
                        Console.WriteLine();
                        Console.WriteLine("Thank you, you're now logged in.");
                        } else
                        {
                            endLoop = true;
                        }
                    }
                }
                Console.WriteLine();
                  
            }

        }
        static void List() //List all available products
        {
            using (var db = new AppDbContext())
            {

                foreach (var products in db.Products)
                {
                    Console.WriteLine(products);
                }
                Console.WriteLine();
            }
        }

        //Adds products to cart
        static void Add()
        {
            if(logedIn == true)
            {
                using (var db = new AppDbContext())
                {
                    try
                    {
                        Console.WriteLine("Enter Product to Add");
                        int addProduct = Convert.ToInt32(Console.ReadLine());

                        //checks to see if product exists
                        var productExists = db.Products.Where(p => p.ProductId == addProduct).FirstOrDefault();
                        
                        if(productExists is null)
                        {
                            Console.WriteLine("This product does not exists.");
                        } 
                        else
                        {
                            try{                 
                            //finds product price
                                var needPrice = db.Products.Where(x => x.ProductId == addProduct).Select(x => x.Price).First();

                                //Check if product is in cart
                                var productInCart = db.CartProducts.Where(p => p.ProductId == addProduct && p.CartId == cartId).FirstOrDefault();

                                if(productInCart is null) //if product doesn't exist, create new product in CartProducts.
                                {
                                    CartProduct newCart = new CartProduct{Quantity = 1, ProductId = addProduct, CartId = cartId, ProductTotal = needPrice};
                                    db.Add(newCart); 
                                    db.SaveChanges();

                                } 
                                else //If product does exist, update quantity in CartProducts
                                {
                                    CartProduct update = db.CartProducts.Find(cartId, addProduct);
                                    //int newQuant = update.Quantity ++;
                                    update.Quantity ++;

                                    update.ProductTotal = (update.Quantity * needPrice);
                                    db.SaveChanges();

                                    var test = db.CartProducts.Where(c => c.CartId == cartId);
                                    
                                    foreach (var product in test)
                                    {
                                        Console.WriteLine(product.Quantity);
                                        Console.WriteLine(product.ProductTotal);
                                    }
                                }
                            }catch
                            {
                                Console.WriteLine("Product doesn't have information.");
                            }
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Invalid input.");
                    }
                }
            }else
            {
                Console.WriteLine("Please log in to continue.");
                Login();
            }              
                Console.WriteLine();           
        }
        static void ListCart() //lists items in user cart.
        {
            using (var db = new AppDbContext())
            {
                var list = db.Carts.Where(c => c.CartId == cartId).Include(cp => cp.CartProducts).ThenInclude(p => p.Products);
                int productCount = 0;
                decimal sum = 0;

                foreach( var cart in list)
                {
                    Console.WriteLine("Products: ");
                    foreach(var product in cart.CartProducts)
                    {
                        Console.WriteLine($"{product.ProductId} - {product.Products.Description} - {product.Quantity}");
                        productCount ++; //counts number of products in cart
                        var total = db.CartProducts.Where(c => c.CartId == cartId).Select(c => c.ProductTotal).Sum();
                        sum = total;
                    }     
                }
                if(sum > 50)
                {
                    Console.WriteLine();
                    Console.WriteLine("***This order qualifies for free shipping***");
                    Console.WriteLine($"Products in Cart: {productCount} \t\t Total Cost: ${sum}");
                    Cart addTotal = db.Carts.Find(cartId);
                    addTotal.Total = sum;
                    db.SaveChanges();
                } else
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    decimal withShipping = (sum + 4.99M);
                    Console.WriteLine($"Products in Cart: {productCount} \t\t Total Cost: ${withShipping}");
                    Cart addTotal = db.Carts.Find(cartId);
                    addTotal.Total = withShipping;
                    db.SaveChanges();
                }
            }
        }

        static void RemoveFromCart() //remove item from cart
        {
           if(logedIn == true)
            {
                using(var db = new AppDbContext())
                {
                    try
                    {
                        Console.WriteLine("Remove Product Number: ");
                        int removeId = Convert.ToInt32(Console.ReadLine());

                        //check to see if item is in cart
                        var inCart = db.CartProducts.Where(p => p.ProductId == removeId && p.CartId == cartId).FirstOrDefault();
                        CartProduct r = db.CartProducts.Find(cartId, removeId);
                        
                        if(inCart is null)
                        {
                            Console.WriteLine();
                            Console.WriteLine("This item is not in your cart.");
                            Console.WriteLine();
                        }
                        else
                        {
                            if(r.Quantity > 1)
                            {
                                r.Quantity --;
                                db.SaveChanges();
                                Console.WriteLine($"New Quantity is: {r.Quantity}");
                            }  else{
                                db.CartProducts.Remove(r);
                                db.SaveChanges();
                            }
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Invalid input.");
                    }
                }
            } 
            else
            {
                Console.WriteLine("You must be logged in to remove items from cart.");
                Login();
            } 
        }
        //Searches for product using user input
        static void SearchProduct()
        {
            using (var db = new AppDbContext())
            {
                try
                {
                    Console.WriteLine("Search: ");
                    string search = Console.ReadLine().ToLower(); //converts input to lower case

                    //Queries products by converting descriptions to lower case and selected those that contain the user input
                    var SearchProduct = db.Products.Where(p => p.Description.ToLower().Contains(search));

                    Console.WriteLine();

                    //Displays results
                    foreach(var product in SearchProduct)
                    {
                        Console.WriteLine(product);
                    }
                }
                catch
                {
                    Console.WriteLine("Invalid input.");
                }
            }        
        }
        static void Main(string[] args)
        {
            using (var db = new AppDbContext())
            {
                //db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                List<Product> products = new List<Product> {
                    new Product { Description = "Cards Against Humanity", Price = 25.00M },
                    new Product { Description = "WHAT DO YOU MEME? Party Game", Price = 29.97M },
                    new Product { Description = "Manhattan Toy Winkel Rattle and Sensory Teether Toy", Price = 14.00M },
                    new Product { Description = "Spider-Man: Into the Spider-Verse [Blu-ray]", Price = 22.95M },
                    new Product { Description = "Aquaman [Blu-ray]", Price = 24.96M },
                    new Product { Description = "Fantastic Beasts: The Crimes of Grindelwald (Blu-ray + DVD + Digital Combo Pack) (BD)", Price = 24.96M },
                    new Product { Description = "Fire TV Stick 4K with all-new Alexa Voice Remote, streaming media  player", Price = 49.99M },
                    new Product { Description = "Fire TV Cube, hands-free with Alexa and 4K Ultra HD (includes all-new Alexa Voice Remote), streaming media player", Price = 119.99M },
                    new Product { Description = "All-new Echo Plus (2nd Gen) - Premium sound with built-in smart home hub - Charcoal", Price = 119.99M },
                    new Product { Description = "Echo Dot (3rd Gen) - New and improved smart speaker with Alexa - Charcoal", Price = 49.99M },
                    new Product { Description = "All-new Kindle Paperwhite – Now Waterproof with 2x the Storage – Includes Special Offers", Price = 99.99M },
                    new Product { Description = "Nintendo Switch – Neon Red and Neon Blue Joy-Con", Price = 299.99M },
                    new Product { Description = "Super Smash Bros. Ultimate (Platform: Nintendo Switch)", Price = 56.99M},
                    new Product { Description = "Kingdom Hearts III (Platform: PlayStation 4)", Price = 59.99M }
                    };

                    db.AddRange(products);
                    db.SaveChanges();
            }
            
            bool exit;
            int selection;
            bool validChoice;

            do{
                exit = false;
                Console.WriteLine();
                Console.WriteLine("1. Login");
                Console.WriteLine("2. List Products");
                Console.WriteLine("3. Add Product to Cart");
                Console.WriteLine("4. View Cart");
                Console.WriteLine("5. Remove Product");
                Console.WriteLine("6. Product Search");
                Console.WriteLine("7. Quit");

                Console.WriteLine();
                do{
                    validChoice = true;
                    Console.Write("Make a selection: ");
                try
                {
                    selection = Convert.ToInt32(Console.ReadLine());
                    switch (selection)
                    {
                        case 1:
                        Console.WriteLine();
                        Login();
                        break;

                        case 2:
                        Console.WriteLine();
                        List();
                        break;

                        case 3:
                        Console.WriteLine();
                        Add();
                        break;

                        case 4:
                        Console.WriteLine();
                        ListCart();
                        break;

                        case 5:
                        Console.WriteLine();
                        RemoveFromCart();
                        break;

                        case 6:
                        Console.WriteLine();
                        SearchProduct();
                        break;

                        case 7:
                        Console.WriteLine();
                        Console.WriteLine("Quitting...");
                        Console.WriteLine();
                        exit = true;
                        break;

                        default:
                        Console.WriteLine("Invalid Selection.");
                        Console.WriteLine();
                        break;
                    }
                }
                    catch (FormatException)
                    {
                        validChoice = false;
                        Console.WriteLine();
                        Console.WriteLine("Invalid Choice. Please try again.");
                        Console.WriteLine();
                    }
                }while (validChoice == false);
            }while (exit == false);
            
        }
    }
}
