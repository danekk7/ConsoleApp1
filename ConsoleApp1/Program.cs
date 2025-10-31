using System;
using System.Collections.Generic;

namespace ConsoleApp1
{
    // Интерфейсы
    public interface IFeedable { void Eat(); }
    public interface IProduceable { Product Produce(); }

    // Базовый класс
    public abstract class Entity
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }

        public Entity(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
        }

        public override string ToString() => $"{Name}";
    }

    // Животные
    public abstract class Animal : Entity, IFeedable, IProduceable
    {
        protected int hunger;
        protected int health;
        protected bool isSick;

        public string Species { get; protected set; }

        protected static Random rnd = new Random();

        public bool IsSick => isSick;

        public Animal(string name, string species) : base(name)
        {
            Species = species;
            hunger = 50;
            health = 100;
            isSick = false;
        }

        public void Eat()
        {
            if (isSick)
            {
                Console.WriteLine($"{Name} ({Species}) не может есть, оно больно!");
                return;
            }
            hunger = Math.Max(0, hunger - 30);
            Console.WriteLine($"{Name} ({Species}) поел. Голод: {hunger}");
        }

        public void RandomEvent()
        {
            if (rnd.Next(0, 10) < 2) // 20% шанс заболеть
            {
                isSick = true;
                Console.WriteLine($"⚠ {Name} ({Species}) заболел!");
            }
        }

        public void Heal()
        {
            if (!isSick)
            {
                Console.WriteLine($"{Name} ({Species}) не болен.");
                return;
            }
            isSick = false;
            health = 100;
            Console.WriteLine($"✅ {Name} ({Species}) выздоровел!");
        }

        public abstract void MakeSound();
        public abstract Product Produce();
    }

    public class Cow : Animal
    {
        public Cow(string name) : base(name, "Корова") { }
        public override void MakeSound() => Console.WriteLine($"{Name} ({Species}): Мууу!");
        public override Product Produce()
        {
            if (IsSick) { Console.WriteLine($"{Name} ({Species}) больна и не даёт молока!"); return null; }
            return new Product("Молоко", 1, ProductType.Milk);
        }
    }

    public class Chicken : Animal
    {
        public Chicken(string name) : base(name, "Курица") { }
        public override void MakeSound() => Console.WriteLine($"{Name} ({Species}): Ко-ко-ко!");
        public override Product Produce()
        {
            if (IsSick) { Console.WriteLine($"{Name} ({Species}) больна и не снесла яйцо!"); return null; }
            return new Product("Яйцо", 1, ProductType.Egg);
        }
    }

    public class Sheep : Animal
    {
        public Sheep(string name) : base(name, "Овца") { }
        public override void MakeSound() => Console.WriteLine($"{Name} ({Species}): Беее!");
        public override Product Produce()
        {
            if (IsSick) { Console.WriteLine($"{Name} ({Species}) больна и не дала шерсти!"); return null; }
            return new Product("Шерсть", 1, ProductType.Wool);
        }
    }

    // Продукты
    public enum ProductType { Milk, Egg, Wool }

    public class Product
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public ProductType Type { get; set; }

        public Product(string name, int qty, ProductType type)
        {
            Name = name;
            Quantity = qty;
            Type = type;
        }

        public override string ToString() => $"{Name} x{Quantity}";
    }

    // Рынок
    public class Market
    {
        private Dictionary<ProductType, int> prices = new Dictionary<ProductType, int>
        {
            { ProductType.Milk, 50 },
            { ProductType.Egg, 20 },
            { ProductType.Wool, 80 }
        };

        public int Sell(Product p)
        {
            int price = prices[p.Type] * p.Quantity;
            Console.WriteLine($"Продан {p} за {price} монет.");
            return price;
        }
    }

    // Фермер
    public class Farmer
    {
        public string Name { get; private set; }
        public int Money { get; private set; }
        public List<Product> Inventory { get; private set; }

        private int feedCountToday;
        private const int MaxFeedsPerDay = 5;

        public Farmer(string name)
        {
            Name = name;
            Money = 100;
            Inventory = new List<Product>();
            feedCountToday = 0;
        }

        public void Feed(Animal a)
        {
            if (feedCountToday >= MaxFeedsPerDay)
            {
                Console.WriteLine("❌ Сегодня больше нельзя кормить животных (лимит достигнут).");
                return;
            }

            a.Eat();
            feedCountToday++;
            Console.WriteLine($"Фермер {Name} покормил {a.Name} ({a.Species}). ({feedCountToday}/{MaxFeedsPerDay})");
        }

        public void ResetDailyFeedCount()
        {
            feedCountToday = 0;
        }

        public void CollectProducts(Animal a)
        {
            var p = a.Produce();
            if (p != null) Inventory.Add(p);
            if (p != null) Console.WriteLine($"{Name} собрал: {p} у {a.Name} ({a.Species})");
        }

        public void SellProducts(Market m)
        {
            foreach (var p in Inventory)
            {
                Money += m.Sell(p);
            }
            Inventory.Clear();
        }

        public void HealAnimal(Animal a)
        {
            if (a.IsSick)
            {
                if (Money >= 30)
                {
                    Money -= 30;
                    a.Heal();
                    Console.WriteLine($"{Name} потратил 30 монет на лечение.");
                }
                else
                {
                    Console.WriteLine("Недостаточно денег для лечения!");
                }
            }
            else
            {
                Console.WriteLine($"{a.Name} ({a.Species}) здоров и не нуждается в лечении.");
            }
        }
    }

    // Главная программа
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("Добро пожаловать в фермерскую симуляцию! (ветка feature/hello)");

            var farmer = new Farmer("Иван");
            var animals = new List<Animal>
            {
                new Cow("Бурёнка"),
                new Chicken("Ряба"),
                new Sheep("Мери")
            };
            var market = new Market();

            int day = 1;
            while (true)
            {
                Console.WriteLine($"\n=== День {day} ===");
                Console.WriteLine($"Баланс: {farmer.Money} монет");
                Console.WriteLine("Животные на ферме:");
                foreach (var a in animals) Console.WriteLine($"- {a.Name} ({a.Species}){(a.IsSick ? " ⚠ БОЛЕН" : "")}");

                Console.WriteLine("\nВыберите действие:");
                Console.WriteLine("1 — Покормить всех");
                Console.WriteLine("2 — Собрать продукты");
                Console.WriteLine("3 — Продать продукты");
                Console.WriteLine("4 — Лечить животных");
                Console.WriteLine("5 — Следующий день");
                Console.WriteLine("0 — Выход");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        foreach (var a in animals) farmer.Feed(a);
                        break;
                    case "2":
                        foreach (var a in animals) farmer.CollectProducts(a);
                        break;
                    case "3":
                        farmer.SellProducts(market);
                        break;
                    case "4":
                        foreach (var a in animals) farmer.HealAnimal(a);
                        break;
                    case "5":
                        day++; farmer.ResetDailyFeedCount();
                        foreach (var a in animals) a.RandomEvent();
                        Console.WriteLine("🌞 Новый день начался!");
                        break;
                    case "0":
                        Console.WriteLine("Выход из игры...");
                        return;
                    default:
                        Console.WriteLine("Неверный выбор!");
                        break;
                }
            }
        }
    }
}