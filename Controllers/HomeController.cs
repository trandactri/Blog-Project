using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Database;
using App.Models;
using App.Models.Blog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.Blog.Controllers
{
    // [Area("Blog")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController>? _logger;
        private readonly MyBlogContext? _context;

        public HomeController(ILogger<HomeController>? logger, MyBlogContext? context)
        {
            _logger = logger;
            _context = context;
        }

        // [Route("/post/{categorySlug?}")]
        public IActionResult Index(string categorySlug, [FromQuery(Name = "p")] int currentPage, int pagesize)
        {
            var categories = GetCategories();
            ViewBag.Categories = categories;
            ViewBag.CategorySlug = categorySlug;

            Category? category = null;

            if (!string.IsNullOrEmpty(categorySlug))
            {
                category = _context.Categories.Where(c => c.Slug == categorySlug)
                                              .Include(c => c.CategoryChildren)
                                              .FirstOrDefault();

                if (category == null)
                {
                    return NotFound("Cannot find Category");
                }
            }

            var posts = _context.Posts
                        .Include(p => p.Author)
                        .Include(p => p.PostCategories)
                        .ThenInclude(p => p.Category)
                        .Where(p => p.Published)
                        .OrderByDescending(p => p.DateUpdated)
                        .AsQueryable();

            if (category != null)
            {
                var ids = new List<int>();
                category.ChildCategoryIDs(ids, null);
                ids.Add(category.Id);

                posts = posts.Where(p => p.Published && p.PostCategories.Where(pc => ids.Contains(pc.CategoryID)).Any());
            }

            int totalPosts = posts.Count();
            if (pagesize <= 0) pagesize = 10;
            int countPages = (int)Math.Ceiling((double)totalPosts / pagesize);

            if (currentPage > countPages) currentPage = countPages;
            if (currentPage < 1) currentPage = 1;

            var pagingModel = new PagingModel()
            {
                countpages = countPages,
                currentpage = currentPage,
                generateUrl = (pageNumber) => Url.Action("Index", new
                {
                    p = pageNumber,
                    pagesize = pagesize
                })
            };

            ViewBag.pagingModel = pagingModel;
            ViewBag.totalPosts = totalPosts;

            var postsInPage = posts.Skip((currentPage - 1) * pagesize)
                             .Take(pagesize);

            ViewBag.category = category;
            return View(postsInPage.ToList());
        }

        [Route("/post/{postSlug}.html")]
        public IActionResult Detail(string postSlug)
        {
            var categories = GetCategories();
            ViewBag.Categories = categories;

            var post = _context.Posts.Where(p => p.Slug == postSlug)
                               .Include(p => p.Author)
                               .Include(p => p.PostCategories)
                               .ThenInclude(pc => pc.Category)
                               .FirstOrDefault();

            if (post == null)
            {
                return NotFound("Cannot find post");
            }

            Category category = post.PostCategories.FirstOrDefault()?.Category;
            ViewBag.category = category;

            var otherPosts = _context.Posts.Where(p => p.PostCategories.Any(c => c.Category.Id == category.Id))
                                            .Where(p => p.PostId != post.PostId)
                                            .OrderByDescending(p => p.DateUpdated)
                                            .Take(5);
            ViewBag.otherPosts = otherPosts;

            return View(post);
        }

        private List<Category> GetCategories()
        {
            var categories = _context.Categories
                             .Include(c => c.CategoryChildren)
                             .AsEnumerable()
                             .Where(c => c.ParentCategory == null)
                             .ToList();

            return categories;
        }
    }
}