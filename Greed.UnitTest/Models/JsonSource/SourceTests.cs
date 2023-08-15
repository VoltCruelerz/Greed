using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text.Json;
using Greed.Models.JsonSource;

namespace Greed.UnitTest.Models.JsonSource
{
    [TestClass]
    public class SourceTests
    {
        [TestMethod]
        public void Merge_Replace_Simple()
        {
            // Arrange
            //var a = JObject.Parse(File.ReadAllText());
            var a = new Source("..\\..\\..\\json\\dummy\\objA.json");
            Console.WriteLine("\nA ===============\n" + a.ToString());
            var b = new Source("..\\..\\..\\json\\dummy\\objB.json");
            Console.WriteLine("\nB ===============\n" + b.ToString());

            // Act
            var c = a.Clone();
            c.Merge(b);
            Console.WriteLine("\nRESULT C ===============\n" + c.ToString());

            // Assert
            var aObj = JObject.Parse(a.ToString());
            var bObj = JObject.Parse(b.ToString());
            var cObj = JObject.Parse(c.ToString());
            Assert.AreEqual(aObj["intA"]!.ToString(), cObj["intA"]!.ToString());
            Assert.AreEqual(bObj["strB"]!.ToString(), cObj["strB"]!.ToString());
            Assert.AreEqual(aObj["objC"]!.ToString(), cObj["objC"]!.ToString());
        }

        [TestMethod]
        public void Merge_Replace_NullRemoval()
        {
            // Arrange
            //var a = JObject.Parse(File.ReadAllText());
            var a = new Source("..\\..\\..\\json\\dummy\\objA.json");
            Console.WriteLine("\nA ===============\n" + a.ToString());
            var b = new Source("..\\..\\..\\json\\dummy\\objB_null.json");
            Console.WriteLine("\nB ===============\n" + b.ToString());

            // Act
            var c = a.Clone();
            c.Merge(b);
            Console.WriteLine("\nRESULT C ===============\n" + c.ToString());

            // Assert
            var aObj = JObject.Parse(a.ToString());
            var cObj = JObject.Parse(c.ToString());
            Assert.AreEqual(aObj["intA"]!.ToString(), cObj["intA"]!.ToString());
            Assert.IsNull(cObj["strB"]);
            Assert.AreEqual(aObj["objC"]!.ToString(), cObj["objC"]!.ToString());
        }

        [TestMethod]
        public void Merge_Replace_Array()
        {
            // Arrange
            //var a = JObject.Parse(File.ReadAllText());
            var a = new Source("..\\..\\..\\json\\dummy\\arrA.json");
            Console.WriteLine("\nA ===============\n" + a.ToString());
            var b = new Source("..\\..\\..\\json\\dummy\\arrB.json");
            Console.WriteLine("\nB ===============\n" + b.ToString());

            // Act
            var c = a.Clone();
            c.Merge(b);
            Console.WriteLine("\nRESULT C ===============\n" + c.ToString());

            // Assert
            var arrA = (JArray)JObject.Parse(a.ToString())["arr"]!;
            var arrB = (JArray)JObject.Parse(b.ToString())["arr"]!;
            var arrC = (JArray)JObject.Parse(c.ToString())["arr"]!;
            Assert.AreEqual(arrA[0]!.ToString(), arrC[0]!.ToString(), "element a[0] should be the same");
            Assert.AreEqual(arrB[2]!.ToString(), arrC[1]!.ToString(), "element a[1] should have been skipped to b[2]");
        }

        [TestMethod]
        public void Merge_Concat_Array()
        {
            // Arrange
            //var a = JObject.Parse(File.ReadAllText());
            var a = new Source("..\\..\\..\\json\\dummy\\arrA.json");
            Console.WriteLine("\nA ===============\n" + a.ToString());
            var b = new Source("..\\..\\..\\json\\dummy\\arrB.json.concat");
            Console.WriteLine("\nB ===============\n" + b.ToString());

            // Act
            var c = a.Clone();
            c.Merge(b);
            Console.WriteLine("\nRESULT C ===============\n" + c.ToString());

            // Assert
            var arrA = (JArray)JObject.Parse(a.ToString())["arr"]!;
            var arrB = (JArray)JObject.Parse(b.ToString())["arr"]!;
            var arrC = (JArray)JObject.Parse(c.ToString())["arr"]!;
            var str = string.Join("", arrC.Select(p => p.ToString()).ToList());
            Assert.AreEqual("abcad", str, "element a[0] should be the same");
        }

        [TestMethod]
        public void Merge_Concat_Localization()
        {
            // Arrange
            //var a = JObject.Parse(File.ReadAllText());
            var a = new Source("..\\..\\..\\json\\localized_text\\localA.json");
            Console.WriteLine("\nA ===============\n" + a.ToString());
            var b = new Source("..\\..\\..\\json\\localized_text\\localB.json.concat");
            Console.WriteLine("\nB ===============\n" + b.ToString());
            var expected = new Source("..\\..\\..\\json\\localized_text\\localMerged.json");

            // Act
            var c = a.Clone();
            c.Merge(b);
            Console.WriteLine("\nRESULT C ===============\n" + c.ToString());

            // Assert
            Assert.AreEqual(expected.ToString(), c.ToString(), "Expected:\n" + expected.ToString());
        }

        [TestMethod]
        public void Merge_Concat_Entity_Player()
        {
            // Arrange
            //var a = JObject.Parse(File.ReadAllText());
            var a = new Source("..\\..\\..\\json\\entities\\playerA.json");
            Console.WriteLine("\nA ===============\n" + a.ToString());
            var b = new Source("..\\..\\..\\json\\entities\\playerB.json.concat");
            Console.WriteLine("\nB ===============\n" + b.ToString());
            var expected = new Source("..\\..\\..\\json\\entities\\playerMerged.json");

            // Act
            var c = a.Clone();
            c.Merge(b);
            Console.WriteLine("\nRESULT C ===============\n" + c.ToString());

            // Assert
            Assert.AreEqual(expected.Minify(), c.Minify());
        }
    }
}