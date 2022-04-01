namespace Pos.Console
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {

            // 飲料店

            // 茶飲
            // 紅茶
            // 綠茶
            // 烏龍茶
            // 奶茶

            Console.InputEncoding = Console.OutputEncoding = System.Text.Encoding.Unicode;
             

            var productFactory = Product.Factory;

            var order = new Order();
            var product = productFactory.CreateProduct(Console.ReadLine());

            order.AddProduct(product);

            var total = order.CalculateTotalPrice();
            Console.WriteLine("總價: " + total);
        }
    }

    // 工廠
    public class ProductFactory
    {

        private readonly ConcurrentDictionary<string, Type> _productMap; 

        internal ProductFactory()
        {
            this._productMap =
            new ConcurrentDictionary<string, Type>((from productType in typeof(ProductFactory).Assembly.GetTypes()
                                                    where productType.IsSubclassOf(typeof(Product)) && !productType.IsAbstract && !productType.IsInterface && productType.IsDefined(typeof(ProductAttribute))
                                                    let name = productType.GetCustomAttribute<ProductAttribute>().Name
                                                    select new
                                                    {
                                                        name,
                                                        productType
                                                    }).ToDictionary(x => x.name, x => x.productType));
        }

        // method chaining
        [Obsolete]
        public ProductFactory Register<TProduct>(string name) where TProduct : Product
        {
            _productMap.TryAdd(name, typeof(TProduct));
            return this;
        }

        public Product CreateProduct(string productName)
        {
            if (_productMap.TryGetValue(productName ?? string.Empty, out var productType))
            {
                return (Product)Activator.CreateInstance(productType);
            }
            return Product.Null;
        }
    }


    public class Order
    {
        //public string Id { get; set; }
        public IList<Product> Products { get; } = new List<Product>();

        public void AddProduct(Product product)
        {
            this.Products.Add(product);
        }

        public decimal CalculateTotalPrice()
        {
            return this.Products.Sum(x => x.GetPrice());
        }

    }


    public abstract class Product : IEquatable<Product>
    {
        /// <summary>
        /// 
        /// </summary>
        public static ProductFactory Factory { get; } = new ProductFactory();


        // Null Pattern 
        private class Empty : Product
        {
            public override string Name { get; } = String.Empty;
            protected override decimal Price => 0;
        }
        public static Product Null { get; } = new Empty();

        //public string Id { get; set; }
        public virtual string Name => this.GetType().GetCustomAttribute<ProductAttribute>().Name;
        protected abstract decimal Price { get; }
        public decimal GetPrice()
        {
            if (Price < 0m)
            {
                throw new ArgumentOutOfRangeException();
            }
            return Price;
        }
        public override int GetHashCode()
        {
            return Name?.GetHashCode() ?? 0;
        }
        public override string ToString()
        {
            return Name;
        }

        public bool Equals(Product other)
        {
            return this.Name == other.Name;
        }
    }

    public abstract class Tea : Product
    {
    }

    public interface IPureTea
    {
    }

    public interface IMixtureProduct { }

    // 模板

    [AttributeUsage(AttributeTargets.Class)]
    public class ProductAttribute : Attribute
    {
        public ProductAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; }
    }

    [Product("紅茶")]
    public class BlackTea : Tea, IPureTea
    {
        protected override decimal Price => 20m;
    }
    [Product("綠茶")]
    public class GreenTea : Tea, IPureTea
    {
        protected override decimal Price => 20m;
    }

    [Product("烏龍茶")]
    public class OolongTea : Tea, IPureTea
    {
        protected override decimal Price => 20m;
    }
     

    // 組合根 (MEF)
    public abstract class ComposableProduct : Product, IMixtureProduct
    {
        public ComposableProduct(params Product[] products)
        {
            Products = products;
        }

        public Product[] Products { get; }

        protected override decimal Price => Products.Sum(x => x.GetPrice());

    }

    // 組合模式
    [Product("奶茶")]
    public class MilkTea : ComposableProduct
    {
        public MilkTea() : base(new Milk(), new BlackTea())
        {

        }
    }


    [Product("牛奶")]
    public class Milk : Product
    {
        protected override decimal Price { get; } = 20m;
    }

}
