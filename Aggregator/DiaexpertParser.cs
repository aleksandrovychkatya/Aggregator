using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.IO;

namespace Aggregator
{
    public class DiaexpertParser
    {
        public static List<Category> makeAllCategories()
        {
            List<string> categoryNames = getCategoriesNames();
            List<Category> categories = new List<Category>();


            foreach (string categoryName in categoryNames)
            {
                categories.Add(makeCategory(categoryName));
            }
            return categories;
        }
        public static Category makeCategory(string categoryName)
        {
            Category category = new Category(categoryName);
            List<string> allProductsLinks = getAllPagesCategoryLinks(categoryName);

            foreach (string productLink in allProductsLinks)
            {
                Product product = createProductFromHTMLText(productLink);
                product.IDCategory = category.ID;
                if (product.Name != "empty")
                {
                    category.Products.Add(product);
                }

            }
            return category;
        }

        public static List<string> getAllPagesCategoryLinks(string categoryName)
        {

            int page = 1;
            List<string> allCategoryLinks = new List<string>();
            List<string> currentPageLinks = getLinksFromCategory(categoryName + makePage(page));

            while (currentPageLinks.Count > 0)
            {
                allCategoryLinks.AddRange(currentPageLinks);
                page++;
                currentPageLinks = getLinksFromCategory(categoryName + makePage(page));
            }
            return allCategoryLinks;
        }
        public static List<string> getCategoriesNames()
        {
            WebClient wc = new WebClient();
            wc.Encoding = System.Text.Encoding.UTF8;
            string htmlStr = wc.DownloadString("https://diaexpert.ua/");
            //681
            var text = FindText(htmlStr, @"<div class=" + '\u0022' + @"mega-toggle-blocks-right" + '\u0022', @"</header>");
            var regexpHref = new Regex(@"href\s*=\s*[""'](.*?)[""']", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var matches = regexpHref.Matches(text);
            List<string> names = new List<string>();
            Console.WriteLine("Test Text:");
            foreach (Match x in matches)
            {
                var name = FindText(x.Value, @"product-category/", @"/");
                if (name != "")
                {
                    names.Add(name);
                }
            }
            IEnumerable<string> distinctNames = names.Distinct();
            return distinctNames.ToList();
        }

        public static Product createProductFromHTMLText(string link)
        {
            try
            {
                string tempLink = link;
                WebClient wc = new WebClient();
                wc.Encoding = System.Text.Encoding.UTF8;
                string htmlStr = wc.DownloadString(tempLink);

                string textForProduct = FindText(htmlStr, @"<div data-thumb=", @"<section class=" + '\u0022' + @"up-sells upsells products" + '\u0022' + @">");

                string name = FindText(textForProduct, @"<h1 class=" + '\u0022' + @"product_title entry-title" + '\u0022' + @">", @"</h1>").Replace("&#8211;", "");
                string brand = FindText(textForProduct, @"title=" + '\u0022' + @"Посмотреть бренд" + '\u0022' + @">", @"</a>");
                var price = int.Parse(FindText(textForProduct, @"<bdi>", @"&nbsp;"));
                string shortDescr = FindText(textForProduct, @"<p>", @"</p>").Replace("&#8211;", "");
                string fullDescr = FindText(textForProduct, @"<div class=" + '\u0022' + @"wcz-inner" + '\u0022' + @">", @"</divS>").Replace("&#8211;", "");
                fullDescr = Regex.Replace(fullDescr, "<[^>]+>", string.Empty);
                string productLink = link;
                string img = FindText(textForProduct, @"<a href=" + '\u0022', '\u0022' + @"><img");
                tempLink = "";

                return new Product
                {
                    Name = name,
                    Brand = brand,
                    Price = price,
                    ShortDescription = shortDescr,
                    FullDescription = fullDescr,
                    Link = productLink,
                    Image = img
                };
            }
            catch (Exception e)
            {
                return new Product
                {
                    Name = "empty"
                };
            }
        }

        public static List<string> getLinksFromCategory(string categoryName)
        {
            try
            {
                WebClient wc = new WebClient();
                wc.Encoding = System.Text.Encoding.UTF8;
                string htmlStr = wc.DownloadString("https://diaexpert.ua/product-category/" + categoryName + "/");

                var txtTitle = FindText(htmlStr, @"<ul class=" + '\u0022' + @"products columns-4" + '\u0022' + @">", @"</ul>");

                //Регулярное выражение для поиска href свойства
                var regexpHref = new Regex(@"href\s*=\s*[""'](.*?)[""']", RegexOptions.IgnoreCase | RegexOptions.Multiline);

                var matches = regexpHref.Matches(txtTitle);

                var links = new List<string>();
                foreach (Match match in matches)
                {
                    links.Add(FindText(match.Value, @"href=" + '\u0022', @"" + '\u0022'));
                };
                //LINQ запрос на сортировку и уникализацию
                links = links.Distinct().OrderBy(el => el).ToList().FindAll(el => el.Contains("http"));
                return links;
            }
            catch
            {
                return new List<string>();
            }

        }
        public static string makePage(int number)
        {
            if (number == 1)
            {
                return "";
            }
            return "/page/" + number + "/";
        }
        public static String FindText(string source, string prefix, string suffix)
        {
            var prefixPosition = source.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
            var suffixPosition = source.IndexOf(suffix, prefixPosition + prefix.Length, StringComparison.OrdinalIgnoreCase);

            if ((prefixPosition >= 0) && (suffixPosition >= 0) && (suffixPosition > prefixPosition) && ((prefixPosition + prefix.Length) <= suffixPosition))
            {
                return source.Substring(
                                prefixPosition + prefix.Length,
                                suffixPosition - prefixPosition - prefix.Length
                    );
            }
            else
            {
                return String.Empty;
            }
        }
    }
}
