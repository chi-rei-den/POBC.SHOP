using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Security.Permissions;

namespace Shop
{
	public class ShopConfing
	{
		public _Shop[] ShopC = new _Shop[0];

		public ShopConfing Write(string file)
		{
			File.WriteAllText(file, JsonConvert.SerializeObject(this, Formatting.Indented));
			return this;
		}

		public static ShopConfing Read(string file)
		{
			if (!File.Exists(file))
			{
				WriteExample(file);
			}
			return JsonConvert.DeserializeObject<ShopConfing>(File.ReadAllText(file));
		}

		public static void WriteExample(string file)
		{
			var Ex = new _Shop()
			{
				id = 1,
				name = "itemname",
				itemid = 1,
				C = 1
			};
			var Conf = new ShopConfing()
			{
				ShopC = new _Shop[] { Ex }
			};
			Conf.Write(file);
		}
	}

	public class _Shop
	{
		public int id = 0;
		public string name;
		public int itemid;
		public int C = 0;
	}
}

