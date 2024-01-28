using Assignment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Assignment.Services
{
    public class CartService
    {
        private static List<CartItem> cartItems = new List<CartItem>();

        public static void AddToCart(property property)
        {
            var existingItem = cartItems.FirstOrDefault(item => item.PropertyId == property.id);

            if (existingItem == null)
            {
                var newItem = new CartItem
                {
                    PropertyId = property.id,
                    PropertyName = property.property_name,
                    PropertyPrice = property.property_price,
                    PropertyImage = property.property_image,
                };

                cartItems.Add(newItem);
            }
        }

        public static List<CartItem> GetCartItems()
        {
            return cartItems;
        }
        public static void ClearCart()
        {
            cartItems.Clear();
        }
    }

}