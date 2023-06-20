using System.Text;

namespace Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 初始化店铺，配置配料与对应的规格描述器，规格描述器均使用单例
            var shop = new Shop()
                .AddSetting<Cup>(BigSmallLevelDescription.Instance)
                .AddSetting<Milk>(HighLowLevelDescription.Instance)
                .AddSetting<Tea>(HighLowLevelDescription.Instance)
                .AddSetting<Sugar>(HighLowLevelDescription.Instance)
                .AddSetting<Pearl>(MoreLessLevelDescription.Instance)
                .AddSetting<Pudding>(MoreLessLevelDescription.Instance);


            // 问题1
            var drink = new Drink(EDrinkItemLevel.Middle)
                .AddIngredient(new Milk(), EDrinkItemLevel.High)
                .AddIngredient(new Sugar(), EDrinkItemLevel.Low)
                .AddIngredient(new Pearl(), EDrinkItemLevel.High);

            Console.WriteLine(shop.Print(drink));

            // 问题2，添加2种新配料
            shop.AddSetting<Mango>(MoreLessLevelDescription.Instance)
                .AddSetting<Grape>(MoreLessLevelDescription.Instance);

            var drink2 = new Drink(EDrinkItemLevel.High)
                .AddIngredient(new Tea(), EDrinkItemLevel.High)
                .AddIngredient(new Sugar(), EDrinkItemLevel.Middle)
                .AddIngredient(new Mango(), EDrinkItemLevel.High)
                .AddIngredient(new Pudding(), EDrinkItemLevel.Low);

            Console.WriteLine(shop.Print(drink2));

            Console.ReadLine();
        }
    }

    /// <summary>
    /// 店铺
    /// </summary>
    public class Shop
    {
        /// <summary>
        /// 配料类型与规格描述器的存储
        /// </summary>
        private readonly Dictionary<Type, ILevelDescription> _ingredientDescriptionSetting;
        private readonly Queue<Drink> _drinks;

        public Shop()
        {
            _ingredientDescriptionSetting = new Dictionary<Type, ILevelDescription>();
            _drinks = new Queue<Drink>();
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <typeparam name="TIngredient"></typeparam>
        /// <param name="levelDescription"></param>
        /// <returns></returns>
        public Shop AddSetting<TIngredient>(ILevelDescription levelDescription) where TIngredient : IIngredient
        {
            _ingredientDescriptionSetting.Add(typeof(TIngredient), levelDescription);
            return this;
        }

        public void AddBill(Drink drink)
        {
            _drinks.Enqueue(drink);
        }

        /// <summary>
        /// 打印饮品
        /// </summary>
        /// <param name="drink"></param>
        /// <returns></returns>
        public string Print(Drink drink)
        {
            return drink.Print(_ingredientDescriptionSetting);
        }
    }

    /// <summary>
    /// 饮品
    /// </summary>
    public class Drink
    {
        /// <summary>
        /// 杯型 + 规格
        /// </summary>
        private readonly DrinkItem _cup;

        /// <summary>
        /// 除杯型外的其他配料 + 规格
        /// </summary>
        private readonly List<DrinkItem> _ingredients;

        /// <summary>
        /// 初始化时，必须传入杯型
        /// </summary>
        /// <param name="cupLevel"></param>
        public Drink(EDrinkItemLevel cupLevel)
        {
            _cup = new DrinkItem(cupLevel, new Cup());
            _ingredients = new List<DrinkItem>();
        }

        /// <summary>
        /// 添加配料
        /// </summary>
        /// <param name="ingredient"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public Drink AddIngredient(IIngredient ingredient, EDrinkItemLevel level)
        {
            _ingredients.Add(new DrinkItem(level, ingredient));
            return this;
        }

        /// <summary>
        /// 打印当前饮品，使用外部传入的规格描述器
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public string Print(Dictionary<Type, ILevelDescription> setting)
        {
            var builder = new StringBuilder();
            var cupDescription = setting[_cup.Ingredient.GetType()] ?? EmptyLevelDescription.Instance;
            builder.Append(_cup.Print(cupDescription));

            foreach (var item in _ingredients)
            {
                var itemDescription = setting[item.Ingredient.GetType()] ?? EmptyLevelDescription.Instance;
                builder.Append(item.Print(itemDescription));
            }
            return builder.ToString();
        }
    }

    /// <summary>
    /// 饮品内配料的规格
    /// </summary>
    public enum EDrinkItemLevel
    {
        High,
        Middle,
        Low
    }

    /// <summary>
    /// 饮品项，代表一种配料及其规格
    /// </summary>
    public class DrinkItem
    {
        private readonly EDrinkItemLevel _level;
        private readonly IIngredient _ingredient;

        public DrinkItem(EDrinkItemLevel level, IIngredient ingredient)
        {
            _level = level;
            _ingredient = ingredient;
        }

        public IIngredient Ingredient { get { return _ingredient; } }
        public EDrinkItemLevel Level { get { return _level; } }

        public string Print(ILevelDescription levelDescription)
        {
            string level = string.Empty;
            switch (_level)
            {
                case EDrinkItemLevel.Low: level = levelDescription.Low; break;
                case EDrinkItemLevel.High: level = levelDescription.High; break;
                case EDrinkItemLevel.Middle: level = levelDescription.Middle; break;
                default: break;
            }

            return level + _ingredient.GetName();
        }
    }

    #region 配料定义

    public interface IIngredient
    {
        string GetName();
    }

    public class Cup : IIngredient
    {
        public string GetName()
        {
            return "杯";
        }
    }

    public class Tea : IIngredient
    {
        public string GetName()
        {
            return "茶";
        }
    }

    public class Milk : IIngredient
    {
        public string GetName()
        {
            return "奶";
        }
    }

    public class Sugar : IIngredient
    {
        public string GetName()
        {
            return "糖";
        }
    }

    public class Pearl : IIngredient
    {
        public string GetName()
        {
            return "珍珠";
        }
    }

    public class Pudding : IIngredient
    {
        public string GetName()
        {
            return "布丁";
        }
    }

    public class Mango : IIngredient
    {
        public string GetName()
        {
            return "芒果";
        }
    }

    public class Grape : IIngredient
    {
        public string GetName()
        {
            return "葡萄";
        }
    }

    #endregion

    #region 规格描述器定义

    /// <summary>
    /// 默认固定为3个规格级别，如果需要考虑扩展，可以使用抽象工厂方法来判断规格
    /// </summary>
    public interface ILevelDescription
    {
        string High { get; }
        string Middle { get; }
        string Low { get; }
    }

    /// <summary>
    /// 大中小规格描述器
    /// </summary>
    public class BigSmallLevelDescription : ILevelDescription
    {
        public static BigSmallLevelDescription Instance = new BigSmallLevelDescription();

        public string High => "大";

        public string Middle => "中";

        public string Low => "小";
    }

    /// <summary>
    /// 高中低规格描述器
    /// </summary>
    public class HighLowLevelDescription : ILevelDescription
    {
        public static HighLowLevelDescription Instance = new HighLowLevelDescription();

        public string High => "高";

        public string Middle => "中";

        public string Low => "低";
    }

    /// <summary>
    /// 多中少规格描述器
    /// </summary>
    public class MoreLessLevelDescription : ILevelDescription
    {
        public static MoreLessLevelDescription Instance = new MoreLessLevelDescription();

        public string High => "多";

        public string Middle => "中";

        public string Low => "少";
    }

    /// <summary>
    /// 空的规格描述器
    /// </summary>
    public class EmptyLevelDescription : ILevelDescription
    {
        public static EmptyLevelDescription Instance = new EmptyLevelDescription();

        public string High => string.Empty;

        public string Middle => string.Empty;

        public string Low => string.Empty;
    }

    #endregion
}