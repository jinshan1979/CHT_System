using CHTSystem.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace CHTSystem.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(int pageIndex = 0)
        {
            var dir = Server.MapPath("~/File");
            string[] imagesFiles = Directory.GetFiles($"{dir}\\Images\\BusinessCard");
            string[] labelsFiles = Directory.GetFiles($"{dir}\\labels\\BusinessCard");

            pageIndex = pageIndex < 0 ? 0 : pageIndex;
            pageIndex = pageIndex >= labelsFiles.Length ? labelsFiles.Length - 1 : pageIndex;

            ViewData["imgFile"] = Path.GetFileName(imagesFiles[pageIndex]);
            ViewData["xmlFile"] = Path.GetFileName(labelsFiles[pageIndex]);

            ViewData["total"] = imagesFiles.Count();
            ViewData["pageIndex"] = pageIndex;

            List<XmlModel> list = readerXml(labelsFiles[pageIndex]);
            return View(list);
        }

        [HttpPost]
        public JsonResult Post(List<XmlModel> xmls)
        {
            updateXml(xmls);
            return Json(new { success = true });
        }

        private List<XmlModel> readerXml(string filePath)
        {
            XmlDocument xmldc = new XmlDocument();
            XmlNamespaceManager xnm = new XmlNamespaceManager(xmldc.NameTable);
            List<XmlModel> list = new List<XmlModel>();
            using (XmlReader reader = XmlReader.Create(filePath))
            {
                xmldc.Load(reader);
                var lines = xmldc.DocumentElement.SelectNodes("/Image/Lines/Line", xnm);
                foreach (var line in lines)
                {
                    list.Add(new XmlModel()
                    {
                        Id= ((XmlNode)line).SelectSingleNode("Id", xnm).InnerText,
                        Value = ((XmlNode)line).SelectSingleNode("Value", xnm).InnerText,
                        Recogonition = ((XmlNode)line).SelectSingleNode("Recogonition", xnm).InnerText,
                        Hint = hintWords(((XmlNode)line).SelectSingleNode("Value", xnm).InnerText)
                    });
                }
            }
            return list;
        }

        private void updateXml(List<XmlModel> imageInfos)
        {
            var dir = Server.MapPath("~/File");
            string filePath = String.Format("{0}\\labels\\BusinessCard\\{1}", dir,imageInfos[0].Hint);


            XmlDocument xmldc = new XmlDocument();
            xmldc.Load(filePath);

            foreach (var node in imageInfos)
            {
                string xpath = String.Format("/Image/Lines/Line[Id={0}]", node.Id);
                XmlNode image = xmldc.SelectSingleNode(xpath);
                XmlElement ele = (XmlElement)image.SelectSingleNode("Value");

                ele.InnerText = node.Value;
            }

            xmldc.Save(filePath);

        }

        private string hintWords(string str)
        {
            var dir = Server.MapPath("~/File");
            //Dictionary<string, string> errorList = new Dictionary<string, string>();
            string result = string.Empty;

            using (StreamReader sr = new StreamReader(dir + @"/errorList.txt", System.Text.Encoding.Default))
            {
                string pair = sr.ReadLine();
                while (pair != null)
                {
                    string key = pair.Split('|')[0];
                    string value = pair.Split('|')[1];

                    if (str.Contains(key))
                    {
                        result = result + " " + pair;
                    }

                    pair = sr.ReadLine();
                }
            }
            return result;
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}