using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.ServiceModel.Syndication;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RSSReader2.Data;
using RSSReader2.Models;

namespace RSSReader2.Controllers
{
    public class RSSFeedsController : Controller
    {
        private readonly RSSReader2Context _context;

        public RSSFeedsController(RSSReader2Context context)
        {
            _context = context;
        }

        // GET: RSSFeeds
        public async Task<IActionResult> Index(string searchString)
        {
            var items = from m in _context.RSSFeed
                        select m;

            if (!String.IsNullOrEmpty(searchString))
            {
                items = items.Where(s => s.Name!.Contains(searchString));
            }

           // return View(await items.ToListAsync());
            return items != null ? 
                        View(await items.ToListAsync()) :
                        Problem("Entity set 'RSSReader2Context.RSSFeed'  is null.");
        }

        // GET: RSSFeeds/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.RSSFeed == null)
            {
                return NotFound();
            }

            var rSSFeed = await _context.RSSFeed
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rSSFeed == null)
            {
                return NotFound();
            }

            return View(rSSFeed);
        }

        // GET: RSSFeeds/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RSSFeeds/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Url,Name")] RSSFeed rSSFeed)
        {
            WebClient wclient = new WebClient();
            string RSSData = wclient.DownloadString(rSSFeed.Url);
            XDocument xml = XDocument.Parse(RSSData);

            ICollection<Article> Articles = GetArticlesFromXml(xml);

            /*var RSSFeedData = from x in xml.Descendants("item")
                               select new Article
                               {
                                   Title = (string)x.Element("title"),
                                   Link = (string)x.Element("link"),
                                   Description = (string)x.Element("description"),
                                   PubDate = (string)x.Element("pubDate"),
                                   RSSFeed = rSSFeed
                               };*/
            /*ICollection<Article> Articles = new List<Article>();
            foreach (var item in articleList)
            {
                Articles.Add(item);
            }*/
            rSSFeed.Articles = Articles;

            if (ModelState.IsValid)
            {
                _context.Add(rSSFeed);
                await _context.SaveChangesAsync();
                //System.Diagnostics.Debug.WriteLine(rSSFeed.Articles.Count);
                return RedirectToAction(nameof(Index));
                
            }
            return View(rSSFeed);
        }

        // GET: RSSFeeds/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.RSSFeed == null)
            {
                return NotFound();
            }

            var rSSFeed = await _context.RSSFeed.FindAsync(id);
            if (rSSFeed == null)
            {
                return NotFound();
            }
            return View(rSSFeed);
        }

        // POST: RSSFeeds/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Url,Name")] RSSFeed rSSFeed)
        {
            if (id != rSSFeed.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rSSFeed);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RSSFeedExists(rSSFeed.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(rSSFeed);
        }

        // GET: RSSFeeds/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.RSSFeed == null)
            {
                return NotFound();
            }

            var rSSFeed = await _context.RSSFeed
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rSSFeed == null)
            {
                return NotFound();
            }

            return View(rSSFeed);
        }

        // POST: RSSFeeds/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.RSSFeed == null)
            {
                return Problem("Entity set 'RSSReader2Context.RSSFeed'  is null.");
            }
            var rSSFeed = await _context.RSSFeed.FindAsync(id);
            if (rSSFeed != null)
            {
                _context.RSSFeed.Remove(rSSFeed);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: RSSFeeds/Articles/5
        public async Task<IActionResult> Articles(int? id, string dateFrom, string dateTo)
        {
            if (id == null || _context.RSSFeed == null)
            {
                return NotFound();
            }
            
            var rSSFeed = await _context.RSSFeed.Include(a => a.Articles)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (rSSFeed == null)
            {
                return NotFound();
            }
            
            var articles = rSSFeed.Articles;

            IEnumerable<Article> articlesEnumerable = rSSFeed.Articles;
            if (!String.IsNullOrEmpty(dateFrom) && !String.IsNullOrEmpty(dateTo))
            {
                articles = new List<Article>();
                articlesEnumerable = articlesEnumerable
                    .Where(s => s.PubDate >= DateTime.Parse(dateFrom) && s.PubDate <= DateTime.Parse(dateTo));
                foreach (var article in articlesEnumerable)
                {
                    articles.Add(article);
                }
            }
           
            ViewData["Articles"] = articles;
            return View();
            
        }

        private bool RSSFeedExists(int id)
        {
          return (_context.RSSFeed?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private ICollection<Article> GetArticlesFromXml(XDocument xml)
        {
            ICollection<Article> Articles = new List<Article>();
            foreach (var x in xml.Descendants("item"))
            {
                Article article = new Article();
                if ((string)x.Element("title") != null)
                {
                    article.Title = (string)x.Element("title");
                }
                else
                {
                    article.Title = "Neuvedeno";
                }
                if ((string)x.Element("link") != null)
                {
                    article.Link = (string)x.Element("link");
                }
                else
                {
                    article.Link = "Neuvedeno";
                }
                if ((string)x.Element("description") != null)
                {
                    article.Description = (string)x.Element("description");
                }
                else
                {
                    article.Description = "Neuvedeno";
                }
                if ((string)x.Element("pubDate") != null)
                {
                    article.PubDate = DateTime.Parse((string)x.Element("pubDate"));
                }
                /*else
                {
                    article.PubDate = "Neuvedeno";
                }*/
                Articles.Add(article);
            }
            return Articles;
        }
        
        
    }
}
