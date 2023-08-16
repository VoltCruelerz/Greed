using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text.Json;
using Greed.Models.JsonSource;
using Greed.Models.JsonSource.Text;

namespace Greed.UnitTest.Models.JsonSource
{
    [TestClass]
    public class SourceTests
    {
        [TestMethod]
        public void Merge_Overwrite_Simple()
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
            Assert.IsNull(cObj["objC"]);
        }

        [TestMethod]
        public void Merge_Replace_Simple()
        {
            // Arrange
            //var a = JObject.Parse(File.ReadAllText());
            var a = new Source("..\\..\\..\\json\\dummy\\objA.json");
            Console.WriteLine("\nA ===============\n" + a.ToString());
            var b = new Source("..\\..\\..\\json\\dummy\\objB.json.gmr");
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
            var b = new Source("..\\..\\..\\json\\dummy\\objB_null.json.gmr");
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
            var b = new Source("..\\..\\..\\json\\dummy\\arrB.json.gmr");
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
        public void Merge_Union_Array()
        {
            // Arrange
            //var a = JObject.Parse(File.ReadAllText());
            var a = new Source("..\\..\\..\\json\\dummy\\arrA.json");
            Console.WriteLine("\nA ===============\n" + a.ToString());
            var b = new Source("..\\..\\..\\json\\dummy\\arrB.json.gmu");
            Console.WriteLine("\nB ===============\n" + b.ToString());

            // Act
            var c = a.Clone();
            c.Merge(b);
            Console.WriteLine("\nRESULT C ===============\n" + c.ToString());

            // Assert
            var arrC = (JArray)JObject.Parse(c.ToString())["arr"]!;
            var result = string.Join("", arrC.ToList());
            Assert.AreEqual("abcd", result);
        }

        [TestMethod]
        public void Merge_Concat_Array()
        {
            // Arrange
            //var a = JObject.Parse(File.ReadAllText());
            var a = new Source("..\\..\\..\\json\\dummy\\arrA.json");
            Console.WriteLine("\nA ===============\n" + a.ToString());
            var b = new Source("..\\..\\..\\json\\dummy\\arrB.json.gmc");
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
            var a = new LocalizedText("..\\..\\..\\json\\localized_text\\localA.localized_text");
            Console.WriteLine("\nA ===============\n" + a.ToString());
            var b = new LocalizedText("..\\..\\..\\json\\localized_text\\localB.localized_text");
            Console.WriteLine("\nB ===============\n" + b.ToString());
            var expected = new LocalizedText("..\\..\\..\\json\\localized_text\\localMerged.localized_text");

            // Act
            var c = a.Clone();
            c.Merge(b);
            Console.WriteLine("\nRESULT C ===============\n" + c.ToString());

            // Assert
            Assert.AreEqual(expected.Minify(), c.Minify());
        }

        [TestMethod]
        public void Merge_Concat_Entity_Player()
        {
            // Arrange
            //var a = JObject.Parse(File.ReadAllText());
            var a = new Source("..\\..\\..\\json\\entities\\playerA.json");
            Console.WriteLine("\nA ===============\n" + a.ToString());
            var b = new Source("..\\..\\..\\json\\entities\\playerB.json.gmc");
            Console.WriteLine("\nB ===============\n" + b.ToString());
            var expected = new Source("..\\..\\..\\json\\entities\\playerMerged.json");

            // Act
            var c = a.Clone();
            c.Merge(b);
            Console.WriteLine("\nRESULT C ===============\n" + c.ToString());

            // Assert
            Assert.AreEqual(expected.Minify(), c.Minify());
        }

        [TestMethod]
        public void ReadJsonc()
        {
            // Arrange
            // Act
            var jsonc = new Source("..\\..\\..\\json\\dummy\\Comments.jsonc");
            var json = new Source("..\\..\\..\\json\\dummy\\CommentsNot.json");

            // Assert
            Assert.AreEqual(json.Minify(), jsonc.Minify());
        }
    }
}