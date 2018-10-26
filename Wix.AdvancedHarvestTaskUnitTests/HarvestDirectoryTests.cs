using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wix.AdvancedHarvestTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using System.Xml;

namespace Wix.AdvancedHarvestTask.Tests
{
    [TestClass()]
    public class HarvestTaskTests
    {
        [TestMethod()]
        public void Harvest_WhenOutputExists_RemovesReadonly()
        {
            //arrange
            var fsmock = new Mock<IFileSystem>();
            var task = new HarvestDirectory(fsmock.Object);

            fsmock.Setup(f => f.FileExists(It.IsAny<string>())).Returns(true);
            fsmock.Setup(f => f.RemoveReadonly(It.IsAny<string>())).Verifiable();

            //act
            task.Execute();

            //assert
            fsmock.Verify();
        }

        [TestMethod]
        public void Harvest_WithExcludeMask_OnlyRelevantFilesAdded()
        {
            //arrange
            var fsmock = new Mock<IFileSystem>();
            var task = new HarvestDirectory(fsmock.Object);
            XmlDocument doc = null;

            fsmock.Setup(f => f.EnumerateFiles(It.IsAny<string>(), It.Is<string>(s => s == "*.excluded"))).Returns(new string[] { @"c:\somefile.excluded" });
            fsmock.Setup(f => f.EnumerateFiles(It.IsAny<string>(), It.Is<string>(s => s == "*.*"))).Returns(new string[] { @"c:\somefile.excluded", @"c:\and-another-included.file" });
            fsmock.Setup(f => f.Save(It.IsAny<XmlDocument>(), It.IsAny<string>())).Callback<XmlDocument, string>((d, s) => doc = d);

            task.ExcludeMask = "*.excluded";
            task.SourceFolder = "x:\non-existent";
            task.OutputFile = "file.wxs";

            //act
            task.Execute();

            //assert
            Assert.IsNotNull(doc);
            var ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("wix", "http://schemas.microsoft.com/wix/2006/wi");
            Assert.AreEqual(1, doc.SelectNodes("/wix:Wix/wix:Fragment/wix:DirectoryRef/wix:Component", ns).Count);
        }
    }
}